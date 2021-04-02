using System.Collections.Generic;
using Shared;
using WEBServer.Shared;

namespace WEBServer.Server.Models
{
    public interface ICompanyService
    {
         
        public IEnumerable<Company> GetCompanies(CompanyFilter filter);
        
        public Company GetCompany(int ID);
        public int RegisterCompany(Company company);

        public void UpdateCompany(Company company);

        public int BindCompanyToUser(int ID_Company,string ID_User);
    }
}