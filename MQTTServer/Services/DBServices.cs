using System;
using System.Collections.Generic;
using System.Data;
using MQTTServer.Models;

namespace MQTTServer
{
    public class DBServices
    {
        private readonly ISQLConnector db;

        public DBServices(ISQLConnector db)
        {
            this.db = db;
        }

        internal LocationInfo getLocationInfo(int ID_Location)
        {
            FormattableString SQL = $"SELECT * FROM LocationInfo WHERE ID_Location = {ID_Location}"; //Correggere *
            DataSet data = db.Query(SQL);

            if (data.Tables[0].Rows.Count == 0)
            {
                return null;
            }

            DataRow dr = data.Tables[0].Rows[0];

            return new LocationInfo()
            {
                ID_Location = dr.Field<int>("ID_Location"),
                Business_Name = dr.Field<string>("Business_Name"),
                //People_Count = dr.Field<long>("People_Count"),
                Address = dr.Field<string>("Address"),
                PostalCode = dr.Field<string>("PostalCode"),
                City = dr.Field<string>("City")
            };
        }

        public async void RegisterDevice(string ID)
        {
            //FormattedStringBuilder
            var res = await db.ScalarQueryAsync($"SELECT COUNT(*) FROM DeviceInfo WHERE Mac_Address={ID}"); //Posso fare anche un UPSERT

            if ((long)res > 0)
            {
                //Console.WriteLine("Bentornato {0}!", ID);
            }
            else
            {
                await db.QueryAsync($"INSERT INTO DeviceInfo(Mac_Address,OTP_Key) Values({ID},'Test') ");
                //await db.QueryAsync($"INSERT INTO RegisteredDevices(ID,Last_Seen) Values({ID},{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}) ");
                //Console.WriteLine("Benvenuto {0}!", ID);
            }
        }

        public DeviceInfo getDevice(string Mac_Address)
        {
            FormattableString SQL = $"SELECT * FROM DeviceInfo WHERE Mac_Address={Mac_Address}";
            DataSet data = db.Query(SQL);

            if (data.Tables[0].Rows.Count == 0)
                return null;

            DataRow dr = data.Tables[0].Rows[0];

            return new DeviceInfo()
            {
                ID_Device = dr.Field<int>("ID_Device"),
                Mac_Address = dr.Field<string>("Mac_Address")
                //Last_Seen = DateTime.Parse(dr[("Last_Seen")].ToString()),
                //Registered_Location = dr.Field<long?>("Registered_Location")
            };
        }

        public int? getDeviceLocation(int id){
            FormattableString SQL = $"SELECT ID_Location_FK FROM Movements WHERE ID_Device_FK={id}";
            DataSet data = db.Query(SQL);

            if (data.Tables[0].Rows.Count == 0)
                return null;

            return data.Tables[0].Rows[0].Field<int>("ID_Location_FK");


        }

        internal void logDeviceDelta(int ID_Device, int value)
        {
            // FormattableString SQL = $"INSERT INTO DeviceLogs(ID_Device,Pushed_Delta) VALUES({ID_Device},{value})";
            FormattableString SQL = $"INSERT INTO Probes(ID_Device_FK,Delta) VALUES({ID_Device},{value})";
            db.Query(SQL);
        }

        // internal void increaseDelta(int ID_Device, int delta)
        // {
        //     //FormattableString SQL =$"UPDATE LocationInfo SET People_Count = (People_Count + {delta}) WHERE ID_Location = {ID_Location}";
        //     db.Query(SQL);
        // }
    }
}