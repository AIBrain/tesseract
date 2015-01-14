using System;
using System.Runtime.InteropServices;

namespace Tesseract {

    using System.Runtime.CompilerServices;
    using Interop;

    public sealed class Page : DisposableBase {
        private bool runRecognitionPhase;
        private Rect regionOfInterest;

        public TesseractEngine Engine {
            get;
        }

        public Pix Image {
            get;
        }

        internal Page( TesseractEngine engine, Pix image, Rect regionOfInterest ) {
            this.Engine = engine;
            this.Image = image;
            this.RegionOfInterest = regionOfInterest;
        }

        /// <summary>
        /// The current region of interest being parsed.
        /// </summary>
        public Rect RegionOfInterest {
            get {
                return this.regionOfInterest;
            }

            set {
                if ( value.X1 < 0 || value.Y1 < 0 || value.X2 > this.Image.Width || value.Y2 > this.Image.Height )
                    throw new ArgumentException( "The region of interest to be processed must be within the image bounds.", "value" );

                if ( this.regionOfInterest != value ) {
                    this.regionOfInterest = value;

                    // update region of interest in image
                    TessApi.Native.BaseApiSetRectangle( this.Engine.Handle, this.regionOfInterest.X1, this.regionOfInterest.Y1, this.regionOfInterest.Width, this.regionOfInterest.Height );

                    // request rerun of recognition on the next call that requires recognition
                    this.runRecognitionPhase = false;
                }
            }
        }

        public PageIterator AnalyseLayout() {
            var resultIteratorHandle = TessApi.Native.BaseAPIAnalyseLayout( this.Engine.Handle );
            return new PageIterator( resultIteratorHandle );
        }

        public ResultIterator GetIterator() {
            this.Recognize();
            var resultIteratorHandle = TessApi.Native.BaseApiGetIterator( this.Engine.Handle );
            return new ResultIterator( resultIteratorHandle );
        }

        public string GetText() {
            this.Recognize();
            return TessApi.BaseAPIGetUTF8Text( this.Engine.Handle );
        }

        public string GetHOCRText( int pageNum ) {
            this.Recognize();
            return TessApi.BaseAPIGetHOCRText( this.Engine.Handle, pageNum );
        }

        public float GetMeanConfidence() {
            this.Recognize();
            return TessApi.Native.BaseAPIMeanTextConf( this.Engine.Handle ) / 100.0f;
        }

#if Net45

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        private void Recognize() {
            if ( !this.runRecognitionPhase ) {
                if ( TessApi.Native.BaseApiRecognize( this.Engine.Handle, new HandleRef( this, IntPtr.Zero ) ) != 0 ) {
                    throw new InvalidOperationException( "Recognition of image failed." );
                }
                this.runRecognitionPhase = true;
            }
        }

        protected override void Dispose( bool disposing ) {
            if ( disposing ) {
                TessApi.Native.BaseAPIClear( this.Engine.Handle );
            }
        }
    }
}