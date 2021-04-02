using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WEBServer.Client.Models;

namespace WEBServer.Client.Services
{
    public class MapquestPlaceConverter : IPlaceConverter
    {

        private readonly HttpClient httpClient;

        public MapquestPlaceConverter(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<PlaceModel> GetPlaceAsync(string address, string postalcode, string city)
        {
            string s = $"https://open.mapquestapi.com/geocoding/v1/address?key=Gug7BBTKXqhpVwlOqpmVzFKSy570r2hG&location={address},{postalcode},{city} ";
            var z = await httpClient.GetFromJsonAsync<System.Text.Json.JsonElement>(s);
            var lat = z.GetProperty("results")[0].GetProperty("locations")[0].GetProperty("latLng").GetProperty("lat").GetDouble();
            var lng = z.GetProperty("results")[0].GetProperty("locations")[0].GetProperty("latLng").GetProperty("lng").GetDouble();
            System.Console.WriteLine("Lat {0} - Lng {1}", lat, lng);

            if(lat == 39.78373 && lng == -100.445882)
                throw new System.Exception("Location non valida");

            return new PlaceModel()
            {
                Latitude = lat,
                Longitude = lng
            };
        }
    }
}