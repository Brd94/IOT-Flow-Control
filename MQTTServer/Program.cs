using System;
using System.Text.Json;
using System.Threading.Tasks;


namespace MQTTServer
{
    class Program
    {

        public static string UID = "Brd";
        public static string PWD = "Errata";
        public static int Port = 1883;
        static SQLiteConnector connector;

        static void Main(string[] args)
        {
            connector = new SQLiteConnector("/Users/brd/Desktop/Work/Uni/Tesi/IOT-Flow-Control/Shared/Data.db");

            var broker = new MQTTBroker();
            broker.StartServer(UID, PWD, Port);

            var client = new MQTTClient();
            client.StartClient(UID, PWD, "MASTER");

            var dBServices = new DBServices(connector);

            var actionprovider = new MQTTActionProvider()
                        .AddEndpointAction("/esp/get_id_location", endpointdata =>
                        {
                            var device = dBServices.getDevice(endpointdata.Msg.ID);

                            if (device.Registered_Location.HasValue)
                                endpointdata.ReturnValue = new { ID_Location = device.Registered_Location };
                            else
                                endpointdata.ReturnValue = new { ID_Location = -2 };

                        })
                        .AddEndpointAction("/esp/get_anagra", endpointdata =>
                        {
                            var device = dBServices.getDevice(endpointdata.Msg.ID);

                            if (device.Registered_Location.HasValue)
                            {
                                var anagra = dBServices.getLocationInfo(device.Registered_Location.Value);
                                endpointdata.ReturnValue = anagra;
                            }
                        })
                        .AddEndpointAction("/esp/put_delta", endpointdata =>
                        {
                            var device = dBServices.getDevice(endpointdata.Msg.ID);

                            if (device.Registered_Location.HasValue)
                                dBServices.increaseDelta(device.Registered_Location.Value, int.Parse(endpointdata.Msg.Payload));

                        });

            broker.OnSubscribe += sub => dBServices.RegisterDevice(sub);

            broker.OnReceive += x =>
            {

                string response = actionprovider.Run(x);

                if (response != null)
                    client.SendMessage("brokr/" + x.ID + "/jsondata", response);

            };

            //     broker.ClientSubscribed += sub =>
            //    {

            //        var device = dBServices.getDevice(sub.ID);

            //        if (device.Registered_Location.HasValue)
            //        {
            //            var location = dBServices.getLocationInfo(device.Registered_Location.Value);

            //            string serialize = JsonSerializer.Serialize(new
            //            {
            //                ID_Location = device.Registered_Location.Value,
            //                People_Count = location.People_Count,
            //                Business_Name = location.Business_Name,
            //                Address = location.Address,
            //                PostalCode = location.PostalCode,
            //                City = location.City
            //            };
            //        }
            //        else
            //        {
            //            string serialize = JsonSerializer.Serialize(new
            //            {
            //                ID_Location = -2
            //            });
            //        }


            //    };


            Console.WriteLine("Server in ascolto sulla porta {0},premere un INVIO per uscire...", Port);
            Console.ReadLine();

        }





    }


}
