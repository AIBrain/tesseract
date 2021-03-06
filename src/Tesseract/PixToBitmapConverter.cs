﻿using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Tesseract {

    public class PixToBitmapConverter {

        public Bitmap Convert( Pix pix, bool includeAlpha = false ) {
            var pixelFormat = this.GetPixelFormat( pix );
            var depth = pix.Depth;
            var img = new Bitmap( pix.Width, pix.Height, pixelFormat );

            BitmapData imgData = null;
            try {
                // TODO: Set X and Y resolution

                // transfer pixel data
                if ( ( pixelFormat & PixelFormat.Indexed ) == PixelFormat.Indexed ) {
                    this.TransferPalette( pix, img );
                }

                // transfer data
                var pixData = pix.GetData();
                imgData = img.LockBits( new Rectangle( 0, 0, img.Width, img.Height ), ImageLockMode.WriteOnly, pixelFormat );

                switch ( depth ) {
                    case 32:
                        this.TransferData32( pixData, imgData, includeAlpha ? 0 : 255 );
                        break;

                    case 16:
                        this.TransferData16( pixData, imgData );
                        break;

                    case 8:
                        this.TransferData8( pixData, imgData );
                        break;

                    case 1:
                        this.TransferData1( pixData, imgData );
                        break;
                }
                return img;
            }
            catch ( Exception ) {
                img.Dispose();
                throw;
            }
            finally {
                if ( imgData != null ) {
                    img.UnlockBits( imgData );
                }
            }
        }

        private unsafe void TransferData32( PixData pixData, BitmapData imgData, int alphaMask ) {
            var imgFormat = imgData.PixelFormat;
            var height = imgData.Height;
            var width = imgData.Width;

            for ( var y = 0 ; y < height ; y++ ) {
                var imgLine = ( byte* )imgData.Scan0 + ( y * imgData.Stride );
                var pixLine = ( uint* )pixData.Data + ( y * pixData.WordsPerLine );

                for ( var x = 0 ; x < width ; x++ ) {
                    var pixVal = PixColor.FromRgba( pixLine[ x ] );

                    var pixelPtr = imgLine + ( x << 2 );
                    pixelPtr[ 0 ] = pixVal.Blue;
                    pixelPtr[ 1 ] = pixVal.Green;
                    pixelPtr[ 2 ] = pixVal.Red;
                    pixelPtr[ 3 ] = ( byte )( alphaMask | pixVal.Alpha ); // Allow user to include alpha or not
                }
            }
        }

        private unsafe void TransferData16( PixData pixData, BitmapData imgData ) {
            var imgFormat = imgData.PixelFormat;
            var height = imgData.Height;
            var width = imgData.Width;

            for ( var y = 0 ; y < height ; y++ ) {
                var pixLine = ( uint* )pixData.Data + ( y * pixData.WordsPerLine );
                var imgLine = ( ushort* )imgData.Scan0 + ( y * imgData.Stride );

                for ( var x = 0 ; x < width ; x++ ) {
                    var pixVal = ( ushort )PixData.GetDataTwoByte( pixLine, x );

                    imgLine[ x ] = pixVal;
                }
            }
        }

        private unsafe void TransferData8( PixData pixData, BitmapData imgData ) {
            var imgFormat = imgData.PixelFormat;
            var height = imgData.Height;
            var width = imgData.Width;

            for ( var y = 0 ; y < height ; y++ ) {
                var pixLine = ( uint* )pixData.Data + ( y * pixData.WordsPerLine );
                var imgLine = ( byte* )imgData.Scan0 + ( y * imgData.Stride );

                for ( var x = 0 ; x < width ; x++ ) {
                    var pixVal = ( byte )PixData.GetDataByte( pixLine, x );

                    imgLine[ x ] = pixVal;
                }
            }
        }

        private unsafe void TransferData1( PixData pixData, BitmapData imgData ) {
            var imgFormat = imgData.PixelFormat;
            var height = imgData.Height;
            var width = imgData.Width / 8;

            for ( var y = 0 ; y < height ; y++ ) {
                var pixLine = ( uint* )pixData.Data + ( y * pixData.WordsPerLine );
                var imgLine = ( byte* )imgData.Scan0 + ( y * imgData.Stride );

                for ( var x = 0 ; x < width ; x++ ) {
                    var pixVal = ( byte )PixData.GetDataByte( pixLine, x );

                    imgLine[ x ] = pixVal;
                }
            }
        }

        private void TransferPalette( Pix pix, Bitmap img ) {
            var pallete = img.Palette;
            var maxColors = pallete.Entries.Length;
            var lastColor = maxColors - 1;
            var colormap = pix.Colormap;
            if ( colormap != null && colormap.Count <= maxColors ) {
                var colormapCount = colormap.Count;
                for ( var i = 0 ; i < colormapCount ; i++ ) {
                    pallete.Entries[ i ] = ( Color )colormap[ i ];
                }
            }
            else {
                for ( var i = 0 ; i < maxColors ; i++ ) {
                    var value = ( byte )( i * 255 / lastColor );
                    pallete.Entries[ i ] = Color.FromArgb( value, value, value );
                }
            }
            // This is required to force the palette to update!
            img.Palette = pallete;
        }

        private PixelFormat GetPixelFormat( Pix pix ) {
            switch ( pix.Depth ) {
                case 1:
                    return PixelFormat.Format1bppIndexed;
                //case 2: return PixelFormat.Format4bppIndexed;
                //case 4: return PixelFormat.Format4bppIndexed;
                case 8:
                    return PixelFormat.Format8bppIndexed;

                case 16:
                    return PixelFormat.Format16bppGrayScale;

                case 32:
                    return PixelFormat.Format32bppArgb;

                default:
                    throw new InvalidOperationException( String.Format( "Pix depth {0} is not supported.", pix.Depth ) );
            }
        }
    }
}