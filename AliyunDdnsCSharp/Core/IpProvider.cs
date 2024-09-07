using AliyunDdnsCSharp.Utils;
using Newtonsoft.Json;
using NLog;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace AliyunDdnsCSharp.Core {

    public class IpProvider {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        //merge from https://github.com/stfei/AliyunDdnsCSharp 
        //to fix https://github.com/xuchao1213/AliyunDdnsCSharp/issues/13
        private const string IPV4_REGEX =
             @"((?:(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))";
        private const string IPV6_REGEX =
            @"^\s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:)))(%.+)?\s*$";

        private static readonly Regex IpV4Regex = new Regex(IPV4_REGEX);
        private static readonly Regex IpV6Regex = new Regex(IPV6_REGEX);

        private IpProviderConf conf;
        public string Name {
        get{
                if (conf.IsLocal()) {
                    return $"{conf.Provider} {conf.AdapterName} ({conf.Prefix}*)";
                } else {
                    return $"{conf.Provider} {conf.Url}";
                }
            } }
        private bool IsV6 = false;

        public IpProvider(IpProviderConf conf, bool isV6 = false) {
            this.conf = conf;
            this.IsV6 = isV6;
        }

        public bool TryResolveIp(out string ip, out string msg) {
            bool ok = false;
            if (conf.IsLocal()) {
                ok = TryResolveLocalIp(conf.AdapterName,conf.Prefix, out ip, out msg);
            } else {
                ok = TryResolveFromUrl(conf.Url, out ip, out msg);
            }
            if (!ok) {
                return ok;
            }
            // double check
            if (!ip.IsIpAddress()) {
                msg = $"[{ip}] is not a valid ipv4 address";
                ip = "";
                return false;
            }

            //double check
            if (IsV6 && !ip.IsIpV6Address()) {
                msg = $"[{ip}] is not a valid ipV66666 address";
                ip = "";
                return false;
            }
            return true;
        }
        private bool TryResolveLocalIp(string adapter,string prefix, out string realIp, out string msg) {
            realIp = "";
            msg = "";
            AddressFamily family = IsV6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
            string ip =NetWorkUtils.GetLocalIpAddress(family, adapter, prefix);
            if (string.IsNullOrWhiteSpace(ip)) {
                msg = "no valid address";
                return false;
            }
            realIp = ip;
            return true;
        }
        private  bool TryResolveFromUrl(string url,out string realIp,out string msg) {
            realIp = "";
            msg = "";
            var getRes = url.GET().Result;
            Match mc;
            //提取IPV6地址
            if (IsV6) {
                mc = IpV6Regex.Match(getRes);
            }
            //提取IPV4地址
            else {
                mc = IpV4Regex.Match(getRes);
            }

            if (mc.Success && mc.Groups.Count > 0) {
                realIp = mc.Groups[0].Value;
                return true;
            }
            msg = $"no valid Ip{(IsV6?"V6":"V4")} address in {url} response";
            return false;
        }
    }
}