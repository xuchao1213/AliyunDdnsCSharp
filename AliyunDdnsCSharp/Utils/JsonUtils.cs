/*--------------------------------------------------------
* 
* File: JsonUtils
* Author: Xu Chao
* Email: xuchao_1213@163.com
* Created: 2018-10-31 21:56:21
* Desc: Json工具 
* 
* -------------------------------------------------------*/
using System;
using Newtonsoft.Json;
using NLog;

namespace AliyunDdnsCSharp.Utils
{
    public static class JsonUtils
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static bool TryDeserializeJsonStr<T>(this string jsonStr, out T data) {
            data = default(T);
            try
            {
                data = JsonConvert.DeserializeObject<T>(jsonStr);
                return true;
            } catch (Exception e)
            {
                Log.Warn($"Try deserialize json string error: {e.Message}");
                return false;
            }
        }

        public static string ToJsonString(this object obj, Formatting formatting = Formatting.None) {
            if (obj == null)
            {
                return "{}";
            }
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = formatting
            });
        }
    }
}
