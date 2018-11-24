using System.Collections.Generic;
using System.Threading.Tasks;
using Diffused.Core.Implementations.Swim;
using Diffused.Core.Infrastructure;

namespace Diffused.Tests.GossipV1
{
    public class NodeCollection : List<SwimNode>
    {
        public static NodeCollection Create(NodeFactory nf, int n)
        {
            var nodeCollection = new NodeCollection();

            for (int i = 0; i < n; i++)
            {
                var node = nf.CreateSwimNode(new SwimNodeConfig {ListenAddress = new Address($"Node[{i + 1}]")}) as SwimNode;
                nodeCollection.Add(node);
            }

            return nodeCollection;
        }

        public async Task StartAllAsync()
        {
            var tasks = new List<Task>();
            foreach (var n in this)
            {
                tasks.Add(n.StartAsync());
            }

            await Task.WhenAll(tasks);
        }

        public async Task StopAllAsync()
        {
            var tasks = new List<Task>();
            foreach (var n in this)
            {
                tasks.Add(n.StopAsync());
            }

            await Task.WhenAll(tasks);
        }
    }
}