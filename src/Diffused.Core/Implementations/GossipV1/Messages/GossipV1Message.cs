using Diffused.Core.Infrastructure;

namespace Diffused.Core.Implementations.GossipV1.Messages
{
    public class GossipV1Message : Message
    {
        public MemberData[] MemberData { get; set; }
    }
}