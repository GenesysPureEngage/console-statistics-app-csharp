using System;
using System.Collections.Generic;
using System.Net;
using CometD.NetCore.Bayeux;
using CometD.NetCore.Bayeux.Client;
using CometD.NetCore.Client;
using CometD.NetCore.Client.Transport;
using Genesys.Internal.Statistics.Client;

namespace consolestatisticsappcsharp
{
    public class Notifications : IMessageListener
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void CometDEventHandler(IClientSessionChannel channel, IMessage message, BayeuxClient client);

        private BayeuxClient bayeuxClient;
        private Dictionary<string, CometDEventHandler> subscriptions;

        public Notifications()
        {
            subscriptions = new Dictionary<string, CometDEventHandler>();
        }

        public void Initialize(ApiClient apiClient)
        {
            WebHeaderCollection headers = new WebHeaderCollection();

            foreach(string key in apiClient.Configuration.DefaultHeader.Keys)
            {
                switch(key)
                {
                    case "x-api-key":
                    case "Cookie":
                    case "Authorization":    
                        headers.Add(key, apiClient.Configuration.DefaultHeader[key]);
                        break;
                }
            }

            /**
             * GWS currently only supports LongPolling as a method to receive events.
             * So tell the CometD library to negotiate a handshake with GWS and setup a LongPolling session.
             */
            LongPollingTransport transport = new LongPollingTransport(null);
            transport.CookieCollection = apiClient.RestClient.CookieContainer.GetCookies(new Uri(apiClient.RestClient.BaseUrl.ToString() + "/notifications"));
            transport.HeaderCollection = headers;

            List<ClientTransport> transports = new List<ClientTransport>();
            transports.Add(transport);

            bayeuxClient = new BayeuxClient(apiClient.RestClient.BaseUrl.ToString() + "/notifications", transports);
            bayeuxClient.SetDebugEnabled(true);

            bayeuxClient.Handshake();
            bayeuxClient.WaitFor(30000, new List<BayeuxClient.State>() { BayeuxClient.State.Connected });

            foreach(string channelName in subscriptions.Keys )
            {
                IClientSessionChannel channel = bayeuxClient.GetChannel(channelName);
                channel.Subscribe(this);
            }
        }

        public void subscribe(String channelName, CometDEventHandler eventHandler)
        {
            subscriptions.Add(channelName, eventHandler);
        }

        public void Disconnect()
        {
            if (bayeuxClient != null && bayeuxClient.Connected)
            {
                bayeuxClient.Disconnect();
            }
        }

        public void OnMessage(IClientSessionChannel channel, IMessage message)
        {
            try
            {
                subscriptions[message.Channel](channel, message, bayeuxClient);
            }
            catch (Exception exc)
            {
                log.Error("Execption handling OnMessageReceived for " + message.Channel, exc);
            }
        }
    }
}
