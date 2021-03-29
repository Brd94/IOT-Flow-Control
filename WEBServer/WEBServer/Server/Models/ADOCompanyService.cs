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
                yield return new Company() {
                    BusinessName = dr.Field<string>("Business_Name"),
                    Address = dr.Field<string>("Address"),
                    PostalCode = dr.Field<string>("PostalCode"),
                    City = dr.Field<string>("City"),
                    Opening = dr.Field<int>("Opening"),
                    Closing = dr.Field<int>("Closing")
                };
            }
        }

        public Company GetCompany(int ID)
        {
            FormattableString SQL = $"SELECT * FROM LocationInfo WHERE ID_Location = {ID}";

            DataSet data = db.Query(SQL);

            if (data.Tables[0].Rows.Count == 0)
                return null;

            DataRow dr = data.Tables[0].Rows[0];

            return new Company()
            {
                BusinessName = dr.Field<string>("Business_Name"),
                Address = dr.Field<string>("Address"),
                PostalCode = dr.Field<string>("PostalCode"),
                City = dr.Field<string>("City"),
                Opening = dr.Field<int>("Opening"),
                Closing = dr.Field<int>("Closing")
            };

        }

        public void RegisterCompany(Company company)
        {
            //Logica di validazione
            FormattableString SQL = $@"INSERT INTO LocationInfo (Business_Name,Address,PostalCode,City,Opening,Closing) VALUES 
                                    ({company.BusinessName},{company.Address},{company.PostalCode},{company.City},{company.Opening},{company.Closing})";

            db.Query(SQL);
               
        }
    }
}