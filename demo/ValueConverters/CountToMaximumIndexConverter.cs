using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace GroupedObservableCollection.Demo.ValueConverters
{
    [ValueConversion(typeof(int), typeof(int))]
    public class CountToMaximumIndexConverter : MarkupExtension, IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (value is int count)
                return Math.Max(0, count - 1);
            throw new NotSupportedException();
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
