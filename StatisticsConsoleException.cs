using System;

namespace consolestatisticsappcsharp
{
    public class StatisticsConsoleException : Exception
    {
        public StatisticsConsoleException(String message) : base(message)
        {
        }

        public StatisticsConsoleException(String message, Exception cause): base(message, cause)
        {
        }
    }
}
