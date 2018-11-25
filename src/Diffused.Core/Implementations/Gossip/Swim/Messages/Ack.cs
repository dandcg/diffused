using Diffused.Core.Implementations.Gossip.Swim.Model;
using Diffused.Core.Infrastructure;

namespace Diffused.Core.Implementations.Gossip.Swim.Messages
{
    public class Ack : Message, ISwimMessage
    {
        public MessageType MessageType { get; } = MessageType.Ack;
        public MemberDataItem[] MemberData { get; set; }
    }
}