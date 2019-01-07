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
    public class TimerWorker : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const string IPV4_REGEX =
            @"((25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d|\\d)\\.){3}(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d|[1-9])";
        private const string IPV6_REGEX =
            @"^\s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:)))(%.+)?\s*$";
        private const string DEFAULT_IP_V4_URL = "http://ip.hiyun.me";
        private readonly WorkerConf conf;
        private static readonly Regex IpV4Regex = new Regex(IPV4_REGEX);
        private static readonly Regex IpV6Regex = new Regex(IPV6_REGEX);
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
            if (conf == null) { return; }
            timer.Interval = conf.Interval * 60 * 1000;
            if (Interlocked.Read(ref runFlag) == 0)
            {
                return;
            }
            Log.Info($"[{Name}] do work ...");
            try
            {
                //获取本机公网IP
                string realIp = "";
                if (conf.GetIpUrls.Count == 0 && conf.IsIpV6)
                {
                    //IPCONFIG
                    realIp = NetWorkUtils.GetLocalIpV6Address();
                }
                if (conf.GetIpUrls.Count == 0 && !conf.IsIpV6)
                {
                    conf.GetIpUrls.Add(DEFAULT_IP_V4_URL);
                }
                if(conf.GetIpUrls.Count>0)
                {
                    foreach (string url in conf.GetIpUrls)
                    {
                        var getRes = await url.Get();
                        if (!getRes.Ok)
                        {
                            Log.Info($"[{Name}] fetch real internet ip from {url} fail , try next url");
                            continue;
                        }
                        Match mc;
                        //提取IPV6地址
                        if (conf.IsIpV6)
                        {
                            mc = IpV6Regex.Match(getRes.HttpResponseString);
                        }
                        //提取IPV4地址
                        else
                        {
                            mc = IpV4Regex.Match(getRes.HttpResponseString);
                        }
                        if (mc.Success && mc.Groups.Count>0)
                        {
                            realIp = mc.Groups[0].Value;
                            Log.Info($"[{Name}] fetch real internet ip from ( {url} ) success, current ip is ( {realIp} )");
                            break;
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(realIp))
                {
                    Log.Info($"[{Name}] fetch real internet ip all failed, skip");
                    return;
                }
                // double check
                if (!realIp.IsIpAddress())
                {
                    Log.Info($"[{Name}] fetch real internet ip [{realIp}] is not a valid ip address, skip");
                    return;
                }
                //double check
                if (conf.IsIpV6 && !realIp.IsIpV6Address())
                {
                    Log.Info($"[{Name}] fetch real internet ip [{realIp}] is not a valid ipv6 address, skip");
                    return;
                }
                //获取阿里云记录
                var describeRes = await new DescribeDomainRecordsRequest(conf.AccessKeyId, conf.AccessKeySecret)
                {
                    DomainName = conf.DomainName,
                    RRKeyWord = conf.SubDomainName,
                    TypeKeyWord = conf.Type,
                }.Execute();
                if (describeRes.HasError)
                {
                    Log.Info($"[{Name}] describe domain records fail ( {describeRes.Message} ) , skip");
                    return;
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
                        Type = conf.Type,
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
                            conf.AccessKeyId, conf.AccessKeySecret)
                        {
                            RecordId = record.RecordId,
                            RR = conf.SubDomainName,
                            Type = conf.Type,
                            Value = realIp,
                        }.Execute();
                        Log.Info(updateRes.HasError
                            ? $"[{Name}] update domain record fail ( {updateRes.Message} ) , skip"
                            : $"[{Name}] update domain record ok , now  record value is {realIp}");
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Warn($"[{ Name}] do work exception : {ex.Message}");
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
