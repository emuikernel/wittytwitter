using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Deployment;
using System.Deployment.Application;

namespace Witty.Controls.Options
{
    /// <summary>
    /// Interaction logic for ClickOnceOptions.xaml
    /// </summary>
    public partial class ClickOnceOptions : UserControl
    {
        public ClickOnceOptions()
        {
            InitializeComponent();
            if (!ClickOnce.Utils.IsApplicationNetworkDeployed)
            {
                this.IsEnabled = false;
                tbCurrentVersion.Text = "The current running version of " + 
                                        Witty.Properties.Settings.Default.ApplicationName + 
                                        " does not support automatic updates.";
            }
            else
            {
                tbCurrentVersion.Text = "Current Version: " + ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
        }

        private void btnCheckForUpdate_Click(object sender, RoutedEventArgs e)
        {
            ClickOnce.Deployment deployment = new Witty.ClickOnce.Deployment(updateStatus);
            deployment.UpdateCompletedEvent += new Witty.ClickOnce.Deployment.UpdateCompletedDelegate(deployment_UpdateCompletedEvent);
            deployment.UpdateApplication();
        }

        void deployment_UpdateCompletedEvent(bool restartApplication)
        {
            if (restartApplication)
            {
                //WPF apps do not have a Restart(), this achieves the same thing.
                System.Windows.Forms.Application.Restart();
                Application.Current.Shutdown();
            }
        }
    }
}
