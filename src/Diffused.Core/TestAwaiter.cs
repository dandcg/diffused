using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Diffused.Core
{
    public static class TestAwaiter
    {
        private static readonly BufferBlock<object> queue = new BufferBlock<object>();

        public static Task SendAsync()
        {
            return queue.SendAsync(new object());
        }

        public static Task ReceiveAsync()
        {
            return queue.ReceiveAsync();
        }
    }
}