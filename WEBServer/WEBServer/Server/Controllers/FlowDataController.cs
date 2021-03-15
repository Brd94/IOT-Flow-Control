using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WEBServer.Server.Models;
using WEBServer.Shared;

namespace WEBServer.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FlowDataController : ControllerBase
    {


        private readonly ILogger<FlowDataController> _logger;
        private readonly IFlowService flowservice;

        public FlowDataController(ILogger<FlowDataController> logger,IFlowService flowservice)
        {
            _logger = logger;
            this.flowservice = flowservice;
        }

        [HttpGet]
        public IEnumerable<FlowData> Get()
        {

            return flowservice.getFlowData();
        }
    }
}
