using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Diffused.Core.Implementations.Gossip.Swim.Messages;
using Diffused.Core.Implementations.Gossip.Swim.Model;
using Diffused.Core.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Diffused.Core.Implementations.Gossip.Swim
{
    public class SwimNode : Node
    {
        public Member Self { get; private set; }
        private int protocolPeriodMs;
        private int ackTimeoutMs;
        private int numberOfIndirectEndpoints;
        public Address[] SeedMembers { get; set; }
        private volatile bool bootstrapping;
        private readonly object memberLocker = new object();
        private readonly Dictionary<string, Member> members = new Dictionary<string, Member>();

        public IList<Member> Members
        {
            get
            {
                IList<Member> membersList;
                lock (memberLocker)
                {
                    membersList = members.Select(s => s.Value).ToList();
                }

                return membersList;
            }
        }

        private readonly object awaitingAcksLock = new object();
        private volatile Dictionary<string, DateTime> awaitingAcks = new Dictionary<string, DateTime>();
        private DateTime lastProtocolPeriod = DateTime.Now;
        private readonly Random rand = new Random();
        private readonly ILogger logger;
        private readonly IMediator mediator;
        private readonly ITransportFactory transportFactory;
        private ITransport transport;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public SwimNode(ILogger<SwimNode> logger, IMediator mediator, ITransportFactory transportFactory)
        {
            this.logger = logger;
            this.mediator = mediator;
            this.transportFactory = transportFactory;
        }

        public void Configure(SwimNodeConfig config)
        {
            protocolPeriodMs = config.ProtocolPeriodMilliseconds;
            ackTimeoutMs = config.AckTimeoutMilliseconds;
            numberOfIndirectEndpoints = config.NumberOfIndirectEndpoints;
            SeedMembers = config.SeedMembers;

            Self = new Member
            {
                State = MemberState.Alive,
                Address = config.ListenAddress,
                Generation = 1,
                Service = config.Service,
                ServicePort = config.ServicePort
            };
        }

        protected override async Task RunAsync()
        {
            transport = await transportFactory.Create(Self.Address);

            logger.LogInformation("{LocalAddress} starting", Self.Address);

            bootstrapping = SeedMembers != null && SeedMembers.Length > 0;

            var bootstrapper = Task.Run(() => Bootstraper(cts.Token), cts.Token);

            var listener = Task.Run(() => Listener(cts.Token), cts.Token);

            var gossiper = Task.Run(() => Gossiper(cts.Token), cts.Token);

            await Task.WhenAll(bootstrapper, listener, gossiper);
        }

        private async Task Bootstraper(CancellationToken cancellationToken)
        {
            if (bootstrapping)
            {
                logger.LogInformation("{LocalAddress} bootstrapping off seeds", Self.Address);

                while (bootstrapping && !cancellationToken.IsCancellationRequested)
                {
                    var i = rand.Next(0, SeedMembers.Length);

                    await PingAsync(SeedMembers[i]);

                    await Task.Delay(protocolPeriodMs, cancellationToken);
                }

                logger.LogInformation("{LocalAddress} finished bootstrapping", Self.Address);
            }

            else
            {
                logger.LogInformation("{LocalAddress} no seeds to bootstrap off", Self.Address);
            }
        }

        private async Task Listener(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var request = await transport.ReceiveAsync();

                    var message = request.Message;

                    logger.LogDebug("{LocalAddress} received {MessageType} from {RemoteEndPoint}", Self.Address, message.GetType().Name, request.RemoteAddress);

                    if (bootstrapping)
                    {
                        bootstrapping = false;
                    }

                    Address destinationAddress = null;
                    Address sourceAddress = null;

                    if (message is Ping || message is Ack)
                    {
                        sourceAddress = request.RemoteAddress;
                        destinationAddress = Self.Address;
                    }

                    else if (message is PingRequest pingRequest)
                    {
                        destinationAddress = pingRequest.DestinationAddress;
                        sourceAddress = pingRequest.SourceAddress;
                    }
                    else if (message is AckRequest ackRequest)
                    {
                        destinationAddress = ackRequest.DestinationAddress;
                        sourceAddress = ackRequest.SourceAddress;
                    }

                    if (message is ISwimMessage gmessage)
                    {
                        UpdateMembers(gmessage);
                        await RequestHandler(request, message, sourceAddress, destinationAddress, GetMembers());
                    }
                }

                catch (Exception ex)
                {
                    logger.LogError(ex, "{LocalAddress} threw an unhandled exception \n{message} \n{stacktrace}",Self.Address, ex.Message, ex.StackTrace);
                }
            }
        }

        private async Task Gossiper(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var membersList = GetMembers();

                    // ping member
                    if (membersList.Length > 0)
                    {
                        var i = rand.Next(0, membersList.Length);
                        var member = membersList[i];

                        AddAwaitingAck(member.Address);
                        await PingAsync(member.Address, membersList);

                        await Task.Delay(ackTimeoutMs, CancellationToken.None);

                        // check was not acked
                        if (CheckWasNotAcked(member.Address))
                        {
                            var indirectEndpoints = GetIndirectAddresss(member.Address, membersList);

                            await PingRequestAsync(member.Address, indirectEndpoints, membersList);

                            await Task.Delay(ackTimeoutMs, CancellationToken.None);

                            if (CheckWasNotAcked(member.Address))
                            {
                                HandleSuspiciousMember(member.Address);
                            }
                        }
                    }

                    HandleDeadMembers();
                }

                catch (Exception ex)
                {
                    logger.LogError(ex, "{LocalAddress} threw an unhandled exception \n{message} \n{stacktrace}",Self.Address, ex.Message, ex.StackTrace);
                }

                await WaitForProtocolPeriod();
            }
        }

        private async Task RequestHandler(MessageContainer request, Message message, Address sourceAddress, Address destinationAddress, Member[] membersList)
        {
            if (message is Ping)
            {
                await AckAsync(sourceAddress, membersList);
            }

            else if (message is Ack)
            {
                RemoveAwaitingAck(sourceAddress);
            }

            else if (message is PingRequest)
            {
                // if we are the destination send an ack request
                if (destinationAddress.Equals(Self.Address))
                {
                    await AckRequestAsync(sourceAddress, request.RemoteAddress, membersList);
                }

                // otherwise forward the request
                else
                {
                    await PingRequestForwardAsync(destinationAddress, sourceAddress, request.Message);
                }
            }

            else if (message is AckRequest)
            {
                // if we are the destination clear awaiting ack
                if (destinationAddress.Equals(Self.Address))
                {
                    RemoveAwaitingAck(sourceAddress);
                }

                // otherwise forward the request
                else
                {
                    await AckRequestForwardAsync(destinationAddress, sourceAddress, request.Message);
                }
            }
        }

        public async Task PingAsync(Address destinationAddress, Member[] membersList = null)
        {
            logger.LogDebug("{LocalAddress} sending Ping to {destinationAddress}", Self.Address, destinationAddress);

            var ping = new Ping();
            WriteMembers(ping, membersList);

            var result = await transport.SendAsync(destinationAddress, ping);

            if (result.Result != MessageSendResultType.OneWay)
            {
                logger.LogWarning("{LocalAddress} sent Ping to {destinationAddress}: Result = {SendResult}", Self.Address, destinationAddress, result.Result.ToString());
            }
        }

        private async Task PingRequestAsync(Address destinationGossipAddress, IEnumerable<Address> indirectAddresses, Member[] membersList = null)
        {
            foreach (var indirectEndpoint in indirectAddresses)
            {
                logger.LogDebug("{LocalAddress} sending PingRequest to {destinationAddress} via {indirectEndpoint}", Self.Address, destinationGossipAddress, indirectEndpoint);

                var pingRequest = new PingRequest {DestinationAddress = destinationGossipAddress, SourceAddress = Self.Address};
                WriteMembers(pingRequest, membersList);

                await transport.SendAsync(indirectEndpoint, pingRequest);
            }
        }

        private async Task PingRequestForwardAsync(Address destinationAddress, Address sourceAddress, Message message)
        {
            logger.LogDebug("{LocalAddress} forwarding PingRequest to {destinationAddress} from {sourceAddress}", Self.Address, destinationAddress, sourceAddress);
            await transport.SendAsync(destinationAddress, message);
        }

        private async Task AckAsync(Address destinationAddress, Member[] membersList)
        {
            logger.LogDebug("{LocalAddress} sending Ack to {destinationAddress}", Self.Address, destinationAddress);

            var ack = new Ack();
            WriteMembers(ack, membersList);

            await transport.SendAsync(destinationAddress, ack);
        }

        private async Task AckRequestAsync(Address destinationAddress, Address indirectEndPoint, Member[] membersList)
        {
            logger.LogDebug("{LocalAddress} sending AckRequest to {destinationAddress} via {indirectEndPoint}", Self.Address, destinationAddress, indirectEndPoint);

            var ackRequest = new AckRequest {DestinationAddress = destinationAddress, SourceAddress = Self.Address};
            WriteMembers(ackRequest, membersList);

            await transport.SendAsync(indirectEndPoint, ackRequest);
        }

        private async Task AckRequestForwardAsync(Address destinationAddress, Address sourceAddress, Message message)
        {
            logger.LogDebug("{LocalAddress} forwarding AckRequest to {destinationAddress} from {sourceAddress}", Self.Address, destinationAddress, sourceAddress);

            await transport.SendAsync(destinationAddress, message);
        }

        private void UpdateMembers(ISwimMessage message)
        {
            foreach (var m in message.MemberData)
            {
                var memberState = m.State;
                var address = m.Address;
                var generation = m.Generation;
                byte service = memberState == MemberState.Alive ? m.Service : (byte) 0;
                ushort servicePort = memberState == MemberState.Alive ? m.ServicePort : (ushort) 0;

                // we don't add ourselves to the member list
                if (address.Value!=Self.Address.Value)
                {
                    lock (memberLocker)
                    {
                        if (members.TryGetValue(address.Value, out var member) &&
                            (member.IsLaterGeneration(generation) ||
                             member.Generation == generation && member.IsStateSuperseded(memberState)))
                        {
                            RemoveAwaitingAck(member.Address); // stops dead claim escalation

                            member.Update(memberState, generation, service, servicePort);

                            logger.LogInformation("{LocalAddress} member state changed {member}", Self.Address, member);
                        }

                        else if (member == null)
                        {
                            member = new Member
                            {
                                State = memberState,
                                Address = address,
                                Generation = generation,
                                ServicePort = servicePort,
                                Service = service
                            };

                            members.Add(address.Value, member);
                            logger.LogInformation("{LocalAddress} member added {member}", Self.Address, member);
                        }
                    }
                }

                // handle any state claims about ourselves
                else if (Self.IsLaterGeneration(generation) || memberState != MemberState.Alive && generation == Self.Generation)
                {
                    Self.Generation = (byte) (generation + 1);
                }
            }
        }

        public void WriteMembers(ISwimMessage memberdataMessage, Member[] membersList)
        {
            List<MemberDataItem> list = new List<MemberDataItem>();

            var totalMembers = new List<Member> {Self};

            if (membersList != null)
            {
                totalMembers.AddRange(membersList);
            }

            // TODO - don't just iterate over the members, optimize make intelligent

            list.AddRange(totalMembers.Select(m => new MemberDataItem
            {
                State = m.State,
                Address = m.Address,
                Generation = m.Generation,
                Service = m.Service,
                ServicePort = m.ServicePort
            }));

            memberdataMessage.MemberData = list.ToArray();
        }

        private Member[] GetMembers()
        {
            lock (memberLocker)
            {
                return members
                    .Values
                    .OrderBy(m => m.GossipCounter)
                    .ToArray();
            }
        }

        private IEnumerable<Address> GetIndirectAddresss(Address directAddress, Member[] membersList)
        {
            if (membersList.Length <= 1)
            {
                return Enumerable.Empty<Address>();
            }

            var randomIndex = rand.Next(0, membersList.Length);

            return Enumerable.Range(randomIndex, numberOfIndirectEndpoints + 1)
                .Select(ri => ri % membersList.Length) // wrap the range around to 0 if we hit the end
                .Select(i => membersList[i])
                .Where(m => m.Address != directAddress)
                .Select(m => m.Address)
                .Distinct()
                .Take(numberOfIndirectEndpoints);
        }

        private void HandleSuspiciousMember(Address address)
        {
            lock (memberLocker)
            {
                if (members.TryGetValue(address.Value, out var member) && member.State == MemberState.Alive)
                {
                    member.Update(MemberState.Suspicious);
                    logger.LogInformation("{LocalAddress} suspicious member {member}", Self.Address, member);
                }
            }
        }

        private void HandleDeadMembers()
        {
            lock (awaitingAcksLock)
            {
                foreach (var awaitingAck in awaitingAcks.ToArray())
                {
                    // if we haven't received an ack before the timeout
                    if (awaitingAck.Value < DateTime.Now)
                    {
                        lock (memberLocker)
                        {
                            if (members.TryGetValue(awaitingAck.Key, out var member) && member.State == MemberState.Alive || member.State == MemberState.Suspicious)
                            {
                                member.Update(MemberState.Dead);
                                awaitingAcks.Remove(awaitingAck.Key);
                                logger.LogInformation("{LocalAddress} dead member {member}", Self.Address, member);
                            }
                        }
                    }

                    // prune dead members
                }
            }
        }

        private void AddAwaitingAck(Address address)
        {
            lock (awaitingAcksLock)
            {
                if (!awaitingAcks.ContainsKey(address.Value))
                {
                    awaitingAcks.Add(address.Value, DateTime.Now.AddMilliseconds(protocolPeriodMs * 5));
                }
            }
        }

        private bool CheckWasNotAcked(Address address)
        {
            bool wasNotAcked;
            lock (awaitingAcksLock)
            {
                wasNotAcked = awaitingAcks.ContainsKey(address.Value);
            }

            return wasNotAcked;
        }

        private void RemoveAwaitingAck(Address address)
        {
            lock (awaitingAcksLock)
            {
                if (awaitingAcks.ContainsKey(address.Value))
                {
                    awaitingAcks.Remove(address.Value);
                }
            }
        }
        
        private async Task WaitForProtocolPeriod()
        {
            var syncTime = protocolPeriodMs - (int) (DateTime.Now - lastProtocolPeriod).TotalMilliseconds;
            if (syncTime > 0)
            {
                await Task.Delay(syncTime);
            }

            lastProtocolPeriod = DateTime.Now;
        }

        public override async Task StopAsync()
        {
            cts.Cancel();

            await transport.DisconnectAsync();
        }
    }
}