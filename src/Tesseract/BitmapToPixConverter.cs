
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Tesseract
{
	/// <summary>
	/// Description of BitmapToPixConverter.
	/// </summary>
	public class BitmapToPixConverter
    {
	    public Pix Convert(Bitmap img)
        {
            var pixDepth = this.GetPixDepth(img.PixelFormat);
            var pix = Pix.Create(img.Width, img.Height, pixDepth);
            
            BitmapData imgData = null;
	        try {
                // TODO: Set X and Y resolution

                if ((img.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed) {
                    this.CopyColormap(img, pix);
                }

                // transfer data
                imgData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, img.PixelFormat);
                var pixData = pix.GetData();

                switch ( imgData.PixelFormat ) {
                    case PixelFormat.Format32bppArgb:
                        this.TransferDataFormat32bppArgb(imgData, pixData);
                        break;
                    case PixelFormat.Format24bppRgb:
                        this.TransferDataFormat24bppRgb(imgData, pixData);
                        break;
                    case PixelFormat.Format8bppIndexed:
                        this.TransferDataFormat8bppIndexed(imgData, pixData);
                        break;
                    case PixelFormat.Format1bppIndexed:
                        this.TransferDataFormat1bppIndexed(imgData, pixData);
                        break;
                }
                return pix;
            } catch (Exception) {
                pix.Dispose();
                throw;
            } finally {
                if (imgData != null) {
                    img.UnlockBits(imgData);
                }
            }
        }


        private void CopyColormap(Bitmap img, Pix pix)
        {
            var imgPalette = img.Palette;
            var imgPaletteEntries = imgPalette.Entries;
            var pixColormap = PixColormap.Create(pix.Depth);
            try {
                for (var i = 0; i < imgPaletteEntries.Length; i++) {
                    if (!pixColormap.AddColor((PixColor)imgPaletteEntries[i])) {
                        throw new InvalidOperationException(String.Format("Failed to add colormap entry {0}.", i));
                    }
                }
                pix.Colormap = pixColormap;
            } catch (Exception) {
                pixColormap.Dispose();
                throw;
            }
        }

        private int GetPixDepth(PixelFormat pixelFormat)
        {
            switch (pixelFormat) {
                case PixelFormat.Format1bppIndexed:
                    return 1;
                case PixelFormat.Format8bppIndexed:
                    return 8;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format24bppRgb:
                    return 32;
                default:
                    throw new InvalidOperationException(String.Format("Source bitmap's pixel format {0} is not supported.", pixelFormat));
            }
        }

        private unsafe void TransferDataFormat32bppArgb(BitmapData imgData, PixData pixData)
        {
            var imgFormat = imgData.PixelFormat;
            var height = imgData.Height;
            var width = imgData.Width;
           
            for (var y = 0; y < height; y++) {
                var imgLine = (byte*)imgData.Scan0 + (y * imgData.Stride);
                var pixLine = (uint*)pixData.Data + (y * pixData.WordsPerLine);

                for (var x = 0; x < width; x++) {
                    var pixelPtr = imgLine + (x << 2);
                    var blue = *pixelPtr;
                    var green = *(pixelPtr + 1);
                    var red = *(pixelPtr + 2);
                    var alpha = *(pixelPtr + 3);
                    PixData.SetDataFourByte(pixLine, x, BitmapHelper.EncodeAsRGBA(red, green, blue, alpha));
                }
            }
        }

        private unsafe void TransferDataFormat24bppRgb(BitmapData imgData, PixData pixData)
        {
            var imgFormat = imgData.PixelFormat;
            var height = imgData.Height;
            var width = imgData.Width;

            for (var y = 0; y < height; y++) {
                var imgLine = (byte*)imgData.Scan0 + (y * imgData.Stride);
                var pixLine = (uint*)pixData.Data + (y * pixData.WordsPerLine);

                for (var x = 0; x < width; x++) {
                    var pixelPtr = imgLine + x*3;
                    var blue = pixelPtr[0];
                    var green = pixelPtr[1];
                    var red = pixelPtr[2];
                    PixData.SetDataFourByte(pixLine, x, BitmapHelper.EncodeAsRGBA(red, green, blue, 255));
                }
            }
        }

        private unsafe void TransferDataFormat8bppIndexed(BitmapData imgData, PixData pixData)
        {
            var height = imgData.Height;
            var width = imgData.Width;

            for (var y = 0; y < height; y++) {
                var imgLine = (byte*)imgData.Scan0 + (y * imgData.Stride);
                var pixLine = (uint*)pixData.Data + (y * pixData.WordsPerLine);

                for (var x = 0; x < width; x++) {
                    var pixelVal = *(imgLine + x);
                    PixData.SetDataByte(pixLine, x, pixelVal);
                }
            }
        }

        private unsafe void TransferDataFormat1bppIndexed(BitmapData imgData, PixData pixData)
        {
            var height = imgData.Height;
            var width = imgData.Width/8;
            for (var y = 0; y < height; y++) {
                var imgLine = (byte*)imgData.Scan0 + (y * imgData.Stride);
                var pixLine = (uint*)pixData.Data + (y * pixData.WordsPerLine);

                for (var x = 0; x < width; x++) {
                    var pixelVal = BitmapHelper.GetDataByte(imgLine, x);
                    PixData.SetDataByte(pixLine, x, pixelVal);
                }
            }
        }
	}
}
