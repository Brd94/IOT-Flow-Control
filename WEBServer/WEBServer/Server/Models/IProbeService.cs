using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEBServer.Shared.Models;

namespace WEBServer.Server.Models
{
    public interface IProbeService
    {
        public IEnumerable<DeviceProbe> GetProbes(int idDevice,DateTime startDate,DateTime endDate);
    }
}
