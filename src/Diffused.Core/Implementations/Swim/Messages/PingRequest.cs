using Diffused.Core.Infrastructure;

namespace Diffused.Core.Implementations.Swim.Messages
{
    public class PingRequest : GossipV1Message
    {
        public Address DestinationAddress { get; set; }
        public Address SourceAddress { get; set; }
    }
}