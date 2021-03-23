using System;
using System.Data;

namespace WEBServer.Server.Services.Infrastructure
{
   
        public interface IDatabaseAccessor
        {
            DataSet Query(FormattableString SQL);
        }
    
}
