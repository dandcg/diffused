using System;
using Diffused.Core.Mediatr.Actor;

namespace Diffused.Core.Actors.Test
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