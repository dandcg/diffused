using Diffused.Core.Infrastructure;

namespace Diffused.Core.Implementations.Swim.Messages
{
    public class PingRequest : Message,IMemberData
    {
        public Address DestinationAddress { get; set; }
        public Address SourceAddress { get; set; }
        public MemberDataItem[] MemberData { get; set; }
    }
}