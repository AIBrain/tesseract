using System;

namespace Tesseract
{
    public struct Scew
    {
        private readonly float angle;
        private readonly float confidence;

        public Scew(float angle, float confidence)
        {
            this.angle = angle;
            this.confidence = confidence;
        }

        public float Angle
        {
            get { return this.angle; }
        }


        public float Confidence
        {
            get { return this.confidence; }
        }

        public override string ToString()
        {
            return String.Format("Scew: {0} [conf: {1}]", this.Angle, this.Confidence);
        }

        #region Equals and GetHashCode implementation
        public override bool Equals(object obj)
        {
            return (obj is Scew) && this.Equals((Scew)obj);
        }

        public bool Equals(Scew other)
        {
            return Math.Abs( this.confidence - other.confidence ) < float.Epsilon && Math.Abs( this.angle - other.angle ) < float.Epsilon;
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            unchecked {
                hashCode += 1000000007 * this.angle.GetHashCode();
                hashCode += 1000000009 * this.confidence.GetHashCode();
            }
            return hashCode;
        }

        public static bool operator ==(Scew lhs, Scew rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Scew lhs, Scew rhs)
        {
            return !(lhs == rhs);
        }
        #endregion
        
    }
}
