/*--------------------------------------------------------
* 
* File: BaseReq
* Author: Xu Chao
* Email: xuchao_1213@163.com
* Created: 2018-10-10 22:16:56
* Desc: BaseReq 包含公共的请求参数
* 
* -------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AliyunDdnsCSharp.Utils;
using NLog;

namespace AliyunDdnsCSharp.Model
{
    public abstract class BaseRequest<TRes> where TRes : BaseResponse, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const string API_URL = "https://alidns.aliyuncs.com";
        private const string DEFAULT_API_VERSION = "2015-01-09";
        private const string DEFAULT_SIGNATURE_METHOD = "HMAC-SHA1";
        private const string DEFAULT_SIGNATURE_VERSION = "1.0";

        #region 公共请求参数
        /// <summary>
        /// 返回值的类型，支持JSON与XML。默认为JSON
        /// </summary>
        public FormatType Format { get; protected set; } = FormatType.DEFAULT;

        /// <summary>
        /// API版本号，为日期形式：YYYY-MM-DD，本版本对应为2015-01-09
        /// </summary>
        public string Version { get; } = DEFAULT_API_VERSION;

        /// <summary>
        /// 签名方式，目前支持HMAC-SHA1
        /// </summary>
        public string SignatureMethod { get; } = DEFAULT_SIGNATURE_METHOD;
        /// <summary>
        /// 签名算法版本，目前版本是1.0
        /// </summary>
        public string SignatureVersion { get; } = DEFAULT_SIGNATURE_VERSION;
        /// <summary>
        /// 唯一随机数，用于防止网络重放攻击。用户在不同请求间要使用不同的随机数值
        /// </summary>
        public string SignatureNonce { get; private set; }

        #endregion

        public string Action { get; set; }
        public string AccessKeyId { get; private set; }
        public string AccessKeySecret { get; private set; }

        private Dictionary<string, string> CommonQueryParameters { get;}

        protected BaseRequest(string accessKeyId,string accessKeySecret, string action) {
            CommonQueryParameters = new Dictionary<string, string>();
            SignatureNonce = Guid.NewGuid().ToString("N");
            AccessKeyId = accessKeyId;
            AccessKeySecret = accessKeySecret;
            Action = action;
            CommonQueryParameters["Format"] = Format.ToString();
            CommonQueryParameters["Version"] = Version;
            CommonQueryParameters["AccessKeyId"] = AccessKeyId;
            CommonQueryParameters["SignatureMethod"] = SignatureMethod;
            CommonQueryParameters["Timestamp"] = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            CommonQueryParameters["SignatureVersion"] = SignatureVersion;
            CommonQueryParameters["SignatureNonce"] = SignatureNonce;
            CommonQueryParameters["Action"] = Action;
        }

        protected abstract Dictionary<string, string> ExtQueryParameters();
        public virtual async Task<TRes> Execute(){
            Dictionary<string, string> queries = new Dictionary<string, string>(CommonQueryParameters);
            var extQueries = ExtQueryParameters();
            if (extQueries != null)
            {
                foreach (var kv in extQueries)
                {
                    queries[kv.Key] = kv.Value;
                }
            }
            //计算签名
            string stringToSign = ComposeStringToSign(queries);
            string signature = SignString(stringToSign);
            Log.Debug($"stringToSign = {stringToSign}");
            Log.Debug($"Signature = {signature}");
            queries["Signature"] = signature;
            string queryStr = ConcatQueryString(queries);
            string url = $"{API_URL}/?{queryStr}";
            //构造完整 的url
            var httpRes = await url.Get();
            if (httpRes.Ok && httpRes.HttpResponseString.TryDeserializeJsonStr(out TRes res))
            {
                return res;
            }
            return new TRes() { Code = "NetError", HasError = true, Message = httpRes.HttpResponseString };
        }

        #region 签名、构建URL
        private string UrlEncode(string value) {
            return HttpUtility.UrlEncode(value, Encoding.UTF8);
        }
        private const string ENCODING_UTF8 = "UTF-8";
        private static string PercentEncode(string value) {
            StringBuilder stringBuilder = new StringBuilder();
            string text = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
            byte[] bytes = Encoding.GetEncoding(ENCODING_UTF8).GetBytes(value);
            foreach (char c in bytes)
            {
                if (text.IndexOf(c) >= 0)
                {
                    stringBuilder.Append(c);
                } else
                {
                    stringBuilder.Append("%").Append(string.Format(CultureInfo.InvariantCulture, "{0:X2}", (int)c));
                }
            }
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 拼接查询字符串
        /// </summary>
        /// <returns></returns>
        private string ConcatQueryString(Dictionary<string, string> queries) {
            var sortedDictionary = new SortedDictionary<string, string>(queries, StringComparer.Ordinal);
            StringBuilder sb = new StringBuilder();
            foreach (var kv in sortedDictionary)
            {
                sb.Append("&")
                    .Append(UrlEncode(kv.Key))
                    .Append("=")
                    .Append(UrlEncode(kv.Value));
            }
            return sb.ToString().Substring(1);
        }

        private string ComposeStringToSign(Dictionary<string, string> queries) {
            var sortedDictionary = new SortedDictionary<string, string>(queries, StringComparer.Ordinal);
            //构造规范化的请求字符串
            StringBuilder canonicalizedQueryString = new StringBuilder();
            foreach (var p in sortedDictionary)
            {
                canonicalizedQueryString
                    .Append("&")
                    .Append(PercentEncode(p.Key))
                    .Append("=")
                    .Append(PercentEncode(p.Value));
            }
            //构造用于计算签名的字符串
            StringBuilder stringToSign = new StringBuilder();
            stringToSign.Append("GET");
            stringToSign.Append("&");
            stringToSign.Append(PercentEncode("/"));
            stringToSign.Append("&");
            stringToSign.Append(PercentEncode(
                canonicalizedQueryString.ToString().Substring(1)));
            return stringToSign.ToString();
        }
        private string SignString(string stringToSign) {
            HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes($"{AccessKeySecret}&"));
            byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            return Convert.ToBase64String(hashValue);
        }
        #endregion
    }
}
