using System.Threading.Tasks;

namespace Diffused.Core.Mediatr.Actor
{
    public interface IActorController
    {
        void Complete();

        Task Completion { get; }
        int InputCount { get; }
        void Cancel();
    }
}