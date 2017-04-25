using NLog;
using System;
using System.Configuration.Install;
using System.ServiceProcess;

namespace AliyunDdnsCSharp {
    static class Program {
        private static Logger logger = LogManager.GetCurrentClassLogger();//日志类
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            // 运行服务
            if (args.Length == 0) {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { new UpdateDNS() };
                ServiceBase.Run(ServicesToRun);
            }
        // 安装服务
        else if (args[0].ToLower() == "/i" || args[0].ToLower() == "-i") {
                try {
                    string[] cmdline = { };
                    string serviceFileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    TransactedInstaller transactedInstaller = new TransactedInstaller();
                    AssemblyInstaller assemblyInstaller = new AssemblyInstaller(serviceFileName, cmdline);
                    transactedInstaller.Installers.Add(assemblyInstaller);
                    transactedInstaller.Install(new System.Collections.Hashtable());
                    logger.Info("Install Success!");
                } catch (Exception ex) {
                    logger.Error("Install Error >> "+ex.Message);
                }
            }
        // 删除服务
        else if (args[0].ToLower() == "/u" || args[0].ToLower() == "-u") {
                try {
                    logger.Info("UnInstall Success!");
                    string[] cmdline = { };
                    string serviceFileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    TransactedInstaller transactedInstaller = new TransactedInstaller();
                    AssemblyInstaller assemblyInstaller = new AssemblyInstaller(serviceFileName, cmdline);
                    transactedInstaller.Installers.Add(assemblyInstaller);
                    transactedInstaller.Uninstall(null);
                } catch (Exception ex) {
                    logger.Error("UnInstall Error >> " + ex.Message);
                }
            }
        }
    }
}
