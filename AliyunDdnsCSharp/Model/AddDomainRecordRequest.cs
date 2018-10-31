/*--------------------------------------------------------
* 
* File: AddDomainRecord
* Author: Xu Chao
* Email: xuchao_1213@163.com
* Created: 2018-10-10 22:39:45
* Desc: 添加解析记录请求参数 
* 
* -------------------------------------------------------*/

using System.Threading.Tasks;

namespace AliyunDdnsCSharp.Model
{
    public class AddDomainRecordRequest:BaseReq<AddDomainRecordResponse>
    {
        public AddDomainRecordRequest(string version, string action) : base(version, action)
        {
        }

        public AddDomainRecordRequest(string action) : base(action)
        {

        }

        public AddDomainRecordRequest Domain(string v)
        {
            return this;
        }

        public override Task<AddDomainRecordResponse> Execute()
        {
            return base.Execute();

        }
    }
}
