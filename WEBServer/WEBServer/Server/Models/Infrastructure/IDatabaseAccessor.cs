using System;
using System.Data;

namespace WEBServer.Server.Models.Infrastructure
{
   
        public interface IDatabaseAccessor
        {
            DataSet Query(FormattableString SQL);
        }
    
}
