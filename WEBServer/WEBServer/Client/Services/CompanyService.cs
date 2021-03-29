using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shared;
using WEBServer.Shared;

namespace WEBServer.Client.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly HttpClient httpClient;

        public CompanyService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public async Task<IEnumerable<Company>> GetCompaniesAsync()
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<Company>>("api/company/getcompanies");
        }

        public async Task<Company> GetCompanyAsync(int ID)
        {
            return await httpClient.GetFromJsonAsync<Company>($"api/company/getcompanies/{ID}");
        }
    }
}