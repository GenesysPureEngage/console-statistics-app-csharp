using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;
using System.Collections;
using System.Threading.Tasks;
using Genesys.Internal.Statistics.Client;
using Genesys.Internal.Statistics.Api;
using Genesys.Internal.Statistics.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using CometD.NetCore.Bayeux.Client;
using CometD.NetCore.Bayeux;
using CometD.NetCore.Client;

namespace consolestatisticsappcsharp
{
    public class StatisticsClientApi
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool statisticsInitialized = false;
        //private ApiClient apiClient;
        private StatisticsApi api;
        private Notifications notifications;
        private JObject jsonSubscription = null;
        private string baseUrl = null;

        public StatisticsClientApi(String apiKey, String baseUrl)
        {
            this.baseUrl = baseUrl;

            api = new StatisticsApi(baseUrl + "/statistics/v3");
            api.Configuration.AddDefaultHeader("x-api-key", apiKey);
            api.Configuration.ApiClient.RestClient.CookieContainer = CookieManager.Cookies;

            notifications = new Notifications();
        } 

        void OnServiceStateChanged(IClientSessionChannel channel, IMessage message, BayeuxClient client)
        {
            log.Debug("++++OnServiceStateChanged++++ received: " + message.ToString());
        }

        void OnStatisticUpdate(IClientSessionChannel channel, IMessage message, BayeuxClient client)
        {
            log.Debug("\n\n\n++++OnStatisticsUpdate++++ received: \n" + message.ToString() + "\n\n\n");
        }

        public void DumpCookies(String baseUrl)
        {
            CookieManager.DumpCookies(baseUrl);
        }

        public void Initialize(string token)
        {
            Initialize(null, null, token);
        }

        public void Initialize(string authCode, string redirectUri, string token)
        {
            api.Configuration.AddDefaultHeader("Authorization", "bearer " + token);

            try
            {
                notifications.subscribe("/statistics/v3/service", OnServiceStateChanged);
                notifications.subscribe("/statistics/v3/updates", OnStatisticUpdate);
                notifications.Initialize(api);
            }
            catch (Exception exc)
            {
                log.Error("Failed to initialize statistics", exc);
            }
        }

        public void Subscribe()
        {
            //string statistics = @"{
            //    'operationId': 'UUID_OID_1_TIAA',
            //    'data':
            //    {
            //        'statistics':
            //        [
            //            {
            //                'statisticId': 'UUID_SID_1_TIAA',
            //                'objectId': 'TIAA_Agents',
            //                'objectType': 'GroupAgents',
            //                'definition': 
            //                {
            //                    'notificationMode': 'Immediate',
            //                    'category': 'CurrentState',
            //                    'subject': 'GroupStatus',
            //                    'mainMask': '*',
            //                    'objects': 'GroupAgents,GroupPlaces'
            //                }
            //            }
            //        ]
            //    }
            //}";

            //string statistics = @"{
            //    'operationId': 'UUID_OID_1_TIAA',
            //    'data':
            //    {
            //        'statistics':
            //        [
            //            {
            //                'statisticId': 'UUID_SID_1_TIAA',
            //                'objectId': 'Jim_Crespino_Agent@sydneydemo.com',
            //                'objectType': 'Agent',
            //                'definition': 
            //                {
            //                    'notificationMode': 'Immediate',
            //                    'category': 'CurrentState',
            //                    'subject': 'AgentStatus',
            //                    'mainMask': '*',
            //                    'objects': 'Agent'
            //                }
            //            }
            //        ]
            //    }
            //}";


            //string statistics = @"{
            //    'operationId': 'UUID_OID_1_TIAA',
            //    'data':
            //    {
            //        'statistics':
            //        [
            //            {
            //                'statisticId': 'UUID_SID_1_TIAA',
            //                'objectId': 'jcrespino_TIAA',
            //                'objectType': 'Agent',
            //                'name': 'CurrentAgentState'
            //            }
            //        ]
            //    }
            //}";

            if (jsonSubscription != null)
                return;
            
            string subscription = @"{
                'operationId': '',
                'data':
                {
                    'statistics':[]
                }
            }";
            
            string template = File.ReadAllText("template.json");

            jsonSubscription = JObject.Parse(subscription);
            jsonSubscription["operationId"] = Guid.NewGuid();

            JArray jsonStatistics = JArray.Parse(template);
            foreach(JObject jsonStatistic in jsonStatistics)
            {
                jsonStatistic["statisticId"] = Guid.NewGuid();
            }
            jsonSubscription["data"]["statistics"] = jsonStatistics;

            StatisticDataResponse response = api.CreateSubscription(jsonSubscription.ToString());

            log.Debug(response.ToJson());
        }

        public void PeekStatistic()
        {
            if (jsonSubscription == null)
                return;
            
            string operationId = (string)jsonSubscription["operationId"];
            JArray jsonStatistics = (JArray)jsonSubscription["data"]["statistics"];

            foreach (JObject jsonStatistic in jsonStatistics)
            {
                string statisticId = (string)jsonStatistic["statisticId"];

                var response = api.PeekSubscriptionStats(operationId, statisticId);

                log.Debug(response.ToJson());
            }
        }

        public void DeleteSubscription()
        {
            if (jsonSubscription == null)
                return;
            
            string operationId = (string)jsonSubscription["operationId"];

            ApiResponse response = api.DeleteSubscription(operationId);

            log.Debug(response.ToJson());

            jsonSubscription = null;
        }

        public void Destroy()
        {
            try {
                if (this.statisticsInitialized)
                {
                    notifications.Disconnect();
                }
            } catch (Exception e) {
                throw new StatisticsConsoleException("destroy failed.", e);
            } finally {
                this.statisticsInitialized = false;
            }
        }
    }
}
