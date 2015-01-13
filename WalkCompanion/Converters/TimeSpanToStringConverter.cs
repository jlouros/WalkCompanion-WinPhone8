﻿using WalkCompanion.Utils;
using System;
using System.Windows.Data;

namespace WalkCompanion.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return UnitsConverter.TimeSpanToString((TimeSpan)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
