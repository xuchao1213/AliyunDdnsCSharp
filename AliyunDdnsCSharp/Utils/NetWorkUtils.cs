using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace AliyunDdnsCSharp.Utils
{
    public static class NetWorkUtils
    {
        public static string GetLocalIpAddress(AddressFamily family, string adapterFilter = "", string prefixFilter="") {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            bool filterAdapter = !string.IsNullOrWhiteSpace(adapterFilter);
            bool filterPrefix = !string.IsNullOrWhiteSpace(prefixFilter);
            string ret = "";
            if (adapters.Length == 0) { return ""; }
            foreach (NetworkInterface adapter in adapters) {
                if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
                    adapter.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                    adapter.NetworkInterfaceType != NetworkInterfaceType.GigabitEthernet) {
                    continue;
                }
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                UnicastIPAddressInformationCollection uniCast = adapterProperties.UnicastAddresses;
                if(uniCast.Count <= 0) {
                    continue;
                }
                string adapterAddress = "";
                foreach (UnicastIPAddressInformation uni in uniCast) {
                    var ipa = uni.Address;
                    if(ipa.AddressFamily != family){
                        continue;
                    }
                    if (uni.DuplicateAddressDetectionState != DuplicateAddressDetectionState.Preferred) {
                        continue;
                    }
                    //IPV4
                    if (family == AddressFamily.InterNetwork) {
                        adapterAddress = ipa.ToString();
                    }
                    if (family == AddressFamily.InterNetworkV6) {
                        if (ipa.IsIPv6LinkLocal || ipa.IsIPv6SiteLocal || ipa.IsIPv6Multicast) { continue; }
                        //优先取Dhcp分配的
                        if (uni.PrefixOrigin == PrefixOrigin.Dhcp && uni.SuffixOrigin == SuffixOrigin.OriginDhcp) {
                            adapterAddress =  ipa.ToString();
                        }
                        //其次取RA & LinkLayerAddress
                        if (uni.PrefixOrigin == PrefixOrigin.RouterAdvertisement && uni.SuffixOrigin == SuffixOrigin.LinkLayerAddress) {
                            adapterAddress = ipa.ToString();
                        }
                        //最后取RA & Random
                        if (uni.PrefixOrigin == PrefixOrigin.RouterAdvertisement && uni.SuffixOrigin == SuffixOrigin.Random) {
                            adapterAddress = ipa.ToString();
                        }
                    }
                }
                string adapterName = adapter.Name;
                if (!string.IsNullOrWhiteSpace(adapterAddress)) {
                    if(filterAdapter || filterPrefix) {
                        if (filterAdapter && adapterName.Contains(adapterFilter)) {
                            ret = adapterAddress;
                            break;
                        }
                        if (filterPrefix && adapterAddress.StartsWith(prefixFilter)) {
                            ret = adapterAddress;
                            break;
                        }
                    } else {
                        ret = adapterAddress;
                        break;
                    }
                }
            }
            return ret;
        }
        //public static string GetLocalIpV4Address() {
        //    NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        //    if (adapters.Length == 0) { return ""; }
        //    foreach (NetworkInterface adapter in adapters) {
        //        if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
        //            adapter.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
        //            adapter.NetworkInterfaceType != NetworkInterfaceType.GigabitEthernet) {
        //            continue;
        //        }
        //        IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
        //        UnicastIPAddressInformationCollection uniCast = adapterProperties.UnicastAddresses;
        //        if (uniCast.Count > 0) {
        //            foreach (UnicastIPAddressInformation uni in uniCast) {
        //                var ipa = uni.Address;
        //                //过滤非IPV6地址
        //                if (ipa.AddressFamily != AddressFamily.InterNetworkV6) {
        //                    continue;
        //                }
        //                if (uni.DuplicateAddressDetectionState != DuplicateAddressDetectionState.Preferred) {
        //                    continue;
        //                }
        //                if (ipa.IsIPv6LinkLocal || ipa.IsIPv6SiteLocal || ipa.IsIPv6Multicast) { continue; }
        //                //优先取Dhcp分配的
        //                if (uni.PrefixOrigin == PrefixOrigin.Dhcp && uni.SuffixOrigin == SuffixOrigin.OriginDhcp) {
        //                    return ipa.ToString();
        //                }
        //                //其次取RA & LinkLayerAddress
        //                if (uni.PrefixOrigin == PrefixOrigin.RouterAdvertisement && uni.SuffixOrigin == SuffixOrigin.LinkLayerAddress) {
        //                    return ipa.ToString();
        //                }
        //                //最后取RA & Random
        //                if (uni.PrefixOrigin == PrefixOrigin.RouterAdvertisement && uni.SuffixOrigin == SuffixOrigin.Random) {
        //                    return ipa.ToString();
        //                }
        //            }
        //        }
        //    }
        //    return "";
        //}
        //public static string GetLocalIpV6Address()
        //{
        //    NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        //    if (adapters.Length == 0) { return ""; }
        //    foreach (NetworkInterface adapter in adapters)
        //    {
        //        if (adapter.NetworkInterfaceType!=NetworkInterfaceType.Ethernet&&
        //            adapter.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
        //            adapter.NetworkInterfaceType != NetworkInterfaceType.GigabitEthernet
        //            )
        //        {
        //            continue;
        //        }
        //        IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
        //        UnicastIPAddressInformationCollection uniCast = adapterProperties.UnicastAddresses;
        //        if (uniCast.Count > 0)
        //        {
        //            foreach (UnicastIPAddressInformation uni in uniCast)
        //            {
        //                var ipa = uni.Address;
        //                //过滤非IPV6地址
        //                if (ipa.AddressFamily != AddressFamily.InterNetworkV6){
        //                    continue;
        //                }
        //                if (uni.DuplicateAddressDetectionState!= DuplicateAddressDetectionState.Preferred)
        //                {
        //                    continue;
        //                }
        //                if (ipa.IsIPv6LinkLocal || ipa.IsIPv6SiteLocal || ipa.IsIPv6Multicast) { continue; }
        //                //优先取Dhcp分配的
        //                if (uni.PrefixOrigin== PrefixOrigin.Dhcp && uni.SuffixOrigin==SuffixOrigin.OriginDhcp)
        //                {
        //                    return ipa.ToString();
        //                }
        //                //其次取RA & LinkLayerAddress
        //                if (uni.PrefixOrigin == PrefixOrigin.RouterAdvertisement && uni.SuffixOrigin == SuffixOrigin.LinkLayerAddress)
        //                {
        //                    return ipa.ToString();
        //                }
        //                //最后取RA & Random
        //                if (uni.PrefixOrigin == PrefixOrigin.RouterAdvertisement && uni.SuffixOrigin == SuffixOrigin.Random)
        //                {
        //                    return ipa.ToString();
        //                }
        //            }
        //        }
        //    }
        //    return "";
        //}
    }
}
