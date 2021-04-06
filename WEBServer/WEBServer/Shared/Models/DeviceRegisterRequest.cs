namespace WEBServer.Shared.Models
{
    public class DeviceRegisterRequest
    {
        public int? Device { get; set; } = null;
        public int Location {get;set;}
        public string OTP_Key { get; set; } = null;
        public InstallationType InstallationType { get; set; } = InstallationType.NotSpecified;
    }
}