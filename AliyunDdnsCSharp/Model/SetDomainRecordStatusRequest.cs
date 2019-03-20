/*--------------------------------------------------------
* 
* File: SetDomainRecordStatusRequest
* Author: Xu Chao
* Email: xuchao_1213@163.com
* Created: 2019-03-20 21:48:45
* Desc: AddDomainRecordRequest 请求
* 
* -------------------------------------------------------*/

using System.Collections.Generic;

namespace AliyunDdnsCSharp.Model
{
    public class SetDomainRecordStatusRequest : BaseRequest<SetDomainRecordStatusResponse>
    {
        /// <summary>
        /// 解析记录的ID，此参数在添加解析时会返回，在获取域名解析列表时会返回
        /// </summary>
        public string RecordId { get; set; }
        /// <summary>
        /// Enable: 启用解析 Disable: 暂停解析
        /// </summary>
        public bool Enable { get; set; }
        public SetDomainRecordStatusRequest(string accessKeyId, string accessKeySecret)
            : base(accessKeyId, accessKeySecret, "SetDomainRecordStatus")
        {
        }

        protected override Dictionary<string, string> ExtQueryParameters()
        {
           return new Dictionary<string, string>()
            {
                ["RecordId"] = RecordId,
                ["Status"] = Enable? "Enable" : "Disable",
            };
        }
    }
}
