using Diffused.Core.Infrastructure;

namespace Diffused.Core.Implementations.Swim.Messages
{
    public class AckRequest : GossipV1Message

    {
        public Address DestinationAddress { get; set; }
        public Address SourceAddress { get; set; }
    }
}