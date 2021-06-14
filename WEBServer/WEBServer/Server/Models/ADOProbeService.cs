using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WEBServer.Server.Services.Infrastructure;
using WEBServer.Shared.Models;

namespace WEBServer.Server.Models
{
    public class ADOProbeService : IProbeService
    {

        private readonly IDatabaseAccessor db;

        public ADOProbeService(IDatabaseAccessor db)
        {
            this.db = db;
        }

        public IEnumerable<DeviceProbe> GetProbes(int idDevice, DateTime startDate, DateTime endDate)
        {
            FormattableString SQL = $"SELECT * FROM Probes WHERE ID_Device_FK={idDevice} AND Date>={startDate} AND Date<={endDate}";

            DataSet dt = db.Query(SQL);

            foreach(DataRow dr in dt.Tables[0].Rows)
            {
                yield return new DeviceProbe()
                {
                    ID_Device = dr.Field<int>("ID_Device_FK"),
                    Delta = dr.Field<short>("Delta"),
                    Date = dr.Field<DateTime>("Date")
                };
            }
        }
    }
}
