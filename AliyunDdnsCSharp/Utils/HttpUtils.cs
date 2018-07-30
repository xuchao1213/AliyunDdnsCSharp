using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace AliyunDdnsCSharp.Utils
{
    /// <summary>
    /// Http Utils
    /// </summary>
    public static class HttpUtils
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        //设置不使用代理
        private static readonly HttpClientHandler GHttpHandler = new HttpClientHandler
        {
            UseProxy = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        private static readonly HttpClient GlobalClient = new HttpClient(GHttpHandler)
        {
            Timeout = TimeSpan.FromSeconds(10),
        };

        static HttpUtils()
        {
            UseHttps();
            GlobalClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("GZIP"));
        }
        private static void UseHttps()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, errors) => true;
        }

        private static HttpContent ToHttpStringContent(this string bodyData)
        {
            var stringContent = new StringContent(bodyData, Encoding.UTF8);
            stringContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return stringContent;
        }

        private static async Task<string> AsyncGetHttpResponseString(this Task<HttpResponseMessage> task)
        {
            using (var rsp = await task.ConfigureAwait(false))
            {
                string resString = await rsp.Content.ReadAsStringAsync();
                if (!rsp.IsSuccessStatusCode)
                {
                    var statusCode = rsp.StatusCode;
                    resString = new JObject()
                    {
                        {"HttpStatusCode",(int)statusCode},
                        {"HttpResponse",resString}
                    }.ToString(Formatting.None);
                }
                logger.Debug($"Http Response >> {resString}");
                return resString;
            }
        }

        private static async Task<T> SafeAsyncCall<T>(this Task<T> t)
        {
            try
            {
                return await t;
            }
            catch (Exception e)
            {
                logger.Warn(e);
            }
            return default(T);
        }

        // ReSharper disable once UnusedMember.Local
        private static Task<string> SendAsync(this HttpRequestMessage request)
        {
            return Task.Run(async () => {
                //异步发送并等待结果
                string retString = String.Empty;
                string errMessage = string.Empty;
                HttpResponseMessage rsp = null;
                HttpStatusCode httpStatusCode = HttpStatusCode.GatewayTimeout;
                try
                {
                    rsp = await GlobalClient.SendAsync(request);
                    httpStatusCode = rsp.StatusCode;
                    var resStr = await rsp.Content.ReadAsStringAsync();
                    if (rsp.IsSuccessStatusCode)
                    {
                        retString = resStr;
                    }
                    else
                    {
                        errMessage = retString;
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug($"HttpUtils SendAsync Exception :{ex.Message} {ex.InnerException?.Message}");
                    errMessage = ex.Message;
                }
                finally
                {
                    if (string.IsNullOrWhiteSpace(retString))
                    {
                        retString = new JObject()
                        {
                            {"HttpStatusCode", (int) httpStatusCode},
                            {"HttpResponse", errMessage}
                        }.ToString(Formatting.None);
                    }
                    rsp?.Dispose();
                }
                logger.Debug($"Http Response >> {retString}");
                return retString;
            });
        }
        public static async Task<string> Get(string url)
        {
            logger.Debug($"Http Get >> {url}");
            return await GlobalClient.GetAsync(url).AsyncGetHttpResponseString().SafeAsyncCall();
            //return await new HttpRequestMessage(HttpMethod.Get, url).SendAsync();
        }
        public static async Task<string> Delete(string url)
        {
            logger.Debug($"Http Delete >> {url}");
            return await GlobalClient.DeleteAsync(url).AsyncGetHttpResponseString().SafeAsyncCall();
            //return await new HttpRequestMessage(HttpMethod.Delete, url).SendAsync();
        }

        public static async Task<string> Post(string url, string bodyData)
        {
            logger.Debug($"Http Post >> {url} {bodyData}");
            return await GlobalClient.PostAsync(url, bodyData.ToHttpStringContent()).AsyncGetHttpResponseString().SafeAsyncCall();
            //return await new HttpRequestMessage(HttpMethod.Post, url) { Content = bodyData.ToHttpStringContent() }.SendAsync();
        }

        public static async Task<string> Put(string url, string bodyData)
        {
            logger.Debug($"Http Put >> {url} {bodyData}");
            return await GlobalClient.PutAsync(url, bodyData.ToHttpStringContent()).AsyncGetHttpResponseString().SafeAsyncCall();
            //  return await new HttpRequestMessage(HttpMethod.Put, url) { Content = bodyData.ToHttpStringContent() }.SendAsync();
        }

        public static async Task<TRet> SendHttpGet<TRet>(this string url)
        {
            return JsonConvert.DeserializeObject<TRet>(await Get(url));
        }
        public static async Task<TRet> SendHttpDelete<TRet>(this string url)
        {
            return JsonConvert.DeserializeObject<TRet>(await Delete(url));
        }

        public static async Task<TRet> SendHttpPost<TRet>(this string url, string reqJsonStr)
        {
            return JsonConvert.DeserializeObject<TRet>(await Post(url, reqJsonStr));
        }

        public static async Task<TRet> SendHttpPut<TRet>(this string url, string reqJsonStr)
        {
            return JsonConvert.DeserializeObject<TRet>(await Put(url, reqJsonStr));
        }
    }
}
