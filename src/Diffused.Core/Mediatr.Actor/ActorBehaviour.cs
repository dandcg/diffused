using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Diffused.Core.Mediatr.Actor
{
    public class ActorBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TResponse : new()
    {
        private readonly ILogger logger;
        private readonly ActorManager actorManager;

        public ActorBehaviour(ILogger<NodeHostedService> logger, ActorManager actorManager)
        {
            this.logger = logger;
            this.actorManager = actorManager;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken token, RequestHandlerDelegate<TResponse> next)
        {
            var actorRequest = request as ActorRequest;

            if (actorRequest == null || actorRequest.HandleNow || actorRequest.ActorRequestType == ActorRequestType.RequestResponse)
            {
                var sw = new Stopwatch();

                sw.Start();

                var result = await next.Invoke();

                sw.Stop();

                logger.LogInformation($"Handled [{typeof(TRequest).Name}] in {sw.ElapsedMilliseconds} ms.");

                return result;
            }

            if (actorRequest?.ActorRequestType == ActorRequestType.Send)
            {
                if (typeof(TResponse) != typeof(ActorResult))
                {
                    throw new Exception($"Incorrect Response Type {typeof(TResponse).Name} - should be Unit!");
                }

                var response = new TResponse();

                var actorResult = response as ActorResult;
                if (actorResult != null)
                {
                    actorResult.Result = await actorManager.Send(request);
                }

                logger.LogInformation($"Queued [{typeof(TRequest).Name}]");

                return response;
            }

            if (actorRequest?.ActorRequestType == ActorRequestType.Post)
            {
                if (typeof(TResponse) != typeof(ActorResult))
                {
                    throw new Exception($"Incorrect Response Type {typeof(TResponse).Name} - should be Unit!");
                }

                var response = new TResponse();

                var actorResult = response as ActorResult;
                if (actorResult != null)
                {
                    actorResult.Result = await actorManager.Post(request);
                }

                return response;
            }

            return new TResponse();
        }
    }
}