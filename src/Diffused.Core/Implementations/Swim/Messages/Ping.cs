using Diffused.Core.Infrastructure;

namespace Diffused.Core.Implementations.Swim.Messages
{
    public class Ping : Message,IMemberData
    {
        public MemberDataItem[] MemberData { get; set; }
    }
}