using Newtonsoft.Json;
using System.Collections.Generic;
using AliyunDdnsCSharp.Utils;

namespace AliyunDdnsCSharp.Core
{
    public class WorkerConf
    {
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
        /// 获取外网地址的网址，默认 http://ip.hiyun.me
        /// </summary>
        [JsonProperty]
        public List<string> GetIpUrls { get; set; } = new List<string> { };
        [JsonIgnore]
        public bool IsIpV6 => Type == "AAAA";

        public void CopyFromOther(WorkerConf conf)
        {
            File = conf.File;
            Interval = conf.Interval;
            AccessKeyId = conf.AccessKeyId;
            AccessKeySecret = conf.AccessKeySecret;
            DomainName = conf.DomainName;
            SubDomainName = conf.SubDomainName;
            Type = conf.Type;
            TTL = conf.TTL;
            Line = conf.Line;
            GetIpUrls = conf.GetIpUrls;

        }

        public bool Reload()
        {
            return LoadFromFile(File);
        }

        public bool LoadFromFile(string file)
        {
            if (file.TryReadAllText(out string content)
                && content.TryDeserializeJsonStr(out WorkerConf conf)
                && conf.Validate())
            {
                conf.File = file;
                CopyFromOther(conf);
                return true;
            }
            return false;
        }

        public bool Validate()
        {
            return Interval > 0
                   && !string.IsNullOrWhiteSpace(AccessKeyId)
                   && !string.IsNullOrWhiteSpace(AccessKeySecret)
                   && !string.IsNullOrWhiteSpace(DomainName)
                   && !string.IsNullOrWhiteSpace(SubDomainName)
                   && !string.IsNullOrWhiteSpace(Type)
                   && (Type == "A" || Type == "AAAA")
                   && GetIpUrls != null
                   && (Type == "AAAA" || GetIpUrls.Count > 0);
        }
    }
}
