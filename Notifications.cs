using System;
using System.Collections.Generic;
using System.Net;
using CometD.NetCore.Bayeux;
using CometD.NetCore.Bayeux.Client;
using CometD.NetCore.Client;
using CometD.NetCore.Client.Transport;
using Genesys.Internal.Statistics.Api;
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

        public void Initialize(StatisticsApi api)
        {
            WebHeaderCollection headers = new WebHeaderCollection();
            foreach (string key in api.Configuration.DefaultHeader.Keys)
            {
                switch (key)
                {
                    case "x-api-key":
                    case "Authorization":
                        headers.Add(key, api.Configuration.DefaultHeader[key]);
                        break;
                }
            }

            CookieCollection cookieCollection = CookieManager.Cookies.GetCookies(new Uri(api.GetBasePath()));
            /**
             * GWS currently only supports LongPolling as a method to receive events.
             * So tell the CometD library to negotiate a handshake with GWS and setup a LongPolling session.
             */
            LongPollingTransport transport = new LongPollingTransport(null);
            transport.CookieCollection = cookieCollection;
            transport.HeaderCollection = headers;

            bayeuxClient = new BayeuxClient(api.GetBasePath() + "/notifications", new List<CometD.NetCore.Client.Transport.ClientTransport>() { transport });

            bayeuxClient.Handshake();
            bayeuxClient.WaitFor(30000, new List<BayeuxClient.State>() { BayeuxClient.State.Connected });

            if (bayeuxClient.Connected)
            {
                foreach (Cookie cookie in cookieCollection)
                {
                    CookieManager.AddCookie(cookie);    
                }

                foreach (string channelName in subscriptions.Keys)
                {
                    IClientSessionChannel channel = bayeuxClient.GetChannel(channelName);
                    channel.Subscribe(this);
                }
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
                subscriptions[message.Channel](channel, message, null);
            }
            catch (Exception exc)
            {
                log.Error("Execption handling OnMessage for " + message.Channel, exc);
            }
        }
    }
}

