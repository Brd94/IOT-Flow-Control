using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Shared;
using WEBServer.Server.Services.Infrastructure;
using WEBServer.Shared;

namespace WEBServer.Server.Models
{
    public class EFCompanyService : ICompanyService
    {
        private readonly dbIOTFCContext db;

        public EFCompanyService(dbIOTFCContext db)
        {
            this.db = db;
        }

        public int BindCompanyToUser(int ID_Company, string ID_User)
        {
            throw new System.NotImplementedException();
        }


        public IEnumerable<Company> GetCompanies(CompanyFilter filter)
        {
            throw new System.NotImplementedException();
        }

        public Company GetCompany(int ID)
        {
            return db.LocationInfos
            .Where(x => x.IdLocation == ID)
            .Select(x => new Company
            {
                IdLocation = x.IdLocation,
                BusinessName = x.BusinessName,
                Address = x.Address,
                PostalCode = x.PostalCode,
                City = x.City,
                Opening = x.Opening,
                Closing = x.Closing

            }).FirstOrDefault();
        }

        public int RegisterCompany(Company company)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateCompany(Company company)
        {
            throw new System.NotImplementedException();
        }
    }
}