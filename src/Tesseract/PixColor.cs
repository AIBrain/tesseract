using System;
using System.Runtime.InteropServices;

namespace Tesseract
{
    using System.Drawing;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
	public struct PixColor : IEquatable<PixColor>
    {
        private readonly byte red;
        private readonly byte blue;
        private readonly byte green;
        private readonly byte alpha;

        public PixColor(byte red, byte green, byte blue, byte alpha = 255)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
            this.alpha = alpha;
        }

        public byte Red { get { return this.red; } }
        public byte Green { get { return this.green; } }
        public byte Blue { get { return this.blue; } }
        public byte Alpha { get { return this.alpha; } }

        public static PixColor FromRgba(uint value)
        {
            return new PixColor(
               (byte)((value >> 24) & 0xFF),
               (byte)((value >> 16) & 0xFF),
               (byte)((value >> 8) & 0xFF),
               (byte)(value & 0xFF));
        }

        public static PixColor FromRgb(uint value)
        {
            return new PixColor(
               (byte)((value >> 24) & 0xFF),
               (byte)((value >> 16) & 0xFF),
               (byte)((value >> 8) & 0xFF),
               (byte)0xFF);
        }

        public uint ToRGBA()
        {
            return BitmapHelper.EncodeAsRGBA(this.red, this.green, this.blue, this.alpha);
        }

        public static explicit operator Color(PixColor color)
        {
            return Color.FromArgb(color.alpha, color.red, color.green, color.blue);
        }

        public static explicit operator PixColor(Color color)
        {
            return new PixColor(color.R, color.G, color.B, color.A);
        }


        #region Equals and GetHashCode implementation
        public override bool Equals(object obj)
		{
			return (obj is PixColor) && this.Equals((PixColor)obj);
		}
        
		public bool Equals(PixColor other)
		{
			return this.red == other.red && this.blue == other.blue && this.green == other.green && this.alpha == other.alpha;
		}
        
		public override int GetHashCode()
		{
			var hashCode = 0;
			unchecked {
				hashCode += 1000000007 * this.red.GetHashCode();
				hashCode += 1000000009 * this.blue.GetHashCode();
				hashCode += 1000000021 * this.green.GetHashCode();
				hashCode += 1000000033 * this.alpha.GetHashCode();
			}
			return hashCode;
		}
        
		public static bool operator ==(PixColor lhs, PixColor rhs)
		{
			return lhs.Equals(rhs);
		}
        
		public static bool operator !=(PixColor lhs, PixColor rhs)
		{
			return !(lhs == rhs);
		}
        #endregion

        public override string ToString()
        {
            return String.Format("Color(0x{0:X})", this.ToRGBA());
        }

    }
}
