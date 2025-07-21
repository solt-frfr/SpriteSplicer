using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Advanced;
using MsBox.Avalonia;
using Image = SixLabors.ImageSharp.Image;


namespace SpriteSplicer.Views;

public partial class MainWindow : Window
{
    public int mode;
    public string bwpath;
    public string ptpath;
    public string batchpath;
    public MemoryStream bwout1;
    public MemoryStream bwout2;
    public MemoryStream ptout1;
    public MemoryStream ptout2;
    public MainWindow()
    {
        InitializeComponent();
        Switch.Content = "Platinum";
        Title.Text = "Pokemon Sprite Splitter - BW";
        mode = 0;
    }

    private async void Import_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Import Image",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Image")
                    {
                        Patterns = new List<string> { "*.png" }
                    }
                }
            });
            if (files.Count == 1)
            {
                if (File.Exists(files[0].Path.LocalPath))
                {
                    using var stream = File.OpenRead(files[0].Path.LocalPath);
                    Bitmap bitmap = new Bitmap(stream);
                    if (mode == 0)
                    {
                        if (bitmap.Size.Height != 128 || bitmap.Size.Width != 86)
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard(
                                $"Error",
                                $@"The sprite must be 86x128. Are you trying to load a sheet or different a different game's sprite?",
                                MsBox.Avalonia.Enums.ButtonEnum.Ok,
                                MsBox.Avalonia.Enums.Icon.Info
                            );
                            await box.ShowAsPopupAsync(this);
                            return;
                        }
                        bwpath = files[0].Path.LocalPath;
                        BWIn.Source = bitmap;
                    }
                    else if (mode == 1)
                    {
                        if (bitmap.Size.Height != 80 || bitmap.Size.Width != 80)
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard(
                                $"Error",
                                $@"The sprite must be 80x80. Are you trying to load a sheet or different a different game's sprite?",
                                MsBox.Avalonia.Enums.ButtonEnum.Ok,
                                MsBox.Avalonia.Enums.Icon.Info
                            );
                            await box.ShowAsPopupAsync(this);
                            return;
                        }
                        ptpath = files[0].Path.LocalPath;
                        PtIn.Source = bitmap;
                    }   
                }
            }
        }
        catch { }
    }

    private async void Split_OnClick(object? sender, RoutedEventArgs e)
    {
        bwout1 = new MemoryStream();
        bwout2 = new MemoryStream();
        ptout1 = new MemoryStream();
        ptout2 = new MemoryStream();
        string filepath = "";
        if (mode == 0)
        {
            filepath = bwpath;
        }
        else if (mode == 1)
        {
            filepath = ptpath;
        }
        if (filepath == null)
        {
            return;
        }
        using (var inStream = File.OpenRead(filepath))
        using (var outStream = new MemoryStream())    
        using (Image<Rgb24> largerCanvas = new Image<Rgb24>(64, 100))
        using (var image = SixLabors.ImageSharp.Image.Load(inStream))
        {
            largerCanvas.Mutate<Rgb24>(ctx =>
            {
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(0, 0, 64, 64))), 
                    new Point(0, 0), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(64, 0, 16, 8))), 
                    new Point(0, 64), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(64, 8, 16, 8))), 
                    new Point(16, 64), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(64, 16, 16, 8))), 
                    new Point(32, 64), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(64, 24, 16, 8))), 
                    new Point(48, 64), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(64, 32, 16, 8))), 
                    new Point(0, 72), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(64, 40, 16, 8))), 
                    new Point(16, 72), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(64, 48, 16, 8))), 
                    new Point(32, 72), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(64, 56, 16, 8))), 
                    new Point(48, 72), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(0, 64, 32, 8))), 
                    new Point(0, 80), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(32, 64, 32, 8))), 
                    new Point(0, 88), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(0, 72, 32, 8))), 
                    new Point(32, 80), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(32, 72, 32, 8))), 
                    new Point(32, 88), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(64, 64, 8, 4))), 
                    new Point(0, 96), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(64, 68, 8, 4))), 
                    new Point(8, 96), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(72, 64, 8, 4))), 
                    new Point(16, 96), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(72, 68, 8, 4))), 
                    new Point(24, 96), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(64, 72, 8, 4))), 
                    new Point(32, 96), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(64, 76, 8, 4))), 
                    new Point(40, 96), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(72, 72, 8, 4))), 
                    new Point(48, 96), 1f);
                ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle(72, 76, 8, 4))), 
                    new Point(56, 96), 1f);
                
            });
            largerCanvas.SaveAsPng(outStream);
            outStream.Seek(0, SeekOrigin.Begin);
            if (mode == 0)
            {
                largerCanvas.SaveAsPng(bwout2);
                BWOut2.Source = new Bitmap(outStream);
            }
            else if (mode == 1)
            {
                largerCanvas.SaveAsPng(ptout2);
                PtOut2.Source = new Bitmap(outStream);
            }            
            AltSplice(largerCanvas);
        }
    }

    public void AltSplice(Image<Rgb24> image)
    {
        int x2 = 0;
        int y2 = 0;
        var largerCanvas = new Image<Rgb24>(80, 80);
        largerCanvas.Mutate<Rgb24>(ctx =>
        {
            for (int y = 0; y < 80; y += 4)
            {
                for (int x = 0; x < 80; x += 8)
                {
                    try
                    {
                        if (y2 >= 96)
                        {
                            break;
                        }
                        else
                        {
                            var tile = image.Clone(i => i.Crop(new Rectangle(x2, y2, 8, 8)));
                            ctx.DrawImage(tile, new Point(x, y), 1f);
                            x2 += 8;
                            if (x2 >= 64)
                            {
                                x2 = 0;
                                y2 += 8;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                if (y2 < 96)
                {
                    y += 4;
                }
            }

            x2 = 0;
            for (int x = 0; x < 32; x += 8)
            {
                for (int y = 0; y < 8; y += 4)
                {
                    var tile = image.Clone(i => i.Crop(new Rectangle(x2, 96, 8, 4)));
                    ctx.DrawImage(tile, new Point(x + 48, y + 72), 1f);
                    x2 += 8;
                }
            }
        });
        MemoryStream outStream = new MemoryStream();
        largerCanvas.SaveAsPng(outStream);
        outStream.Seek(0, SeekOrigin.Begin);
        if (mode == 0)
        {
            largerCanvas.SaveAsPng(bwout1);
            BWOut1.Source = new Bitmap(outStream);
        }
        else if (mode == 1)
        {
            largerCanvas.SaveAsPng(ptout1);
            PtOut1.Source = new Bitmap(outStream);
        }   
    }

    private async void BWSave1(object? sender, RoutedEventArgs e)
    {
        var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Spliced Sprite",
            FileTypeChoices = new List<FilePickerFileType>
            {
                new FilePickerFileType("Sprite")
                {
                    Patterns = new List<string> { "*.png" }
                }
            }
        });
        if (file == null)
        {
            Console.WriteLine("Save file operation canceled.");
            return;
        }
        bwout1.Seek(0, SeekOrigin.Begin);
        File.WriteAllBytes(file.Path.LocalPath, bwout1.ToArray());
    }
    
    private async void BWSave2(object? sender, RoutedEventArgs e)
    {
        var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Spliced Sprite",
            FileTypeChoices = new List<FilePickerFileType>
            {
                new FilePickerFileType("Sprite")
                {
                    Patterns = new List<string> { "*.png" }
                }
            }
        });
        if (file == null)
        {
            Console.WriteLine("Save file operation canceled.");
            return;
        }
        bwout2.Seek(0, SeekOrigin.Begin);
        File.WriteAllBytes(file.Path.LocalPath, bwout2.ToArray());
    }
    
    private async void PtSave1(object? sender, RoutedEventArgs e)
    {
        var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Spliced Sprite",
            FileTypeChoices = new List<FilePickerFileType>
            {
                new FilePickerFileType("Sprite")
                {
                    Patterns = new List<string> { "*.png" }
                }
            }
        });
        if (file == null)
        {
            Console.WriteLine("Save file operation canceled.");
            return;
        }
        ptout1.Seek(0, SeekOrigin.Begin);
        File.WriteAllBytes(file.Path.LocalPath, ptout1.ToArray());
    }
    
    private async void PtSave2(object? sender, RoutedEventArgs e)
    {
        var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Spliced Sprite",
            FileTypeChoices = new List<FilePickerFileType>
            {
                new FilePickerFileType("Sprite")
                {
                    Patterns = new List<string> { "*.png" }
                }
            }
        });
        if (file == null)
        {
            Console.WriteLine("Save file operation canceled.");
            return;
        }
        ptout2.Seek(0, SeekOrigin.Begin);
        File.WriteAllBytes(file.Path.LocalPath, ptout2.ToArray());
    }

    private void Switch_OnClick(object? sender, RoutedEventArgs e)
    {
        mode = (mode + 1) % 2;
        if (mode == 0)
        {
            Switch.Content = "Platinum";
            Title.Text = "Pokemon Sprite Splitter - BW";
            PtGrid.IsVisible = false;
            BWGrid.IsVisible = true;
        }
        else if (mode == 1)
        {
            Switch.Content = "BW";
            Title.Text = "Pokemon Sprite Splitter - Platinum";
            PtGrid.IsVisible = true;
            BWGrid.IsVisible = false;
        }
    }

    private async void Batch_OnClick(object? sender, RoutedEventArgs e)
    {
        var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import Sheet",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new FilePickerFileType("Sprite Sheet")
                {
                    Patterns = new List<string> { "*.png" }
                }
            }
        });
        batchpath = files[0].Path.LocalPath;
        if (files.Count == 1)
        {
            if (mode == 0)
            {
                Batch.BWSplit(batchpath, this);
            }
            else if (mode == 1)
            {
                Batch.PtSplit(batchpath, this);
            }
        }
    }
}