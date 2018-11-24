using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Diffused.Core.Infrastructure
{
    public class InMemRouter
    {
        public ConcurrentDictionary<string, ITransport> Connections { get; private set; }

        public InMemRouter()
        {
            Connections = new ConcurrentDictionary<string, ITransport>();
        }

        public Task<ITransport> Register(Address address, bool oneWay)
        {
            if (address == null)
            {
                throw new Exception("Address must be defined!");
            }

            var trans = new InMemRouterTransport(this, address, oneWay);
            Connections.AddOrUpdate(address.Value, trans, (s, t) => trans);
            return Task.FromResult((ITransport) trans);
        }

        public Task<ITransport> GetPeer(Address address)
        {
            if (!Connections.TryGetValue(address.Value, out var peer))
            {
                return Task.FromResult((ITransport)null);
            }

            return Task.FromResult(peer);
        }

        // Disconnect is used to remove the ability to route to a given peer.
        public Task DisconnectAsync(Address peer)
        {
            Connections.TryRemove(peer.Value, out var transport);
            return Task.FromResult(1);
        }

        // DisconnectAll is used to remove all routes to peers.
        public Task DisconnectAllAsync()
        {
            Connections = new ConcurrentDictionary<string, ITransport>();
            return Task.FromResult(1);
        }
    }
}