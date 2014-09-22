using System;

namespace Tesseract
{
    using System.Runtime.Serialization;

    [Serializable]
    public class LeptonicaException : Exception
    {
        public LeptonicaException() { }
        public LeptonicaException(string message) : base(message) { }
        public LeptonicaException(string message, Exception inner) : base(message, inner) { }
        protected LeptonicaException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }
}
