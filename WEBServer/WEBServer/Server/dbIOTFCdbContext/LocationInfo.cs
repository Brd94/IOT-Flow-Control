using System;
using System.Collections.Generic;

#nullable disable

namespace WEBServer.Server.dbIOTFCdbContext
{
    public partial class LocationInfo
    {
        public LocationInfo()
        {
            Movements = new HashSet<Movement>();
            UserLocations = new HashSet<UserLocation>();
        }

        public int IdLocation { get; set; }
        public string BusinessName { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public int PeopleCount { get; set; }
        public int Opening { get; set; }
        public int Closing { get; set; }
        public byte Status { get; set; }

        public virtual ICollection<Movement> Movements { get; set; }
        public virtual ICollection<UserLocation> UserLocations { get; set; }
    }
}
