using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ApplicationLayer.Views.Converters
{
    public class DrawingColorToMediaColorConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var drawingColor = (System.Drawing.Color)value;

                return System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
            }
            catch
            {
                return System.Windows.Media.Color.FromArgb(0, 0, 0, 0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var mediaColor = (System.Windows.Media.Color)value;

                return System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
            }
            catch
            {
                return System.Drawing.Color.FromArgb(0, 0, 0, 0);
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
