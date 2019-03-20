/*--------------------------------------------------------
* 
* File: UpdateDomainRecordRequest
* Author: Xu Chao
* Email: xuchao_1213@163.com
* Created: 2018-11-07 23:48:37
* Desc: 修改解析记录 请求 
* 
* -------------------------------------------------------*/
using System.Collections.Generic;

namespace AliyunDdnsCSharp.Model
{
    public class UpdateDomainRecordRequest : BaseRequest<UpdateDomainRecordResponse>
    {
        public UpdateDomainRecordRequest(string accessKeyId, string accessKeySecret) 
            : base(accessKeyId,accessKeySecret, "UpdateDomainRecord")
        {
        }

        /// <summary>
        /// 解析记录的ID，此参数在添加解析时会返回，在获取域名解析列表时会返回
        /// </summary>
        public string RecordId { get; set; }
        /// <summary>
        /// 主机记录，如果要解析@.exmaple.com，主机记录要填写"@”，而不是空
        /// </summary>
        public string RR { get; set; }

        /// <summary>
        /// 解析记录类型，参见解析记录类型格式(https://help.aliyun.com/document_detail/29805.html?spm=a2c4g.11186623.2.19.29f17d8ciNDiKK)
        /// </summary>
        public string Type { get; set; } = "A";

        /// <summary>
        /// 记录值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 生存时间，默认为600秒（10分钟），参见TTL定义说明(https://help.aliyun.com/document_detail/29806.html?spm=a2c4g.11186623.2.20.29f17d8cFvRltO)
        /// </summary>
        public long TTL { get; set; } = 600;

        /// <summary>
        /// MX记录的优先级，取值范围[1,10]，记录类型为MX记录时，此参数必须
        /// </summary>
        public long Priority { get; set; }

        /// <summary>
        /// 解析线路，默认为default。参见解析线路枚举(https://help.aliyun.com/document_detail/29807.html?spm=a2c4g.11186623.2.21.29f17d8ciNDiKK)
        /// </summary>
        public string Line { get; set; }

        protected override Dictionary<string, string> ExtQueryParameters() {
            var ret = new Dictionary<string, string>() {
                ["RecordId"] = RecordId,
                ["RR"] = RR,
                ["Type"] = Type,
                ["Value"] = Value,
            };
            if (TTL > 0)
            {
                ret["TTL"] = TTL.ToString();
            }
            //if (Priority > 0 && Priority < 500)
            //{
            //    ret["Priority"] = Priority.ToString();
            //}
            if (!string.IsNullOrWhiteSpace(Line))
            {
                ret["Line"] = Line;
            }
            return ret;
        }
    }
}
