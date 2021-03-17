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

        internal LocationInfo getLocationInfo(long ID_Location)
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
                ID_Location = dr.Field<long>("ID_Location"),
                Business_Name = dr.Field<string>("Business_Name"),
                People_Count = dr.Field<long>("People_Count"),
                Address = dr.Field<string>("Address"),
                PostalCode = dr.Field<string>("PostalCode"),
                City = dr.Field<string>("City")
            };
        }

        public async void RegisterDevice(string ID)
        {
            //FormattedStringBuilder
            var res = await db.ScalarQueryAsync($"SELECT COUNT(*) FROM RegisteredDevices WHERE ID={ID}"); //Posso fare anche un UPSERT

            if ((long)res > 0)
            {
                //Console.WriteLine("Bentornato {0}!", ID);
            }
            else
            {
                await db.QueryAsync($"INSERT INTO RegisteredDevices(ID) Values({ID}) ");
                //await db.QueryAsync($"INSERT INTO RegisteredDevices(ID,Last_Seen) Values({ID},{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}) ");
                //Console.WriteLine("Benvenuto {0}!", ID);
            }
        }

        public RegisteredDevice getDevice(string ID)
        {
            FormattableString SQL = $"SELECT * FROM RegisteredDevices";
            DataSet data = db.Query(SQL);

            if (data.Tables[0].Rows.Count == 0)
                return null;

            DataRow dr = data.Tables[0].Rows[0];

            return new RegisteredDevice()
            {
                ID = dr["ID"].ToString(),
                //Last_Seen = DateTime.Parse(dr[("Last_Seen")].ToString()),
                Registered_Location = dr.Field<long?>("Registered_Location")
            };
        }

        internal void logDeviceDelta(string ID_Device, int value)
        {
            FormattableString SQL = $"INSERT INTO DeviceLogs VALUES({ID_Device},{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},{value})";
            db.NonQueryAsync(SQL);
        }

        internal void increaseDelta(long ID_Location, int delta)
        {
            FormattableString SQL =$"UPDATE LocationInfo SET People_Count = (People_Count + {delta}) WHERE ID_Location = {ID_Location}";
            db.Query(SQL);
        }
    }
}