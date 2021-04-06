using System;
using System.Data;
using WEBServer.Server.Services.Infrastructure;
using WEBServer.Shared.Models;

namespace WEBServer.Server.Models
{
    public class ADODeviceService : IDeviceService
    {
        private readonly IDatabaseAccessor db;

        public ADODeviceService(IDatabaseAccessor db)
        {
            this.db = db;
        }
        public Device GetDevice(string otp)
        {
            FormattableString SQL = $"SELECT * FROM DeviceInfo WHERE OTP_Key={otp}";

            DataSet data = db.Query(SQL);

            if (data.Tables[0].Rows.Count == 0)
                return null;

            DataRow dr = data.Tables[0].Rows[0];

            return new Device()
            {
                ID_Device = dr.Field<int>("ID_Device"),
                MacAddress = dr.Field<string>("Mac_Address"),
                OTP_Key = dr.Field<string>("OTP_Key")
            };

        }
    }
}