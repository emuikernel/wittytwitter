using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Input;

namespace Witty
{
    public partial class Options
    {
        // Settings used by the application
        private Properties.Settings AppSettings = Properties.Settings.Default;

        private bool isSettingSkin = false;

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

            PlaySounds = AppSettings.PlaySounds;

            isSettingSkin = true;
            SkinsComboBox.ItemsSource = App.Skins;

            // select the current skin
            if (!string.IsNullOrEmpty(AppSettings.Skin))
                SkinsComboBox.SelectedItem = AppSettings.Skin;

            isSettingSkin = false;
        }

        public bool PlaySounds
        {
            get { return (bool)GetValue(PlaySoundsProperty); }
            set { SetValue(PlaySoundsProperty, value); }
        }

        public static readonly DependencyProperty PlaySoundsProperty =
            DependencyProperty.Register("PlaySounds", typeof(bool), typeof(Options),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPlaySoundsChanged)));

        private static void OnPlaySoundsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Properties.Settings.Default.PlaySounds = (bool)args.NewValue;
            Properties.Settings.Default.Save();
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

        private void SkinsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isSettingSkin && e.AddedItems.Count >= 0)
            {
                string skin = e.AddedItems[0] as string;
                ResourceDictionary rd = Application.LoadComponent(new Uri(skin, UriKind.Relative)) as ResourceDictionary;
                Application.Current.Resources = rd;

                // save the skin setting
                AppSettings.Skin = skin;
                AppSettings.Save();
            }
        }

        /// <summary>
        /// Checks for keyboard shortcuts
        /// </summary>
        /// <param name="e">EventArgs</param>
        private void Window_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { this.Close(); };
        }
    }
}