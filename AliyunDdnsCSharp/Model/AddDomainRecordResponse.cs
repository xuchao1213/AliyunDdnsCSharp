/*--------------------------------------------------------
* 
* File: AddDomainRecordResponse
* Author: Xu Chao
* Email: xuchao_1213@163.com
* Created: 2018-10-10 22:37:43
* Desc: 添加解析记录返回结果 
* 
* -------------------------------------------------------*/

namespace AliyunDdnsCSharp.Model
{
    /// <summary>
    ///  添加解析记录返回结果 
    /// </summary>
    public class AddDomainRecordResponse:BaseResponse
    {
        public string RecordId { get; set; }
    }
}
