using Newtonsoft.Json;
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
            connector = new SQLiteConnector(@"C:\Users\Brd\Desktop\IOT-Flow-Control\Shared\Data.db"); // TODO : Cambiare db -> mysql

            var broker = new MQTTBroker();
            broker.StartServer(UID, PWD, Port);

            var client = new MQTTClient();
            client.StartClient(UID, PWD, "MASTER");

            var dBServices = new DBServices(connector);

            var actionprovider = new MQTTActionProvider()
                        .AddEndpointAction("esp/get_anagra", endpointdata =>
                        {
                            var device = dBServices.getDevice(endpointdata.ID);

                            if (device.Registered_Location.HasValue)
                            {
                                var anagra = dBServices.getLocationInfo(device.Registered_Location.Value);
                                client.SendMessage("brokr/" +endpointdata.ID + "/anagra", JsonConvert.SerializeObject(anagra));
                            }
                            else
                            {
                                client.SendMessage("brokr/" +endpointdata.ID + "/anagra", JsonConvert.SerializeObject(new { ID_Location = -2 }));
                            }


                        })
                        .AddEndpointAction("esp/put_delta", endpointdata =>
                        {
                            var device = dBServices.getDevice(endpointdata.ID);

                            if (device.Registered_Location.HasValue)
                            {
                                dynamic doc = JsonConvert.DeserializeObject(endpointdata.Payload);

                                var value = (int)doc.not_synced_delta;
                                dBServices.increaseDelta(device.Registered_Location.Value, value);
                                dBServices.logDeviceDelta(device.ID, value);
                                var anagra = dBServices.getLocationInfo(device.Registered_Location.Value);

                                client.SendMessage("brokr/" + endpointdata.ID + "/reset_delta", "");


                            }

                        })
                        .AddEndpointAction("esp/get_pcount", endpointdata =>
                        {
                            var device = dBServices.getDevice(endpointdata.ID);

                            if (device.Registered_Location.HasValue)
                            {
                                var anagra = dBServices.getLocationInfo(device.Registered_Location.Value);

                                client.SendMessage("brokr/" + device.Registered_Location + "/pcount", JsonConvert.SerializeObject(new { People_Count = anagra.People_Count }));

                            }


                        });

            broker.OnSubscribe += sub => dBServices.RegisterDevice(sub);

            broker.OnReceive += x => actionprovider.Run(x); //Rispondo alla richiesta ricevuta dal broker


            Console.WriteLine("Server in ascolto sulla porta {0},premere un INVIO per uscire...", Port);
            Console.ReadLine();

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


        }





    }


}
