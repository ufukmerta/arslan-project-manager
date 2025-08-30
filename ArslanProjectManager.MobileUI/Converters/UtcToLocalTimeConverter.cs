using System;
using System.Globalization;

namespace ArslanProjectManager.MobileUI.Converters
{
    public class UtcToLocalTimeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime utcDateTime)
            {
                return utcDateTime.ToLocalTime();
            }
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime localDateTime)
            {
                return localDateTime.ToUniversalTime();
            }
            return value;
        }
    }
} 