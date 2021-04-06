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

        public IEnumerable<Company> GetCompanies(CompanyFilter filter)
        {
            //FormattableString SQL = $"SELECT * FROM UserLocations,LocationInfo WHERE UserLocations.ID_Location = LocationInfo.ID_Location AND ID_User = {ID_User}";
            // FormattableString SQL = $@"SELECT * FROM LocationInfo 
            // WHERE ID_Location %{filter.filterLocations}% 
            // AND Latitude>={filter.filterLatitudeStart} 
            // AND Latitude<={filter.filterLatitudeEnd} 
            // AND Longitude>={filter.filterLongitudeStart} 
            // AND Longitude<={filter.filterLongitudeEnd}";


            FormattableString SQL = $@"SELECT * FROM LocationInfo WHERE STATUS <> 0";


            DataSet data = db.Query(SQL);

            foreach (DataRow dr in data.Tables[0].Rows)
            {
                yield return new Company()
                {
                    IdLocation = dr.Field<int>("ID_Location"),
                    BusinessName = dr.Field<string>("Business_Name"),
                    Address = dr.Field<string>("Address"),
                    PostalCode = dr.Field<string>("PostalCode"),
                    City = dr.Field<string>("City"),
                    Latitude = dr.Field<double>("Latitude"),
                    Longitude = dr.Field<double>("Longitude"),
                    Opening = dr.Field<int>("Opening"),
                    Closing = dr.Field<int>("Closing")
                };
            }
        }

        public Company GetCompany(int ID)
        {
            FormattableString SQL = $"SELECT * FROM LocationInfo WHERE ID_Location = {ID} AND Status <> 0";

            DataSet data = db.Query(SQL);

            if (data.Tables[0].Rows.Count == 0)
                return null;

            DataRow dr = data.Tables[0].Rows[0];

            return new Company()
            {
                IdLocation = dr.Field<int>("ID_Location"),
                BusinessName = dr.Field<string>("Business_Name"),
                Address = dr.Field<string>("Address"),
                PostalCode = dr.Field<string>("PostalCode"),
                City = dr.Field<string>("City"),
                Latitude = dr.Field<double>("Latitude"),
                Longitude = dr.Field<double>("Longitude"),
                Opening = dr.Field<int>("Opening"),
                Closing = dr.Field<int>("Closing")
            };

        }

        public int RegisterCompany(Company company)
        {
            //Logica di validazione
            FormattableString SQL = $@"INSERT INTO LocationInfo (Business_Name,Address,PostalCode,City,Latitude,Longitude,Opening,Closing) VALUES 
                                    ({company.BusinessName},{company.Address},{company.PostalCode},{company.City},{company.Latitude},{company.Longitude},{company.Opening},{company.Closing});SELECT LAST_INSERT_ID();";

            DataSet data = db.Query(SQL);

            if (data.Tables[0].Rows.Count == 0)
                throw new DataException("Not inserted");

            return (int)data.Tables[0].Rows[0].Field<ulong>(0);

        }

        public void UpdateCompany(Company company)
        {
            FormattableString SQL = $@"UPDATE LocationInfo SET Business_Name={company.BusinessName},Address={company.Address},PostalCode={company.PostalCode},
            City={company.City},Latitude={company.Latitude},Longitude={company.Longitude},Opening={company.Opening},Closing={company.Closing} WHERE ID_Location={company.IdLocation}";

            db.Query(SQL);
        }

        public int BindCompanyToUser(int ID_Company, string ID_User)
        {

            FormattableString SQL = $@"INSERT INTO UserLocations (ID_Location,ID_User) VALUES ({ID_Company},{ID_User});SELECT LAST_INSERT_ID();";

            DataSet data = db.Query(SQL);

            if (data.Tables[0].Rows.Count == 0)
                throw new DataException("Not inserted");

            return (int)data.Tables[0].Rows[0].Field<ulong>(0);
        }
    }
}