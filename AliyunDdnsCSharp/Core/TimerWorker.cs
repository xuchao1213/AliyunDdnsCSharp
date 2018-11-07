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
        public bool IsWorking => Interlocked.Read(ref runFlag) == 1;

        public TimerWorker(WorkerConf workerConf)
        {
            conf = workerConf;
            timer = new Timer();
            timer.Elapsed += Work;
            timer.Interval = 2000;
        }
        private async void Work(object sender, ElapsedEventArgs e)
        {
            if (conf == null) { return;}
            timer.Interval = conf.Interval * 30 * 1000;
            if (Interlocked.Read(ref runFlag) == 0)
            {
                return;
            }
            Log.Info($"[{Name}] do work ...");
            //获取本机公网IP
            var getRes = await GET_IP_URL.Get();
            if (!getRes.Ok)
            {
                return;
            }
            Match mc = IpRegex.Match(getRes.HttpResponseString);
            if (!mc.Success)
            {
                Log.Info($"[{Name}] fetch real internet ip fail( parser res error ), skip");
                return;
            }
            string realIp = mc.Groups[0].Value;
            Log.Info($"real ip : {realIp}");
            //获取阿里云记录
            var describeRes = await new DescribeDomainRecordsRequest(conf.AccessKeyId, conf.AccessKeySecret)
            {
                DomainName = conf.DomainName,
                RRKeyWord = conf.SubDomainName,
                TypeKeyWord = "A",
            }.Execute();
            if (describeRes.HasError)
            {
                Log.Info($"[{Name}] describe domain records fail ( {describeRes.Message} ) , skip");
            }
            //未查到记录，添加
            if (describeRes.TotalCount == 0)
            {
                //add
                Log.Info($"[{Name}] prepare to add domain record ...");
                var addRes = await new AddDomainRecordRequest(
                    conf.AccessKeyId, conf.AccessKeySecret)
                {
                    DomainName = conf.DomainName,
                    RR = conf.SubDomainName,
                    Type = "A",
                    Value = realIp,
                }.Execute();
                Log.Info(addRes.HasError
                    ? $"[{Name}] add domain record fail ( {addRes.Message} ) , skip"
                    : $"[{Name}] add domain record ok , now  record value is {realIp}");
            }
            else
            {
                foreach (var record in describeRes.DomainRecords.Records)
                {
                    if (record.Value == realIp)
                    {
                        Log.Info($"[{Name}] ip not chanage , skip");
                        continue;
                    }
                    //理论上只会有一条，多条记录时，只更新一条，
                    //update
                    Log.Info($"[{Name}] prepare to update domain record ...");
                    var updateRes = await new UpdateDomainRecordRequest(
                        conf.AccessKeyId, conf.AccessKeySecret) {
                        RecordId = record.RecordId,
                        RR = conf.SubDomainName,
                        Type = "A",
                        Value = realIp,
                    }.Execute();
                    Log.Info(updateRes.HasError
                        ? $"[{Name}] update domain record fail ( {updateRes.Message} ) , skip"
                        : $"[{Name}] update domain record ok , now  record value is {realIp}");
                }
            }
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
