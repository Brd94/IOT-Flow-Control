using System;
using System.Globalization;
using WEBServer.Server.Models;
using WEBServer.Shared;

namespace WEBServer.Server.Services
{
    /// <summary>
    /// Calcola gli spostamenti sulla base dell'orario di apertura o di chiusura
    /// </summary>
    public class ProbeCalculator : IProbeCalculator
    {
        private readonly IDeviceService deviceService;
        private readonly IMovementsService movementsService;
        private readonly IProbeService probeService;

        public ProbeCalculator(IMovementsService movementsService, IProbeService probeService)
        {
            this.movementsService = movementsService;
            this.probeService = probeService;
        }

        public Company CalculatePeopleCount(Company company, DateTime? startdate = null,DateTime? enddate = null)
        {

            startdate = startdate ?? DateTime.Today;
            enddate = enddate ?? DateTime.Today;

            DateTime opening = DateTime.ParseExact(startdate.Value.ToString("yyyyMMdd") + company.Opening.ToString().PadLeft(4, '0'), "yyyyMMddHHmm", CultureInfo.CurrentCulture);
            DateTime closing = DateTime.ParseExact(enddate.Value.ToString("yyyyMMdd") + company.Closing.ToString().PadLeft(4, '0'), "yyyyMMddHHmm", CultureInfo.CurrentCulture);

            if (opening > closing)
                closing.AddDays(1);


            var devices = movementsService.GetCurrentBind(company.IdLocation);

            

            foreach(var device in devices)
            {
                var probes = probeService.GetProbes(device.ID_Device, opening, closing);

                foreach(var probe in probes)
                {
                    company.PeopleCount += probe.Delta;
                }

            }


            if (company.PeopleCount < 0)
                company.PeopleCount = -1; //BISOGNA METTERE IN UN FILE DI LOG L'ERRORE


            return company;
            
        }
    }
}
