using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace SnipJoin.Services;

public class ImageProcessor
{
    public static async Task<Image<Rgba32>> ProcessImageAsync(Image<Rgba32> sourceImage, System.Drawing.RectangleF selectionRect, bool isHorizontalMode)
    {
        return await Task.Run(() =>
        {
            var width = sourceImage.Width;
            var height = sourceImage.Height;

            if (isHorizontalMode)
            {
                // Horizontal cut: remove a horizontal strip
                var cutStart = (int)(selectionRect.Y * height);
                var cutHeight = (int)(selectionRect.Height * height);
                var cutEnd = cutStart + cutHeight;

                // Clamp values
                cutStart = Math.Max(0, cutStart);
                cutEnd = Math.Min(height, cutEnd);
                cutHeight = cutEnd - cutStart;

                if (cutHeight <= 0) return sourceImage.Clone();

                var newHeight = height - cutHeight;
                if (newHeight <= 0) return sourceImage.Clone();

                var result = new Image<Rgba32>(width, newHeight);

                // Copy top part (above cut)
                if (cutStart > 0)
                {
                    var topPart = sourceImage.Clone(ctx => ctx.Crop(new SixLabors.ImageSharp.Rectangle(0, 0, width, cutStart)));
                    result.Mutate(ctx => ctx.DrawImage(topPart, new SixLabors.ImageSharp.Point(0, 0), 1.0f));
                }

                // Copy bottom part (below cut) 
                if (cutEnd < height)
                {
                    var bottomHeight = height - cutEnd;
                    var bottomPart = sourceImage.Clone(ctx => ctx.Crop(new SixLabors.ImageSharp.Rectangle(0, cutEnd, width, bottomHeight)));
                    result.Mutate(ctx => ctx.DrawImage(bottomPart, new SixLabors.ImageSharp.Point(0, cutStart), 1.0f));
                }

                return result;
            }
            else
            {
                // Vertical cut: remove a vertical strip
                var cutStart = (int)(selectionRect.X * width);
                var cutWidth = (int)(selectionRect.Width * width);
                var cutEnd = cutStart + cutWidth;

                // Clamp values
                cutStart = Math.Max(0, cutStart);
                cutEnd = Math.Min(width, cutEnd);
                cutWidth = cutEnd - cutStart;

                if (cutWidth <= 0) return sourceImage.Clone();

                var newWidth = width - cutWidth;
                if (newWidth <= 0) return sourceImage.Clone();

                var result = new Image<Rgba32>(newWidth, height);

                // Copy left part (before cut)
                if (cutStart > 0)
                {
                    var leftPart = sourceImage.Clone(ctx => ctx.Crop(new SixLabors.ImageSharp.Rectangle(0, 0, cutStart, height)));
                    result.Mutate(ctx => ctx.DrawImage(leftPart, new SixLabors.ImageSharp.Point(0, 0), 1.0f));
                }

                // Copy right part (after cut)
                if (cutEnd < width)
                {
                    var rightWidth = width - cutEnd;
                    var rightPart = sourceImage.Clone(ctx => ctx.Crop(new SixLabors.ImageSharp.Rectangle(cutEnd, 0, rightWidth, height)));
                    result.Mutate(ctx => ctx.DrawImage(rightPart, new SixLabors.ImageSharp.Point(cutStart, 0), 1.0f));
                }

                return result;
            }
        });
    }

    public static async Task<Image<Rgba32>> LoadImageFromStreamAsync(Stream stream)
    {
        return await Task.Run(async () =>
        {
            stream.Position = 0;
            return await Image.LoadAsync<Rgba32>(stream);
        });
    }

    public static async Task<Image<Rgba32>> LoadImageFromFileAsync(string filePath)
    {
        return await Task.Run(async () =>
        {
            return await Image.LoadAsync<Rgba32>(filePath);
        });
    }

    public static async Task<byte[]> SaveImageToPngBytesAsync(Image<Rgba32> image)
    {
        return await Task.Run(async () =>
        {
            using var stream = new MemoryStream();
            await image.SaveAsPngAsync(stream);
            return stream.ToArray();
        });
    }

    public static async Task SaveImageToFileAsync(Image<Rgba32> image, string filePath)
    {
        await Task.Run(async () =>
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    await image.SaveAsJpegAsync(filePath);
                    break;
                case ".bmp":
                    await image.SaveAsBmpAsync(filePath);
                    break;
                case ".gif":
                    await image.SaveAsGifAsync(filePath);
                    break;
                default:
                    await image.SaveAsPngAsync(filePath);
                    break;
            }
        });
    }
}