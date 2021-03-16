using System;
using System.Globalization;
using System.Windows.Data;
using WDLT.Clients.POE.Enums;
using WDLT.PoESpy.Services;

namespace WDLT.PoESpy.Helpers.Convertors
{
    [ValueConversion(typeof(string), typeof(Uri))]
    public class CurrencyToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string currency && parameter is EPOEStaicType type)
            {
                if (ImageCacheService.Exist(currency))
                {
                    return new Uri(ImageCacheService.Get(currency));
                }
                else
                {
                    return Binding.DoNothing;
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