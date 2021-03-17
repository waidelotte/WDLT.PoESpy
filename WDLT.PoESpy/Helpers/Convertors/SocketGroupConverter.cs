using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using WDLT.Clients.POE.Models;

namespace WDLT.PoESpy.Helpers.Convertors
{
    [ValueConversion(typeof(IEnumerable<POEFetchItemSocket>), typeof(IEnumerable<IGrouping<int, POEFetchItemSocket>>))]
    public class SocketGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<POEFetchItemSocket> list)
            {
                return list.GroupBy(g => g.Group);
            }
            else
            {
                return Binding.DoNothing;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}