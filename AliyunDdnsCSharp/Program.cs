using NLog;
using System;
using System.Configuration.Install;
using System.ServiceProcess;
using AliyunDdnsCSharp.Core;
using AliyunDdnsCSharp.Services;

namespace AliyunDdnsCSharp {
    static class Program {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            if (args.Length == 0)
            {
                //运行
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Log.Debug("vs debug");
                    RunAsConsole();
                }
                else
                {
                    RunAsService();
                }
            }
            else if (args[0].ToLower() == "/c" || args[0].ToLower() == "-c")
            {
                //控制台方式运行
                RunAsConsole();
            }
            else if (args[0].ToLower() == "/i" || args[0].ToLower() == "-i")
            {
                //安装服务
                InstallService();
            }
            else if (args[0].ToLower() == "/u" || args[0].ToLower() == "-u")
            {
                //卸载服务
                UnInstallService();
            }
        }

        private static void RunAsConsole()
        {
            WorkerManager.GetInstance().BootStrap();
            Console.ReadLine();
            WorkerManager.GetInstance().Stop();
        }

        private static void RunAsService()
        {
            var servicesToRun = new ServiceBase[] { new DdnsService() };
            ServiceBase.Run(servicesToRun);
        }
        private static void InstallService()
        {
            string serviceFileName = "";
            try
            {
                serviceFileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                TransactedInstaller transactedInstaller = new TransactedInstaller();
                AssemblyInstaller assemblyInstaller = new AssemblyInstaller(serviceFileName, null);
                transactedInstaller.Installers.Add(assemblyInstaller);
                transactedInstaller.Install(new System.Collections.Hashtable());
                Log.Debug($"{serviceFileName} install success !");
            }
            catch (Exception ex)
            {
                Log.Error($"{serviceFileName} install error ${ex.Message}!");
            }
        }

        private static void UnInstallService()
        {
            string serviceFileName = "";
            try
            {
                serviceFileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                TransactedInstaller transactedInstaller = new TransactedInstaller();
                AssemblyInstaller assemblyInstaller = new AssemblyInstaller(serviceFileName, null);
                transactedInstaller.Installers.Add(assemblyInstaller);
                transactedInstaller.Uninstall(null);
                Log.Debug($"{serviceFileName} uninstall success !");
            }
            catch (Exception ex)
            {
                Log.Error($"{serviceFileName} uninstall error ${ex.Message}!");
            }
        }
    }
}
