using MediatR;

namespace Diffused.Core.Mediatr.Actor
{
    public class ActorRequest : IRequest<ActorResult>
    {
        public ActorRequestType ActorRequestType { get; } = ActorRequestType.Send;
        public bool HandleNow { get; private set; }

        public void SetHandleNow()
        {
            HandleNow = true;
        }
    }
}