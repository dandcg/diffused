using System;
using MediatR;

namespace Diffused.Core.ActorImpl
{
    public class Test :INotification
    {
        private readonly Guid nodeId;

        public Test(Guid nodeId)
        {
            this.nodeId = nodeId;
  }
    }


}
