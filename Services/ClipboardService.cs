using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SnipJoin.Services;

public static class ClipboardService
{
    public static async Task<Image<Rgba32>?> GetImageFromClipboardAsync()
    {
        try
        {
            // Both clipboard access and conversion must happen on UI thread
            return await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (!Clipboard.ContainsImage())
                    return null;

                var clipboardImage = Clipboard.GetImage();
                if (clipboardImage == null)
                    return null;

                // Convert BitmapSource to Image<Rgba32>
                return ConvertBitmapSourceToImageSharp(clipboardImage);
            });
        }
        catch
        {
            // Silently handle clipboard access errors
            return null;
        }
    }

    public static async Task<bool> SetImageToClipboardAsync(Image<Rgba32> image)
    {
        return await Task.Run(async () =>
        {
            try
            {
                var bitmapSource = await ConvertImageSharpToBitmapSourceAsync(image);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Clipboard.SetImage(bitmapSource);
                });
                
                return true;
            }
            catch
            {
                // Silently handle clipboard access errors
                return false;
            }
        });
    }

    private static Image<Rgba32> ConvertBitmapSourceToImageSharp(BitmapSource bitmapSource)
    {
        // Convert to Bgra32 format if needed
        var formatConvertedBitmap = new FormatConvertedBitmap(bitmapSource, PixelFormats.Bgra32, null, 0);
        
        var width = formatConvertedBitmap.PixelWidth;
        var height = formatConvertedBitmap.PixelHeight;
        var stride = width * 4; // 4 bytes per pixel for BGRA32
        var pixelData = new byte[stride * height];
        
        formatConvertedBitmap.CopyPixels(pixelData, stride, 0);
        
        var image = new Image<Rgba32>(width, height);
        
        // Convert BGRA to RGBA
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var offset = (y * stride) + (x * 4);
                var b = pixelData[offset];
                var g = pixelData[offset + 1];
                var r = pixelData[offset + 2];
                var a = pixelData[offset + 3];
                
                image[x, y] = new Rgba32(r, g, b, a);
            }
        }
        
        return image;
    }

    private static async Task<BitmapSource> ConvertImageSharpToBitmapSourceAsync(Image<Rgba32> image)
    {
        return await Task.Run(() =>
        {
            var width = image.Width;
            var height = image.Height;
            var pixelData = new byte[width * height * 4]; // RGBA format
            
            // Extract pixel data
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = image[x, y];
                    var offset = (y * width + x) * 4;
                    
                    pixelData[offset] = pixel.B;     // B
                    pixelData[offset + 1] = pixel.G; // G
                    pixelData[offset + 2] = pixel.R; // R
                    pixelData[offset + 3] = pixel.A; // A
                }
            }
            
            return BitmapSource.Create(
                width, height,
                96, 96, // DPI
                PixelFormats.Bgra32, // Use 32-bit BGRA format
                null, // palette
                pixelData,
                width * 4 // stride
            );
        });
    }

    public static ImageSource? ConvertToWpfImageSource(Image<Rgba32> image)
    {
        try
        {
            var width = image.Width;
            var height = image.Height;
            var pixelData = new byte[width * height * 4]; // BGRA format
            
            // Extract pixel data
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = image[x, y];
                    var offset = (y * width + x) * 4;
                    
                    pixelData[offset] = pixel.B;     // B
                    pixelData[offset + 1] = pixel.G; // G
                    pixelData[offset + 2] = pixel.R; // R
                    pixelData[offset + 3] = pixel.A; // A
                }
            }
            
            var bitmapSource = BitmapSource.Create(
                width, height,
                96, 96, // DPI
                PixelFormats.Bgra32, // Use 32-bit BGRA format
                null, // palette
                pixelData,
                width * 4 // stride
            );
            
            bitmapSource.Freeze(); // Make it cross-thread accessible
            return bitmapSource;
        }
        catch
        {
            // Silently handle image conversion errors
            return null;
        }
    }
}