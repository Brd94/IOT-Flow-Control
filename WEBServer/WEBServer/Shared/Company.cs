namespace WEBServer.Shared
{
    public class Company
    {


        public int IdLocation { get; set; } = -1;
        public string BusinessName { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public int PeopleCount { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Opening { get; set; }
        public int Closing { get; set; }


    }
}