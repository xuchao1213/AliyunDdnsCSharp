using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AliyunDdnsCSharp.Core {
    public class IpProviderConf {
        public const string DEFAULT_IP_V4_URL = "https://ip.xrdp.cc";
        public const string P_URL = "URL";
        public const string P_LOCAL = "LOCAL";
        /// <summary>
        /// 获取IP方式(url|local)
        /// </summary>
        [JsonProperty("Provider")]
        public string Provider { get; set; }

        /// <summary>
        /// url : provider = URL时有效
        /// </summary>
        [JsonProperty("Url")]
        public string Url { get; set; }

        #region provider = LOCAL 时有效

        /// <summary>
        /// 网卡名称
        /// </summary>
        [JsonProperty("AdapterName")]
        public string AdapterName { get; set; }

        /// <summary>
        /// 地址前缀
        /// </summary>
        [JsonProperty("Prefix")]
        public string Prefix { get; set; }

        #endregion provider = LOCAL 时有效

        public static IpProviderConf NewDefaultLocal() {
            return new IpProviderConf() {
                Provider = P_LOCAL,
                Url = "",
                AdapterName = "",
                Prefix = "",
            };
        }

        public static IpProviderConf NewFromUrl(string url) {
            return new IpProviderConf() {
                Provider = P_URL,
                Url = url,
                AdapterName ="",
                Prefix ="",
            };
        }

        public static IpProviderConf CopyFromOther(IpProviderConf conf) {
            return new IpProviderConf() {
                Provider = conf.Provider,
                Url = conf.Url,
                AdapterName = conf.AdapterName,
                Prefix = conf.Prefix,
            };
        }

        public bool TryValiddate(out string messae) {
            messae = "";
            if (string.IsNullOrWhiteSpace(this.Provider)) {
                messae = "Empty provider";
                return false;
            }
            if (IsUrl()&& string.IsNullOrWhiteSpace(this.Url)) {
                messae = "Empty Url";
                return false;
            }
            if (IsUrl()    //url
               || IsLocal()) {     //local
                return true;
            }
            messae = $"Unsupported provider '{Provider}' ,Must be one of ['LOCAL','URL']";
            return false;
        }

        public bool IsLocal() {
            return string.Compare(this.Provider, P_LOCAL, true) == 0;
        }
        public bool IsUrl() {
            return string.Compare(this.Provider, P_URL, true) == 0;
        }
        public bool IsAdapterMatch(string adapter) {
            return adapter.Contains(this.AdapterName);
        }
    }
}
