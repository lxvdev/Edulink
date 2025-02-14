using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace Edulink.Converters
{
    public class BooleanToSortDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ListSortDirection direction)
            {
                return direction == ListSortDirection.Descending;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked)
            {
                return isChecked ? ListSortDirection.Descending : ListSortDirection.Ascending;
            }
            return ListSortDirection.Ascending;
        }
    }
}
