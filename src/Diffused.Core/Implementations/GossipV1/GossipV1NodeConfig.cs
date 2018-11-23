using Diffused.Core.Infrastructure;

namespace Diffused.Core.Implementations.GossipV1
{
    public class GossipV1NodeConfig
    {
        public int MaxUdpPacketBytes { get; set; }
        public int ProtocolPeriodMilliseconds { get; set; }
        public int AckTimeoutMilliseconds { get; set; }
        public int NumberOfIndirectEndpoints { get; set; }
        public Address ListenAddress { get; set; }
        public ushort ListenPort { get; set; }
        public byte Service { get; set; }
        public ushort ServicePort { get; set; }
        public Address[] SeedMembers { get; set; }
    }
}