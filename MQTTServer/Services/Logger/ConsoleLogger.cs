using System;

namespace MQTTServer.Services.Logger
{
    public class ConsoleLogger : ILogger
    {
        public ConsoleLogger()
        {

        }

        public int LogLevel { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Log(string sender, int level, string toLog)
        {
            if (level >= LogLevel)
                Console.WriteLine("{0} - {1} : {2}", DateTime.Now, sender, toLog);
        }

    }
}