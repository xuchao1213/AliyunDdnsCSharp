/*--------------------------------------------------------
* 
* File: AliApiClient
* Author: Xu Chao
* Email: xuchao_1213@163.com
* Created: 2018-10-11 22:20:49
* Desc: TODO 
* 
* -------------------------------------------------------*/

using System.Threading.Tasks;

namespace AliyunDdnsCSharp.Model
{
    public class AliApiClient
    {

        public Task<TRes> Execute<TReq, TRes>(TReq req)
        {
            return Task.Factory.StartNew(() => { return default(TRes); });
        }
    }
}
