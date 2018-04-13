using System;
using System.IO;
using System.Threading.Tasks;
using log4net.Config;

namespace consolestatisticsappcsharp
{
    public class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            try
            {
                FileInfo fileInfo = new FileInfo("log4net.config");
                XmlConfigurator.Configure(fileInfo);

                Options options = Options.parseOptions(args);
                if (options == null)
                {
                    return;
                }

                StatisticsConsole console = new StatisticsConsole(options);

                // Run() is an async method so we have to call Wait()
                // have this thread wait for it to finish executing.
                console.Run().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error!:\n" + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
