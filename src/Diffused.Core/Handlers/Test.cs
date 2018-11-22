using System;
using Diffused.Core.Mediatr.Actor;

namespace Diffused.Core.Handlers
{
    public class Test : ActorRequest
    {
        public Guid NodeId { get; }

        public Test(Guid nodeId)
        {
            NodeId = nodeId;
        }
    }
}