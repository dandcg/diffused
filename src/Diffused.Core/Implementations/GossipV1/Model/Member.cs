using System.Threading;
using Diffused.Core.Infrastructure;

namespace Diffused.Core.Implementations.GossipV1.Model
{
    public class Member
    {
        private long gossipCounter;

        public MemberState State { get; set; }
        public Address Address { get; set; }
        public byte Generation { get; set; }
        public byte Service { get; set; }
        public ushort ServicePort { get; set; }
        public long GossipCounter => Interlocked.Read(ref gossipCounter);

        public void Update(MemberState state, byte generation, byte service = 0, ushort servicePort = 0)
        {
            State = state;
            Generation = generation;

            if (state == MemberState.Alive)
            {
                Service = service;
                ServicePort = servicePort;
            }

            Interlocked.Exchange(ref gossipCounter, 0);
        }

        public void Update(MemberState state)
        {
            State = state;
            Interlocked.Exchange(ref gossipCounter, 0);
        }

        public bool IsLaterGeneration(byte newGeneration)
        {
            return 0 < newGeneration - Generation && newGeneration - Generation < 191
                   || newGeneration - Generation <= -191;
        }

        public bool IsStateSuperseded(MemberState newState)
        {
            // alive < suspicious < dead < left
            return State == MemberState.Alive && newState != MemberState.Alive ||
                   State == MemberState.Suspicious && (newState == MemberState.Dead || newState == MemberState.Left) ||
                   State == MemberState.Dead && newState == MemberState.Left;
        }

        public override string ToString()
        {
            return $"State:{State} Address:{Address} Generation:{Generation} Service:{Service} ServicePort:{ServicePort}";
        }
    }
}