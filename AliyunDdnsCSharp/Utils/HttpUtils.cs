/*--------------------------------------------------------
* 
* File: HttpUtils
* Author: Xu Chao
* Email: xuchao_1213@163.com
* Created: 2018-10-10 20:36:38
* Desc: Http Utils
* 
* -------------------------------------------------------*/
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NLog;

namespace AliyunDdnsCSharp.Utils
{
    public static class HttpUtils
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        //设置不使用代理
        private static readonly HttpClientHandler GHttpHandler = new HttpClientHandler
        {
            UseProxy = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            //UseCookies=false,
            AllowAutoRedirect=false,
        };
        private static readonly HttpClient GlobalClient = new HttpClient(GHttpHandler)
        {
            Timeout = TimeSpan.FromSeconds(10),
        };

        static HttpUtils()
        {
            UseHttps();
            GlobalClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("GZIP"));
            //fix https://github.com/xuchao1213/AliyunDdnsCSharp/issues/2
            GlobalClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");
        }
        private static void UseHttps()
        {
            ServicePointManager.ServerCertificateValidationCallback =(sender, certificate, chain, errors) => true;
        }
        public class HttpGetResult
        {
            public int HttpStatusCode { get; set; } = -1;
            public string HttpResponseString { get; set; }=string.Empty;
            public bool Ok => HttpStatusCode >= 200 & HttpStatusCode <= 299;
        }

        public static async Task<HttpGetResult> Get(this string url)
        {
            Log.Debug($"Http Get >> {url}");
            HttpGetResult ret=null;
            try
            {
                using (var rsp = await GlobalClient.GetAsync(url).ConfigureAwait(false))
                {
                    string resString = await rsp.Content.ReadAsStringAsync();
                    ret=new HttpGetResult()
                    {
                        HttpStatusCode = (int)rsp.StatusCode,
                        HttpResponseString = resString,
                    };
                }
            }
            catch (Exception ex)
            {
                ret = new HttpGetResult() {
                    HttpStatusCode = -1,
                    HttpResponseString = $"网络异常：{ex.Message}({ex.InnerException?.Message})",
                };
            }
            finally
            {
                if (ret == null)
                {
                    ret=new HttpGetResult();
                }
                Log.Debug($"Http Response >> {ret.HttpResponseString}");
            }
            return ret;
        }
    }
}
