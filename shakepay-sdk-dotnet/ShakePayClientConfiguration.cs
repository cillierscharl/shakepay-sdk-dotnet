using System;
using System.Collections.Generic;
using System.Text;

namespace ShakePay
{
    public class ShakePayClientConfiguration
    {
        public string DeviceName { get; set; }
        public string PrivateIpAddress { get; set; }
        public string DeviceUniqueId { get; set; }
        public string Jwt { get; set; }
    }
}
