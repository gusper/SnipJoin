using Microsoft.Win32;
using SnipJoin.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SnipJoin;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private bool _isSelecting;
    private Point _selectionStart;
    private Rectangle? _selectionRectangle;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
        
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainViewModel.CurrentImage):
                UpdateImageDisplay();
                break;
            case nameof(MainViewModel.ProcessedImage):
                UpdateProcessedImageDisplay();
                break;
            case nameof(MainViewModel.ProcessedImageSource):
                UpdateProcessedImageDisplay();
                break;
            case nameof(MainViewModel.StatusMessage):
                StatusText.Text = _viewModel.StatusMessage;
                break;
        }
    }

    private async void LoadFromClipboard_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadFromClipboardAsync();
    }

    private async void LoadFromFile_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files (*.*)|*.*",
            Title = "Select an image file"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            await _viewModel.LoadFromFileAsync(openFileDialog.FileName);
        }
    }

    private void SetHorizontalMode_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.IsHorizontalMode = true;
        UpdateModeButtons();
        ClearSelection();
    }

    private void SetVerticalMode_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.IsHorizontalMode = false;
        UpdateModeButtons();
        ClearSelection();
    }

    private void UpdateModeButtons()
    {
        var defaultBrush = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3));
        var activeBrush = Brushes.DarkBlue;
        
        HorizontalModeBtn.Background = _viewModel.IsHorizontalMode ? activeBrush : defaultBrush;
        VerticalModeBtn.Background = !_viewModel.IsHorizontalMode ? activeBrush : defaultBrush;
        
        StatusText.Text = _viewModel.IsHorizontalMode ? 
            "Horizontal mode: Click and drag to select a horizontal segment to remove" : 
            "Vertical mode: Click and drag to select a vertical segment to remove";
    }

    private void ClearSelection_Click(object sender, RoutedEventArgs e)
    {
        ClearSelection();
    }

    private async void ProcessImage_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectionRect.HasValue)
        {
            await _viewModel.ProcessImageAsync();
        }
    }

    private async void SaveToClipboard_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveToClipboardAsync();
    }

    private async void SaveToFile_Click(object sender, RoutedEventArgs e)
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "PNG files (*.png)|*.png|JPEG files (*.jpg)|*.jpg|All files (*.*)|*.*",
            Title = "Save processed image",
            DefaultExt = "png"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            await _viewModel.SaveToFileAsync(saveFileDialog.FileName);
        }
    }

    private void SelectionCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.CurrentImage == null) return;

        _isSelecting = true;
        _selectionStart = e.GetPosition(SelectionCanvas);
        SelectionCanvas.CaptureMouse();
        
        ClearSelectionRectangle();
    }

    private void SelectionCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isSelecting || _viewModel.CurrentImage == null) return;

        var currentPos = e.GetPosition(SelectionCanvas);
        UpdateSelectionRectangle(_selectionStart, currentPos);
    }

    private void SelectionCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isSelecting) return;

        _isSelecting = false;
        SelectionCanvas.ReleaseMouseCapture();

        var endPos = e.GetPosition(SelectionCanvas);
        FinalizeSelection(_selectionStart, endPos);
    }

    private void UpdateSelectionRectangle(Point start, Point end)
    {
        if (_selectionRectangle == null)
        {
            _selectionRectangle = new Rectangle
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)),
                StrokeDashArray = new DoubleCollection { 5, 3 }
            };
            SelectionCanvas.Children.Add(_selectionRectangle);
        }

        var rect = new Rect(start, end);
        
        if (_viewModel.IsHorizontalMode)
        {
            // Horizontal selection spans full width
            rect.X = 0;
            rect.Width = SelectionCanvas.ActualWidth;
        }
        else
        {
            // Vertical selection spans full height
            rect.Y = 0;
            rect.Height = SelectionCanvas.ActualHeight;
        }

        Canvas.SetLeft(_selectionRectangle, rect.Left);
        Canvas.SetTop(_selectionRectangle, rect.Top);
        _selectionRectangle.Width = rect.Width;
        _selectionRectangle.Height = rect.Height;
    }

    private void FinalizeSelection(Point start, Point end)
    {
        if (_viewModel.CurrentImage == null) return;

        var imageRect = GetImageBounds();
        var selectionRect = new Rect(start, end);

        // Convert to image coordinates
        var imageX = Math.Max(0, (selectionRect.X - imageRect.X) / imageRect.Width);
        var imageY = Math.Max(0, (selectionRect.Y - imageRect.Y) / imageRect.Height);
        var imageWidth = Math.Min(1.0, selectionRect.Width / imageRect.Width);
        var imageHeight = Math.Min(1.0, selectionRect.Height / imageRect.Height);

        if (_viewModel.IsHorizontalMode)
        {
            imageX = 0;
            imageWidth = 1.0;
        }
        else
        {
            imageY = 0;
            imageHeight = 1.0;
        }

        var finalRect = new System.Drawing.RectangleF(
            (float)imageX, (float)imageY, (float)imageWidth, (float)imageHeight);
            
        // Debug: Check for valid selection
        if (finalRect.Width <= 0 || finalRect.Height <= 0)
        {
            StatusText.Text = "Invalid selection - too small or zero size";
            return;
        }
        
        _viewModel.SelectionRect = finalRect;

        ProcessBtn.IsEnabled = true;
        SelectionInfoText.Text = _viewModel.IsHorizontalMode ? 
            $"Horizontal selection: {selectionRect.Height:F0}px" : 
            $"Vertical selection: {selectionRect.Width:F0}px";
            
    }

    private Rect GetImageBounds()
    {
        // Since SelectionCanvas is sized to match the image, and both are centered in the Grid,
        // we can use the SelectionCanvas bounds directly
        return new Rect(0, 0, SelectionCanvas.Width, SelectionCanvas.Height);
    }

    private void ClearSelection()
    {
        ClearSelectionRectangle();
        _viewModel.SelectionRect = null;
        ProcessBtn.IsEnabled = false;
        SelectionInfoText.Text = "";
    }

    private void ClearSelectionRectangle()
    {
        if (_selectionRectangle != null)
        {
            SelectionCanvas.Children.Remove(_selectionRectangle);
            _selectionRectangle = null;
        }
    }

    private void UpdateImageDisplay()
    {
        if (_viewModel.CurrentImage != null && _viewModel.CurrentImageSource != null)
        {
            MainImage.Source = _viewModel.CurrentImageSource;
            SelectionCanvas.Width = _viewModel.CurrentImageSource.Width;
            SelectionCanvas.Height = _viewModel.CurrentImageSource.Height;
            
            ImageInfoText.Text = $"{_viewModel.CurrentImageSource.Width}×{_viewModel.CurrentImageSource.Height}px";
            
            // Reset processed image display
            SaveToClipboardBtn.IsEnabled = false;
            SaveToFileBtn.IsEnabled = false;
        }
        else
        {
            MainImage.Source = null;
            SelectionCanvas.Width = 0;
            SelectionCanvas.Height = 0;
            ImageInfoText.Text = "";
        }
        
        ClearSelection();
    }

    private void UpdateProcessedImageDisplay()
    {
        if (_viewModel.ProcessedImage != null && _viewModel.ProcessedImageSource != null)
        {
            MainImage.Source = _viewModel.ProcessedImageSource;
            SelectionCanvas.Width = _viewModel.ProcessedImageSource.Width;
            SelectionCanvas.Height = _viewModel.ProcessedImageSource.Height;
            
            ImageInfoText.Text = $"{_viewModel.ProcessedImageSource.Width}×{_viewModel.ProcessedImageSource.Height}px (Processed)";
            
            SaveToClipboardBtn.IsEnabled = true;
            SaveToFileBtn.IsEnabled = true;
            ProcessBtn.IsEnabled = false;
        }
        
        ClearSelection();
    }
}