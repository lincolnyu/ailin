using System;
using System.Globalization;
using System.Windows.Data;
using AiLinWpf.Helpers;

namespace AiLinWpf.Converters
{
    public class HasNontrivialConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => InterpretationHelper.IsNontrivial(value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
