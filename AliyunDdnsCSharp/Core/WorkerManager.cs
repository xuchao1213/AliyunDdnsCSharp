using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Timers;
using AliyunDdnsCSharp.Std;
using AliyunDdnsCSharp.Utils;
using NLog;
using Timer = System.Timers.Timer;

namespace AliyunDdnsCSharp.Core
{
    public class WorkerManager
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // 未加载配置时每隔10s扫描一次配置文件
        private const int WORKER_SCANNER_INIT_INTERVAL = 10*1000;
        //每隔1分钟扫描一次配置文件
        private const int WORKER_SCANNER_INTERVAL = 60*1000;

        private readonly Dictionary<string, TimerWorker> workers;
        private readonly ManualResetEvent runEvent = new ManualResetEvent(false);
        private Timer workerScannerTimer;

        #region Singleton
        private static readonly Once MgrOnce = new Once();
        private static WorkerManager instance;
        private WorkerManager() {
            workers = new Dictionary<string, TimerWorker>();
        }
        public static WorkerManager GetInstance() {
            MgrOnce.Do(() => {
                instance = new WorkerManager();
            });
            return instance;
        }
        #endregion
        public void BootStrap()
        {
            BootStrapAsync();
            runEvent.WaitOne();
        }
        public void BootStrapAsync() {
            //load conf
            ScheduleWorkerScanner();
        }

        public void Stop()
        {
            foreach (var worker in workers)
            {
                worker.Value.Stop();
            }
            workers.Clear();
            runEvent.Set();
        }

        private void ScheduleWorkerScanner()
        {
            workerScannerTimer = new Timer();
            workerScannerTimer.Elapsed += WorkerScan;
            workerScannerTimer.Interval = 10;
            workerScannerTimer.Start();
        }
        /// <summary>
        /// 定期读取配置文件，支持动态配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkerScan(object sender, ElapsedEventArgs e)
        {
            workerScannerTimer.Interval = 1000000;
            string confDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SysConst.CONF_DIR);
            if (Directory.Exists(confDir))
            {
                try
                {
                    var files = Directory.GetFiles(confDir);
                    foreach (var file in files)
                    {
                        if (file.TryReadAllText(out string content)
                            && content.TryDeserializeJsonStr(out WorkerConf conf)
                            && conf.Validate())
                        {
                            if (workers.ContainsKey(conf.Name))
                            {
                                Log.Debug($"{conf.Name} already exist ,skip !");
                                continue;
                            }
                            var worker = new TimerWorker(conf);
                            workers.Add(conf.Name, worker);
                            worker.Run();
                        }
                    }
                } catch (Exception ex)
                {
                    Log.Warn($"Load conf error : {ex.Message} , skip");
                }
            }
            workerScannerTimer.Interval = workers.Count > 0 ? WORKER_SCANNER_INTERVAL : WORKER_SCANNER_INIT_INTERVAL;
        }
    }
}
