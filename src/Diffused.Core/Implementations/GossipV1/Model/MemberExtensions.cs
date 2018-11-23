using System.Linq;
using Diffused.Core.Implementations.GossipV1.Messages;

namespace Diffused.Core.Implementations.GossipV1.Model
{
    public static class MemberExtensions
    {
        public static MemberData[] ToWire(this Member[] members)
        {
            return members.Select(m => new MemberData
            {
                State = m.State,
                Address = m.Address,
                Generation = m.Generation,
                Service = m.Service,
                ServicePort = m.ServicePort
            }).ToArray();
        }
    }
}