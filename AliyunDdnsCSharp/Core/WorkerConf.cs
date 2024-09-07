using Newtonsoft.Json;
using System.Collections.Generic;
using AliyunDdnsCSharp.Utils;
using AliyunDdnsCSharp.Std;
using NLog;

namespace AliyunDdnsCSharp.Core {
    public class WorkerConf {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        [JsonIgnore]
        public string File { get; set; }
        [JsonIgnore]
        public string Name => $"{Type}&{SubDomainName}.{DomainName}";
        /// <summary>
        /// 刷新时间间隔
        /// </summary>
        [JsonProperty]
        public int Interval { get; set; }
        /// <summary>
        /// 阿里云AccessKeyId See https://help.aliyun.com/knowledge_detail/38738.html?spm=5176.11065259.1996646101.searchclickresult.73c9490e2I0S3U
        /// </summary>
        [JsonProperty]
        public string AccessKeyId { get; set; }
        /// <summary>
        /// 阿里云AccessKeySecret
        /// </summary>
        [JsonProperty]
        public string AccessKeySecret { get; set; }
        /// <summary>
        /// 阿里云域名 如 google.com
        /// </summary>
        [JsonProperty]
        public string DomainName { get; set; }
        /// <summary>
        /// 阿里云子域名 如 test
        /// </summary>
        [JsonProperty]
        public string SubDomainName { get; set; }
        /// <summary>
        /// 记录类型，A,AAAA
        /// </summary>
        [JsonProperty]
        public string Type { get; set; } = "A";
        [JsonProperty]
        /// <summary>
        /// 生存时间，默认为600秒（10分钟），参见TTL定义说明(https://help.aliyun.com/document_detail/29806.html?spm=a2c4g.11186623.2.20.29f17d8cFvRltO)
        /// </summary>
        public string TTL { get; set; } = "600";
        [JsonIgnore]
        public int TtlV => int.TryParse(TTL, out int v) ? v : 600;
        [JsonProperty]
        /// <summary>
        /// 解析线路，默认为default。参见解析线路枚举 https://help.aliyun.com/document_detail/29807.html?spm=a2c4g.11186623.2.22.41dd2846rHiL1v
        /// </summary>
        public string Line { get; set; } = "default";
        /// <summary>
        /// 获取外网地址的网址，默认 https://ip.xrdp.cc
        /// </summary>
        [JsonProperty]
        public List<string> GetIpUrls { get; set; } = new List<string> { };
        [JsonIgnore]
        public bool IsIpV6 => Type == "AAAA";
        [JsonIgnore]
        public bool IsIpV4 => Type == "A";

        /// <summary>
        /// 获取IP地址配置（新)
        /// </summary>
        [JsonProperty]
        public List<IpProviderConf> IpProviders { get; set; } = new List<IpProviderConf> { };

        [JsonIgnore]
        public List<IpProvider> IpProviderImpls { get; private set; } = new List<IpProvider>();

        private readonly Once providerInitOnce = new Once();

        public void CopyFromOther(WorkerConf conf) {
            File = conf.File;
            Interval = conf.Interval;
            AccessKeyId = conf.AccessKeyId;
            AccessKeySecret = conf.AccessKeySecret;
            DomainName = conf.DomainName;
            SubDomainName = conf.SubDomainName;
            Type = conf.Type;
            TTL = conf.TTL;
            Line = conf.Line;
            GetIpUrls.Clear();
            if (Interval <= 0) {
                Interval = 30;
            }
            if (conf.GetIpUrls != null) {
                foreach (var url in conf.GetIpUrls) {
                    GetIpUrls.Add(url);
                }
            }
            IpProviders.Clear();
            if (conf.IpProviders != null) {
                foreach (var p in conf.IpProviders) {
                    IpProviders.Add(IpProviderConf.CopyFromOther(p));
                }
            }
        }

        public bool Reload() {
            return LoadFromFile(File);
        }

        public bool LoadFromFile(string file) {
            if (file.TryReadAllText(out string content)
                && content.TryDeserializeJsonStr(out WorkerConf conf)
                && conf.Validate()) {
                conf.File = file;
                CopyFromOther(conf);
                return true;
            }
            return false;
        }

        public bool Validate() {
            return Interval > 0
                   && !string.IsNullOrWhiteSpace(AccessKeyId)
                   && !string.IsNullOrWhiteSpace(AccessKeySecret)
                   && !string.IsNullOrWhiteSpace(DomainName)
                   && !string.IsNullOrWhiteSpace(SubDomainName)
                   && !string.IsNullOrWhiteSpace(Type)
                   && (Type == "A" || Type == "AAAA");
        }
        public void InitIpProviderOnce() {
            providerInitOnce.Do(() => {
                // 新的配置方式
                if(this.IpProviders!=null && this.IpProviders.Count > 0) {
                    for (int i= 0;i < this.IpProviders.Count;++i) {
                        var cnf = this.IpProviders[i];
                        if (!cnf.TryValiddate(out string msg)) {
                            Log.Warn($"Invalid Ip Provider Conf At {i} : {msg}");
                            continue;
                        }
                        this.IpProviderImpls.Add(new IpProvider(cnf, IsIpV6));
                    }
                }
                //兼容老的配置方式
                if(this.GetIpUrls!=null && this.GetIpUrls.Count > 0) {
                    for (int i = 0; i < this.GetIpUrls.Count; ++i) {
                        var url = this.GetIpUrls[i];
                        if (string.IsNullOrWhiteSpace(url)) {
                            continue;
                        }
                        this.IpProviderImpls.Add(new IpProvider(IpProviderConf.NewFromUrl(url), IsIpV6));
                    }
                }
                // 未配置provider时采用本地Ipv6地址
                if(IsIpV6 && this.IpProviderImpls.Count == 0) {
                    this.IpProviderImpls.Add(new IpProvider(IpProviderConf.NewDefaultLocal(), IsIpV6));
                }
                // 未配置provider时采用默认Url地址
                if (this.IsIpV4 && this.IpProviderImpls.Count == 0) {
                    this.IpProviderImpls.Add(new IpProvider(IpProviderConf.NewFromUrl(IpProviderConf.DEFAULT_IP_V4_URL), IsIpV6));
                }
            });
        }
    }
}
