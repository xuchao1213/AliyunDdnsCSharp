using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace AliyunDdnsCSharp.Utils
{
    public static class NetWorkUtils
    {
        public static string GetLocalIpV6Address()
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            if (adapters.Length == 0) { return ""; }
            foreach (NetworkInterface adapter in adapters)
            {
                if (adapter.NetworkInterfaceType!=NetworkInterfaceType.Ethernet&&
                    adapter.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                    adapter.NetworkInterfaceType != NetworkInterfaceType.GigabitEthernet
                    )
                {
                    continue;
                }
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                UnicastIPAddressInformationCollection uniCast = adapterProperties.UnicastAddresses;
                if (uniCast.Count > 0)
                {
                    foreach (UnicastIPAddressInformation uni in uniCast)
                    {
                        var ipa = uni.Address;
                        //过滤非IPV6地址
                        if (ipa.AddressFamily != AddressFamily.InterNetworkV6){
                            continue;
                        }
                        if (uni.DuplicateAddressDetectionState!= DuplicateAddressDetectionState.Preferred)
                        {
                            continue;
                        }
                        if (ipa.IsIPv6LinkLocal || ipa.IsIPv6SiteLocal || ipa.IsIPv6Multicast) { continue; }
                        //优先取Dhcp分配的
                        if (uni.PrefixOrigin== PrefixOrigin.Dhcp && uni.SuffixOrigin==SuffixOrigin.OriginDhcp)
                        {
                            return ipa.ToString();
                        }
                        //其次取RA & LinkLayerAddress
                        if (uni.PrefixOrigin == PrefixOrigin.RouterAdvertisement && uni.SuffixOrigin == SuffixOrigin.LinkLayerAddress)
                        {
                            return ipa.ToString();
                        }
                        //最后取RA & Random
                        if (uni.PrefixOrigin == PrefixOrigin.RouterAdvertisement && uni.SuffixOrigin == SuffixOrigin.Random)
                        {
                            return ipa.ToString();
                        }
                    }
                }
            }
            return "";
        //    string ret = "";
        //    string name = Dns.GetHostName();
        //    IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
        //    foreach (IPAddress ipa in ipadrlist)
        //    {
        //        if (ipa.AddressFamily == AddressFamily.InterNetworkV6)
        //        {
        //            if (ipa.IsIPv6LinkLocal || ipa.IsIPv6SiteLocal ||ipa.IsIPv6Multicast) { continue; }
        //            ret = ipa.ToString();
        //            break;
        //        }
        //    }
        //    return ret;
        }
    }
}
