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
