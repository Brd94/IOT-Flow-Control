using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBServer.Server.Models;
using WEBServer.Shared.Models;

namespace WEBServer.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService deviceService;
        private readonly IMovementsService movementsService;
        private readonly IProbeService probeService;

        public DevicesController(IDeviceService deviceService, IMovementsService movementsService,IProbeService probeService)
        {
            this.deviceService = deviceService;
            this.movementsService = movementsService;
            this.probeService = probeService;
        }

        [HttpPost]
        public IActionResult SetBinding(DeviceRegisterRequest request)
        {
            int? device = request.Device;

            if (!request.Device.HasValue)
                device = deviceService.GetDevice(request.OTP_Key)?.ID_Device ?? null;
            
            if (device != null)
            {
                movementsService.CreateMovent(device.Value, request.Location ,(int)request.InstallationType);
                return Ok();
            }
            else
            {
                return BadRequest("OTP Key non valida!");
            }
        }

        [HttpGet]
        public IEnumerable<Device> GetBindedLocation(int location)
        {
            //Controllare,oltre all'autorizzazione,l'abilitazione alla visualizzazione dei dettagli dell'azienda
            return movementsService.GetCurrentBind(location);
        }

        [HttpGet]
        public IEnumerable<DeviceProbe> GetProbes(int idDevice,DateTime startDate,DateTime endDate)
        {
            return probeService.GetProbes(idDevice, startDate, endDate);
        }


    }
}
