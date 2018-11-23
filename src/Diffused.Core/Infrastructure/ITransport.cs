using System;
using System.Threading.Tasks;

namespace Diffused.Core.Infrastructure
{
    public interface ITransport
    {
        Address Address { get; }

        Task<MessageContainer> ReceiveAsync();

        Task<MessageSendResult> SendAsync(Address address, Message message, TimeSpan? timeout = null);

        Task DisconnectAsync();
    }

    public interface ITransportInternal
    {
    }
}