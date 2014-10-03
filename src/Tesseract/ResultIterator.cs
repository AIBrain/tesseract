using System;

namespace Tesseract {

    using Interop;

    public sealed class ResultIterator : PageIterator {

        internal ResultIterator( IntPtr handle )
            : base( handle ) {
        }

        public float GetConfidence( PageIteratorLevel level ) {
            return TessApi.Native.ResultIteratorGetConfidence( this.handle, level );
        }

        public string GetText( PageIteratorLevel level ) {
            return TessApi.ResultIteratorGetUTF8Text( this.handle, level );
        }
    }
}