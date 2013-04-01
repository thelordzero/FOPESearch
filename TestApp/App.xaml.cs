using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Deployment.Application;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            string[] args = null;
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                var inputArgs = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;
                if (inputArgs != null && inputArgs.Length > 0)
                {
                    args = inputArgs[0].Split(new char[] { ',' });
                    List<string> emailsToSearch = new List<string>();
                    for (int i = 0; i != args.Length; ++i)
                    {
                        emailsToSearch.Add(args[i].ToString());
                    }
                    MainWindow mainWindow = new MainWindow(emailsToSearch);
                    mainWindow.Show();
                }
                else
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                }
            }
            else if (e.Args.Length != 0)
            {
                List<string> emailsToSearch = new List<string>();
                for (int i = 0; i != e.Args.Length; ++i)
                {
                    emailsToSearch.Add(e.Args[i].ToString());
                }
                MainWindow mainWindow = new MainWindow(emailsToSearch);
                mainWindow.Show();
            }
            else
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }
    }
}
