﻿using System;
using System.Windows.Data;
using TwitterLib;

namespace Common.Converters
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
}
