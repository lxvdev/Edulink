using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Edulink.Converters
{
    internal class MarginToBorderRadiusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is CornerRadius baseCornerRadius && values[1] is Thickness margin)
            {
                double adjustment = (margin.Left + margin.Top) / 4;

                return new CornerRadius(
                    Math.Max(baseCornerRadius.TopLeft - adjustment, 0),
                    Math.Max(baseCornerRadius.TopRight - adjustment, 0),
                    Math.Max(baseCornerRadius.BottomRight - adjustment, 0),
                    Math.Max(baseCornerRadius.BottomLeft - adjustment, 0)
                );
            }

            return new CornerRadius(0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
