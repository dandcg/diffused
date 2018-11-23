using System.Threading.Tasks;

namespace Diffused.Core.Infrastructure
{
    public interface ITransportFactory
    {
        Task<ITransport> Create(Address address);
    }
}