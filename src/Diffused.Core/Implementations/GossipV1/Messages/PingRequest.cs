using Diffused.Core.Infrastructure;

namespace Diffused.Core.Implementations.GossipV1.Messages
{
    public class PingRequest : GossipV1Message
    {
        public Address DestinationAddress { get; set; }
        public Address SourceAddress { get; set; }
    }
}