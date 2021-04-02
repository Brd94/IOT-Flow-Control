using System.Collections.Generic;
using System.Threading.Tasks;
using WEBServer.Shared;

namespace WEBServer.Client.Services
{
    public interface ICompanyService
    {
        public Task<IEnumerable<Company>> GetCompaniesAsync(CompanyFilter filter);
        public Task<Company> GetCompanyAsync(int ID);
        public Task RegisterCompany(Company company);
    }
}