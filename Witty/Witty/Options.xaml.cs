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

        public bool IsTopMost
        {
            get { return (bool)GetValue(IsTopMostProperty); }
            set { SetValue(IsTopMostProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTopMost.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTopMostProperty =
            DependencyProperty.Register("IsTopMost", typeof(bool), typeof(Options), new UIPropertyMetadata(false));

		public Options()
		{
			this.InitializeComponent();

            UsernameTextBox.Text = AppSettings.Username;
            PasswordTextBox.Password = AppSettings.Password;

            Binding binding = new Binding();
            binding.Path = new PropertyPath("Topmost");
            binding.Source = this;

            AlwaysOnTopCheckBox.SetBinding(CheckBox.IsCheckedProperty, binding);

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

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.Username = string.Empty;
            AppSettings.Password = string.Empty;
            AppSettings.LastUpdated = string.Empty;

            AppSettings.Save();

            DialogResult = false;
            this.Close();
        }
	}
}