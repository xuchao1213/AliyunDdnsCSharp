using System.ServiceProcess;
using AliyunDdnsCSharp.Core;
using NLog;

namespace AliyunDdnsCSharp.Services
{
    partial class DdnsService : ServiceBase {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public DdnsService() {
            InitializeComponent();
        }
        ///// <summary>
        ///// 获得外网IP地址
        ///// </summary>
        ///// <returns>IP地址</returns>
        //private string GetInternetIp() {
        //    string ip = string.Empty;
        //    try {
        //        using (WebClient webClient = new WebClient()) {
        //            webClient.Encoding = Encoding.UTF8;
        //            var content = webClient.DownloadString("http://ip.qq.com/"); //获得IP的网页
        //             //判断IP是否合法
        //            Regex r = new Regex("((25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d|\\d)\\.){3}(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d|[1-9])", RegexOptions.None);
        //            Match mc = r.Match(content);
        //            if (mc.Success)
        //            {
        //                //获取匹配到的IP
        //                ip = mc.Groups[0].Value;
        //            }
        //        }
        //    } catch (Exception ex) {
        //       Log.Error("Get Internet Ip Error : "+ex.Message);
        //        ip = "";
        //    }
        //    return ip;
        //}
        ////定时执行事件
        //private void TimedEvent(object sender, System.Timers.ElapsedEventArgs e) {
        //    string currentInternetIp =GetInternetIP();
        //    if (currentInternetIp.Length == 0) {
        //        Log.Info("Can't Get Current Internet Ip ...... 【Skip】");
        //        return;
        //    }
        //    IClientProfile clientProfile = DefaultProfile.GetProfile("cn-hangzhou", accessKeyId_, accessKeySecret_);
        //    DefaultAcsClient client = new DefaultAcsClient();
        //    DescribeDomainRecordsRequest reqFetch = new DescribeDomainRecordsRequest();
        //    reqFetch.AcceptFormat = Aliyun.Acs.Core.Http.FormatType.JSON;
        //    reqFetch.DomainName = domainName_;

        //    string dnsIp = "",recordId="";
        //    try {
        //        DescribeDomainRecordsResponse resFetch = client.GetAcsResponse(reqFetch);
        //        foreach (var r in resFetch.DomainRecords) {
        //            if (r.RR == subDomainName_) {
        //                dnsIp=r.Value;
        //                recordId = r.RecordId;
        //            }
        //        }
        //    } catch (ServerException ex) {
        //        Log.Error("Server Error >> code : " + ex.ErrorCode + " | Error Message : " + ex.ErrorMessage);
        //    } catch (ClientException ex) {
        //        Log.Error("Client Error >> code : " + ex.ErrorCode + " | Error Message : " + ex.ErrorMessage);
        //    }
        //    if (dnsIp.Length == 0) {
        //        Log.Info("Can't Get Dns Record , Add New Record .");
        //        AddDomainRecordRequest reqAdd = new AddDomainRecordRequest();
        //        reqAdd.AcceptFormat = Aliyun.Acs.Core.Http.FormatType.JSON;
        //        reqAdd.DomainName = domainName_;
        //        reqAdd.RR = subDomainName_;
        //        reqAdd.Type = "A";
        //        reqAdd.Value = currentInternetIp;
        //        try {
        //            //添加解析记录
        //            AddDomainRecordResponse resAdd = client.GetAcsResponse(reqAdd);
        //            Log.Info("\r\n Dns Record Add ...... 【OK】 \r\n New Dns Record Is : " + currentInternetIp);
        //        } catch (ServerException ex) {
        //            Log.Error("Dns Record Add ...... 【Error】 \r\n Server Error >> code : " + ex.ErrorCode + " | Error Message : " + ex.ErrorMessage);
        //        } catch (ClientException ex) {
        //            Log.Error("Dns Record Add ...... 【Error】 \r\n Client Error >> code : " + ex.ErrorCode + " | Error Message : " + ex.ErrorMessage);
        //        }
        //    } else {
        //        if (currentInternetIp == dnsIp) {
        //            Log.Info("Current Internet Ip Is : " + currentInternetIp + " ,Same As Dns Record ...... 【Skip】");
        //        } else {
        //            //更新记录
        //            UpdateDomainRecordRequest reqUpdate = new UpdateDomainRecordRequest();
        //            reqUpdate.AcceptFormat = Aliyun.Acs.Core.Http.FormatType.JSON;
        //            reqUpdate.RecordId = recordId;
        //            reqUpdate.RR = subDomainName_;
        //            reqUpdate.Type = "A";
        //            reqUpdate.Value = currentInternetIp;
        //            try {
        //                //更新解析记录
        //                UpdateDomainRecordResponse resUpdate = client.GetAcsResponse(reqUpdate);
        //                Log.Info("\r\n Update Dns  Record ...... 【OK】 \r\n New Dns Record Is : " + currentInternetIp);
        //            } catch (ServerException ex) {
        //                Log.Error("Dns Record Update ...... 【Error】 \r\n Server Error >> code : " + ex.ErrorCode + " | Error Message : " + ex.ErrorMessage);
        //            } catch (ClientException ex) {
        //                Log.Error("Dns Record Update ...... 【Error】 \r\n Client Error >> code : " + ex.ErrorCode + " | Error Message : " + ex.ErrorMessage);
        //            }
                   
        //        }
        //    }
        //}
        protected override void OnStart(string[] args) {
            base.OnStart(args);
            Log.Info($"{SysConst.SERVICE_NAME}({SysConst.SERVICE_DESCRIPTION}) service running");
            WorkerManager.GetInstance().BootStrapAsync();
        }

        protected override void OnStop() {
            base.OnStop();
            WorkerManager.GetInstance().Stop();
            Log.Info($"{SysConst.SERVICE_NAME}({SysConst.SERVICE_DESCRIPTION}) service stopped");
        }
        protected override void OnShutdown() {
            base.OnShutdown();
            Log.Info($"{SysConst.SERVICE_NAME}({SysConst.SERVICE_DESCRIPTION}) service stopped cause of power off");
        }
    }
}
