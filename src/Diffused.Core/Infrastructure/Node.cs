using System.Threading.Tasks;

namespace Diffused.Core.Infrastructure
{
    public abstract class Node : INode
    {
        private Task executingTask;

        public Task StartAsync()
        {
            executingTask = RunAsync();

            return executingTask.IsCompleted ? executingTask : Task.CompletedTask;
        }

        protected abstract Task RunAsync();

        public abstract Task StopAsync();
    }
}