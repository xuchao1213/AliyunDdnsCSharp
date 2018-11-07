/*--------------------------------------------------------
* 
* File: DescribeDomainRecordsRequest
* Author: Xu Chao
* Email: xuchao_1213@163.com
* Created: 2018-11-06 23:33:24
* Desc:  获取解析记录列表 请求
* 
* -------------------------------------------------------*/

using System.Collections.Generic;

namespace AliyunDdnsCSharp.Model
{
    public class DescribeDomainRecordsRequest : BaseRequest<DescribeDomainRecordsResponse>
    {
        /// <summary>
        /// 域名名称
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// 当前页数，起始值为1，默认为1
        /// </summary>
        public long PageNumber { get; set; } = 1;

        /// <summary>
        /// 分页查询时设置的每页行数，最大值500，默认为20
        /// </summary>
        public long PageSize { get; set; } = 20;
        /// <summary>
        /// 主机记录的关键字，按照”%RRKeyWord%”模式搜索，不区分大小写
        /// </summary>
        public string RRKeyWord { get; set; }
        /// <summary>
        /// 解析类型的关键字，按照全匹配搜索，不区分大小写
        /// </summary>
        public string TypeKeyWord { get; set; } = "A";
        /// <summary>
        /// 记录值的关键字，按照”%ValueKeyWord%”模式搜索，不区分大小写
        /// </summary>
        public string ValueKeyWord { get; set; }
        public DescribeDomainRecordsRequest(string accessKeyId, string accessKeySecret) : base(accessKeyId, accessKeySecret, "DescribeDomainRecords") {

        }
        protected override Dictionary<string, string> ExtQueryParameters() {
            var ret= new Dictionary<string, string>() {
                ["DomainName"] = DomainName,
            };
            if (PageNumber > 0)
            {
                ret["RRKeyWord"] = PageNumber.ToString();
            }
            if (PageSize > 0 && PageSize<500)
            {
                ret["PageSize"] = PageSize.ToString();
            }
            if (!string.IsNullOrWhiteSpace(RRKeyWord))
            {
                ret["RRKeyWord"] = RRKeyWord;
            }
            if (!string.IsNullOrWhiteSpace(TypeKeyWord))
            {
                ret["TypeKeyWord"] = TypeKeyWord;
            }
            if (!string.IsNullOrWhiteSpace(ValueKeyWord))
            {
                ret["ValueKeyWord"] = ValueKeyWord;
            }
            return ret;
        }
    }
}
