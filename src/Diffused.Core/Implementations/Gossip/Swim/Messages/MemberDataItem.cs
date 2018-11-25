using Diffused.Core.Implementations.Gossip.Swim.Model;
using Diffused.Core.Infrastructure;

namespace Diffused.Core.Implementations.Gossip.Swim.Messages
{
    public class MemberDataItem
    {
        public MemberState State { get; set; }
        public Address Address { get; set; }

        public byte Generation { get; set; }
        public byte Service { get; set; }
        public ushort ServicePort { get; set; }
    }
}