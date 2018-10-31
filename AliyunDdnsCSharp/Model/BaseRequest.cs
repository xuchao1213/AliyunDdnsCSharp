/*--------------------------------------------------------
* 
* File: BaseReq
* Author: Xu Chao
* Email: xuchao_1213@163.com
* Created: 2018-10-10 22:16:56
* Desc: BaseReq 包含公共的请求参数
* 
* -------------------------------------------------------*/

using System.Collections.Generic;
using System.Threading.Tasks;

namespace AliyunDdnsCSharp.Model
{
    public class BaseReq<TRes>
    {
        private const string DEFAULT_API_VERSION = "2015-01-09";
        private const string DEFAULT_SIGNATURE_METHOD = "HMAC-SHA1";
        private const string DEFAULT_SIGNATURE_VERSION = "1.0";

        #region 公共请求参数
        /// <summary>
        /// 返回值的类型，支持JSON与XML。默认为XML
        /// </summary>
        public FormatType Format { get; protected set; } = FormatType.DEFAULT;

        /// <summary>
        /// API版本号，为日期形式：YYYY-MM-DD，本版本对应为2015-01-09
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// 阿里云颁发给用户的访问服务所用的密钥ID
        /// </summary>
        public string AccessKeyId { get; set; }

        /// <summary>
        /// 签名结果串
        /// </summary>
        protected string Signature { get; set; }

        /// <summary>
        /// 签名方式，目前支持HMAC-SHA1
        /// </summary>
        protected string SignatureMethod { get; set; } = DEFAULT_SIGNATURE_METHOD;
        /// <summary>
        /// 签名算法版本，目前版本是1.0
        /// </summary>
        protected string SignatureVersion { get; set; } = DEFAULT_SIGNATURE_VERSION;
        /// <summary>
        /// 唯一随机数，用于防止网络重放攻击。用户在不同请求间要使用不同的随机数值
        /// </summary>
        protected string SignatureNonce { get; set; }

        #endregion

        public string Action { get; set; }
        protected Dictionary<string, string> QueryParameters { get; set; }

        protected BaseReq(string version, string action)
        {
            Version = version;
            Action = action;
        }

        protected BaseReq(string action) : this(DEFAULT_API_VERSION, action)
        {

        }

        public BaseReq<TRes> SetFormat(FormatType formatType)
        {
            Format = formatType;
            return this;
        }

        public BaseReq<TRes> AccessKeySecret(string accessKeySecret)
        {


            return this;
        }

        public BaseReq<TRes> Build()
        {
            //拼接参数


            //计算签名


            return this;
        }

        public virtual Task<TRes> Execute()
        {
            return Task.Factory.StartNew(() =>
            {
                return default(TRes);
            });
        }
    }
}
