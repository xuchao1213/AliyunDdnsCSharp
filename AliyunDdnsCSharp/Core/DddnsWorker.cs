using System;
using System.Diagnostics;
using System.Timers;
using NLog;

namespace AliyunDdnsCSharp.Core
{
    public class DddnsWorker:IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();//日志类
        private readonly EnvConf envConfCtx;
        private readonly Timer timer;
        public bool IsWorking { get; set; }

        private DddnsWorker()
        {
            timer = new Timer();
            timer.Elapsed += Work;
            timer.Interval = envConfCtx.Interval * 1000 * 60;
        }
        public DddnsWorker(EnvConf envConf) : base()
        {
            envConfCtx = envConf;
        }

        private void Work(object sender, ElapsedEventArgs e)
        {
            //获取本机公网IP

            //获取阿里云记录

        }
        public void Run()
        {
            Debug.Assert(envConfCtx!=null);
            logger.Debug($"{envConfCtx.SubDomainName}.{envConfCtx.DomainName} Worker Run ...");
            timer.Start();
        }

        public void Stop()
        {
            Debug.Assert(envConfCtx != null);
            logger.Debug($"{envConfCtx.SubDomainName}.{envConfCtx.DomainName} Worker Stop ...");
            timer.Stop();
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

        ~DddnsWorker()
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
