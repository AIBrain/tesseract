using System;

namespace Tesseract {

    public struct Scew {
        public Scew( float angle, float confidence ) {
            this.Angle = angle;
            this.Confidence = confidence;
        }

        public float Angle { get; }

        public float Confidence { get; }

        public override string ToString() => String.Format( "Scew: {0} [conf: {1}]", this.Angle, this.Confidence );

        #region Equals and GetHashCode implementation

        public override bool Equals( object obj ) => ( obj is Scew ) && this.Equals( ( Scew )obj );

        public bool Equals( Scew other ) => Math.Abs( this.Confidence - other.Confidence ) < float.Epsilon && Math.Abs( this.Angle - other.Angle ) < float.Epsilon;

        public override int GetHashCode() {
            var hashCode = 0;
            unchecked {
                hashCode += 1000000007 * this.Angle.GetHashCode();
                hashCode += 1000000009 * this.Confidence.GetHashCode();
            }
            return hashCode;
        }

        public static bool operator ==( Scew lhs, Scew rhs ) => lhs.Equals( rhs );

        public static bool operator !=( Scew lhs, Scew rhs ) => !( lhs == rhs );
        #endregion Equals and GetHashCode implementation
    }
}