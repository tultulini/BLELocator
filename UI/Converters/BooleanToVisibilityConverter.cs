﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BLELocator.UI.Converters
{
    public class BooleanToVisibilityConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}