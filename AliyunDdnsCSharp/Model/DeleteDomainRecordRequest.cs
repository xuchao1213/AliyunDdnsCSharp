using System.Collections.Generic;

namespace AliyunDdnsCSharp.Model
{
    public class DeleteDomainRecordRequest : BaseRequest<DeleteDomainRecordResponse>
    {
        /// <summary>
        /// 解析记录的ID，此参数在添加解析时会返回，在获取域名解析列表时会返回
        /// </summary>
        public string RecordId { get; set; }

        public DeleteDomainRecordRequest(string accessKeyId, string accessKeySecret) : base(accessKeyId, accessKeySecret, "DeleteDomainRecord")
        {
        }

        protected override Dictionary<string, string> ExtQueryParameters()
        {
            var ret = new Dictionary<string, string>()
            {
                ["RecordId"] = RecordId,
            };
            return ret;
        }
    }
}
