using System.Collections.Generic;
using System.Linq;
using Diffused.Core.Implementations.Swim.Messages;

namespace Diffused.Core.Implementations.Swim.Model
{
    public static class MemberExtensions
    {
        public static MemberData[] ToWire(this Member[] members)
        {
            List<MemberData> list = new List<MemberData>();
            if (members == null)
            {
                return list.ToArray();
            }

            list.AddRange(members.Select(m => new MemberData
            {
                State = m.State,
                Address = m.Address,
                Generation = m.Generation,
                Service = m.Service,
                ServicePort = m.ServicePort
            }));

            return list.ToArray();
        }
    }
}