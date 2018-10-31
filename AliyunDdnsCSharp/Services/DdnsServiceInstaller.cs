using System.Collections;
using System.ComponentModel;
using System.ServiceProcess;
using NLog;

namespace AliyunDdnsCSharp.Services {
    [RunInstaller(true)]
    public partial class DdnsServiceInstaller : System.Configuration.Install.Installer {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public DdnsServiceInstaller()
        {
            InitializeComponent();
            ServiceProcessInstaller spi = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };
            ServiceInstaller si = new ServiceInstaller
            {
                ServiceName = SysConst.SERVICE_NAME,
                DisplayName = SysConst.SERVICE_DISPLAY_NAME,
                Description = SysConst.SERVICE_DESCRIPTION,
                StartType = ServiceStartMode.Automatic
            };
            Installers.Add(si);
            Installers.Add(spi);
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            Log.Debug($"{SysConst.SERVICE_NAME} ( {SysConst.SERVICE_DESCRIPTION} ) installing ...");
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            Log.Debug($"{SysConst.SERVICE_NAME} ( {SysConst.SERVICE_DESCRIPTION} ) uninstalling ...");
        }
    }
}
