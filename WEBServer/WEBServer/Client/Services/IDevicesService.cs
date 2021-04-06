using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEBServer.Shared.Models;

namespace WEBServer.Client.Services
{
    interface IDevicesService
    {
        Task SetBinding(DeviceRegisterRequest registerRequest);
        Task<IEnumerable<Device>> GetAllBindedToCompany(int idCompany);
        Task<IEnumerable<DeviceProbe>> GetDeviceProbes(int idDevice, DateTime startDate, DateTime endDate);
    }
}
