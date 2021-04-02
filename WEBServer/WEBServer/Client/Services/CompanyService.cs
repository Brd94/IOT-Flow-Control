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
        public async Task<IEnumerable<Company>> GetCompaniesAsync(CompanyFilter filter)
        {
            var response = await httpClient.PostAsJsonAsync<CompanyFilter>("api/company/getcompanies",filter);
            System.Console.WriteLine("RES : " + await response.Content.ReadAsStringAsync());
            return await response.Content.ReadFromJsonAsync<IEnumerable<Company>>();
        }

        public async Task<Company> GetCompanyAsync(int ID)
        {
            return await httpClient.GetFromJsonAsync<Company>($"api/company/getcompany?id={ID}");
        }

        public async Task RegisterCompany(Company company)
        {
            var result = await httpClient.PostAsJsonAsync($"api/company/registercompany", company);
            result.EnsureSuccessStatusCode();
        }
    }
}