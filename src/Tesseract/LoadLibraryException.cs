using System;

namespace Tesseract {

    using System.Runtime.Serialization;

    [Serializable]
    public class LoadLibraryException : SystemException {

        public LoadLibraryException() {
        }

        public LoadLibraryException( string message ) : base( message ) {
        }

        public LoadLibraryException( string message, Exception inner ) : base( message, inner ) {
        }

        protected LoadLibraryException(
          SerializationInfo info,
          StreamingContext context )
            : base( info, context ) {
        }
    }
}