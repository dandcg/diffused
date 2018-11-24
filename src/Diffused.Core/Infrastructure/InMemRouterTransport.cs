using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Diffused.Core.Infrastructure
{
    public class InMemRouterTransport : ITransport
    {
        public TimeSpan Timeout { get; set; }

        public InMemRouterTransport(InMemRouter router, Address address, bool oneWay)
        {
            Consumer = new BufferBlock<MessageContainer>();
            Address = address;
            OneWay = oneWay;
            Router = router;
            Timeout = TimeSpan.FromMilliseconds(2000);
        }

        private BufferBlock<MessageContainer> Consumer { get; }

        public Address Address { get; }
        public bool OneWay { get; }
        public InMemRouter Router { get; }

        private string GenerateUuid()
        {
            return Guid.NewGuid().ToString();
        }

        public Task<MessageContainer> ReceiveAsync()
        {
            return Consumer.ReceiveAsync();
        }

        public async Task<MessageSendResult> SendAsync(Address address, Message message, TimeSpan? timeout = null)
        {


            var peer = await Router.GetPeer(address);

            if (peer == null)
            {
                return new MessageSendResult(null, MessageSendResultType.NotFound);
            }
            
            var messageContainer = new MessageContainer(message, Address, OneWay);

            await ((InMemRouterTransport) peer).Consumer.SendAsync(messageContainer);

            if (OneWay)
            {
                return new MessageSendResult(null, MessageSendResultType.OneWay);
            }

            var responseTask = messageContainer.ResponseChannel.ReceiveAsync();

            var timeoutTask = Task.Delay(timeout ?? TimeSpan.FromSeconds(30));

            var resultTask = await Task.WhenAny(responseTask, timeoutTask);

            if (resultTask == timeoutTask)
            {
                return new MessageSendResult(null, MessageSendResultType.Timeout);
            }

            var response = await responseTask;

            return new MessageSendResult(response, MessageSendResultType.Timeout);
        }

        // Disconnect is used to remove the ability to route to a given peer.
        public Task DisconnectAsync()
        {
            return Router.DisconnectAsync(Address);
        }
    }
}