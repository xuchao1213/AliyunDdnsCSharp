using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AliyunDdnsCSharp.Utils
{
   public static class NetWorkUtils
    {
        public static string GetLocalIpV6Address()
        {
            string ret = "";
            string name = Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            foreach (IPAddress ipa in ipadrlist)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    if (ipa.IsIPv6LinkLocal || ipa.IsIPv6SiteLocal ||ipa.IsIPv6Multicast) { continue; }
                    ret = ipa.ToString();
                    break;
                }
            }
            return ret;
        }
    }
}
