using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace MQTTServer.Services
{
    public class SignalRClient 
    {
        private HubConnection connection;

        public bool RaiseException = false;

        public SignalRClient(string IP, int port, string hubName)
        {
            string connectionString = string.Format("http://{0}:{1}/{2}", IP, port, hubName);

            connection = new HubConnectionBuilder().WithUrl(connectionString).Build();

            //myHub = connection.CreateHubProxy(hubName);
        }


        public async Task Connect()
        {

            if (connection.State != HubConnectionState.Connected)
            {
                await connection.StartAsync();
            }

            connection.On<object>("UpdateReceived", x => Console.WriteLine(x.ToString()));


        }

        public async Task SendMessage(string UpdateType, object message)
        {
            try
            {
                await Connect();

                await connection.InvokeAsync(UpdateType, message);
            }
            catch (Exception e)
            {
                if (RaiseException)
                    throw e;
            }


        }

       
    }
}