using System.Collections;
using System.Collections.Generic;
using WEBServer.Shared.Models;

namespace WEBServer.Server.Models
{
    public interface IMovementsService
    {
        void CreateMovent(int idDevice, int idLocation,int type = 0);

        IEnumerable<Device> GetCurrentBind(int idCompany);
    }
}