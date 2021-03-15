namespace MQTTServer.Services.Logger
{
    public interface ILogger
    {
        public int LogLevel {get;set;}
        public void Log(string sender,int logLevel,string toLog);
    }
}