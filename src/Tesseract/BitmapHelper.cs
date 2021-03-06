﻿using System;

namespace Tesseract {

    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Description of BitmapHelper.
    /// </summary>
    public static unsafe class BitmapHelper {

        /// <summary>
        /// gets the number of Bits Per Pixel (BPP)
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static int GetBPP( Bitmap bitmap ) {
            switch ( bitmap.PixelFormat ) {
                case PixelFormat.Format1bppIndexed:
                    return 1;

                case PixelFormat.Format4bppIndexed:
                    return 4;

                case PixelFormat.Format8bppIndexed:
                    return 8;

                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                    return 16;

                case PixelFormat.Format24bppRgb:
                    return 24;

                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    return 32;

                case PixelFormat.Format48bppRgb:
                    return 48;

                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return 64;

                default:
                    throw new ArgumentException( String.Format( "The bitmap's pixel format of {0} was not recognised.", bitmap.PixelFormat ), nameof( bitmap ) );
            }
        }

        #region Bitmap Data Access

#if Net45

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        public static byte GetDataBit( byte* data, int index ) => ( byte )( ( *( data + ( index >> 3 ) ) >> ( index & 0x7 ) ) & 1 );

#if Net45

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        public static void SetDataBit( byte* data, int index, byte value ) {
            var wordPtr = data + ( index >> 3 );
            *wordPtr &= ( byte )~( 0x80 >> ( index & 7 ) ); 			// clear bit, note first pixel in the byte is most significant (1000 0000)
            *wordPtr |= ( byte )( ( value & 1 ) << ( 7 - ( index & 7 ) ) );		// set bit, if value is 1
        }

#if Net45

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        public static byte GetDataQBit( byte* data, int index ) => ( byte )( ( *( data + ( index >> 1 ) ) >> ( 4 * ( index & 1 ) ) ) & 0xF );

#if Net45

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        public static void SetDataQBit( byte* data, int index, byte value ) {
            var wordPtr = data + ( index >> 1 );
            *wordPtr &= ( byte )~( 0xF0 >> ( 4 * ( index & 1 ) ) ); // clears qbit located at index, note like bit the qbit corresponding to the first pixel is the most significant (0xF0)
            *wordPtr |= ( byte )( ( value & 0x0F ) << ( 4 - ( 4 * ( index & 1 ) ) ) ); // applys qbit to n
        }

#if Net45

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        public static byte GetDataByte( byte* data, int index ) => *( data + index );

#if Net45

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        public static void SetDataByte( byte* data, int index, byte value ) {
            *( data + index ) = value;
        }

#if Net45

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        public static ushort GetDataUInt16( ushort* data, int index ) => *( data + index );

#if Net45

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        public static void SetDataUInt16( ushort* data, int index, ushort value ) {
            *( data + index ) = value;
        }

#if Net45

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        public static uint GetDataUInt32( uint* data, int index ) => *( data + index );

#if Net45

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        public static void SetDataUInt32( uint* data, int index, uint value ) {
            *( data + index ) = value;
        }

        #endregion Bitmap Data Access

        #region PixelFormat conversion

#if Net45

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        public static uint ConvertRgb555ToRGBA( uint val ) {
            var red = ( ( val & 0x7C00 ) >> 10 );
            var green = ( ( val & 0x3E0 ) >> 5 );
            var blue = ( val & 0x1F );

            return ( ( red << 3 | red >> 2 ) << 24 ) |
                ( ( green << 3 | green >> 2 ) << 16 ) |
                ( ( blue << 3 | blue >> 2 ) << 8 ) |
                0xFF;
        }

#if Net45

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        public static uint ConvertRgb565ToRGBA( uint val ) {
            var red = ( ( val & 0xF800 ) >> 11 );
            var green = ( ( val & 0x7E0 ) >> 5 );
            var blue = ( val & 0x1F );

            return ( ( red << 3 | red >> 2 ) << 24 ) |
                ( ( green << 2 | green >> 4 ) << 16 ) |
                ( ( blue << 3 | blue >> 2 ) << 8 ) |
                0xFF;
        }

#if Net45

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        public static uint ConvertArgb1555ToRGBA( uint val ) {
            var alpha = ( ( val & 0x8000 ) >> 15 );
            var red = ( ( val & 0x7C00 ) >> 10 );
            var green = ( ( val & 0x3E0 ) >> 5 );
            var blue = ( val & 0x1F );

            return ( ( red << 3 | red >> 2 ) << 24 ) |
                ( ( green << 3 | green >> 2 ) << 16 ) |
                ( ( blue << 3 | blue >> 2 ) << 8 ) |
                ( ( alpha << 8 ) - alpha ); // effectively alpha * 255, only works as alpha will be either 0 or 1
        }

#if Net45

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

        public static uint EncodeAsRGBA( byte red, byte green, byte blue, byte alpha ) => ( uint )( ( red << 24 ) |
                                                                                                    ( green << 16 ) |
                                                                                                    ( blue << 8 ) |
                                                                                                    alpha );

        #endregion PixelFormat conversion
    }
}