namespace WEBServer.Shared
{
    public class CompanyFilter
    {
        public string filterUser { get; set; } = "";
        public string filterLocations { get; set; } = "";
        public double? filterLatitudeStart { get; set; } = double.MinValue;
        public double? filterLatitudeEnd { get; set; } = double.MaxValue;
        public double? filterLongitudeStart { get; set; } = double.MinValue;
        public double? filterLongitudeEnd { get; set; } = double.MaxValue;

    }
}