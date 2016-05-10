namespace Virgil.Sync.View.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;

    public class WatermarkHelperConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null)
            {
                var isFocused = values.OfType<bool>().Any(b => b);
                if (isFocused)
                {
                    return Visibility.Collapsed;
                }

                var isEmpty = string.IsNullOrEmpty(values.OfType<string>().FirstOrDefault());

                if (isEmpty)
                {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}