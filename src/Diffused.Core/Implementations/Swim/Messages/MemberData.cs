using Diffused.Core.Implementations.Swim.Model;
using Diffused.Core.Infrastructure;

namespace Diffused.Core.Implementations.Swim.Messages
{
    public class MemberData
    {
        public MemberState State { get; set; }
        public Address Address { get; set; }

        public byte Generation { get; set; }
        public byte Service { get; set; }
        public ushort ServicePort { get; set; }
    }
}