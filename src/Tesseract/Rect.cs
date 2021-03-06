﻿using System;

namespace Tesseract {

    public struct Rect : IEquatable<Rect> {
        public static readonly Rect Empty = new Rect();

        #region Fields
        #endregion Fields

        #region Constructors + Factory Methods

        public Rect( int x, int y, int width, int height ) {
            this.X1 = x;
            this.Y1 = y;
            this.Width = width;
            this.Height = height;
        }

        public static Rect FromCoords( int x1, int y1, int x2, int y2 ) => new Rect( x1, y1, x2 - x1, y2 - y1 );
        #endregion Constructors + Factory Methods

        #region Properties

        public int X1 { get; }

        public int Y1 { get; }

        public int X2 {
            get {
                return this.X1 + this.Width;
            }
        }

        public int Y2 {
            get {
                return this.Y1 + this.Height;
            }
        }

        public int Width { get; }

        public int Height { get; }
        #endregion Properties

        #region Equals and GetHashCode implementation

        public override bool Equals( object obj ) => ( obj is Rect ) && this.Equals( ( Rect )obj );

        public bool Equals( Rect other ) => this.X1 == other.X1 && this.Y1 == other.Y1 && this.Width == other.Width && this.Height == other.Height;

        public override int GetHashCode() {
            var hashCode = 0;
            unchecked {
                hashCode += 1000000007 * this.X1.GetHashCode();
                hashCode += 1000000009 * this.Y1.GetHashCode();
                hashCode += 1000000021 * this.Width.GetHashCode();
                hashCode += 1000000033 * this.Height.GetHashCode();
            }
            return hashCode;
        }

        public static bool operator ==( Rect lhs, Rect rhs ) => lhs.Equals( rhs );

        public static bool operator !=( Rect lhs, Rect rhs ) => !( lhs == rhs );
        #endregion Equals and GetHashCode implementation

        #region ToString

        public override string ToString() => string.Format( "[Rect X={0}, Y={1}, Width={2}, Height={3}]", this.X1, this.Y1, this.Width, this.Height );
        #endregion ToString
    }
}