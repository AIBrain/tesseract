using System.Drawing;

namespace Tesseract {

    public static class PixConverter {
        private static readonly BitmapToPixConverter bitmapConverter = new BitmapToPixConverter();
        private static readonly PixToBitmapConverter pixConverter = new PixToBitmapConverter();

        public static Pix ToPix( Bitmap img ) => bitmapConverter.Convert( img );

        public static Bitmap ToBitmap( Pix pix ) => pixConverter.Convert( pix );
    }
}