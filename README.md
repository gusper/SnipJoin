# SnipJoin

A Windows WPF application that removes segments from screenshots/images and joins the remaining parts together. Makes it easy
to remove unwanted sections from screenshots and save them to a file or to the clipboard.

![SnipJoin Demo](snipjoin-0.1.gif)

## Features

- **Load images** from clipboard or files
- **Remove horizontal segments** - select and remove horizontal strips from images
- **Remove vertical segments** - select and remove vertical strips from images  
- **Export results** to clipboard or save as files
- **Multiple formats** - supports PNG, JPEG, BMP, GIF

## How to Use

1. **Load an image**:
   - Click "From Clipboard" to load a screenshot from clipboard
   - Click "Open File" to load an image file

2. **Select cut mode**:
   - Click "Horizontal Cut" to remove horizontal segments
   - Click "Vertical Cut" to remove vertical segments

3. **Make selection**:
   - Click and drag on the image to select the segment you want to remove
   - In horizontal mode: selection spans full width, drag vertically to set height
   - In vertical mode: selection spans full height, drag horizontally to set width

4. **Process image**:
   - Click "Cut & Join" to remove the selected segment and join the remaining parts

5. **Export result**:
   - Click "To Clipboard" to copy the processed image to clipboard
   - Click "Save File" to save the processed image as a file

## Build Requirements

- .NET 8.0 SDK
- Windows (WPF application)

## Use Cases

- Remove unwanted sections from screenshots
- Join disconnected parts of images
- Clean up long screenshots by removing middle sections
- Create compact images from lengthy content

## Technical Details

- Built with WPF and .NET 8.0
- Uses ImageSharp for image processing
- Implements MVVM pattern for clean architecture
- Native Windows clipboard integration

## Open Source Dependencies

This project uses the following open source libraries:

- **[SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp)** (v3.1.6) - A modern, cross-platform, 2D graphics API for .NET
  - License: Six Labors Split License
  - Used for: Image loading, processing, cutting, joining operations, and format conversion

- **[SixLabors.ImageSharp.Drawing](https://github.com/SixLabors/ImageSharp.Drawing)** (v2.1.4) - 2D drawing API for ImageSharp
  - License: Six Labors Split License  
  - Used for: Additional drawing operations support

- **[CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)** (v8.2.2) - .NET Community Toolkit MVVM library
  - License: MIT
  - Used for: MVVM pattern implementation, property change notifications