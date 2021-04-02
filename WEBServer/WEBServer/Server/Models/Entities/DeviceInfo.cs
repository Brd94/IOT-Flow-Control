using System;
using System.Collections.Generic;

#nullable disable

namespace WEBServer.Server.Models.Entities
{
    public partial class DeviceInfo
    {
        public DeviceInfo()
        {
            Movements = new HashSet<Movement>();
            Probes = new HashSet<Probe>();
        }

        public int IdDevice { get; set; }
        public string MacAddress { get; set; }

        public string OTP_Key {get;set;}

        public virtual ICollection<Movement> Movements { get; set; }
        public virtual ICollection<Probe> Probes { get; set; }
    }
}
