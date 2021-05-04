using System;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace MQTTServer
{


    public delegate void MQTTBrokerEvent(string id);
    public delegate void MQTTBrokerMessage(Models.MQTTBrokerMessage msg);

    public class MQTTBroker
    {
        public event MQTTBrokerEvent OnSubscribe;
        public event MQTTBrokerMessage OnReceive;

        public MQTTBroker()
        {

        }

        public void StartServer(string UID, string PWD, int Port)
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
                    OnSubscribe?.Invoke(c.ClientId);

                });


            var factory = new MqttFactory();
            var server = factory.CreateMqttServer();
            server.UseApplicationMessageReceivedHandler(x => OnReceive?.Invoke(
                new Models.MQTTBrokerMessage()
                {
                    ID = x.ClientId,
                    Payload = x.ApplicationMessage.ConvertPayloadToString(),
                    Topic = x.ApplicationMessage.Topic
                }));
            server.StartAsync(optionsBuilder.Build()).Wait();
            server.ClientSubscribedTopicHandler = new MqttServerClientSubscribedHandlerDelegate(e =>
            {
                Console.WriteLine("{0} {1} è in ascolto sul topic {2}", DateTime.Now, e.ClientId, e.TopicFilter.Topic);
            });

        }





     


    }
}