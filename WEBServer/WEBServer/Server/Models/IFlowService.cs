using System;
using System.Collections.Generic;
using WEBServer.Shared;

namespace WEBServer.Server.Models
{
    public interface IFlowService
    {
        IEnumerable<FlowData> getAllData();
    }
}
