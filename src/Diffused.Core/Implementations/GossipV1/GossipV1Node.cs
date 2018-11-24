using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Diffused.Core.Implementations.GossipV1.Messages;
using Diffused.Core.Implementations.GossipV1.Model;
using Diffused.Core.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Diffused.Core.Implementations.GossipV1
{
    public class GossipV1Node : Node
    {
        private Member self;
        private int protocolPeriodMs;
        private int ackTimeoutMs;
        private int numberOfIndirectEndpoints;
        private  Address[] seedMembers;
        internal volatile bool Bootstrapping;
        private readonly object memberLocker = new object();
        private readonly Dictionary<Address, Member> members = new Dictionary<Address, Member>();
        private readonly object awaitingAcksLock = new object();
        private volatile Dictionary<Address, DateTime> awaitingAcks = new Dictionary<Address, DateTime>();
        private DateTime lastProtocolPeriod = DateTime.Now;
        private readonly Random rand = new Random();
        private readonly ILogger logger;
        private readonly IMediator mediator;
        private readonly ITransportFactory transportFactory;
        private ITransport transport;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private Task executingTask;

        public GossipV1Node(ILogger<GossipV1Node> logger, IMediator mediator,  ITransportFactory transportFactory)
        {
            this.logger = logger;
            this.mediator = mediator;
            this.transportFactory = transportFactory;

        }

        public void Configure(GossipV1NodeConfig config)
        {
        
            protocolPeriodMs = config.ProtocolPeriodMilliseconds;
            ackTimeoutMs = config.AckTimeoutMilliseconds;
            numberOfIndirectEndpoints = config.NumberOfIndirectEndpoints;
            seedMembers = config.SeedMembers;
            Bootstrapping = config.SeedMembers != null && config.SeedMembers.Length > 0;

            self = new Member
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
            transport = await transportFactory.Create(null);

            logger.LogInformation("Starting GossipV1 on {LocalAddress}", self.Address);

            var bootstrapper = Bootstraper(cts.Token);

            var listener = Listener(cts.Token);

            var gossiper = Gossiper(cts.Token);

            await Task.WhenAll(bootstrapper, listener, gossiper);
        }

        private async Task Bootstraper(CancellationToken cancellationToken)
        {
            if (Bootstrapping)
            {
                logger.LogInformation("GossipV1 bootstrapping off seeds");

                while (Bootstrapping && !cancellationToken.IsCancellationRequested)
                {
                    var i = rand.Next(0, seedMembers.Length);

                    await PingAsync(seedMembers[i]);

                    await Task.Delay(protocolPeriodMs, cancellationToken);
                }
            }

            else
            {
                logger.LogInformation("GossipV1 no seeds to bootstrap off");
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

                    logger.LogDebug("GossipV1 received {MessageType} from {RemoteEndPoint}", message.GetType().Name, request.RemoteAddress);

                    if (Bootstrapping)
                    {
                        Bootstrapping = false;
                        logger.LogInformation("GossipV1 finished bootstrapping");
                    }

                    Address destinationAddress = null;
                    Address sourceAddress = null;

                    if (message is Ping || message is Ack)
                    {
                        sourceAddress = request.RemoteAddress;
                        destinationAddress = self.Address;
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

                    if (message is GossipV1Message gmessage)
                    {
                        UpdateMembers(gmessage);
                        await RequestHandler(request, message, sourceAddress, destinationAddress, GetMembers());
                    }
                }

                catch (Exception ex)
                {
                    logger.LogError(ex, "GossipV1 threw an unhandled exception \n{message} \n{stacktrace}", ex.Message, ex.StackTrace);
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

                        await Task.Delay(ackTimeoutMs, cancellationToken);

                        // check was not acked
                        if (CheckWasNotAcked(member.Address))
                        {
                            var indirectEndpoints = GetIndirectAddresss(member.Address, membersList);

                            await PingRequestAsync(member.Address, indirectEndpoints, membersList);

                            await Task.Delay(ackTimeoutMs, cancellationToken);

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
                    logger.LogError(ex, "GossipV1 threw an unhandled exception \n{message} \n{stacktrace}", ex.Message, ex.StackTrace);
                }

                await WaitForProtocolPeriod().ConfigureAwait(false);
            }
        }

        private async Task RequestHandler(MessageContainer request, Message message, Address sourceAddress, Address destinationAddress, Member[] membersList)
        {
            if (message is Ping)
            {
                await AckAsync(sourceAddress, membersList).ConfigureAwait(false);
            }

            else if (message is Ack)
            {
                RemoveAwaitingAck(sourceAddress);
            }

            else if (message is PingRequest)
            {
                // if we are the destination send an ack request
                if (EndPointsMatch(destinationAddress, self.Address))
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
                if (EndPointsMatch(destinationAddress, self.Address))
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
            logger.LogDebug("GossipV1 sending Ping to {destinationAddress}", destinationAddress);
            await transport.SendAsync(destinationAddress, new Ping {MemberData = membersList.ToWire()});
        }

        private async Task PingRequestAsync(Address destinationGossipAddress, IEnumerable<Address> indirectAddresses, Member[] membersList = null)
        {
            foreach (var indirectEndpoint in indirectAddresses)
            {
                logger.LogDebug("GossipV1 sending PingRequest to {destinationAddress} via {indirectEndpoint}", destinationGossipAddress, indirectEndpoint);

                await transport.SendAsync(indirectEndpoint, new PingRequest {DestinationAddress = destinationGossipAddress, SourceAddress = self.Address, MemberData = membersList.ToWire()});
            }
        }

        private async Task PingRequestForwardAsync(Address destinationAddress, Address sourceAddress, Message message)
        {
            logger.LogDebug("GossipV1 forwarding PingRequest to {destinationAddress} from {sourceAddress}", destinationAddress, sourceAddress);
            await transport.SendAsync(destinationAddress, message);
        }

        private async Task AckAsync(Address destinationAddress, Member[] membersList)
        {
            logger.LogDebug("GossipV1 sending Ack to {destinationAddress}", destinationAddress);
            await transport.SendAsync(destinationAddress, new Ack {MemberData = membersList.ToWire()});
        }

        private async Task AckRequestAsync(Address destinationAddress, Address indirectEndPoint, Member[] membersList)
        {
            logger.LogDebug("GossipV1 sending AckRequest to {destinationAddress} via {indirectEndPoint}", destinationAddress, indirectEndPoint);
            await transport.SendAsync(indirectEndPoint, new AckRequest {DestinationAddress = destinationAddress, SourceAddress = self.Address, MemberData = membersList.ToWire()});
        }

        private async Task AckRequestForwardAsync(Address destinationAddress, Address sourceAddress, Message message)
        {
            logger.LogDebug("GossipV1 forwarding AckRequest to {destinationAddress} from {sourceAddress}", destinationAddress, sourceAddress);
            await transport.SendAsync(destinationAddress, message);
        }

        private void UpdateMembers(GossipV1Message message)
        {
            foreach (var m in message.MemberData)
            {
                var memberState = m.State;
                var address = m.Address;
                var generation = m.Generation;
                byte service = memberState == MemberState.Alive ? m.Service : (byte) 0;
                ushort servicePort = memberState == MemberState.Alive ? m.ServicePort : (ushort) 0;

                // we don't add ourselves to the member list
                if (!EndPointsMatch(address, self.Address))
                {
                    lock (memberLocker)
                    {
                        if (members.TryGetValue(address, out var member) &&
                            (member.IsLaterGeneration(generation) ||
                             member.Generation == generation && member.IsStateSuperseded(memberState)))
                        {
                            RemoveAwaitingAck(member.Address); // stops dead claim escalation

                            member.Update(memberState, generation, service, servicePort);

                            logger.LogInformation("GossipV1 member state changed {member}", member);
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

                            members.Add(address, member);
                            logger.LogInformation("GossipV1 member added {member}", member);
                        }
                    }
                }

                // handle any state claims about ourselves
                else if (self.IsLaterGeneration(generation) || memberState != MemberState.Alive && generation == self.Generation)
                {
                    self.Generation = (byte) (generation + 1);
                }
            }
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
                if (members.TryGetValue(address, out var member) && member.State == MemberState.Alive)
                {
                    member.Update(MemberState.Suspicious);
                    logger.LogInformation("GossipV1 suspicious member {member}", member);
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
                            if (members.TryGetValue(awaitingAck.Key, out var member) && (member.State == MemberState.Alive || member.State == MemberState.Suspicious))
                            {
                                member.Update(MemberState.Dead);
                                awaitingAcks.Remove(awaitingAck.Key);
                                logger.LogInformation("GossipV1 dead member {member}", member);
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
                if (!awaitingAcks.ContainsKey(address))
                {
                    awaitingAcks.Add(address, DateTime.Now.AddMilliseconds(protocolPeriodMs * 5));
                }
            }
        }

        private bool CheckWasNotAcked(Address address)
        {
            bool wasNotAcked;
            lock (awaitingAcksLock)
            {
                wasNotAcked = awaitingAcks.ContainsKey(address);
            }

            return wasNotAcked;
        }

        private void RemoveAwaitingAck(Address address)
        {
            lock (awaitingAcksLock)
            {
                if (awaitingAcks.ContainsKey(address))
                {
                    awaitingAcks.Remove(address);
                }
            }
        }

        private bool EndPointsMatch(Address ipEndPointA, Address ipEndPointB)
        {
            return ipEndPointA.Value == ipEndPointB.Value;
        }

        private async Task WaitForProtocolPeriod()
        {
            var syncTime = protocolPeriodMs - (int) (DateTime.Now - lastProtocolPeriod).TotalMilliseconds;
            await Task.Delay(syncTime).ConfigureAwait(false);
            lastProtocolPeriod = DateTime.Now;
        }

        public override async Task StopAsync()
        {

            if (executingTask == null)
            {
                return;
            }

            cts.Cancel();

            await transport.DisconnectAsync();
        }

   
    }
}