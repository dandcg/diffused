using System;
using MediatR;

namespace Diffused.Core.ActorImpl
{
    public class Test : IRequest
    {
        public Guid NodeId { get; }

        public Test(Guid nodeId)
        {
            NodeId = nodeId;
        }
    }
}