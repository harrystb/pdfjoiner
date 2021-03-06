﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace pdfjoiner.DesktopClient.UserControls
{
    [ValueConversion(typeof(FileExplorerItemType), typeof(BitmapImage))]
    public class HeaderToImageConverter : IValueConverter
    {
        private static readonly HeaderToImageConverter instance = new HeaderToImageConverter();

        public static HeaderToImageConverter Instance { get => instance; } 

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new BitmapImage(new Uri($"pack://application:,,,/pdfjoiner.DesktopClient;component/Images/{value}.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
