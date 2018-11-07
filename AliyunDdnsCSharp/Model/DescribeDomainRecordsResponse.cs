/*--------------------------------------------------------
* 
* File: DescribeDomainRecordsResponse
* Author: Xu Chao
* Email: xuchao_1213@163.com
* Created: 2018-11-06 23:34:05
* Desc: 获取解析记录列表 返回 
* 
* -------------------------------------------------------*/

using System.Collections.Generic;
using Newtonsoft.Json;

namespace AliyunDdnsCSharp.Model
{
    public class DescribeDomainRecordsResponse:BaseResponse
    {
        /// <summary>
        /// 解析记录总数
        /// </summary>
        public long TotalCount { get; set; }
        /// <summary>
        /// 当前页码
        /// </summary>
        public long PageNumber { get; set; }
        /// <summary>
        /// 本次查询获取的解析数量
        /// </summary>
        public long PageSize { get; set; }
        /// <summary>
        /// 解析记录列表
        /// </summary>
        public Record DomainRecords { get; set; }
    }

    public class Record
    {
        [JsonProperty("Record")]
        public List<RecordDetail> Records { get; set; }
    }
}
