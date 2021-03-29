using System;
using System.Collections.Generic;

#nullable disable

namespace WEBServer.Server.dbIOTFCdbContext
{
    public partial class AspNetUser
    {
        public AspNetUser()
        {

            //UserLocations = new HashSet<UserLocation>();
        }

        public string Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public virtual ICollection<UserLocation> UserLocations { get; set; }
    }
}
