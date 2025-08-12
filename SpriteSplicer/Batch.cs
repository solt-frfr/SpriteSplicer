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
using System.Reflection;
using SharpCompress;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace SpriteSplicer;

public class Batch
{
    public static async void BWSplit(string filepath, Window parent)
    {
        Directory.CreateDirectory(Paths.temp);
        Directory.CreateDirectory(Path.Combine(Paths.temp, "Uncut"));
        Directory.CreateDirectory(Path.Combine(Paths.temp, "80x80"));
        Directory.CreateDirectory(Path.Combine(Paths.temp, "64x100"));
        string uncutpath = Path.Combine(Paths.temp, "Uncut");
        string outputpath = "";
        var files = await parent.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Save Sprites",
            FileTypeChoices = new List<FilePickerFileType>
            {
                new FilePickerFileType("ZIP Archive")
                {
                    Patterns = new List<string> { "*.zip" }
                }
            }
        });
        if (files != null)
        {
            if (!string.IsNullOrWhiteSpace(files.Path.LocalPath))
            {
                outputpath = files.Path.LocalPath;
            }
        }
        else
        {
            return;
        }
        int index = 0;
        using (var inStream = File.OpenRead(filepath))
        using (var outStream = new MemoryStream())    
        using (var image = SixLabors.ImageSharp.Image.Load(inStream))
        {
            for (int y = 0; y < 1537; y += 128)
            {
                for (int x = 0; x < 603; x += 86)
                {
                    try
                    {
                        if (index >= 95)
                        {
                            break;
                        }
                        else if (index == 37 || index == 38 || index == 40)
                        {
                            var sprite = image.Clone(i => i.Crop(new Rectangle(x, y, 256, 128)));
                            sprite.SaveAsPng(Path.Combine(uncutpath, ((BWSprites)index).ToString() + ".png"));
                            x += 170;
                        }
                        else
                        {
                            var sprite = image.Clone(i => i.Crop(new Rectangle(x, y, 86, 128)));
                            sprite.SaveAsPng(Path.Combine(uncutpath, ((BWSprites)index).ToString() + ".png"));
                        }
                        index++;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
        string[] sprites = Directory.GetFiles(Path.Combine(Paths.temp, "Uncut"), "*.png", SearchOption.AllDirectories);
        foreach (string sprite in sprites)
        {
            GenericSplit(sprite);
        }
        using var archive = SharpCompress.Archives.Zip.ZipArchive.Create();
        archive.AddAllFromDirectory(Paths.temp);
        archive.SaveTo(outputpath, SharpCompress.Common.CompressionType.Deflate);
        Directory.Delete(Paths.temp, true);
    }
    public static async void PtSplit(string filepath, Window parent)
    {
        Directory.CreateDirectory(Paths.temp);
        Directory.CreateDirectory(Path.Combine(Paths.temp, "Uncut"));
        Directory.CreateDirectory(Path.Combine(Paths.temp, "80x80"));
        Directory.CreateDirectory(Path.Combine(Paths.temp, "64x100"));
        string uncutpath = Path.Combine(Paths.temp, "Uncut");
        string outputpath = "";
        var files = await parent.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Save Sprites",
            FileTypeChoices = new List<FilePickerFileType>
            {
                new FilePickerFileType("ZIP Archive")
                {
                    Patterns = new List<string> { "*.zip" }
                }
            }
        });
        if (files != null)
        {
            if (!string.IsNullOrWhiteSpace(files.Path.LocalPath))
            {
                outputpath = files.Path.LocalPath;
            }
        }
        int index = 0;
        using (var inStream = File.OpenRead(filepath))
        using (var outStream = new MemoryStream())    
        using (var image = SixLabors.ImageSharp.Image.Load(inStream))
        {
            for (int y = 18; y < 1602; y += 98)
            {
                for (int x = 1; x < 973; x += 81)
                {
                    try
                    {
                        if (index >= 105)
                        {
                            break;
                        }
                        else if (index == 101)
                        {
                            
                        }
                        else if (Enum.IsDefined(typeof(PtAnimSprites), index))
                        {
                            var sprite = image.Clone(i => i.Crop(new Rectangle(x, y, 242, 80)));
                            sprite.SaveAsPng(Path.Combine(uncutpath, ((PtSprites)index).ToString() + ".png"));
                            x += 162;
                        }
                        else
                        {
                            var sprite = image.Clone(i => i.Crop(new Rectangle(x, y, 80, 80)));
                            sprite.SaveAsPng(Path.Combine(uncutpath, ((PtSprites)index).ToString() + ".png"));
                        }
                        if (Enum.IsDefined(typeof(PtRegionalSprites), index))
                        {
                            x += 81;
                        }
                        if (Enum.IsDefined(typeof(PtEndSprites), index))
                        {
                            x += 973;
                        }
                        if (index == 101)
                        {
                            y += 16;
                        }
                        index++;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
        string[] sprites = Directory.GetFiles(Path.Combine(Paths.temp, "Uncut"), "*.png", SearchOption.AllDirectories);
        for (int i = 0; i < sprites.Length; i++)
        {
            string sprite = Path.GetFileNameWithoutExtension(sprites[i]);
            if (Enum.IsDefined(typeof(PtAnimSprites), sprite))
            {
                int j = (int)(PtAnimSprites)Enum.Parse(typeof(PtAnimSprites), sprite);
                var data = GetPtValues(j);
                LongSplit(sprites[i], data.Item1, data.Item2, data.Item3);
            }
            else
            {
                GenericSplit(sprites[i]);
            }
        }
        using var archive = SharpCompress.Archives.Zip.ZipArchive.Create();
        archive.AddAllFromDirectory(Paths.temp);
        archive.SaveTo(outputpath, SharpCompress.Common.CompressionType.Deflate);
        Directory.Delete(Paths.temp, true);
    }
    
    private static async void GenericSplit(string filepath)
    {
        using (var inStream = File.OpenRead(filepath))
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
            largerCanvas.SaveAsPng(Path.Combine(Paths.temp, "64x100", Path.GetFileName(filepath)));
            GenericAltSplice(largerCanvas, filepath);
        }
    }
    
    private static async void LongSplit(string filepath, int[] banks, int width, int height)
    {
        bool isodd = (banks.Length % 2 == 1);
        int count = banks.Length;
        if (isodd)
        {
            count -= 1;
        }
        using (var inStream = File.OpenRead(filepath))
        using (Image<Rgb24> largerCanvas = new Image<Rgb24>(64, banks.Length * 100))
        using (var image = SixLabors.ImageSharp.Image.Load(inStream))
        {
            largerCanvas.Mutate<Rgb24>(ctx =>
            {
                for (var j = 0; j < count; j += 2)
                {
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j] * 81) + 0, 0, 64, 64))), 
                        new Point(0, 0 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j] * 81) + 64, 0, 16, 8))), 
                        new Point(0, 64 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j] * 81) + 64, 8, 16, 8))), 
                        new Point(16, 64 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j] * 81) + 64, 16, 16, 8))), 
                        new Point(32, 64 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j] * 81) + 64, 24, 16, 8))), 
                        new Point(48, 64 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j] * 81) + 64, 32, 16, 8))), 
                        new Point(0, 72 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j] * 81) + 64, 40, 16, 8))), 
                        new Point(16, 72 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j] * 81) + 64, 48, 16, 8))), 
                        new Point(32, 72 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j] * 81) + 64, 56, 16, 8))), 
                        new Point(48, 72 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j] * 81) + 0, 64, 32, 8))), 
                        new Point(0, 80 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j] * 81) + 32, 64, 32, 8))), 
                        new Point(0, 88 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j] * 81) + 0, 72, 32, 8))), 
                        new Point(32, 80 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j] * 81) + 32, 72, 32, 8))), 
                        new Point(32, 88 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j] * 81) + 64, 64, 16, 8))), 
                        new Point(0, 96 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j] * 81) + 64, 72, 16, 8))), 
                        new Point(16, 96 + (j * 100)), 1f);
                    
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j + 1] * 81) + 0, 0, 32, 64))), 
                        new Point(32, 96 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j + 1] * 81) + 32, 0, 32, 64))), 
                        new Point(0, 104 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j + 1] * 81) + 64, 0, 16, 8))), 
                        new Point(32, 160 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j + 1] * 81) + 64, 8, 16, 8))), 
                        new Point(48, 160 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j + 1] * 81) + 64, 16, 16, 8))), 
                        new Point(0, 168 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j + 1] * 81) + 64, 24, 16, 8))), 
                        new Point(16, 168 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j + 1] * 81) + 64, 32, 16, 8))), 
                        new Point(32, 168 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j + 1] * 81) + 64, 40, 16, 8))), 
                        new Point(48, 168 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j + 1] * 81) + 64, 48, 16, 8))), 
                        new Point(0, 176 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j + 1] * 81) + 64, 56, 16, 8))), 
                        new Point(16, 176 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j + 1] * 81) + 0, 64, 32, 8))), 
                        new Point(32, 176 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j + 1] * 81) + 32, 64, 32, 8))), 
                        new Point(32, 184 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j + 1] * 81) + 0, 72, 32, 8))), 
                        new Point(0, 184 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j + 1] * 81) + 32, 72, 32, 8))), 
                        new Point(0, 192 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j + 1] * 81) + 64, 64, 16, 8))), 
                        new Point(32, 192 + (j * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[j + 1] * 81) + 64, 72, 16, 8))), 
                        new Point(48, 192 + (j * 100)), 1f);
                }

                if (isodd)
                {
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 0, 0, 64, 64))), 
                        new Point(0, 0 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 64, 0, 16, 8))), 
                        new Point(0, 64 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 64, 8, 16, 8))), 
                        new Point(16, 64 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 64, 16, 16, 8))), 
                        new Point(32, 64 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 64, 24, 16, 8))), 
                        new Point(48, 64 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 64, 32, 16, 8))), 
                        new Point(0, 72 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 64, 40, 16, 8))), 
                        new Point(16, 72 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 64, 48, 16, 8))), 
                        new Point(32, 72 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 64, 56, 16, 8))), 
                        new Point(48, 72 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 0, 64, 32, 8))), 
                        new Point(0, 80 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 32, 64, 32, 8))), 
                        new Point(0, 88 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 0, 72, 32, 8))), 
                        new Point(32, 80 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 32, 72, 32, 8))), 
                        new Point(32, 88 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 64, 64, 8, 4))), 
                        new Point(0, 96 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 64, 68, 8, 4))), 
                        new Point(8, 96 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 72, 64, 8, 4))), 
                        new Point(16, 96 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 72, 68, 8, 4))), 
                        new Point(24, 96 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 64, 72, 8, 4))), 
                        new Point(32, 96 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 64, 76, 8, 4))), 
                        new Point(40, 96 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 72, 72, 8, 4))), 
                        new Point(48, 96 + (count * 100)), 1f);
                    ctx.DrawImage(image.Clone(i => i.Crop(new Rectangle((banks[count] * 81) + 72, 76, 8, 4))), 
                        new Point(56, 96 + (count * 100)), 1f);    
                }
            });
            largerCanvas.SaveAsPng(Path.Combine(Paths.temp, "64x100", Path.GetFileName(filepath)));
            LongAltSplice(largerCanvas, filepath, width, height, count, isodd);
        }
    }
    
    public static void GenericAltSplice(Image<Rgb24> image, string filepath)
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
        largerCanvas.SaveAsPng(Path.Combine(Paths.temp, "80x80", Path.GetFileName(filepath)));
    }
    
    public static void LongAltSplice(Image<Rgb24> image, string filepath, int width, int height, int count, bool isodd)
    {
        int x2 = 0;
        int y2 = 0;
        int count2 = count;
        if (isodd)
        {
            count2 += 1;
        }
        var largerCanvas = new Image<Rgb24>(width, height);
        largerCanvas.Mutate<Rgb24>(ctx =>
        {
            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 8)
                {
                    try
                    {
                        if (y2 >= count2 * 100 - 4)
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
                if (y2 < count2 * 100 - 4)
                {
                    y += 4;
                }
            }

            x2 = 0;
            for (int x = 0; x < 32; x += 8)
            {
                for (int y = 0; y < 8; y += 4)
                {
                    var tile = image.Clone(i => i.Crop(new Rectangle(x2, count2 * 100 - 4, 8, 4)));
                    ctx.DrawImage(tile, new Point(x + 48, y + 72), 1f);
                    x2 += 8;
                }
            }
        });
        largerCanvas.SaveAsPng(Path.Combine(Paths.temp, "80x80", Path.GetFileName(filepath)));
    }
    
    // ReSharper disable InconsistentNaming
    public enum BWSprites
    {
        a072_000 = 0,
        a072_008 = 1,
        a072_016 = 2,
        a072_024 = 3,
        a072_032 = 4,
        a072_040 = 5,
        a072_048 = 6,
        a072_056 = 7,
        a072_064 = 8,
        a072_072 = 9,
        a072_080 = 10,
        a072_088 = 11,
        a072_096 = 12,
        a072_104 = 13,
        a072_112 = 14,
        a072_120 = 15,
        a072_128 = 16,
        a072_136 = 17,
        a072_144 = 18,
        a072_152 = 19,
        a072_160 = 20,
        a072_168 = 21,
        a072_176 = 22,
        a072_184 = 23,
        a072_192 = 24,
        a072_200 = 25,
        a072_208 = 26,
        a072_216 = 27,
        a072_224 = 28,
        a072_232 = 29,
        a072_240 = 30,
        a072_248 = 31,
        a072_256 = 32,
        a072_264 = 33,
        a072_272 = 34,
        a072_280 = 35,
        a072_288 = 36,
        a072_296 = 37,
        a072_304 = 38,
        a072_312 = 39,
        a072_320 = 40,
        a072_328 = 41,
        a072_336 = 42,
        a072_344 = 43,
        a072_352 = 44,
        a072_360 = 45,
        a072_368 = 46,
        a072_376 = 47,
        a072_384 = 48,
        a072_392 = 49,
        a072_400 = 50,
        a072_408 = 51,
        a072_416 = 52,
        a072_424 = 53,
        a072_432 = 54,
        a072_440 = 55,
        a072_448 = 56,
        a072_456 = 57,
        a072_464 = 58,
        a072_472 = 59,
        a072_480 = 60,
        a072_488 = 61,
        a072_496 = 62,
        a072_504 = 63,
        a072_512 = 64,
        a072_520 = 65,
        a072_528 = 66,
        a072_536 = 67,
        a072_544 = 68,
        a072_552 = 69,
        a072_560 = 70,
        a072_568 = 71,
        a072_576 = 72,
        a072_584 = 73,
        a072_592 = 74,
        a072_600 = 75,
        a072_608 = 76,
        a072_616 = 77,
        a072_624 = 78,
        a072_632 = 79,
        a072_640 = 80,
        a072_648 = 81,
        a072_656 = 82,
        a072_664 = 83,
        a072_672 = 84,
        a072_680 = 85,
        a072_688 = 86,
        a072_696 = 87,
        a072_704 = 88,
        a072_712 = 89,
        a072_720 = 90,
        a072_728 = 91,
        a072_736 = 92,
        a072_744 = 93,
        a072_752 = 94
    }
    public enum PtSprites
    {
        trfgra_0 = 0,
        trfgra_5 = 1,
        trfgra_315 = 2, // Barry (3)
        trfgra_10 = 3,
        trfgra_15 = 4,
        trfgra_20 = 5,
        trfgra_25 = 6,
        trfgra_30 = 7,
        trfgra_35 = 8,
        trfgra_40 = 9, // end
        trfgra_45 = 10,
        trfgra_50 = 11,
        trfgra_55 = 12,
        trfgra_60 = 13,
        trfgra_65 = 14,
        trfgra_70 = 15,
        trfgra_75 = 16,
        trfgra_80 = 17,
        trfgra_85 = 18,
        trfgra_90 = 19,
        trfgra_95 = 20,
        trfgra_100 = 21, // end
        trfgra_105 = 22,
        trfgra_110 = 23,
        trfgra_115 = 24,
        trfgra_120 = 25,
        trfgra_125 = 26,
        trfgra_130 = 27,
        trfgra_135 = 28,
        trfgra_140 = 29,
        trfgra_145 = 30,
        trfgra_150 = 31,
        trfgra_155 = 32,
        trfgra_160 = 33, // end
        trfgra_165 = 34,
        trfgra_170 = 35,
        trfgra_175 = 36,
        trfgra_180 = 37,
        trfgra_185 = 38,
        trfgra_190 = 39,
        trfgra_195 = 40,
        trfgra_200 = 41,
        trfgra_205 = 42,
        trfgra_210 = 43, // Swimmer, skip next sprite
        trfgra_215 = 44, // end
        trfgra_220 = 45,
        trfgra_225 = 46,
        trfgra_230 = 47,
        trfgra_240 = 48,
        trfgra_245 = 49,
        trfgra_250 = 50,
        trfgra_255 = 51,
        trfgra_260 = 52,
        trfgra_265 = 53,
        trfgra_270 = 54,
        trfgra_275 = 55,
        trfgra_280 = 56, // end
        trfgra_285 = 57,
        trfgra_290 = 58,
        trfgra_295 = 59,
        trfgra_300 = 60,
        trfgra_305 = 61,
        trfgra_350 = 62,
        trfgra_355 = 63,
        trfgra_400 = 64,
        trfgra_405 = 65, // end
        trfgra_410 = 66,
        trfgra_415 = 67,
        trfgra_420 = 68,
        trfgra_425 = 69, // nice
        trfgra_490 = 70,
        trfgra_475 = 71,
        trfgra_480 = 72, // end
        trfgra_310 = 73, // Roark [0, 1, 1, 1, 1, 2, 2]
        trfgra_370 = 74, // Gardenia [0, 1, 1, 2, 2, 2]
        trfgra_380 = 75, // Maylene [0, 0, 1, 1, 2, 2, 2]
        trfgra_375 = 76, // Crasher Wake [0, 0, 1, 2] end
        trfgra_385 = 77, // Fantina [0, 0, 1, 1, 1, 2, 2, 2]
        trfgra_320 = 78, // Byron [0, 1, 1, 2, 2]
        trfgra_390 = 79, // Candice [0, 1, 1, 1, 2, 2]
        trfgra_395 = 80, // Volkner [0, 0, 1, 1, 2, 2] end
        trfgra_325 = 81, // Aaron [0, 0, 0, 0, 1, 1, 1, 2, 2]
        trfgra_330 = 82, // Bertha [0, 1, 2]
        trfgra_335 = 83, // Flint [0, 0, 1, 2]
        trfgra_340 = 84, // Lucian [0, 1, 1, 2, 2] end
        trfgra_365 = 85,
        trfgra_445 = 86,
        trfgra_360 = 87,
        trfgra_435 = 88,
        trfgra_440 = 89,
        trfgra_430 = 90, // Cyrus [0, 1, 1, 2, 2, 2] end
        trfgra_345 = 91, // Cynthia [0, 1, 1, 2, 2]
        trfgra_450 = 92, // Cheryl [0, 0, 1, 1, 1, 1, 2, 2]
        trfgra_455 = 93, // Riley [0, 1, 1, 2, 0]
        trfgra_460 = 94, // Marley [0, 1, 2] end
        trfgra_465 = 95, // Buck [0, 1, 2, 2, 2]
        trfgra_470 = 96, // Mira [0, 0, 1, 1, 1, 2, 2, 2] end
        trfgra_485 = 97, // Palmer [0, 1, 2, 2, 2]
        trfgra_495 = 98, // Argenta [0, 1, 1, 2, 2, 2]
        trfgra_500 = 99, // Thorton [0, 0, 0, 1, 2]
        trfgra_505 = 100, // Dahlia [0, 1, 1, 1, 1, 1, 1, 2, 2, 0, 2] end
        trfgra_510 = 101, // Caitlin and Daroach. Needs special runing. end
        trfgra_235 = 102, // swimmer duo, skip next
        trfgra_515 = 103,
        trfgra_520 = 104, // end
    }

    public enum PtEndSprites
    {
        trfgra_40 = 9, // end
        trfgra_100 = 21, // end
        trfgra_160 = 33, // end
        trfgra_215 = 44, // end
        trfgra_280 = 56, // end
        trfgra_405 = 65, // end
        trfgra_480 = 72, // end
        trfgra_375 = 76, // Crasher Wake [0, 0, 1, 2] end
        trfgra_395 = 80, // Volkner [0, 0, 1, 1, 2, 2] end
        trfgra_340 = 84, // Lucian [0, 1, 1, 2, 2] end
        trfgra_430 = 90, // Cyrus [0, 1, 1, 2, 2, 2] end
        trfgra_460 = 94, // Marley [0, 1, 2] end
        trfgra_470 = 96, // Mira [0, 0, 1, 1, 1, 2, 2, 2] end
        trfgra_505 = 100, // Dahlia [0, 1, 1, 1, 1, 1, 1, 2, 2, 0, 2] end
        trfgra_510 = 101, // Caitlin and Daroach. Needs special runing. end
        trfgra_520 = 104, // end

    }

    public enum PtAnimSprites
    {
        trfgra_315 = 2, // Barry 256x150 [0, 0, 1, 1, 2, 2]
        trfgra_310 = 73, // Roark 256x175 [0, 1, 1, 1, 1, 2, 2]
        trfgra_370 = 74, // Gardenia 256x150 [0, 1, 1, 2, 2, 2]
        trfgra_380 = 75, // Maylene 256x175 [0, 0, 1, 1, 2, 2, 2]
        trfgra_375 = 76, // Crasher Wake 160x160 [0, 0, 1, 2] end
        trfgra_385 = 77, // Fantina 256x200 [0, 0, 1, 1, 1, 2, 2, 2]
        trfgra_320 = 78, // Byron 256x125 [0, 1, 1, 2, 2]
        trfgra_390 = 79, // Candice 256x150 [0, 1, 1, 1, 2, 2]
        trfgra_395 = 80, // Volkner 256x150 [0, 0, 1, 1, 2, 2] end
        trfgra_325 = 81, // Aaron 240x240 [0, 0, 0, 0, 1, 1, 1, 2, 2]
        trfgra_330 = 82, // Bertha 256x75 [0, 1, 2]
        trfgra_335 = 83, // Flint 160x160 [0, 0, 1, 2]
        trfgra_340 = 84, // Lucian 256x125 [0, 1, 1, 2, 2] end
        trfgra_430 = 90, // Cyrus 256x150 [0, 1, 1, 2, 2, 2] end
        trfgra_345 = 91, // Cynthia 256x125 [0, 1, 1, 2, 2]
        trfgra_450 = 92, // Cheryl 256x200 [0, 0, 1, 1, 1, 1, 2, 2]
        trfgra_455 = 93, // Riley 256x125 [0, 1, 1, 2, 0]
        trfgra_460 = 94, // Marley 256x75 [0, 1, 2] end
        trfgra_465 = 95, // Buck 256x125 [0, 1, 2, 2, 2]
        trfgra_470 = 96, // Mira 256x200 [0, 0, 1, 1, 1, 2, 2, 2] end
        trfgra_485 = 97, // Palmer 256x125 [0, 1, 2, 2, 2]
        trfgra_495 = 98, // Argenta 256x150 [0, 1, 1, 2, 2, 2]
        trfgra_500 = 99, // Thorton 256x125 [0, 0, 0, 1, 2]
        trfgra_505 = 100, // Dahlia 256x275 [0, 1, 1, 1, 1, 1, 1, 2, 2, 0, 2] end
    }
    
    static (int[], int, int) GetPtValues(int value)
    {
        int[] anim = value switch
        {
            2 => new[] { 0, 0, 1, 1, 2, 2 },
            73 => new[] { 0, 1, 1, 1, 1, 2, 2 },
            74 => new[] { 0, 1, 1, 2, 2, 2 },
            75 => new[] { 0, 0, 1, 1, 2, 2, 2 },
            76 => new[] { 0, 0, 1, 2 },
            77 => new[] { 0, 0, 1, 1, 1, 2, 2, 2 },
            78 => new[] { 0, 1, 1, 2, 2 },
            79 => new[] { 0, 1, 1, 1, 2, 2 },
            80 => new[] { 0, 0, 1, 1, 2, 2 },
            81 => new[] { 0, 0, 0, 0, 1, 1, 1, 2, 2 },
            82 => new[] { 0, 1, 2 },
            83 => new[] { 0, 0, 1, 2 },
            84 => new[] { 0, 1, 1, 2, 2 },
            90 => new[] { 0, 1, 1, 2, 2, 2 },
            91 => new[] { 0, 1, 1, 2, 2 },
            92 => new[] { 0, 0, 1, 1, 1, 1, 2, 2 },
            93 => new[] { 0, 1, 1, 2, 0 },
            94 => new[] { 0, 1, 2 },
            95 => new[] { 0, 1, 2, 2, 2 },
            96 => new[] { 0, 0, 1, 1, 1, 2, 2, 2 },
            97 => new[] { 0, 1, 2, 2, 2 },
            98 => new[] { 0, 1, 1, 2, 2, 2 },
            99 => new[] { 0, 0, 0, 1, 2 },
            100 => new[] { 0, 1, 1, 1, 1, 1, 1, 2, 2, 0, 2 },
            _ => Array.Empty<int>()
        };
        int width = value switch
        {
            2 => 256,
            73 => 256,
            74 => 256,
            75 => 256,
            76 => 160,
            77 => 256,
            78 => 256,
            79 => 256,
            80 => 256,
            81 => 240,
            82 => 256,
            83 => 160,
            84 => 256,
            90 => 256,
            91 => 256,
            92 => 256,
            93 => 256,
            94 => 256,
            95 => 256,
            96 => 256,
            97 => 256,
            98 => 256,
            99 => 256,
            100 => 256,
            _ => 80
        };
        int height = value switch
        {
            2 => 150,
            73 => 175,
            74 => 150,
            75 => 175,
            76 => 160,
            77 => 200,
            78 => 125,
            79 => 150,
            80 => 150,
            81 => 240,
            82 => 75,
            83 => 160,
            84 => 125,
            90 => 150,
            91 => 125,
            92 => 200,
            93 => 125,
            94 => 75,
            95 => 125,
            96 => 200,
            97 => 125,
            98 => 150,
            99 => 125,
            100 => 275,
            _ => 80
        };
        return (anim, width, height);
    }

    public enum PtRegionalSprites
    {
        trfgra_210 = 43, // Swimmer, skip next sprite
        trfgra_235 = 102, // swimmer duo, skip next
    }
}
public static class Paths
{
    public static readonly string program = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
    public static readonly string temp = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Temp");
}