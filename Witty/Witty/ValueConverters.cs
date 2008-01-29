using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TwitterLib;

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

    /// <summary>
    /// Takes a double value and round it to the nearest integer
    /// </summary>
    public class RoundConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double inputValue = Math.Round(Double.Parse(value.ToString()));
            return inputValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                double inputValue = Math.Round(Double.Parse(value.ToString()));
                return inputValue;
            }
            catch
            {
                return 0;
            }
        }

        #endregion
    }

    public sealed class IndexToIsAlternateRowConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int index = (int)value;
            return (index % 2 == 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Convert back is not used in the binding
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        #endregion
    }
}

