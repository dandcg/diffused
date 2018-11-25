using Diffused.Core.Infrastructure;

namespace Diffused.Core.Implementations.Gossip.Swim
{
    public class SwimNodeConfig
    {
        public int ProtocolPeriodMilliseconds { get; set; } = 200;
        public int AckTimeoutMilliseconds { get; set; } = 80;
        public int NumberOfIndirectEndpoints { get; set; } = 2;
        public Address ListenAddress { get; set; }
        public byte Service { get; set; }
        public ushort ServicePort { get; set; }
        public Address[] SeedMembers { get; set; }
    }
}