using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace AliyunDdnsCSharp.Utils {

    public class BaseHttpResponse {

        [JsonProperty("httpStatusCode")]
        public int HttpStatusCode { get; set; } = 200;

        [JsonProperty("httpResponse")]
        public string HttpResponse { get; set; }

        [JsonIgnore]
        protected bool IsSuccessStatusCode => HttpStatusCode >= 200 && HttpStatusCode <= 209;
    }

    public static class HttpUtils {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const string ContentTypeJson = "application/json";

        private static readonly WebRequestHandler GWebHandler = new WebRequestHandler() {
            UseProxy = false,
            AllowAutoRedirect = true,
            ReadWriteTimeout = 30 * 1000,
            ClientCertificateOptions = ClientCertificateOption.Manual,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        private static readonly HttpClient GlobalClient = new HttpClient(GWebHandler) {
            Timeout = TimeSpan.FromSeconds(30),
        };

        static HttpUtils() {
            InitSsl();
        }

        private static void InitSsl() {
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12
                |SecurityProtocolType.SystemDefault
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls
                | SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, errors) => true;
        }

        public static HttpContent ToHttpStringContent(this string bodyData, string contentType) {
            var stringContent = new StringContent(bodyData, Encoding.UTF8);
            stringContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            return stringContent;
        }

        public static HttpContent ToHttpJsonStringContent(this string bodyData, string contentType) {
            return ToHttpStringContent(bodyData, contentType);
        }

        private static async Task<string> AsyncGetHttpResponseString(this Task<HttpResponseMessage> task) {
            using (var rsp = await task.ConfigureAwait(false)) {
                string resString = await rsp.Content.ReadAsStringAsync();
                if (rsp.StatusCode < HttpStatusCode.OK || rsp.StatusCode > HttpStatusCode.OK + 9) {
                    var statusCode = rsp.StatusCode;
                    resString = new JObject()
                    {
                        {"HttpStatusCode", (int) statusCode},
                        {"HttpResponse", resString}
                    }.ToString(Formatting.None);
                }
                Log.Debug($"Http Response >>\r\n{resString}");
                return resString;
            }
        }

        private static async Task<string> SafeAsyncCall(this Task<string> t) {
            string ret = "";
            try {
                ret = await t;
            } catch (Exception e) {
                Log.Warn(e);
            } finally {
                if (string.IsNullOrEmpty(ret)) {
                    ret = new JObject()
                    {
                        {"HttpStatusCode", -1},
                        {"HttpResponse", "请求异常,无法连接到远程服务器"}
                    }.ToString(Formatting.None);
                }
            }

            return ret;
        }

        public static Dictionary<string, string> GlobalHeaders = new Dictionary<string, string>();

        // ReSharper disable once UnusedMember.Local
        public static async Task<string> SendAsync(this HttpRequestMessage request, Dictionary<string, string> headers = null) {
            try {
                if (GlobalHeaders != null && GlobalHeaders.Count > 0) {
                    foreach (var kv in GlobalHeaders) {
                        if (request.Headers.Contains(kv.Key)) {
                            request.Headers.Remove(kv.Key);
                        }
                        request.Headers.Add(kv.Key, kv.Value);
                    }
                }
                if (headers != null && headers.Count > 0) {
                    foreach (var kv in headers) {
                        if (request.Headers.Contains(kv.Key)) {
                            request.Headers.Remove(kv.Key);
                        }
                        request.Headers.Add(kv.Key, kv.Value);
                    }
                }

                if (request.Method == HttpMethod.Get || request.Method == HttpMethod.Delete) {
                    Log.Debug($"Http {request.Method} >>{request.RequestUri} ");
                } else {
                    var body = await request.Content.ReadAsStringAsync();
                    Log.Debug($"Http {request.Method} >>{request.RequestUri} \r\n{body}");
                }
            } catch (Exception e) {
                Log.Warn($"http log error:{e.Message}");
            }
            return await GlobalClient.SendAsync(request).AsyncGetHttpResponseString().SafeAsyncCall();
        }

        public static async Task<string> GET(this Uri url, Dictionary<string, string> headers = null) {
            return await new HttpRequestMessage(HttpMethod.Get, url).SendAsync(headers);
        }

        public static async Task<string> GET(this string url, Dictionary<string, string> headers = null) {
            return await new HttpRequestMessage(HttpMethod.Get, url).SendAsync(headers);
        }

        public static async Task<TRet> GET<TRet>(this Uri url, Dictionary<string, string> headers = null) {
            return JsonConvert.DeserializeObject<TRet>(await GET(url, headers));
        }

        public static async Task<TRet> GET<TRet>(this string url, Dictionary<string, string> headers = null) {
            return JsonConvert.DeserializeObject<TRet>(await GET(url, headers));
        }

        public static async Task<string> POST(this Uri url, string bodyData, Dictionary<string, string> headers = null, string contentType = null) {
            return await new HttpRequestMessage(HttpMethod.Post, url) { Content = bodyData.ToHttpJsonStringContent(contentType) }.SendAsync(headers);
        }

        public static async Task<string> POST(this string url, string bodyData, Dictionary<string, string> headers = null, string contentType = null) {
            return await new HttpRequestMessage(HttpMethod.Post, url) { Content = bodyData.ToHttpJsonStringContent(contentType) }.SendAsync(headers);
        }

        public static async Task<string> POST(this Uri url, HttpContent bodyData, Dictionary<string, string> headers = null) {
            return await new HttpRequestMessage(HttpMethod.Post, url) { Content = bodyData, Version = HttpVersion.Version10 }.SendAsync(headers);
        }

        public static async Task<string> POST(this string url, HttpContent bodyData, Dictionary<string, string> headers = null) {
            return await new HttpRequestMessage(HttpMethod.Post, url) { Content = bodyData, Version = HttpVersion.Version10 }.SendAsync(headers);
        }

        public static async Task<TRet> POST<TRet>(this Uri url, string reqJsonStr, Dictionary<string, string> headers = null, string contentType = ContentTypeJson) {
            return JsonConvert.DeserializeObject<TRet>(await POST(url, reqJsonStr, headers, contentType));
        }

        public static async Task<TRet> POST<TRet>(this string url, string body, Dictionary<string, string> headers = null, string contentType = ContentTypeJson) {
            return JsonConvert.DeserializeObject<TRet>(await POST(url, body, headers, contentType));
        }

        public static async Task<TRet> POST<TRet>(this Uri url, HttpContent formData, Dictionary<string, string> headers = null) {
            return JsonConvert.DeserializeObject<TRet>(await POST(url, formData, headers));
        }

        public static async Task<TRet> POST<TRet>(this Uri url, object bodyData, Dictionary<string, string> headers = null) {
            return await url.POST<TRet>(bodyData.ToJsonString(), headers);
        }

        public static async Task<TRet> POST<TRet>(this string url, object bodyData, Dictionary<string, string> headers = null) {
            return await url.POST<TRet>(bodyData.ToJsonString(), headers);
        }

        public static async Task<string> PUT(this Uri url, string bodyData, Dictionary<string, string> headers = null, string contentType = ContentTypeJson) {
            return await new HttpRequestMessage(HttpMethod.Put, url) { Content = bodyData.ToHttpJsonStringContent(contentType) }.SendAsync(headers);
        }

        public static async Task<string> PUT(this string url, string bodyData, Dictionary<string, string> headers = null, string contentType = ContentTypeJson) {
            return await new HttpRequestMessage(HttpMethod.Put, url) { Content = bodyData.ToHttpJsonStringContent(contentType) }.SendAsync(headers);
        }

        public static async Task<TRet> PUT<TRet>(this Uri url, string reqJsonStr, Dictionary<string, string> headers = null) {
            return JsonConvert.DeserializeObject<TRet>(await PUT(url, reqJsonStr, headers));
        }

        public static async Task<TRet> PUT<TRet>(this string url, string reqJsonStr, Dictionary<string, string> headers = null) {
            return JsonConvert.DeserializeObject<TRet>(await PUT(url, reqJsonStr, headers));
        }

        public static async Task<TRet> PUT<TRet>(this Uri url, object bodyData, Dictionary<string, string> headers = null) {
            return await url.PUT<TRet>(bodyData.ToJsonString(), headers);
        }

        public static async Task<TRet> PUT<TRet>(this string url, object bodyData, Dictionary<string, string> headers = null) {
            return await url.PUT<TRet>(bodyData.ToJsonString(), headers);
        }

        public static async Task<string> DELETE(this Uri url, Dictionary<string, string> headers = null) {
            return await new HttpRequestMessage(HttpMethod.Delete, url).SendAsync(headers);
        }

        public static async Task<string> DELETE(this string url, Dictionary<string, string> headers = null) {
            return await new HttpRequestMessage(HttpMethod.Delete, url).SendAsync(headers);
        }

        public static async Task<TRet> DELETE<TRet>(this Uri url, Dictionary<string, string> headers = null) {
            return JsonConvert.DeserializeObject<TRet>(await DELETE(url, headers));
        }

        public static async Task<TRet> DELETE<TRet>(this string url, Dictionary<string, string> headers = null) {
            return JsonConvert.DeserializeObject<TRet>(await DELETE(url, headers));
        }

    }
}