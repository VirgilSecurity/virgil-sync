namespace Virgil.Disk.View.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class HeightToNegativeMargin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d;

            bool parsed = double.TryParse(parameter?.ToString() ?? "", out d);

            if (!parsed)
            {
                d = 5;
            }
            var bottom = -((double) value + d);
            return new Thickness(0, 0, 0, bottom);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}