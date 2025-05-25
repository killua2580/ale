using Avalonia.Data.Converters;
using System;
using Avalonia.Media.Imaging;

namespace IHECLibrary.Converters
{
    public class BookCoverFallbackConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            var cover = value as string;
            var fallback = parameter as string ?? "avares://IHECLibrary/Assets/book illustration.png";
            if (!string.IsNullOrWhiteSpace(cover))
                return cover;
            return fallback;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 