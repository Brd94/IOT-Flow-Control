using WEBServer.Server.Models.Entities;
using WEBServer.Shared.Models;

namespace WEBServer.Server.Models
{
    public interface IDeviceService
    {
         public Device GetDevice(string otp);
         
    }
}