using System;
using System.Collections.Generic;
using System.Text.Json;
using MQTTServer.Models;

namespace MQTTServer
{

    public delegate void MQTTEndpointAction(Models.MQTTBrokerMessage data);

    public class MQTTActionProvider
    {
        private Dictionary<string, MQTTEndpointAction> endpoints_actions = new Dictionary<string, MQTTEndpointAction>();

        public MQTTActionProvider AddEndpointAction(string endpoint, MQTTEndpointAction action)
        {
            endpoints_actions.Add(endpoint, action);
            return this;
        }
        private readonly DBServices dbService;

        public void Run(Models.MQTTBrokerMessage msg)
        {

            if (endpoints_actions.ContainsKey(msg.Topic)) //Se l'endpoint ha un'azione configurata  
                endpoints_actions[msg.Topic].Invoke(msg);

        }
    }



}