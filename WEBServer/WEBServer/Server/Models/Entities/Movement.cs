using System;
using System.Collections.Generic;

#nullable disable

namespace WEBServer.Server.Models.Entities
{
    public partial class Movement
    {
        public int IdMove { get; set; }
        public int IdDeviceFk { get; set; }
        public int IdLocationFk { get; set; }
        public byte Type { get; set; }

        public virtual DeviceInfo IdDeviceFkNavigation { get; set; }
        public virtual LocationInfo IdLocationFkNavigation { get; set; }
    }
}
