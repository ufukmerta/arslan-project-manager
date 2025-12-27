using System.Globalization;

namespace ArslanProjectManager.MobileUI.Converters
{
    public class RoleToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var role = value as string ?? string.Empty;
            return role.ToLowerInvariant() switch
            {
                "admin" => Color.FromArgb("#1f6feb"),
                "manager" => Color.FromArgb("#6f42c1"),
                "member" => Color.FromArgb("#2ea043"),
                _ => Color.FromArgb("#6c757d")
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

