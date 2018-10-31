using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using AliyunDdnsCSharp.Model;
using AliyunDdnsCSharp.Utils;
using NLog;
using Timer = System.Timers.Timer;

namespace AliyunDdnsCSharp.Core
{
    public class TimerWorker:IDisposable
    {
        private static  readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const string GET_IP_URL = "http://ip.qq.com/";

        private const string IP_REGEX =
            "((25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d|\\d)\\.){3}(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d|[1-9])";
        private readonly WorkerConf conf;
        private static readonly Regex IpRegex=new Regex(IP_REGEX);
        private readonly Timer timer;
        private long runFlag;
        private string Name => conf.Name;
        public bool IsWorking { get; set; }

        public TimerWorker(WorkerConf workerConf)
        {
            conf = workerConf;
            timer = new Timer();
            timer.Elapsed += Work;
            timer.Interval = conf.Interval * 1000 * 60;
        }

        private async void Work(object sender, ElapsedEventArgs e)
        {
            if (Interlocked.Read(ref runFlag) == 0)
            {
                return;
            }
            Log.Info($"{Name} worker do work ...");
            //获取本机公网IP
            string content = await GET_IP_URL.SendHttpGet<string>();
            Match mc = IpRegex.Match(content);
            if (!mc.Success)
            {
                Log.Info($"{Name} worker do work fail ( fetch real internet ip error ) , skip");
                return;
            }
            string realIp = mc.Groups[0].Value;
            //获取阿里云记录
            var res= await new AddDomainRecordRequest("")
                .AccessKeySecret("")
                .Build()
                .Execute();
        }

        public void Run()
        {
            if (Interlocked.CompareExchange(ref runFlag, 1, 0) == 0)
            {
                Debug.Assert(conf != null);
                Log.Debug($"{Name} worker running ...");
                timer.Start();
            }
        }

        public void Stop()
        {
            Log.Debug($"{Name} worker stopping ...");
            Interlocked.Exchange(ref runFlag, 0);
            timer.Stop();
            Log.Debug($"worker [ {Name} ]  stopped");
        }

        #region IDisposable Support
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                   timer.Dispose();
                }
                disposedValue = true;
            }
        }

        ~TimerWorker()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
             GC.SuppressFinalize(this);
        }
        #endregion
    }
}
