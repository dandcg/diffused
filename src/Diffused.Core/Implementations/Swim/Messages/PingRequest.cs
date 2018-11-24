using Diffused.Core.Implementations.Swim.Model;
using Diffused.Core.Infrastructure;

namespace Diffused.Core.Implementations.Swim.Messages
{
    public class PingRequest : Message, ISwimMessage
    {
        public MessageType MessageType { get; } = MessageType.PingRequest;
        public Address DestinationAddress { get; set; }
        public Address SourceAddress { get; set; }
        public MemberDataItem[] MemberData { get; set; }
    }
}