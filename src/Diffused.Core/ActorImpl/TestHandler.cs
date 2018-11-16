using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Diffused.Core.ActorImpl
{
    public class TestHandler:INotificationHandler<Test>
    {
        public Task Handle(Test notification, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}