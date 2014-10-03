namespace Tesseract {

    /// <summary>
    /// Represents properties that describe a text block's orientation.
    /// </summary>
    public struct ElementProperties {
        private readonly Orientation orientation;
        private readonly TextLineOrder textLineOrder;
        private readonly WritingDirection writingDirection;
        private readonly float deskewAngle;

        public ElementProperties( Orientation orientation, TextLineOrder textLineOrder, WritingDirection writingDirection, float deskewAngle ) {
            this.orientation = orientation;
            this.textLineOrder = textLineOrder;
            this.writingDirection = writingDirection;
            this.deskewAngle = deskewAngle;
        }

        /// <summary>
        /// Gets the <see cref="Orientation" /> for corresponding text block.
        /// </summary>
        public Orientation Orientation {
            get {
                return this.orientation;
            }
        }

        /// <summary>
        /// Gets the <see cref="TextLineOrder" /> for corresponding text block.
        /// </summary>
        public TextLineOrder TextLineOrder {
            get {
                return this.textLineOrder;
            }
        }

        /// <summary>
        /// Gets the <see cref="WritingDirection" /> for corresponding text block.
        /// </summary>
        public WritingDirection WritingDirection {
            get {
                return this.writingDirection;
            }
        }

        /// <summary>
        /// Gets the angle the page would need to be rotated to deskew the text block.
        /// </summary>
        public float DeskewAngle {
            get {
                return this.deskewAngle;
            }
        }
    }
}