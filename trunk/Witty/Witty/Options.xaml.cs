using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace Witty
{
	public partial class Options
	{
        // Settings used by the application
        private Properties.Settings AppSettings = Properties.Settings.Default;

		public Options()
		{
			this.InitializeComponent();

            UsernameTextBox.Text = AppSettings.Username;
            PasswordTextBox.Password = AppSettings.Password;

            if (!string.IsNullOrEmpty(AppSettings.RefreshInterval))
                RefreshSlider.Value = Double.Parse(AppSettings.RefreshInterval);
		}

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.RefreshInterval = SliderValueTextBlock.Text;
            AppSettings.Save();

            DialogResult = true;

            this.Close();
        }
	}
}