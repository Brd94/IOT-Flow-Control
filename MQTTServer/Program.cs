using System;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace MQTTServer
{
    class Program
    {



        static SQLiteConnector connector;

        static void Main(string[] args)
        {
            connector = new SQLiteConnector("/Users/brd/Desktop/Work/Uni/Tesi/IOT-Flow-Control/Shared/Data.db");
            connector.Connect().Wait();
            StartServer("Brd", "Errata", 1883);

        }

        public static void StartServer(string UID, string PWD, int Port)
        {
            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithConnectionBacklog(100)
                .WithDefaultEndpointPort(Port)
                .WithConnectionValidator(c =>
                {
                    if (c.Username != UID)
                    {
                        c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        return;
                    }

                    if (c.Password != PWD)
                    {
                        c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        return;
                    }

                    c.ReasonCode = MqttConnectReasonCode.Success;

                    //Dopo aver validato il Client,Registro la connessione

                    var task = connector.ScalarQuery($"SELECT COUNT(*) FROM Data WHERE ID='{c.ClientId}'");
                    task.Wait();
                    var res = task.Result;

                    if((long)res >0){
                        Console.WriteLine("Bentornato {0}!",c.ClientId);
                    }else{
                        connector.Query($"INSERT INTO Data(ID,Last_Seen) Values('{c.ClientId}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}') ").Wait();;
                        Console.WriteLine("Benvenuto {0}!",c.ClientId);
                    }
                });
              

            var factory = new MqttFactory();
            var server = factory.CreateMqttServer();
            server.UseApplicationMessageReceivedHandler(x => OnReceive(x));
            server.StartAsync(optionsBuilder.Build()).Wait();

            Console.WriteLine("Server in ascolto sulla porta {0},premere un INVIO per uscire...", Port);
            Console.ReadLine();

            server.StopAsync().Wait();
        }

        private static async void OnReceive(MqttApplicationMessageReceivedEventArgs x)
        {           
            string topic = x.ApplicationMessage.Topic;
            string content = x.ApplicationMessage.ConvertPayloadToString();
            
            Console.WriteLine("{0} Messaggio sul topic {1} da {2} : '{3}' ",DateTime.Now,topic, x.ClientId, content);

            string subTopic = topic.Substring(4);

            switch(subTopic){
                case "anagra_pcount":
                    string SQL = $"UPDATE Data SET P_Count = {content},Last_Seen = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE ID = '{x.ClientId}'";
                  await connector.Query(SQL);
                break;
            }
            
            //informo ascoltatori di un avvenuto aggiornamento
        }

        // public static void StartClient(string UID, string PWD, string Topic)
        // {
        //     var optionsBuilder = new MqttClientOptionsBuilder()
        //     .WithCredentials(UID, PWD)
        //     .WithClientId(LISTENER_PREFIX + "_" + Topic);

        //     var factory = new MqttFactory();
        //     var client = factory.CreateMqttClient();

        //     client.SubscribeAsync(optionsBuilder.Build(), new System.Threading.CancellationToken());
        // }

    }


}
