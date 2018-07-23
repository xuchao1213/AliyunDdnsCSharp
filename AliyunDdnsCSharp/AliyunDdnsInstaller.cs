using System.Collections;
using System.ComponentModel;
using System.ServiceProcess;

namespace AliyunDdnsCSharp {
    [RunInstaller(true)]
    public partial class AliyunDdnsInstaller : System.Configuration.Install.Installer {
        public AliyunDdnsInstaller() {
            InitializeComponent();
            ServiceProcessInstaller spi = new ServiceProcessInstaller {Account = ServiceAccount.LocalSystem};
            ServiceInstaller si = new ServiceInstaller
            {
                ServiceName = "AliyunDdns",
                DisplayName = "AliyunDdns",
                Description = "采用阿里云SDK实现的DDNS客户端",
                StartType = ServiceStartMode.Automatic
            };
            this.Installers.Add(si);
            this.Installers.Add(spi);
        }
        public override void Install(IDictionary stateSaver) {
            base.Install(stateSaver);
        }

        public override void Uninstall(IDictionary savedState) {
            base.Uninstall(savedState);
        }
    }
}
