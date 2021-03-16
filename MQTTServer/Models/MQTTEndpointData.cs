using Newtonsoft.Json;

namespace MQTTServer.Models
{
    public class MQTTEndpointData
    {
        public MQTTBrokerMessage Msg;
        public dynamic ReturnValue;

        public string ReturnTopic;

        public string getJsonReturnValue()
        {
            if (ReturnValue != null)
                return JsonConvert.SerializeObject(ReturnValue);
            else return null;
        }
    }
}