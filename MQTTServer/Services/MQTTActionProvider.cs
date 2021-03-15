using System;
using System.Collections.Generic;
using System.Text.Json;
using MQTTServer.Models;

namespace MQTTServer
{
  
    public delegate void MQTTEndpointAction(MQTTEndpointData data);

    public class MQTTActionProvider
    {
        private Dictionary<string, MQTTEndpointAction> endpoints_actions = new Dictionary<string, MQTTEndpointAction>();

        public MQTTActionProvider AddEndpointAction(string endpoint, MQTTEndpointAction action)
        {
            endpoints_actions.Add(endpoint, action);
            return this;
        }
        private readonly DBServices dbService;

        public string Run(Models.MQTTBrokerMessage msg)
        {
            var data = new MQTTEndpointData() { Msg = msg };

            if (endpoints_actions.ContainsKey(msg.Topic)) //Se l'endpoint ha un'azione configurata  
                endpoints_actions[msg.Topic].Invoke(data);

            if (data.ReturnValue != null)
                return JsonSerializer.Serialize(data.ReturnValue);
            else return null;
        }
    }



}