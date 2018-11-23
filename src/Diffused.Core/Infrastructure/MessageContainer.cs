using System.Threading.Tasks.Dataflow;

namespace Diffused.Core.Infrastructure
{
    public class MessageContainer
    {
        public MessageContainer(Message message, Address address, bool oneWay)
        {
            Message = message;
            RemoteAddress = address;

            if (!oneWay)
            {
                ResponseChannel = new BufferBlock<Message>();
            }
        }

        public Address RemoteAddress { get; }

        public Message Message { get; }

        public BufferBlock<Message> ResponseChannel { get; }
    }
}