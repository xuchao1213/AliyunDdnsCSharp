using Newtonsoft.Json;
using System.Collections.Generic;

namespace AliyunDdnsCSharp.Core
{
    public class WorkerConf
    {
        [JsonIgnore]
        public string Name => $"{SubDomainName}.{DomainName}";
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
        /// <summary>
        /// 获取外网地址的网址，默认 http://ip.hiyun.me
        /// </summary>
        [JsonProperty]
        public List<string> GetIpUrls { get; set; } = new List<string> { };
        [JsonIgnore]
        public bool IsIpV6 => Type == "AAAA";

        public bool Validate() {
            return Interval > 0
                   && !string.IsNullOrWhiteSpace(AccessKeyId)
                   && !string.IsNullOrWhiteSpace(AccessKeySecret)
                   && !string.IsNullOrWhiteSpace(DomainName)
                   && !string.IsNullOrWhiteSpace(SubDomainName)
                   &&!string.IsNullOrWhiteSpace(Type)
                   &&(Type=="A"||Type=="AAAA")
                   && GetIpUrls != null
                   && (Type == "AAAA"||GetIpUrls.Count > 0);
        }
    }
}
