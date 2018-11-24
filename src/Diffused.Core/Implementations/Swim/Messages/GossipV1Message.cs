using Diffused.Core.Infrastructure;

namespace Diffused.Core.Implementations.Swim.Messages
{
    public class GossipV1Message : Message
    {
        public MemberData[] MemberData { get; set; }
    }
}