using System.Globalization;

namespace ArslanProjectManager.MobileUI.Converters
{
    public class ZeroToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isZero = false;
            
            if (value is int intValue)
            {
                isZero = intValue == 0;
            }
            else if (value is long longValue)
            {
                isZero = longValue == 0;
            }
            else
            {
                isZero = false;
            }
                        
            if (parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase))
            {
                return !isZero;
            }
            
            return isZero;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
