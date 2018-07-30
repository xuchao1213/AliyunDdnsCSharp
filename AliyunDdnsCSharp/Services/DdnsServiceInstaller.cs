using System.Collections;
using System.ComponentModel;
using System.ServiceProcess;
using NLog;

namespace AliyunDdnsCSharp.Services {
    [RunInstaller(true)]
    public partial class DdnsServiceInstaller : System.Configuration.Install.Installer {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public DdnsServiceInstaller()
        {
            InitializeComponent();
            ServiceProcessInstaller spi = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };
            ServiceInstaller si = new ServiceInstaller
            {
                ServiceName = Configuration.GetIns().ServiceName,
                DisplayName = Configuration.GetIns().ServiceDisplayName,
                Description = Configuration.GetIns().ServiceDescription,
                StartType = ServiceStartMode.Automatic
            };
            Installers.Add(si);
            Installers.Add(spi);
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            logger.Debug($"{Configuration.GetIns().ServiceName} ( {Configuration.GetIns().ServiceDescription} )【安装服务】");
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            logger.Debug($"{Configuration.GetIns().ServiceName} ( {Configuration.GetIns().ServiceDescription} )【卸载服务】");
        }
    }
}
