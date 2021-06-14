using System;
using WEBServer.Shared;

namespace WEBServer.Server.Services
{
    public interface IProbeCalculator
    {
        Company CalculatePeopleCount(Company company, DateTime? startdate = null, DateTime? enddate = null);
    }
}
