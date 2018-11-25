using Diffused.Core.Implementations.Gossip.Swim.Model;
using Diffused.Core.Infrastructure;

namespace Diffused.Core.Implementations.Gossip.Swim.Messages
{
    public class Ping : Message, ISwimMessage
    {
        public MessageType MessageType { get; } = MessageType.Ping;
        public MemberDataItem[] MemberData { get; set; }
    }
}