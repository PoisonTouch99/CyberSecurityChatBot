using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CyberSecurityChatBot.Converters
{
    /// <summary>       
    /// Converts boolean to Visibility.
    /// True → Visible, False → Collapsed.
    /// Renamed to avoid conflict with the inline converter in MainWindow.xaml.cs.
    /// </summary>
    public class BoolToVisibilityConverterEx : IValueConverter
    {
        // Convert bool to Visibility
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }

        // Optional: Implement ConvertBack if you need two-way binding
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
                return visibility == Visibility.Visible;
            return false;
        }
    }
}
