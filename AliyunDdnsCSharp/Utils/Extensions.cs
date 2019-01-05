using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AliyunDdnsCSharp.Utils
{
    public static class Extensions
    {
        public static bool IsIpAddress(this string v)
        {
            return IPAddress.TryParse(v, out IPAddress ip);
        }

        public static bool IsIpV6Address(this string v)
        {
            return IPAddress.TryParse(v, out IPAddress ip) && ip.AddressFamily == AddressFamily.InterNetworkV6;
        }

        public static bool IsIpV4Address(this string v)
        {
            return IPAddress.TryParse(v, out IPAddress ip) && ip.AddressFamily == AddressFamily.InterNetwork;
        }
    }
}
