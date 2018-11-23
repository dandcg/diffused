namespace Diffused.Core.Infrastructure
{
    public class MessageSendResult
    {
        public MessageSendResult(Message response, MessageSendResultType result)
        {
            Response = response;
            Result = result;
        }

        public Message Response { get; }

        public MessageSendResultType Result { get; }
    }
}