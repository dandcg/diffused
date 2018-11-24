using Diffused.Core.Implementations.Swim.Model;

namespace Diffused.Core.Implementations.Swim.Messages
{
    public interface ISwimMessage
    {
        MessageType MessageType { get; }
        MemberDataItem[] MemberData { get; set; }
    }
}