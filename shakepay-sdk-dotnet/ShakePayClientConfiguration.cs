namespace ShakePay
{
    public class ShakePayClientConfiguration
    {
        public string DeviceName { get; set; }
        public string PrivateIpAddress { get; set; }
        public string DeviceUniqueId { get; set; }
        public string Jwt { get; set; }
        /// <summary>Renew the JWT every 5 minutes until the client is disposed.</summary>
        public bool AutoRefreshToken { get; set; }
    }
}
