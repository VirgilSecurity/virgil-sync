namespace Virgil.Disk.View.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    [ValueConversion(typeof(string), typeof(Visibility))]
    public sealed class EmptyStringToVisibilityConverter : IValueConverter
    {
        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }

        public EmptyStringToVisibilityConverter()
        {
            // set defaults
            this.TrueValue = Visibility.Visible;
            this.FalseValue = Visibility.Collapsed;
        }

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? this.TrueValue : this.FalseValue;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}