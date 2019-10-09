using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private readonly Dictionary<string, TimerWorker> workers;
        private readonly ManualResetEvent runEvent = new ManualResetEvent(false);

        #region Singleton
        private static readonly Once MgrOnce = new Once();
        private static WorkerManager instance;
        private string confDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SysConst.CONF_DIR);
        private readonly FileSystemWatcher confDirWatcher;
        private WorkerManager() {
            workers = new Dictionary<string, TimerWorker>();
            //conf dir watcher
            confDirWatcher = new FileSystemWatcher { Path = confDir };
            confDirWatcher.NotifyFilter = NotifyFilters.FileName
                                   | NotifyFilters.DirectoryName
                                   | NotifyFilters.Size
                                   | NotifyFilters.LastWrite
                                   | NotifyFilters.LastAccess
                                   | NotifyFilters.CreationTime;
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
            WorkerScan();
            confDirWatcher.Changed += OnConfDirChanged;
            confDirWatcher.Created += OnConfCreated;
            confDirWatcher.Deleted += OnConfDeleted; ;
            //confDirWatcher.Renamed += OnConfDirChanged;
            // Begin watching.
            confDirWatcher.EnableRaisingEvents = true;
        }

        private void OnConfDeleted(object sender, FileSystemEventArgs e)
        {
            string file = e.FullPath ?? "";
            if( string.IsNullOrWhiteSpace(file)){ return;}

            string key = "";
            foreach (var kv in workers)
            {
                if (kv.Value.Conf.File == file)
                {
                    key = kv.Key;
                    kv.Value.Stop();
                    break;
                }
            }
            if (!string.IsNullOrWhiteSpace(key))
            {
                workers.Remove(key);
            }
        }

        private void OnConfCreated(object sender, FileSystemEventArgs e)
        {
            string file = e.FullPath ?? "";
            if (string.IsNullOrWhiteSpace(file)) { return; }
            int count = 0;
            while (!file.IsFileReady())
            {
                if (!File.Exists(file))
                {
                    return;
                }
                Thread.Sleep(100);
                if (++count >= 50)
                {
                    return;
                }
            }
            WorkerConf conf=new WorkerConf();
            if (!conf.LoadFromFile(file))
            {
                return;
            }
            if (workers.ContainsKey(conf.Name))
            {
                Log.Debug($"{conf.Name} already exist ,skip !");
                return;
            }
            var worker = new TimerWorker(conf);
            workers.Add(conf.Name, worker);
            worker.Run();
        }

        private void OnConfDirChanged(object sender, FileSystemEventArgs e)
        {
            Log.Debug($"File: {e.FullPath} {e.ChangeType}");
            string file = e.FullPath ?? "";
            if (string.IsNullOrWhiteSpace(file)) { return; }

            foreach (var worker in workers.Values)
            {
                if (worker.Conf.File == file)
                {
                    if (!worker.Conf.Reload())
                    {
                        Log.Warn($"reload :{file} failed");
                    }
                    break;
                }
            }
        }

        public void Stop()
        {
            foreach (var worker in workers)
            {
                worker.Value.Stop();
            }
            workers.Clear();
            runEvent.Set();
            // Begin watching.
            confDirWatcher.EnableRaisingEvents = false;
            confDirWatcher.Changed -= OnConfDirChanged;
            confDirWatcher.Created -= OnConfCreated;
            confDirWatcher.Deleted -= OnConfDeleted; ;
        }

        /// <summary>
        ///  读取配置文件，支持动态配置
        /// </summary>
        private void WorkerScan()
        {
            if (Directory.Exists(confDir))
            {
                try
                {
                    var files = Directory.GetFiles(confDir);
                    foreach (var file in files)
                    {
                       var conf=new WorkerConf();
                       if (conf.LoadFromFile(file))
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
        }
    }
}
