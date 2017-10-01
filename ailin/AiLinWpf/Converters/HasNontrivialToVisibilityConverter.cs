using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using AiLinWpf.Helpers;

namespace AiLinWpf.Converters
{
    public class HasNontrivialToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => InterpretationHelper.IsNontrivial(value) ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
