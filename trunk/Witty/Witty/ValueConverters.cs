using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using TwitterLib;
using System.Windows.Documents;
using System.Windows.Controls;

namespace Witty
{
    public class CharRemainingValueConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return TwitterNet.CharacterLimit - (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        #endregion
    }

    public class TextToInlinesValueConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TextBlock container = new TextBlock();

            if (value != null)
            {
                string[] words = ((string)value).Split(' ');

                foreach (string word in words)
                {
                    if (StringUtils.IsHyperlink(word))
                    {
                        try
                        {
                            Hyperlink link = new Hyperlink();
                            link.NavigateUri = new Uri(word);
                            link.Inlines.Add(word);
                            //link.Click += Hyperlink_Click;
                            container.Inlines.Add(link);
                        }
                        catch
                        {
                            container.Inlines.Add(word);
                        }
                    }
                    else
                        container.Inlines.Add(word);

                    container.Inlines.Add(" ");
                }
            }
            return container.Inlines;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        #endregion
    }
}
