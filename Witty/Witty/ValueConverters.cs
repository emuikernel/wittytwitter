using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using TwitterLib;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Drawing;
using System.Windows;

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

    public sealed class BackgroundConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ListBoxItem item = (ListBoxItem)value;
            ListBox listbox = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;

            // Get the index of a ListBoxItem
            int index = listbox.ItemContainerGenerator.IndexFromContainer(item);

            if (index % 2 == 0)
            {
                return Brushes.LightBlue;
            }
            else
            {
                return Brushes.Beige;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        #endregion
    }
}
