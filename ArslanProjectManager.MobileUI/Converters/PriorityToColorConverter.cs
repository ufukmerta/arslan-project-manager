using System.Globalization;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.MobileUI.Converters
{
    public class PriorityToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ProjectTask.TaskPriority priority)
            {
                return priority switch
                {
                    ProjectTask.TaskPriority.Low => Color.FromArgb("#28a745"),
                    ProjectTask.TaskPriority.Medium => Color.FromArgb("#ffc107"),
                    ProjectTask.TaskPriority.High => Color.FromArgb("#dc3545"),
                    _ => Color.FromArgb("#6c757d")
                };
            }
            return Color.FromArgb("#6c757d");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}