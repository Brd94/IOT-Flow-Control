using System.Collections.Generic;
using Shared;
using WEBServer.Shared;

namespace WEBServer.Server.Models
{
    public interface ICompanyService
    {
         
        public IEnumerable<Company> GetCompanies(string ID_User);
        public Company GetCompany(int ID);
        public void RegisterCompany(Company company);
    }
}