namespace Virgil.Sync.View.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    [ValueConversion(typeof(bool), typeof(Color))]
    public class BoolToDropboxStatusColorConverter : IValueConverter
    {
        public Brush WarningColor { get; set; } = Brushes.Red;
        public Brush NormalColor { get; set; } = new SolidColorBrush(Color.FromRgb(0x5d, 0xc7, 0xb9));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? this.WarningColor : this.NormalColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}