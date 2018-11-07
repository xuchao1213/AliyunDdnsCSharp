/*--------------------------------------------------------
* 
* File: BaseResponse
* Author: Xu Chao
* Email: xuchao_1213@163.com
* Created: 2018-10-10 22:36:36
* Desc: 返回结果基类
* 
* -------------------------------------------------------*/

namespace AliyunDdnsCSharp.Model
{
    public class BaseResponse
    {
        /// <summary>
        /// 唯一请求识别码
        /// </summary>
        public string RequestId { get; set; }
        public string HostId { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public bool HasError { get; set; }
    }
}
