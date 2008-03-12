using System;
using System.Windows.Data;
using TwitterLib;
using log4net;
using log4net.Config;

namespace Witty
{
    public class CharRemainingValueConverter : IValueConverter
    {
        private static readonly ILog logger = LogManager.GetLogger("Witty.Logging");

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return TwitterNet.CharacterLimit - (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            logger.Error("Error converting value (CharRemainingValueConverter.ConvertBack is not implemented).");
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
        private static readonly ILog logger = LogManager.GetLogger("Witty.Logging");

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int index = (int)value;
            return (index % 2 == 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Convert back is not used in the binding
            logger.Error("Error converting value (IndexToIsAlternateRowConverter.ConvertBack is not implemented).");
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        #endregion
    }
}

