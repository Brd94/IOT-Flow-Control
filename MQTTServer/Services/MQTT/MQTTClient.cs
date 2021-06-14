using System;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Client.Subscribing;

namespace MQTTServer
{
    public delegate void OnMsgReceived(MqttApplicationMessageReceivedEventArgs args);
    public class MQTTClient
    {
        public event OnMsgReceived OnMsgReceived;
        private IMqttClient client;
        public MQTTClient()
        {
        }
        public string ClientName { get; set; }

        public void StartClient(string UID, string PWD, string ClientName)
        {
            this.ClientName = ClientName;

            var optionsBuilder = new MqttClientOptionsBuilder()
            .WithCredentials(UID, PWD)
            .WithTcpServer("localhost", Program.Port)
            .WithClientId(ClientName);


            var factory = new MqttFactory();
            client = factory.CreateMqttClient();


            // var objSubOptions = new MqttClientSubscribeOptions();
            // var topicFilter =new MqttTopicFilter();
            // topicFilter.Topic =  "brokr/rcv/";
            // objSubOptions.TopicFilters.Add(topicFilter);


            // client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(e =>
            // {
            //     if (e.ClientId != "MASTER")
            //         OnMsgReceived?.Invoke(e);
            // });
            //client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(e =>OnReceive(e));
            client.ConnectAsync(optionsBuilder.Build()).Wait();
            //client.SubscribeAsync(new TopicFilterBuilder().WithTopic("esp/jsondata").Build()).Wait();
            Console.WriteLine("MASTER CLIENT STARTED");
        }

        private void OnReceive(MqttApplicationMessageReceivedEventArgs msg)
        {
            Console.WriteLine("OnRCV MASTER {0} : {1}", msg.ClientId, msg.ApplicationMessage.ConvertPayloadToString());
        }

        public void SendMessage(string Topic, string Message)
        {
            if (client?.IsConnected ?? false)
            {
                client.PublishAsync(Topic, Message);
                //Console.WriteLine("MASTER dice {1} sul Topic : {0} ", Topic, Message);
            }
        }
    }
}