﻿namespace Tesseract {

    /// <summary>
    /// Represents the parameters for a sweep search used by scew algorithms.
    /// </summary>
    public struct ScewSweep {
        public static ScewSweep Default = new ScewSweep( DefaultReduction, DefaultRange, DefaultDelta );

        #region Constants and Fields

        public const int DefaultReduction = 4; // Sweep part; 4 is good
        public const float DefaultRange = 7.0F;
        public const float DefaultDelta = 1.0F;

        private int reduction;
        private float range;
        private float delta;

        #endregion Constants and Fields

        #region Factory Methods + Constructor

        public ScewSweep( int reduction = DefaultReduction, float range = DefaultRange, float delta = DefaultDelta ) {
            this.reduction = reduction;
            this.range = range;
            this.delta = delta;
        }

        #endregion Factory Methods + Constructor

        #region Properties

        public int Reduction {
            get {
                return this.reduction;
            }
        }

        public float Range {
            get {
                return this.range;
            }
        }

        public float Delta {
            get {
                return this.delta;
            }
        }

        #endregion Properties
    }
}