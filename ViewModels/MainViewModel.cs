using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SnipJoin.Services;
using System.Windows.Media;

namespace SnipJoin.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private Image<Rgba32>? _currentImage;
    private Image<Rgba32>? _processedImage;
    private ImageSource? _currentImageSource;
    private ImageSource? _processedImageSource;
    private bool _isHorizontalMode = true;
    private System.Drawing.RectangleF? _selectionRect;
    private string _statusMessage = "Load an image from clipboard or file to begin";

    public Image<Rgba32>? CurrentImage
    {
        get => _currentImage;
        set => SetProperty(ref _currentImage, value);
    }

    public Image<Rgba32>? ProcessedImage
    {
        get => _processedImage;
        set => SetProperty(ref _processedImage, value);
    }

    public ImageSource? CurrentImageSource
    {
        get => _currentImageSource;
        set => SetProperty(ref _currentImageSource, value);
    }

    public ImageSource? ProcessedImageSource
    {
        get => _processedImageSource;
        set => SetProperty(ref _processedImageSource, value);
    }

    public bool IsHorizontalMode
    {
        get => _isHorizontalMode;
        set => SetProperty(ref _isHorizontalMode, value);
    }

    public System.Drawing.RectangleF? SelectionRect
    {
        get => _selectionRect;
        set => SetProperty(ref _selectionRect, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public async Task LoadFromClipboardAsync()
    {
        try
        {
            StatusMessage = "Loading image from clipboard...";
            
            var image = await ClipboardService.GetImageFromClipboardAsync();
            if (image == null)
            {
                StatusMessage = "No image found in clipboard";
                return;
            }

            // Update properties on UI thread
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var imageSource = ClipboardService.ConvertToWpfImageSource(image);
                    
                    // Set all related properties without firing PropertyChanged until the end
                    _currentImage = image;
                    _currentImageSource = imageSource;
                    _processedImage = null;
                    _processedImageSource = null;
                    _selectionRect = null;
                    _statusMessage = $"Image loaded from clipboard ({image.Width}×{image.Height}px)";
                    
                    // Now notify of all the changes
                    OnPropertyChanged(nameof(CurrentImage));
                    OnPropertyChanged(nameof(CurrentImageSource));
                    OnPropertyChanged(nameof(ProcessedImage));
                    OnPropertyChanged(nameof(ProcessedImageSource));
                    OnPropertyChanged(nameof(SelectionRect));
                    OnPropertyChanged(nameof(StatusMessage));
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error converting image: {ex.Message}";
                }
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading from clipboard: {ex.Message}";
        }
    }

    public async Task LoadFromFileAsync(string filePath)
    {
        try
        {
            StatusMessage = "Loading image from file...";
            
            if (!System.IO.File.Exists(filePath))
            {
                StatusMessage = "File does not exist";
                return;
            }
            
            var image = await ImageProcessor.LoadImageFromFileAsync(filePath);
            
            if (image == null)
            {
                StatusMessage = "Failed to load image from file";
                return;
            }

            // Update properties on UI thread
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var imageSource = ClipboardService.ConvertToWpfImageSource(image);
                    
                    // Set all related properties without firing PropertyChanged until the end
                    _currentImage = image;
                    _currentImageSource = imageSource;
                    _processedImage = null;
                    _processedImageSource = null;
                    _selectionRect = null;
                    _statusMessage = $"Image loaded from file ({image.Width}×{image.Height}px)";
                    
                    // Now notify of all the changes
                    OnPropertyChanged(nameof(CurrentImage));
                    OnPropertyChanged(nameof(CurrentImageSource));
                    OnPropertyChanged(nameof(ProcessedImage));
                    OnPropertyChanged(nameof(ProcessedImageSource));
                    OnPropertyChanged(nameof(SelectionRect));
                    OnPropertyChanged(nameof(StatusMessage));
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error converting image: {ex.Message}";
                }
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading file: {ex.Message}";
        }
    }

    public async Task ProcessImageAsync()
    {
        // Check if we have any image to work with (either original or processed)
        if ((CurrentImage == null && ProcessedImage == null) || !SelectionRect.HasValue)
        {
            StatusMessage = "No image or selection to process";
            return;
        }

        try
        {
            StatusMessage = "Processing image...";
            
            // Use the most recent image - if we have a processed image, use that; otherwise use the original
            var sourceImage = ProcessedImage ?? CurrentImage;
            var processedImage = await ImageProcessor.ProcessImageAsync(
                sourceImage, SelectionRect.Value, IsHorizontalMode);
            
            // Update on UI thread
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ProcessedImage = processedImage;
                ProcessedImageSource = ClipboardService.ConvertToWpfImageSource(processedImage);
            });
            
            var mode = IsHorizontalMode ? "horizontal" : "vertical";
            StatusMessage = $"Image processed - {mode} segment removed and parts joined";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error processing image: {ex.Message}";
        }
    }

    public async Task SaveToClipboardAsync()
    {
        if (ProcessedImage == null)
        {
            StatusMessage = "No processed image to save to clipboard";
            return;
        }

        try
        {
            StatusMessage = "Copying to clipboard...";
            
            var success = await ClipboardService.SetImageToClipboardAsync(ProcessedImage);
            if (success)
            {
                StatusMessage = "Image copied to clipboard successfully";
            }
            else
            {
                StatusMessage = "Failed to copy image to clipboard";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error copying to clipboard: {ex.Message}";
        }
    }

    public async Task SaveToFileAsync(string filePath)
    {
        if (ProcessedImage == null)
        {
            StatusMessage = "No processed image to save";
            return;
        }

        try
        {
            StatusMessage = "Saving image to file...";
            
            await ImageProcessor.SaveImageToFileAsync(ProcessedImage, filePath);
            StatusMessage = $"Image saved to {System.IO.Path.GetFileName(filePath)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving file: {ex.Message}";
        }
    }
}