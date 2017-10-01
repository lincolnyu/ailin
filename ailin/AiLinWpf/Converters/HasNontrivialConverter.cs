using System;
using System.Globalization;
using System.Windows.Data;

namespace AiLinWpf.Converters
{
    public class HasNontrivialConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                return !string.IsNullOrWhiteSpace(s);
            }
            else if (value.GetType().IsValueType)
            {
                var defaultVal = Activator.CreateInstance(value.GetType());
                return value != defaultVal;
            }
            else
            {
                return value != null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
