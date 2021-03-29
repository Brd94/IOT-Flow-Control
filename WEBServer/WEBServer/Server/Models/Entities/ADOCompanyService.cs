using System;
using System.Collections.Generic;
using System.Data;
using Shared;
using WEBServer.Server.Models;
using WEBServer.Server.Services.Infrastructure;
using WEBServer.Shared;

namespace WEBServer.Server.Models
{
    public class ADOCompanyService : ICompanyService
    {
        private readonly IDatabaseAccessor db;

        public ADOCompanyService(IDatabaseAccessor db)
        {
            this.db = db;
        }

        public IEnumerable<Company> GetCompanies(string ID_User)
        {
            FormattableString SQL = $"SELECT * FROM UserLocations,LocationInfo WHERE UserLocations.ID_Location = LocationInfo.ID_Location AND ID_User = {ID_User}";
            DataSet data = db.Query(SQL);

            foreach (DataRow dr in data.Tables[0].Rows)
            {
                yield return new Company(){
                BusinessName = dr.Field<string>("Business_Name"),
                // Address = x.Address,
                // PostalCode = x.PostalCode,
                // City = x.City,
                // Opening = x.Opening,
                // Closing = x.Closing
                };
            }
        }

        public Company GetCompany(int ID)
        {
            return new Company(){BusinessName = "TEST"};
        }
    }
}