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
        //merge from https://github.com/stfei/AliyunDdnsCSharp 
        //to fix https://github.com/xuchao1213/AliyunDdnsCSharp/issues/13
        private const string IPV4_REGEX =
             @"((?:(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))";
        private const string IPV6_REGEX =
            @"^\s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:)))(%.+)?\s*$";
        private const string DEFAULT_IP_V4_URL = "http://ip.hiyun.me";
        public WorkerConf Conf { get; private set; }
        private static readonly Regex IpV4Regex = new Regex(IPV4_REGEX);
        private static readonly Regex IpV6Regex = new Regex(IPV6_REGEX);
        private readonly Timer timer;
        private long runFlag;
        public string Name => Conf.Name;
        public bool IsWorking => Interlocked.Read(ref runFlag) == 1;

        public TimerWorker(WorkerConf workerConf)
        {
            Conf = workerConf;
            timer = new Timer();
            timer.Elapsed += Work;
            timer.Interval = 2000;
        }
        private async void Work(object sender, ElapsedEventArgs e)
        {
            if (Conf == null) { return; }
            timer.Interval = Conf.Interval * 60 * 1000;
            if (Interlocked.Read(ref runFlag) == 0)
            {
                return;
            }
            Log.Info($"[{Name}] do work ...");
            try
            {
                //获取本机公网IP
                string realIp = "";
                if (Conf.GetIpUrls.Count == 0 && Conf.IsIpV6)
                {
                    //IPCONFIG
                    realIp = NetWorkUtils.GetLocalIpV6Address();
                }

                if (Conf.GetIpUrls.Count == 0 && !Conf.IsIpV6)
                {
                    Conf.GetIpUrls.Add(DEFAULT_IP_V4_URL);
                }

                if (Conf.GetIpUrls.Count > 0)
                {
                    foreach (string url in Conf.GetIpUrls)
                    {
                        var getRes = await url.Get();
                        if (!getRes.Ok)
                        {
                            Log.Info($"[{Name}] fetch real internet ip from {url} fail , try next url");
                            continue;
                        }

                        Match mc;
                        //提取IPV6地址
                        if (Conf.IsIpV6)
                        {
                            mc = IpV6Regex.Match(getRes.HttpResponseString);
                        }
                        //提取IPV4地址
                        else
                        {
                            mc = IpV4Regex.Match(getRes.HttpResponseString);
                        }

                        if (mc.Success && mc.Groups.Count > 0)
                        {
                            realIp = mc.Groups[0].Value;
                            Log.Info(
                                $"[{Name}] fetch real internet ip from ( {url} ) success, current ip is ( {realIp} )");
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
                if (Conf.IsIpV6 && !realIp.IsIpV6Address())
                {
                    Log.Info($"[{Name}] fetch real internet ip [{realIp}] is not a valid ipv6 address, skip");
                    return;
                }

                //获取阿里云记录
                var describeRes = await new DescribeDomainRecordsRequest(Conf.AccessKeyId, Conf.AccessKeySecret)
                {
                    DomainName = Conf.DomainName,
                    RRKeyWord = Conf.SubDomainName,
                    TypeKeyWord = Conf.Type,
                }.Execute();
                if (describeRes.HasError)
                {
                    Log.Info($"[{Name}] describe domain records fail ( {describeRes.Message} ) , skip");
                    return;
                }

                //未查到记录，添加
                if (describeRes.TotalCount == 0)
                {
                    goto ADD;
                }

                foreach (var record in describeRes.DomainRecords.Records)
                {
                    //fix https://github.com/xuchao1213/AliyunDdnsCSharp/issues/3
                    //fix https://github.com/xuchao1213/AliyunDdnsCSharp/issues/11
                    if (string.Compare(record.Type , Conf.Type, StringComparison.OrdinalIgnoreCase) ==0
                        && string.Compare(record.RR, Conf.SubDomainName, StringComparison.OrdinalIgnoreCase)==0)
                    {
                        if (record.Value == realIp)
                        {
                            Log.Info($"[{Name}] ip not chanage , skip");
                            return;
                        }
                        //
                        //update
                        Log.Info($"[{Name}] prepare to update domain record ...");
                        var updateRes = await new UpdateDomainRecordRequest(
                            Conf.AccessKeyId, Conf.AccessKeySecret)
                        {
                            RecordId = record.RecordId,
                            RR = Conf.SubDomainName,
                            Type = Conf.Type,
                            Value = realIp,
                            TTL = Conf.TtlV,
                            Line = Conf.Line,
                        }.Execute();
                        Log.Info(updateRes.HasError
                            ? $"[{Name}] update domain record fail ( {updateRes.Message} ) , skip"
                            : $"[{Name}] update domain record ok , now  record value is {realIp}");
                        if (updateRes.HasError)
                        {
                            return;
                        }

                        //更新成功后，暂停解析->启用解析，以此来解决更新后不立即生效的问题
                        var disableRes = await new SetDomainRecordStatusRequest(
                            Conf.AccessKeyId, Conf.AccessKeySecret)
                        {
                            RecordId = updateRes.RecordId,
                            Enable = false
                        }.Execute();
                        Log.Info(disableRes.HasError
                            ? $"[{Name}] set domain records status to disable error ( {disableRes.Message} ) , skip"
                            : $"[{Name}] set domain records status to disable ok , now enable it");
                        if (disableRes.HasError)
                        {
                            return;
                        }

                        var enableRes = await new SetDomainRecordStatusRequest(
                            Conf.AccessKeyId, Conf.AccessKeySecret)
                        {
                            RecordId = updateRes.RecordId,
                            Enable = true
                        }.Execute();
                        Log.Info(enableRes.HasError
                            ? $"[{Name}] set domain records status to enable error ( {enableRes.Message} ) , skip"
                            : $"[{Name}] set domain records status to enable ok , just enjoy it :)");
                        return;
                    }
                }

                ADD:
                {
                    //add
                    Log.Info($"[{Name}] prepare to add domain record ...");
                    var addRes = await new AddDomainRecordRequest(
                        Conf.AccessKeyId, Conf.AccessKeySecret)
                    {
                        DomainName = Conf.DomainName,
                        RR = Conf.SubDomainName,
                        Type = Conf.Type,
                        Value = realIp,
                        TTL = Conf.TtlV,
                        Line = Conf.Line,
                    }.Execute();
                    Log.Info(addRes.HasError
                        ? $"[{Name}] add domain record fail ( {addRes.Message} ) , skip"
                        : $"[{Name}] add domain record ok , now  record value is {realIp}");
                }
            }
            catch (Exception ex)
            {
                Log.Warn($"[{Name}] do work exception : {ex.Message}");
            }
        }

        public void Run()
        {
            if (Interlocked.CompareExchange(ref runFlag, 1, 0) == 0)
            {
                Debug.Assert(Conf != null);
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
            Dispose(true);
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
