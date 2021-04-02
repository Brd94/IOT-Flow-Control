using System.Threading.Tasks;
using WEBServer.Client.Models;

namespace WEBServer.Client.Services
{
    public interface IPlaceConverter
    {

        public Task<PlaceModel> GetPlaceAsync(string address,string postalcode,string city);
        
    }
}