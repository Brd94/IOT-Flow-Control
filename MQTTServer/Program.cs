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
        static ISQLConnector connector;

        static void Main(string[] args)
        {
            //connector = new SQLiteConnector(@"C:\Users\Brd\Desktop\IOT-Flow-Control\Shared\Data.db"); // TODO : Cambiare db -> mysql
            connector = new MySQLConnector("192.168.178.20", "dbIOTFC", "admin", "errata"); //Da portare su file esterno

            var broker = new MQTTBroker();
            broker.StartServer(UID, PWD, Port);

            var client = new MQTTClient();
            client.StartClient(UID, PWD, "MASTER");

            var dBServices = new DBServices(connector);

            var actionprovider = new MQTTActionProvider()
                        .AddEndpointAction("esp/get_anagra", endpointdata =>
                        {
                            var device = dBServices.getDevice(endpointdata.ID);
                            var location = dBServices.getDeviceLocation(device.ID_Device);

                            if (location.HasValue)
                            {
                                var anagra = dBServices.getLocationInfo(location.Value);
                                client.SendMessage("brokr/" + endpointdata.ID + "/anagra", JsonConvert.SerializeObject(anagra));
                            }
                            else
                            {
                                client.SendMessage("brokr/" + endpointdata.ID + "/anagra", JsonConvert.SerializeObject(new { ID_Location = -2 }));
                            }


                        })
                        .AddEndpointAction("esp/put_delta", endpointdata =>
                        {
                            var device = dBServices.getDevice(endpointdata.ID);
                            var location = dBServices.getDeviceLocation(device.ID_Device); //Controllo se ha una location valida

                            if (location.HasValue)
                            {
                                dynamic doc = JsonConvert.DeserializeObject(endpointdata.Payload);

                                var value = (int)doc.not_synced_delta;

                                dBServices.logDeviceDelta(device.ID_Device, value);

                                client.SendMessage("brokr/" + endpointdata.ID + "/reset_delta", "");


                            }

                        })
                        .AddEndpointAction("esp/get_pcount", endpointdata =>
                        {
                            var device = dBServices.getDevice(endpointdata.ID);
                            var location = dBServices.getDeviceLocation(device.ID_Device);

                            if (location.HasValue)
                            {
                                var anagra = dBServices.getLocationInfo(location.Value);

                                if (anagra != null)
                                {
                                    //client.SendMessage("brokr/" + endpointdata.ID + "/pcount", JsonConvert.SerializeObject(new { People_Count = anagra.People_Count })); //Informo il sender con la nuova conta 

                                    //! Da aggiungere la logica per chiamare tutti i dispositivi associati a quella location !

                                }

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
