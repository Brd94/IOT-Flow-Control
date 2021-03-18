using System;
using System.Collections.Generic;
using System.Data;
using WEBServer.Server.Models.Infrastructure;
using WEBServer.Shared;

namespace WEBServer.Server.Models
{
    public class ADOFlowService : IFlowService
    {
        private readonly IDatabaseAccessor db;

        public ADOFlowService(IDatabaseAccessor db)
        {
            this.db = db;
        }

        public IEnumerable<FlowData> getFlowData()
        {
            FormattableString SQL = $"SELECT * FROM LocationInfo";
            DataSet data = db.Query(SQL);

            foreach(DataRow dr in data.Tables[0].Rows)
            {
                yield return new FlowData()
                {
                    Name = dr["Business_Name"].ToString(),
                    PeopleNumber = dr.Field<long?>("People_Count"),
                    //LastUpdate = DateTime.Parse(dr.Field<string>("Last_Seen"))
                    Info = "Aperto fino alle " + dr.Field<DateTime>("Closing").ToString("HH:mm")
                };
            }
        }
    }
}
