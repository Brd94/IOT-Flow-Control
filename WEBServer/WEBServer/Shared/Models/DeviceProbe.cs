using System;

namespace WEBServer.Shared.Models
{
    public class DeviceProbe
    {
        public int ID_Device { get; set; }
        public int Delta { get; set; }
        public DateTime Date { get; set; }
    }
}