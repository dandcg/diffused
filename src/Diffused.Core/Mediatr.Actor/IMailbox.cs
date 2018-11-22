using System.Threading.Tasks;

namespace Diffused.Core.Mediatr.Actor
{
    public interface IMailbox
    {
        void Complete();

        Task Completion { get; }
    }
}