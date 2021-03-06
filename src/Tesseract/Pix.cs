﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Tesseract {

    using Interop;

    public sealed class Pix : DisposableBase {
        // Skew Defaults
        public const int DefaultBinarySearchReduction = 2; // binary search part

        public const int DefaultBinaryThreshold = 130;

        /// <summary>
        /// A small angle, in radians, for threshold checking. Equal to about 0.06 degrees.
        /// </summary>
        private const float VerySmallAngle = 0.001F;

        private static readonly List<int> AllowedDepths = new List<int> { 1, 2, 4, 8, 16, 32 };

        /// <summary>
        /// Used to lookup image formats by extension.
        /// </summary>
        private static readonly Dictionary<string, ImageFormat> ImageFomatLookup = new Dictionary<string, ImageFormat>
    		{
    			{ ".jpg", ImageFormat.JfifJpeg },
    			{ ".jpeg", ImageFormat.JfifJpeg },
    			{ ".gif", ImageFormat.Gif },
    			{ ".tif", ImageFormat.Tiff },
    			{ ".tiff", ImageFormat.Tiff },
    			{ ".png", ImageFormat.Png },
    			{ ".bmp", ImageFormat.Bmp }
    		};

        private PixColormap _colormap;

        #region Create\Load methods

        public static Pix Create( int width, int height, int depth ) {
            if ( !AllowedDepths.Contains( depth ) )
                throw new ArgumentException( "Depth must be 1, 2, 4, 8, 16, or 32 bits.", nameof( depth ) );

            if ( width <= 0 )
                throw new ArgumentException( "Width must be greater than zero", nameof( width ) );
            if ( height <= 0 )
                throw new ArgumentException( "Height must be greater than zero", nameof( height ) );

            var handle = LeptonicaApi.Native.pixCreate( width, height, depth );
            if ( handle == IntPtr.Zero )
                throw new InvalidOperationException( "Failed to create pix, this normally occurs because the requested image size is too large, please check Standard Error Output." );

            return Create( handle );
        }

        public static Pix Create( IntPtr handle ) {
            if ( handle == IntPtr.Zero )
                throw new ArgumentException( "Pix handle must not be zero (null).", nameof( handle ) );

            return new Pix( handle );
        }

        public static Pix LoadFromFile( string filename ) {
            var pixHandle = LeptonicaApi.Native.pixRead( filename );
            if ( pixHandle == IntPtr.Zero ) {
                throw new IOException( String.Format( "Failed to load image '{0}'.", filename ) );
            }
            return Create( pixHandle );
        }

        /// <summary>
        /// Creates a new pix instance using an existing handle to a pix structure.
        /// </summary>
        /// <remarks>
        /// Note that the resulting instance takes ownership of the data structure.
        /// </remarks>
        /// <param name="handle"></param>
        private Pix( IntPtr handle ) {
            if ( handle == IntPtr.Zero )
                throw new ArgumentNullException( nameof( handle ) );

            this.Handle = new HandleRef( this, handle );
            this.Width = LeptonicaApi.Native.pixGetWidth( this.Handle );
            this.Height = LeptonicaApi.Native.pixGetHeight( this.Handle );
            this.Depth = LeptonicaApi.Native.pixGetDepth( this.Handle );

            var colorMapHandle = LeptonicaApi.Native.pixGetColormap( this.Handle );
            if ( colorMapHandle != IntPtr.Zero ) {
                this._colormap = new PixColormap( colorMapHandle );
            }
        }

        #endregion Create\Load methods

        #region Properties

        public PixColormap Colormap {
            get {
                return this._colormap;
            }
            set {
                if ( value != null ) {
                    if ( LeptonicaApi.Native.pixSetColormap( this.Handle, value.Handle ) == 0 ) {
                        this._colormap = value;
                    }
                }
                else {
                    if ( LeptonicaApi.Native.pixDestroyColormap( this.Handle ) == 0 ) {
                        this._colormap = null;
                    }
                }
            }
        }

        public int Width { get; }

        public int Height { get; }

        public int Depth { get; }

        public PixData GetData() => new PixData( this );

        internal HandleRef Handle { get; private set; }
        #endregion Properties

        #region Save methods

        /// <summary>
        /// Saves the image to the specified file.
        /// </summary>
        /// <param name="filename">The path to the file.</param>
        /// <param name="format">The format to use when saving the image, if not specified the file extension is used to guess the format.</param>
        public void Save( string filename, ImageFormat? format = null ) {
            ImageFormat actualFormat;
            if ( !format.HasValue ) {
                // ReSharper disable once PossibleNullReferenceException
                var extension = Path.GetExtension( filename ).ToLowerInvariant();
                if ( !ImageFomatLookup.TryGetValue( extension, out actualFormat ) ) {
                    // couldn't find matching format, perhaps there is no extension or it's not recognised, fallback to default.
                    actualFormat = ImageFormat.Default;
                }
            }
            else {
                actualFormat = format.Value;
            }

            if ( LeptonicaApi.Native.pixWrite( filename, this.Handle, actualFormat ) != 0 ) {
                throw new IOException( String.Format( "Failed to save image '{0}'.", filename ) );
            }
        }

        #endregion Save methods

        #region Clone

        /// <summary>
        /// Increments this pix's reference count and returns a reference to the same pix data.
        /// </summary>
        /// <remarks>
        /// A "clone" is simply a reference to an existing pix. It is implemented this way because
        /// image can be large and hence expensive to copy and extra handles need to be made with a simple
        /// policy to avoid double frees and memory leaks.
        ///
        /// The general usage protocol is:
        /// <list type="number">
        /// 	<item>Whenever you want a new reference to an existing <see cref="Pix" /> call <see cref="Pix.Clone" />.</item>
        ///     <item>
        /// 		Always call <see cref="Pix.Dispose" /> on all references. This decrements the reference count and
        /// 		will destroy the pix when the reference count reaches zero.
        /// 	</item>
        /// </list>
        /// </remarks>
        /// <returns>The pix with it's reference count incremented.</returns>
        public Pix Clone() {
            var clonedHandle = LeptonicaApi.Native.pixClone( this.Handle );
            return new Pix( clonedHandle );
        }

        #endregion Clone

        #region Image manipulation

        /// <summary>
        /// Determines the scew angle and if confidence is high enough returns the descewed image as the result, otherwise returns clone of original image.
        /// </summary>
        /// <remarks>
        /// This binarizes if necessary and finds the skew angle.  If the
        /// angle is large enough and there is sufficient confidence,
        /// it returns a deskewed image; otherwise, it returns a clone.
        /// </remarks>
        /// <returns>Returns deskewed image if confidence was high enough, otherwise returns clone of original pix.</returns>
        public Pix Deskew() {
            Scew scew;
            return this.Deskew( DefaultBinarySearchReduction, out scew );
        }

        /// <summary>
        /// Determines the scew angle and if confidence is high enough returns the descewed image as the result, otherwise returns clone of original image.
        /// </summary>
        /// <remarks>
        /// This binarizes if necessary and finds the skew angle.  If the
        /// angle is large enough and there is sufficient confidence,
        /// it returns a deskewed image; otherwise, it returns a clone.
        /// </remarks>
        /// <param name="scew">The scew angle and confidence</param>
        /// <returns>Returns deskewed image if confidence was high enough, otherwise returns clone of original pix.</returns>
        public Pix Deskew( out Scew scew ) => this.Deskew( DefaultBinarySearchReduction, out scew );

        /// <summary>
        /// Determines the scew angle and if confidence is high enough returns the descewed image as the result, otherwise returns clone of original image.
        /// </summary>
        /// <remarks>
        /// This binarizes if necessary and finds the skew angle.  If the
        /// angle is large enough and there is sufficient confidence,
        /// it returns a deskewed image; otherwise, it returns a clone.
        /// </remarks>
        /// <param name="redSearch">The reduction factor used by the binary search, can be 1, 2, or 4.</param>
        /// <param name="scew">The scew angle and confidence</param>
        /// <returns>Returns deskewed image if confidence was high enough, otherwise returns clone of original pix.</returns>
        public Pix Deskew( int redSearch, out Scew scew ) => this.Deskew( ScewSweep.Default, redSearch, DefaultBinaryThreshold, out scew );

        /// <summary>
        /// Determines the scew angle and if confidence is high enough returns the descewed image as the result, otherwise returns clone of original image.
        /// </summary>
        /// <remarks>
        /// This binarizes if necessary and finds the skew angle.  If the
        /// angle is large enough and there is sufficient confidence,
        /// it returns a deskewed image; otherwise, it returns a clone.
        /// </remarks>
        /// <param name="sweep">linear sweep parameters</param>
        /// <param name="redSearch">The reduction factor used by the binary search, can be 1, 2, or 4.</param>
        /// <param name="thresh">The threshold value used for binarizing the image.</param>
        /// <param name="scew">The scew angle and confidence</param>
        /// <returns>Returns deskewed image if confidence was high enough, otherwise returns clone of original pix.</returns>
        public Pix Deskew( ScewSweep sweep, int redSearch, int thresh, out Scew scew ) {
            float pAngle, pConf;
            var resultPixHandle = LeptonicaApi.Native.pixDeskewGeneral( this.Handle, sweep.Reduction, sweep.Range, sweep.Delta, redSearch, thresh, out pAngle, out pConf );
            if ( resultPixHandle == IntPtr.Zero )
                throw new TesseractException( "Failed to deskew image." );
            scew = new Scew( pAngle, pConf );
            return new Pix( resultPixHandle );
        }

        /// <summary>
        /// Binarization of the input image based on the passed parameters and the Otsu method
        /// </summary>
        /// <param name="sx"> sizeX Desired tile X dimension; actual size may vary.</param>
        /// <param name="sy"> sizeY Desired tile Y dimension; actual size may vary.</param>
        /// <param name="smoothx"> smoothX Half-width of convolution kernel applied to threshold array: use 0 for no smoothing.</param>
        /// <param name="smoothy"> smoothY Half-height of convolution kernel applied to threshold array: use 0 for no smoothing.</param>
        /// <param name="scorefract"> scoreFraction Fraction of the max Otsu score; typ. 0.1 (use 0.0 for standard Otsu).</param>
        /// <returns> ppixd is a pointer to the thresholded PIX image.</returns>
        public Pix BinarizeOtsuAdaptiveThreshold( int sx, int sy, int smoothx, int smoothy, float scorefract ) {
            IntPtr ppixth, ppixd;
            var result = LeptonicaApi.Native.pixOtsuAdaptiveThreshold( this.Handle, sx, sy, smoothx, smoothy, scorefract, out ppixth, out ppixd );
            if ( result == 1 )
                throw new TesseractException( "Failed to binarize image." );
            return new Pix( ppixd );
        }

        /// <summary>
        /// Conversion from RBG to 8bpp grayscale.
        /// </summary>
        /// <param name="rwt">Red weight</param>
        /// <param name="gwt">Green weight</param>
        /// <param name="bwt">Blue weight</param>
        /// <returns></returns>
        public Pix ConvertRGBToGray( float rwt, float gwt, float bwt ) {
            var resultPixHandle = LeptonicaApi.Native.pixConvertRGBToGray( this.Handle, rwt, gwt, bwt );
            if ( resultPixHandle == IntPtr.Zero )
                throw new TesseractException( "Failed to convert to grayscale." );
            return new Pix( resultPixHandle );
        }

        /// <summary>
        /// Creates a new image by rotating this image about it's centre.
        /// </summary>
        /// <remarks>
        /// Please note the following:
        /// <list type="bullet">
        /// <item>
        /// Rotation will bring in either white or black pixels, as specified by <paramref name="fillColor" /> from
        /// the outside as required.
        /// </item>
        /// <item>Above 20 degrees, sampling rotation will be used if shear was requested.</item>
        /// <item>Colormaps are removed for rotation by area map and shear.</item>
        /// <item>
        /// The resulting image can be expanded so that no image pixels are lost. To invoke expansion,
        /// input the original width and height. For repeated rotation, use of the original width and heigh allows
        /// expansion to stop at the maximum required size which is a square of side = sqrt(w*w + h*h).
        /// </item>
        /// </list>
        /// <para>
        /// Please note there is an implicit assumption about RGB component ordering.
        /// </para>
        /// </remarks>
        /// <param name="angle">The angle to rotate by, in radians; clockwise is positive.</param>
        /// <param name="method">The rotation method to use.</param>
        /// <param name="fillColor">The fill color to use for pixels that are brought in from the outside.</param>
        /// <param name="width">The original width; use 0 to avoid embedding</param>
        /// <param name="height">The original height; use 0 to avoid embedding</param>
        /// <returns>The image rotated around it's centre.</returns>
        public Pix Rotate( float angle, RotationMethod method = RotationMethod.AreaMap, RotationFill fillColor = RotationFill.White, int? width = null, int? height = null ) {
            if ( width == null )
                width = this.Width;
            if ( height == null )
                height = this.Height;

            if ( Math.Abs( angle ) < VerySmallAngle )
                return this.Clone();

            IntPtr resultHandle;

            var rotations = 2 * angle / Math.PI;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if ( Math.Abs( rotations - Math.Floor( rotations ) ) < VerySmallAngle ) {
                // handle special case of orthoganal rotations (90, 180, 270)
                resultHandle = LeptonicaApi.Native.pixRotateOrth( this.Handle, ( int )rotations );
            }
            else {
                // handle general case
                resultHandle = LeptonicaApi.Native.pixRotate( this.Handle, angle, method, fillColor, width.Value, height.Value );
            }

            if ( resultHandle == IntPtr.Zero )
                throw new LeptonicaException( "Failed to rotate image around it's centre." );

            return new Pix( resultHandle );
        }

        #endregion Image manipulation

        #region Disposal

        protected override void Dispose( bool disposing ) {
            var tmpHandle = this.Handle.Handle;
            LeptonicaApi.Native.pixDestroy( ref tmpHandle );
            this.Handle = new HandleRef( this, IntPtr.Zero );
        }

        #endregion Disposal
    }
}