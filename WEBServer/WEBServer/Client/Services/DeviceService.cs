using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WEBServer.Shared.Models;

namespace WEBServer.Client.Services
{
    public class DeviceService : IDevicesService
    {
        private readonly HttpClient httpClient;

        public DeviceService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<Device>> GetAllBindedToCompany(int idCompany)
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<Device>>($"api/devices/GetBindedLocation?location={idCompany}");

        }

        public async Task SetBinding(DeviceRegisterRequest registerRequest)
        {
            var result = await httpClient.PostAsJsonAsync($"api/devices/setbinding",registerRequest);
            if (result.StatusCode == System.Net.HttpStatusCode.BadRequest) throw new Exception(await result.Content.ReadAsStringAsync());
            result.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<DeviceProbe>> GetDeviceProbes(int idDevice,DateTime startDate,DateTime endDate)
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<DeviceProbe>>($"api/devices/GetBindedLocation?device={idDevice},startDate={startDate}&endDate={endDate}");

        }

    
    }
}
