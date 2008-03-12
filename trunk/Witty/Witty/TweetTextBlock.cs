using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using TwitterLib;
using log4net;
using log4net.Config;

namespace Witty
{
    public class TweetTextBlock : TextBlock
    {
        private static readonly ILog logger = LogManager.GetLogger("Witty.Logging");

        public string TweetText
        {
            get { return (string)GetValue(TweetTextProperty); }
            set { SetValue(TweetTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TweetText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TweetTextProperty =
            DependencyProperty.Register("TweetText", typeof(string), typeof(TweetTextBlock),
            new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnTweetTextChanged)));

        private static void OnTweetTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            //Format hyperlinks
            //TODO: format @name
            string text = args.NewValue as string;
            if (!string.IsNullOrEmpty(text))
            {
                TweetTextBlock textblock = (TweetTextBlock)obj;
                textblock.Inlines.Clear();

                string[] words = text.Split(' ');

                foreach (string word in words)
                {
                    if (StringHelper.IsHyperlink(word))
                    {
                        try
                        {
                            Hyperlink link = new Hyperlink();
                            link.NavigateUri = new Uri(word);
                            link.Inlines.Add(word);
                            link.Click += new RoutedEventHandler(link_Click);
                            link.ToolTip = "Open link in the default browser";
                            textblock.Inlines.Add(link);
                        }
                        catch
                        {
                            //TODO:What are we catching here? Why? Log it?
                            textblock.Inlines.Add(word);
                        }
                    }
                    else
                    {
                        textblock.Inlines.Add(word);
                    }

                    textblock.Inlines.Add(" ");
                }
            }
        }

        static void link_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(((Hyperlink)sender).NavigateUri.ToString());
            }
            catch
            {
                logger.Error("There was a problem launching the specified URL.");
                //TODO: Log specific URL that caused error
                MessageBox.Show("There was a problem launching the specified URL.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

    }
}
