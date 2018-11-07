using System;

namespace AliyunDdnsCSharp.Utils
{
    public class DateTimeUtils
    {
        // ReSharper disable once InconsistentNaming
        private static readonly DateTime Date1970_1_1 = new DateTime(1970, 1, 1);

        /// <summary>
        /// 时间戳转为C#格式时间
        /// </summary>
        /// <param name="timeStampSecond"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public bool TryParse(string timeStampSecond, out DateTime dateTime) {
            dateTime = DateTime.Now;
            if (long.TryParse(timeStampSecond, out long t))
            {
                dateTime = GetTime(t);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 时间戳转为C#格式时间
        /// </summary>
        /// <param name="timeStampSecond">Unix时间戳格式</param>
        /// <returns>C#格式时间</returns>
        public static DateTime GetTime(string timeStampSecond) {
            long timeStamp = long.Parse(timeStampSecond);
            return GetTime(timeStamp);
        }

        public static DateTime GetTime(long timeStampSecond) {
            return Date1970_1_1.AddSeconds(timeStampSecond).ToLocalTime();
        }
        public static string GetTimeStr(long timeStampSecond,string format = "yyyy-MM-dd HH:mm:ss") {
            return GetTime(timeStampSecond).ToString(format);
        }
        /// <summary>
        /// 获取当前时间
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>

        public static string GetTimeStr(string format = "yyyy-MM-dd HH:mm:ss") {
            return DateTime.Now.ToString(format);
        }

        /// <summary>
        /// 获取指定时间的时间戳
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static Int64 GetUnixTimeStamp(string date) {
            if (DateTime.TryParse(date, out DateTime dt))
            {
                return GetUnixTimeStamp(dt);
            }
            return 0;
        }

        /// <summary>
        /// 获取当前时间的Unix时间戳格式
        /// </summary>
        /// <returns>Unix时间戳格式</returns>
        public static Int64 GetUnixTimeStamp(bool millsec = false) {
            return GetUnixTimeStamp(DateTime.Now, millsec);
        }

        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time"> DateTime时间格式</param>
        /// <param name = "millsec" >精确到毫秒</param>
        /// <returns>Unix时间戳格式</returns>
        public static Int64 GetUnixTimeStamp(DateTime time, bool millsec = false) {
            TimeSpan ts = DateTime.UtcNow - Date1970_1_1;
            return millsec ? (Int64)ts.TotalMilliseconds : (Int64)ts.TotalSeconds;
        }
    }
}