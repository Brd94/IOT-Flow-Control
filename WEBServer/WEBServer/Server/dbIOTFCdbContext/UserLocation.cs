using System;
using System.Collections.Generic;

#nullable disable

namespace WEBServer.Server.dbIOTFCdbContext
{
    public partial class UserLocation
    {
        public int IdUl { get; set; }
        public int IdLocation { get; set; }
        public string IdUser { get; set; }

        public virtual LocationInfo IdLocationNavigation { get; set; }
        public virtual AspNetUser IdUserNavigation { get; set; }
    }
}
