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

        public IEnumerable<Company> GetCompanies(string ID_User)
        {
            // return db.UserLocations
            // .Where(x=>x.IdUser == ID_User)
            // .Select(x => new Company
            // {
            //     IdLocation = x.IdLocationNavigation.IdLocation,
            //     BusinessName = x.IdLocationNavigation.BusinessName,
            //     Address = x.IdLocationNavigation.Address,
            //     PostalCode = x.IdLocationNavigation.PostalCode,
            //     City = x.IdLocationNavigation.City,
            //     Opening = x.IdLocationNavigation.Opening,
            //     Closing = x.IdLocationNavigation.Closing

            // });
            return null;
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
    }
}