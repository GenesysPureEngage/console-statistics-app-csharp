using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesys.Internal.Authentication.Api;
using Genesys.Internal.Authentication.Client;
using Genesys.Internal.Authentication.Model;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace consolestatisticsappcsharp
{
    public class Command
    {
        public String Name { get; set; }
        public List<String> Args;

        public Command(String name, List<String> args)
        {
            this.Name = name;
            this.Args = args;
        }
    }

    public class CompleteParams
    {
        public String ConnId { get; set; }
        public String ParentConnId { get; set; }

        public CompleteParams(String connId, String parentConnId)
        {
            this.ConnId = connId;
            this.ParentConnId = parentConnId;
        }
    }

    public class StatisticsConsole
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Options options;
        private StatisticsClientApi api;

        public StatisticsConsole(Options options)
        {
            this.options = options;
            api = new StatisticsClientApi(this.options.ApiKey, this.options.BaseUrl);
        }

        private void Write(String msg)
        {
            Console.WriteLine(msg);
        }

        private void Prompt()
        {
            Console.Write("cmd> ");
        }

        private void Prompt(String msg)
        {
            Console.Write(msg);
        }

        private Command ParseInput(String input)
        {
            String[] pieces = input.Split(' ');
            if (pieces.Length == 0)
            {
                return null;
            }

            String name = pieces[0].ToLower();
            List<String> args = new List<String>();
            if (pieces.Length > 1)
            {
                for (int i = 1; i < pieces.Length; i++)
                {
                    args.Add(pieces[i]);
                }
            }

            return new Command(name, args);
        }

        private void PrintHelp()
        {
            this.Write("Statistics Api Console commands:");
            this.Write("initialize|init|i");
            this.Write("logout|l");
            this.Write("subscribe|s");
            this.Write("peek|p");
            this.Write("delete|d");
            this.Write("config|conf");
            this.Write("exit|x");
            this.Write("debug|d");
            this.Write("help|?");
            this.Write("");
            //this.Write("Note: <id> parameter can be omitted for call operations if there is only one active call.");
            this.Write("");
        }

        private String GetAuthToken()
        {
            log.Debug("Getting auth token...");
            String baseUrl = this.options.AuthBaseUrl ?? this.options.BaseUrl;
            ApiClient authClient = new ApiClient(baseUrl + "/auth/v3");
            ((Configuration)authClient.Configuration).BasePath = baseUrl + "/auth/v3";
            authClient.Configuration.ApiKey.Add("x-api-key", this.options.ApiKey);
            authClient.Configuration.DefaultHeader.Add("x-api-key", this.options.ApiKey);
            authClient.RestClient.AddDefaultHeader("x-api-key", this.options.ApiKey);
            
            String encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(this.options.ClientId + ":" + this.options.ClientSecret));
            String authorization = "Basic " + encoded;

            AuthenticationApi authApi = new AuthenticationApi((Genesys.Internal.Authentication.Client.Configuration)authClient.Configuration);

            DefaultOAuth2AccessToken response = authApi.RetrieveToken(
                    "password", authorization, null, "*",
                    this.options.ClientId, null, this.options.Username, this.options.Password);

            return response.AccessToken;
        }

        private void Init()
        {
            try 
            {
                String token = this.GetAuthToken();
                if (token == null) {
                    throw new StatisticsConsoleException("Failed to get auth token.");
                }

                this.Write("Initializing API...");
                api.Initialize(token);
                this.Write("Initialization complete.");
            }
            catch (Exception e)
            {
                log.Error("Failed to initialize", e);
            }
        }

        private void DoAutoLogin()
        {
            try
            {
                if (this.options.AutoLogin)
                {
                    this.Write("autoLogin is true...");
                    this.Init();
                }
            }
            catch (Exception e)
            {
                this.Write("autoLogin failed!" + e);
            }
        }


        public async Task Run()
        {
            try
            {
                this.Write("Statistics Api Console");
                this.Write("");
                this.DoAutoLogin();

                for (;;)
                {
                    try
                    {
                        this.Prompt();
                        Command cmd = this.ParseInput(Console.ReadLine());
                        if (cmd == null)
                        {
                            continue;
                        }

                        List<string> args = cmd.Args;
                        string id;
                        string destination;
                        string key;
                        string value;
                        CompleteParams completeParams;

                        switch (cmd.Name)
                        {
                            case "initialize":
                            case "init":
                            case "i":
                                this.Init();
                                break;

                            case "logout":
                            case "l":
                                this.Write("Cleaning up and logging out...");
                                api.Destroy();
                                break;

                            case "subscribe":
                            case"s":
                                api.Subscribe();
                                break;

                            case "peek":
                            case "p":
                                api.PeekStatistic();
                                break;

                            case "delete":
                            case "d":
                                api.DeleteSubscription();
                                break;

                            case "config":
                            case "conf":
                                this.Write("Configuration:\n"
                                    + "apiKey: " + this.options.ApiKey + "\n"
                                    + "baseUrl: " + this.options.BaseUrl + "\n"
                                    + "clientId: " + this.options.ClientId + "\n"
                                    + "username: " + this.options.Username + "\n"
                                    + "password: " + this.options.Password + "\n"
                                    + "autoLogin: " + this.options.AutoLogin + "\n"
                                    );
                                break;

                            case "exit":
                            case "x":
                                this.Write("Cleaning up and exiting...");
                                return;

                            case "?":
                            case "help":
                                this.PrintHelp();
                                break;

                            default:
                                break;

                        }
                    }
                    catch(Exception e)
                    {
                        Write("Exception!" + e.Message);
                    }
                }
            } 
            catch(Exception e) 
            {
                Write("Exception!" + e.Message);
            }
        }
    }
}
