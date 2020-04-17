using Company.Fred.Core.Configuration.Interfaces;
using Company.Fred.Subscription.Communication.Configuration.Interfaces;
using Company.Fred.Subscription.Exceptions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Company.Fred.Subscription.Communication.Services.Proxies
{
    public abstract class CommunicationProxyBase<TClient> where TClient : ClientBase
    {
        private readonly ICommunicationConfigurationSettings communicationConfiguration;
        private readonly ILogger<CommunicationProxyBase<TClient>> logger;
        private readonly IRetryConfiguration retryConfiguration;
        private readonly Func<Channel, TClient> clientCreator;

        public CommunicationProxyBase(ICommunicationConfigurationSettings communicationConfiguration,
            IRetryConfiguration retryConfiguration,
            ILogger<CommunicationProxyBase<TClient>> logger,
            Func<ChannelBase, TClient> clientCreator)
        {
            this.communicationConfiguration = communicationConfiguration;
            this.logger = logger;
            this.retryConfiguration = retryConfiguration;
            this.clientCreator = clientCreator;
        }

        protected async Task<TResult> Request<TResult>(Func<TClient, Task<TResult>> func)
        {

            for (int i = 0; i < retryConfiguration.Limit; i++)
            {
                var clientConnection = GetInitializedConnection();
                try
                {
                    return await func(clientConnection.Client);
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Restarting connection to central Gazoo ({clientConnection.Channel.Target})\n Exception: {ex.Message}");
                    await Task.Delay(retryConfiguration.Frequency);
                }
                finally
                {
                    await CloseConnectionAsync(clientConnection.Channel);
                }
            }

            throw new RequestFailedException();
        }

        private ClientConnection GetInitializedConnection()
        {
            var channel = new Channel($"{communicationConfiguration.Ip}:{communicationConfiguration.Port}", ChannelCredentials.Insecure);
            var clientConnection = clientCreator(channel);

            return new ClientConnection()
            {
                Channel = channel,
                Client = clientConnection
            };
        }

        protected async Task CloseConnectionAsync(Channel channel)
        {
            try
            {
                if (channel.State != ChannelState.Shutdown)
                    await channel.ShutdownAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Channel for ({channel.Target} couldn't close.)", ex.Message);
            }
        }

        #region Nested Class
        private class ClientConnection
        {
            public Channel Channel { get; set; }
            public TClient Client { get; set; }
        }
        #endregion
    }
}
