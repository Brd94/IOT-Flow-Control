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
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService deviceService;
        private readonly IMovementsService movementsService;

        public DevicesController(IDeviceService deviceService, IMovementsService movementsService)
        {
            this.deviceService = deviceService;
            this.movementsService = movementsService;
        }

        [Authorize]
        [HttpPost]
        public IActionResult RegisterDevice(DeviceRegisterRequest request)
        {
            var device = deviceService.GetDevice(request.OTP_Key);
            
            if (device != null)
            {
                movementsService.CreateMovent(device.ID_Device, request.CurrentLocation);
                return Ok();
            }
            return Unauthorized();
        }

        //[Authorize]
        [HttpGet]
        public IEnumerable<Device> GetBindedLocation(string location)
        {
            return movementsService.GetCurrentBind(int.Parse(location));
        }

    }
}
