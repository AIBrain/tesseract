using System;

namespace Tesseract
{
    public struct Rect : IEquatable<Rect>
    {
        public static readonly Rect Empty = new Rect();

        #region Fields

        private readonly int x;
        private readonly int y;
        private readonly int width;
        private readonly int height;

        #endregion

        #region Constructors + Factory Methods

        public Rect(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public static Rect FromCoords(int x1, int y1, int x2, int y2)
        {
            return new Rect(x1, y1, x2 - x1, y2 - y1);
        }

        #endregion
        
        #region Properties

        public int X1
        {
            get { return this.x; }
        }

        public int Y1
        {
            get { return this.y; }
        }

        public int X2
        {
            get { return this.x + this.width; }
        }

        public int Y2
        {
            get { return this.y + this.height; }
        }

        public int Width
        {
            get { return this.width; }
        }

        public int Height
        {
            get { return this.height; }
        }

        #endregion

        #region Equals and GetHashCode implementation
        public override bool Equals(object obj)
        {
            return (obj is Rect) && this.Equals((Rect)obj);
        }

        public bool Equals(Rect other)
        {
            return this.x == other.x && this.y == other.y && this.width == other.width && this.height == other.height;
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            unchecked {
                hashCode += 1000000007 * this.x.GetHashCode();
                hashCode += 1000000009 * this.y.GetHashCode();
                hashCode += 1000000021 * this.width.GetHashCode();
                hashCode += 1000000033 * this.height.GetHashCode();
            }
            return hashCode;
        }

        public static bool operator ==(Rect lhs, Rect rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Rect lhs, Rect rhs)
        {
            return !(lhs == rhs);
        }
        #endregion
        
        #region ToString
        
		public override string ToString()
		{
			return string.Format("[Rect X={0}, Y={1}, Width={2}, Height={3}]", this.x, this.y, this.width, this.height);
		}

        
        #endregion
        
    }
}
