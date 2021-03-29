using System.Collections.Generic;
using System.Threading.Tasks;
using WEBServer.Shared;

namespace WEBServer.Client.Services
{
    public interface ICompanyService
    {
        public Task<IEnumerable<Company>> GetCompaniesAsync();
        public Task<Company> GetCompanyAsync(int ID);
    }
}