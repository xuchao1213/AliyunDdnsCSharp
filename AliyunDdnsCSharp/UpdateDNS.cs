using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Alidns.Model.V20150109;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace AliyunDdnsCSharp {
    partial class UpdateDNS : ServiceBase {
        private static Logger logger = LogManager.GetCurrentClassLogger();//日志类
        private const bool Debug_ = false;
        private static string APP_PATH = AppDomain.CurrentDomain.BaseDirectory;
        private static string CONFG_FILE = "config.txt";
        private static int timeSpan_ = 30;//刷新间隔(Min)
        private static string accessKeyId_ = "";//access key id
        private static string accessKeySecret_ = "";//access key secret
        private static string domainName_ = "";//domain name_
        private static string subDomainName_ = ""; //sub domain name
        private bool configExist_ = false;
        private static System.Timers.Timer timer_;

        public UpdateDNS() {
            InitializeComponent();
            timer_ = new System.Timers.Timer();
            timer_.Elapsed += new System.Timers.ElapsedEventHandler(TimedEvent);
            timer_.Interval = 10 * 1000;//没加载配置文件的情况下每10秒重试一次
            timer_.Enabled = true;
        }
        /// <summary>
        /// 获得外网IP地址
        /// </summary>
        /// <returns>IP地址</returns>
        private string GetInternetIP() {
            string ip = string.Empty;
            try {
                using (WebClient webClient = new WebClient()) {
                    webClient.Encoding = Encoding.UTF8;
                    var content = webClient.DownloadString("http://ip.qq.com/"); //获得IP的网页
                     //判断IP是否合法
                    Regex r = new Regex("((25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d|\\d)\\.){3}(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d|[1-9])", RegexOptions.None);
                    Match mc = r.Match(content);
                    //获取匹配到的IP
                    ip = mc.Groups[0].Value;
                    //  ip = new Regex(@"\[((\d{1,3}\.){3}\d{1,3})\]").Match(content).Groups[1].Value;
                }
            } catch (Exception ex) {
               logger.Error("Get Internet Ip Error : "+ex.Message);
                ip = "";
            }
            return ip;
        }
        private void GetConfig() {
            configExist_ = false;
            string configFile = Path.Combine(APP_PATH, CONFG_FILE);
            if (!File.Exists(configFile)) {
                logger.Error(CONFG_FILE + " Not Exist ...... 【Skip】");
                return;
            }
            try {
                var configs = File.ReadAllLines(configFile);
                foreach (string str in configs) {
                    //跳过空行
                    if (str == null || str.Trim().Length == 0) {
                        continue;
                    }
                    //跳过注释行
                    if (str.Trim().StartsWith("#")) {
                        continue;
                    }
                    string[] kv = str.Split('=');
                    //跳过不合法行
                    if (kv.Length != 2) {
                        continue;
                    }
                    if ("TimeSpan" == kv[0].Trim()) {
                        int.TryParse(kv[1].Trim(), out timeSpan_);
                    }
                    if ("AccessKeyId" == kv[0].Trim()) {
                        accessKeyId_ = kv[1].Trim();
                    }
                    if ("AccessKeySecret" == kv[0].Trim()) {
                        accessKeySecret_ = kv[1].Trim();
                    }
                    if ("DomainName" == kv[0].Trim()) {
                        domainName_ = kv[1].Trim();
                    }
                    if ("SubDomainName" == kv[0].Trim()) {
                        subDomainName_ = kv[1].Trim();
                    }
                }
            } catch (Exception ex) {
                logger.Error("Get Config Error : " + ex.Message);
            }
            if (accessKeyId_.Length == 0 || accessKeySecret_.Length == 0 || domainName_.Length == 0 || subDomainName_.Length == 0) {
                logger.Error("Invalid Config ...... 【Skip】");
                return;
            }
            timer_.Interval = timeSpan_ * 1000 * 60;
            if (Debug_) {
                logger.Info("\r\n config :\r\n AccessKeyId = " + accessKeyId_
                    + "\r\n AccessKeySecret = " + accessKeySecret_
                    + "\r\n DomainName = " + domainName_
                    + "\r\n SubDomainName = " + subDomainName_);
            }
            configExist_ = true;
        }
        //定时执行事件
        private void TimedEvent(object sender, System.Timers.ElapsedEventArgs e) {
            //每次都重新获取配置（可以动态修改而不用重启服务）
            GetConfig();
            if (!configExist_) {
                logger.Error(" Not Config Yet ...... 【Skip】");
                return;
            }
            string currentInternetIp =GetInternetIP();
            if (currentInternetIp.Length == 0) {
                logger.Info("Can't Get Current Internet Ip ...... 【Skip】");
                return;
            }
            IClientProfile clientProfile = DefaultProfile.GetProfile("cn-hangzhou", accessKeyId_, accessKeySecret_);
            DefaultAcsClient client = new DefaultAcsClient();
            DescribeDomainRecordsRequest reqFetch = new DescribeDomainRecordsRequest();
            reqFetch.AcceptFormat = Aliyun.Acs.Core.Http.FormatType.JSON;
            reqFetch.DomainName = domainName_;

            string dnsIp = "",recordId="";
            try {
                DescribeDomainRecordsResponse resFetch = client.GetAcsResponse(reqFetch);
                foreach (var r in resFetch.DomainRecords) {
                    if (r.RR == subDomainName_) {
                        dnsIp=r.Value;
                        recordId = r.RecordId;
                    }
                }
            } catch (ServerException ex) {
                logger.Error("Server Error >> code : " + ex.ErrorCode + " | Error Message : " + ex.ErrorMessage);
            } catch (ClientException ex) {
                logger.Error("Client Error >> code : " + ex.ErrorCode + " | Error Message : " + ex.ErrorMessage);
            }
            if (dnsIp.Length == 0) {
                logger.Info("Can't Get Dns Record , Add New Record .");
                AddDomainRecordRequest reqAdd = new AddDomainRecordRequest();
                reqAdd.AcceptFormat = Aliyun.Acs.Core.Http.FormatType.JSON;
                reqAdd.DomainName = domainName_;
                reqAdd.RR = subDomainName_;
                reqAdd.Type = "A";
                reqAdd.Value = currentInternetIp;
                try {
                    //添加解析记录
                    AddDomainRecordResponse resAdd = client.GetAcsResponse(reqAdd);
                    logger.Info("\r\n Dns Record Add ...... 【OK】 \r\n New Dns Record Is : " + currentInternetIp);
                } catch (ServerException ex) {
                    logger.Error("Dns Record Add ...... 【Error】 \r\n Server Error >> code : " + ex.ErrorCode + " | Error Message : " + ex.ErrorMessage);
                } catch (ClientException ex) {
                    logger.Error("Dns Record Add ...... 【Error】 \r\n Client Error >> code : " + ex.ErrorCode + " | Error Message : " + ex.ErrorMessage);
                }
            } else {
                if (currentInternetIp == dnsIp) {
                    logger.Info("Current Internet Ip Is : " + currentInternetIp + " ,Same As Dns Record ...... 【Skip】");
                } else {
                    //更新记录
                    UpdateDomainRecordRequest reqUpdate = new UpdateDomainRecordRequest();
                    reqUpdate.AcceptFormat = Aliyun.Acs.Core.Http.FormatType.JSON;
                    reqUpdate.RecordId = recordId;
                    reqUpdate.RR = subDomainName_;
                    reqUpdate.Type = "A";
                    reqUpdate.Value = currentInternetIp;
                    try {
                        //更新解析记录
                        UpdateDomainRecordResponse resUpdate = client.GetAcsResponse(reqUpdate);
                        logger.Info("\r\n Update Dns  Record ...... 【OK】 \r\n New Dns Record Is : " + currentInternetIp);
                    } catch (ServerException ex) {
                        logger.Error("Dns Record Update ...... 【Error】 \r\n Server Error >> code : " + ex.ErrorCode + " | Error Message : " + ex.ErrorMessage);
                    } catch (ClientException ex) {
                        logger.Error("Dns Record Update ...... 【Error】 \r\n Client Error >> code : " + ex.ErrorCode + " | Error Message : " + ex.ErrorMessage);
                    }
                   
                }
            }
        }
        protected override void OnStart(string[] args) {
            // TODO: 在此处添加代码以启动服务。
            logger.Info("Update DNS Service Start Running ......【服务启动】");
        }

        protected override void OnStop() {
            // TODO: 在此处添加代码以执行停止服务所需的关闭操作。
            logger.Info("Update DNS Service Stopped ...... 【服务停止】");
        }
        protected override void OnShutdown() {
            logger.Info("Update DNS Service Stopped ...... 【计算机关闭】");
        }
    }
}
