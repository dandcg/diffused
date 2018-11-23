using System.Threading.Tasks;

namespace Diffused.Core.Infrastructure
{
    public interface INode
    {
        Task RunAsync();
        Task StopAsync();
    }
}