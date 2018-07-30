namespace AliyunDdnsCSharp.Core
{
    public class EnvConf
    {
        /// <summary>
        /// 刷新时间间隔
        /// </summary>
        public int Interval { get; set; }
        /// <summary>
        /// 阿里云AccessKeyId See https://help.aliyun.com/knowledge_detail/38738.html?spm=5176.11065259.1996646101.searchclickresult.73c9490e2I0S3U
        /// </summary>
        public string AccessKeyId { get; set; }
        /// <summary>
        /// 阿里云AccessKeySecret
        /// </summary>
        public string AccessKeySecret { get; set; }
        /// <summary>
        /// 阿里云域名 如 google.com
        /// </summary>
        public string DomainName { get; set; }
        /// <summary>
        /// 阿里云子域名 如 test
        /// </summary>
        public string SubDomainName { get; set; }

        public bool Validate()
        {
            return Interval > 0
                   && !string.IsNullOrWhiteSpace(AccessKeyId)
                   && !string.IsNullOrWhiteSpace(AccessKeySecret)
                   && !string.IsNullOrWhiteSpace(DomainName)
                   && !string.IsNullOrWhiteSpace(SubDomainName);
        }
    }
}
