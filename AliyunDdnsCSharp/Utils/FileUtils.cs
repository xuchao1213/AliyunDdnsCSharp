/*--------------------------------------------------------
* 
* File: FileUtils
* Author: Xu Chao
* Email: xuchao_1213@163.com
* Created: 2018-10-31 22:00:34
* Desc: 文件操作
* 
* -------------------------------------------------------*/
using System;
using System.IO;
using NLog;

namespace AliyunDdnsCSharp.Utils
{
    public static class FileUtils
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static bool TryReadAllText(this string path, out string content)
        {
            content = "";
            try
            {
                content = File.ReadAllText(path);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn($"try read all text from {path} error : {ex.Message}");
                return false;
            }
        }
        public static bool IsFileReady(this string filename)
        {
            FileInfo fi = new FileInfo(filename);
            FileStream fs = null;
            try
            {
                fs = fi.Open(FileMode.Open, FileAccess.Read,FileShare.None);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            finally
            {
                fs?.Close();
            }
        }
    }
}
