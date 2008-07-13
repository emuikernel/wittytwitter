using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using TwitterLib;
using System.Windows.Input;

namespace Common.Controls
{
    /// <summary>
    /// Custom TextBlock to allow parsing hyperlinks and @names
    /// </summary>
    public class TweetTextBlock : TextBlock
    {
        #region Dependency properties

        public string TweetText
        {
            get { return (string)GetValue(TweetTextProperty); }
            set { SetValue(TweetTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TweetText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TweetTextProperty =
            DependencyProperty.Register("TweetText", typeof(string), typeof(TweetTextBlock),
            new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnTweetTextChanged)));

        #endregion

        private static void OnTweetTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            string text = args.NewValue as string;
            if (!string.IsNullOrEmpty(text))
            {
                TweetTextBlock textblock = (TweetTextBlock)obj;
                textblock.Inlines.Clear();

                string[] words = text.Split(' ');

                foreach (string word in words)
                {
                    // clickable hyperlinks
                    if (TinyUrlHelper.IsUrl(word))
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
                    // clickable @name
                    else if (word.StartsWith("@"))
                    {
                        string userName = String.Empty;
                        Match foundUsername = Regex.Match(word, @"@(\w+)(?<suffix>.*)");
                        if (foundUsername.Success)
                        {
                            userName = foundUsername.Groups[1].Captures[0].Value;
                            Hyperlink name = new Hyperlink();
                            name.Inlines.Add(userName);
                            name.NavigateUri = new Uri("http://twitter.com/" + userName);
                            name.ToolTip = "View @" + userName + "'s recent tweets";
                            name.Tag = userName;

                            name.Click += new RoutedEventHandler(name_Click);

                            textblock.Inlines.Add("@");
                            textblock.Inlines.Add(name);
                            textblock.Inlines.Add(foundUsername.Groups["suffix"].Captures[0].Value);
                        }
                    }

                    // clickable #hashtag
                    else if (word.StartsWith("#"))
                    {
                        string hashtag = String.Empty;
                        Match foundHashtag = Regex.Match(word, @"#(\w+)(?<suffix>.*)");
                        if (foundHashtag.Success)
                        {
                            hashtag = foundHashtag.Groups[1].Captures[0].Value;
                            Hyperlink tag = new Hyperlink();
                            tag.Inlines.Add(hashtag);
                            tag.NavigateUri = new Uri("http://twemes.com/" + hashtag);
                            tag.ToolTip = "Show this hashtag on Twemes.com";
                            tag.Tag = hashtag;

                            tag.Click += new RoutedEventHandler(link_Click);

                            textblock.Inlines.Add("#");
                            textblock.Inlines.Add(tag);
                            textblock.Inlines.Add(foundHashtag.Groups["suffix"].Captures[0].Value);
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
                //TODO: Log specific URL that caused error
                MessageBox.Show("There was a problem launching the specified URL.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        #region Clickable @name

        public static readonly RoutedEvent NameClickEvent = EventManager.RegisterRoutedEvent(
            "NameClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TweetTextBlock));

        public event RoutedEventHandler NameClick
        {
            add { AddHandler(NameClickEvent, value); }
            remove { RemoveHandler(NameClickEvent, value); }
        }

        static void name_Click(object sender, RoutedEventArgs e)
        {
            //TODO: this should be configurable to show the user Witty or open in browser.
            try
            {
                if (e.OriginalSource is Hyperlink)
                {
                    Hyperlink h = e.OriginalSource as Hyperlink;
                    if (h.Parent is TweetTextBlock)
                    {
                        TweetTextBlock p = h.Parent as TweetTextBlock;
                        p.RaiseEvent(new RoutedEventArgs(NameClickEvent, h));
                        return;
                    }
                }

                System.Diagnostics.Process.Start(((Hyperlink)sender).NavigateUri.ToString());

            }
            catch
            {
                //TODO: Log specific URL that caused error
                MessageBox.Show("There was a problem launching the specified URL.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        #endregion
    }
}
