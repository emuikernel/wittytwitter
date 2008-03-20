using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using log4net;
using log4net.Config;

namespace Witty
{
    public partial class Options
    {
        private static readonly ILog logger = LogManager.GetLogger("Witty.Logging");

        private readonly Properties.Settings AppSettings = Properties.Settings.Default;

        // bool to prevent endless recursion
        private bool isSettingSkin = false;

        public Options()
        {
            this.InitializeComponent();

            #region Initialize Options

            UsernameTextBox.Text = AppSettings.Username;
            PasswordTextBox.Password = AppSettings.Password;

            if (!string.IsNullOrEmpty(AppSettings.RefreshInterval))
            {
                RefreshSlider.Value = Double.Parse(AppSettings.RefreshInterval);
            }

            isSettingSkin = true;
            SkinsComboBox.ItemsSource = App.Skins;
            // select the current skin
            if (!string.IsNullOrEmpty(AppSettings.Skin))
            {
                SkinsComboBox.SelectedItem = AppSettings.Skin;
            }
            isSettingSkin = false;

            AlwaysOnTopCheckBox.IsChecked = AppSettings.AlwaysOnTop;

            PlaySounds = AppSettings.PlaySounds;
            MinimizeToTray = AppSettings.MinimizeToTray;
            PersistLogin = AppSettings.PersistLogin;

            #endregion
        }

        #region PlaySounds

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

        #endregion

        #region MinimizeToTray

        public bool MinimizeToTray
        {
            get { return (bool)GetValue(MinimizeToTrayProperty); }
            set { SetValue(MinimizeToTrayProperty, value); }
        }

        public static readonly DependencyProperty MinimizeToTrayProperty =
            DependencyProperty.Register("MinimizeToTray", typeof(bool), typeof(Options),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnMinimizeToTrayChanged)));

        private static void OnMinimizeToTrayChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Properties.Settings.Default.MinimizeToTray = (bool)args.NewValue;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region PersistLogin

        public bool PersistLogin
        {
            get { return (bool)GetValue(PersistLoginProperty); }
            set { SetValue(PersistLoginProperty, value); }
        }

        public static readonly DependencyProperty PersistLoginProperty =
            DependencyProperty.Register("PersistLogin", typeof(bool), typeof(Options),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPersistLoginChanged)));

        private static void OnPersistLoginChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Properties.Settings.Default.PersistLogin = (bool)args.NewValue;
            Properties.Settings.Default.Save();
        }

        #endregion

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
            if ((!(isSettingSkin)) && (e.AddedItems.Count >= 0))
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
        /// <param name="sender"></param>
        /// <param name="e">EventArgs</param>
        private void Window_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { this.Close(); };
        }

        private void AlwaysOnTopCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)((CheckBox)sender).IsChecked)
                this.Topmost = true;
            else
                this.Topmost = false;

            AppSettings.AlwaysOnTop = this.Topmost;
            AppSettings.Save();
        }
    }
}