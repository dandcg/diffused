using System.Threading.Tasks;

namespace Diffused.Core.Infrastructure
{
    public class TransportFactory : ITransportFactory
    {
        private readonly InMemRouter router;

        public TransportFactory(InMemRouter router)
        {
            this.router = router;
        }

        public Task<ITransport> Create(Address address)
        {
            return router.Register(address, true);
        }
    }
}