using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using WEBServer.Server.Services.Infrastructure;
using WEBServer.Shared.Models;

namespace WEBServer.Server.Models
{
    public class ADOMovementsService : IMovementsService
    {
        private readonly IDatabaseAccessor db;

        public ADOMovementsService(IDatabaseAccessor db)
        {
            this.db = db;
        }
        public void CreateMovent(int idDevice, int idLocation, int type = 0)
        {
            FormattableString SQL = $"INSERT INTO Movements(ID_Device_FK,ID_Location_FK,Type) VALUES({idDevice},{idLocation},{type})";

            db.Query(SQL);
        }

        public IEnumerable<Device> GetCurrentBind(int idCompany)
        {
            FormattableString SQL = $"SELECT m1.* FROM `Movements` m1 WHERE ID_Location_FK={idCompany} AND m1.Movement_Date = (SELECT MAX(m2.Movement_Date) FROM `Movements` m2 WHERE m1.ID_Device_FK = m2.ID_Device_FK) ";

            db.Query(SQL);

            DataSet data = db.Query(SQL);

            foreach(DataRow dr in data.Tables[0].Rows)
            {
                yield return new Device(){
                    ID_Device = dr.Field<int>("ID_Device")
                };
            }
        }

    
    }
}