using Diffused.Core.Implementations.Gossip.Swim.Model;

namespace Diffused.Core.Implementations.Gossip.Swim.Messages
{
    public interface ISwimMessage
    {
        MessageType MessageType { get; }
        MemberDataItem[] MemberData { get; set; }
    }
}