using System;
using System.Collections.Generic;

#nullable disable

namespace WEBServer.Server.dbIOTFCdbContext
{
    public partial class EfmigrationsHistory
    {
        public string MigrationId { get; set; }
        public string ProductVersion { get; set; }
    }
}
