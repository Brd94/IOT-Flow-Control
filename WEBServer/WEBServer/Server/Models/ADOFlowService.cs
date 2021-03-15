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
            string SQL = "SELECT * FROM Data";
            DataSet data = db.Query(SQL);

            foreach(DataRow dr in data.Tables[0].Rows)
            {
                yield return new FlowData()
                {
                    Name = dr["Business_Name"].ToString(),
                    PeopleNumber = dr.Field<long?>("P_Count"),
                    LastUpdate = DateTime.Parse(dr.Field<string>("Last_Seen"))
                };
            }
        }
    }
}
