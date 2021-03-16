using System;
using System.Globalization;
using System.Windows.Data;

namespace WDLT.PoESpy.Helpers.Convertors
{
    [ValueConversion(typeof(DateTimeOffset), typeof(string))]
    public class TimeDifferenceToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTimeOffset time)
            {
                var diff = DateTimeOffset.Now - time;

                if (diff <= TimeSpan.FromMinutes(1))
                {
                    return $"{(int) diff.TotalSeconds}s.";
                } 
                else if (diff <= TimeSpan.FromHours(1))
                {
                    return $"{(int)diff.TotalMinutes}m.";
                }
                else if (diff <= TimeSpan.FromDays(1))
                {
                    return $"{(int)diff.TotalHours}h.";
                }
                else
                {
                    return $"{(int)diff.TotalDays}d.";
                }
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