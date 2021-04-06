using System;
using System.Collections.Generic;

#nullable disable

namespace WEBServer.Server.Models.Entities
{
    public partial class Probe
    {
        public int IdProbes { get; set; }
        public int IdDeviceFk { get; set; }
        public short Delta { get; set; }
        public DateTime Date { get; set; }

        public virtual DeviceInfo IdDeviceFkNavigation { get; set; }
    }
}
