#region License & Information

// This notice must be kept visible in the source.
//
// This section of source code belongs to Rick@AIBrain.Org unless otherwise specified,
// or the original license has been overwritten by the automatic formatting of this code.
// Any unmodified sections of source code borrowed from other projects retain their original license and thanks goes to the Authors.
//
// Donations and Royalties can be paid via
// PayPal: paypal@aibrain.org
// bitcoin:1Mad8TxTqxKnMiHuZxArFvX8BuFEB9nqX2
// bitcoin:1NzEsF7eegeEWDr5Vr9sSSgtUC4aL6axJu
// litecoin:LeUxdU2w3o6pLZGVys5xpDZvvo8DUrjBp9
//
// Usage of the source code or compiled binaries is AS-IS.
// I am not responsible for Anything You Do.
//
// Contact me by email if you have any questions or helpful criticism.
//
// "Tesseract/PixArray.cs" was last cleaned by Rick on 2014/09/22 at 4:55 PM

#endregion License & Information

namespace Tesseract {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using Internal;
    using Interop;

    /// <summary>
    ///     Represents an array of <see cref="Pix" />.
    /// </summary>
    public class PixArray : DisposableBase, IEnumerable<Pix> {

        #region Static Constructors

        /// <summary>
        ///     Loads the multi-page tiff located at <paramref name="filename" />.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static PixArray LoadMultiPageTiffFromFile( string filename ) {
            var pixaHandle = LeptonicaApi.Native.pixaReadMultipageTiff( filename );
            if ( pixaHandle == IntPtr.Zero ) {
                throw new IOException( String.Format( "Failed to load image '{0}'.", filename ) );
            }

            return new PixArray( pixaHandle );
        }

        #endregion Static Constructors

        #region Enumerator implementation

        /// <summary>
        ///     Handles enumerating through the <see cref="Pix" /> in the PixArray.
        /// </summary>
        private class PixArrayEnumerator : DisposableBase, IEnumerator<Pix> {

            #region Fields

            private readonly PixArray array;
            private readonly Pix[] items;
            private readonly int version;
            private Pix current;
            private int index;

            #endregion Fields

            #region Constructor

            public PixArrayEnumerator( PixArray array ) {
                this.array = array;
                this.version = array.version;
                this.items = new Pix[ array.Count ];
                this.index = 0;
                this.current = null;
            }

            #endregion Constructor

            #region Enumerator Implementation

            /// <inheritdoc />
            public bool MoveNext() {
                this.VerifyArrayUnchanged();
                this.VerifyNotDisposed();

                if ( this.index < this.items.Length ) {
                    if ( this.items[ this.index ] == null ) {
                        this.items[ this.index ] = this.array.GetPix( this.index );
                    }
                    this.current = this.items[ this.index ];
                    this.index++;
                    return true;
                }
                this.index = this.items.Length + 1;
                this.current = null;
                return false;
            }

            /// <inheritdoc />
            public Pix Current {
                get {
                    this.VerifyArrayUnchanged();
                    this.VerifyNotDisposed();

                    return this.current;
                }
            }

            // IEnumerator imp

            /// <inheritdoc />
            void IEnumerator.Reset() {
                this.VerifyArrayUnchanged();
                this.VerifyNotDisposed();

                this.index = 0;
                this.current = null;
            }

            /// <inheritdoc />
            object IEnumerator.Current {
                get {
                    // note: Only the non-generic requires an exception check according the MSDN docs (Generic version just undefined if it's not currently pointing to an item). Go figure.
                    if ( this.index == 0 || this.index == this.items.Length + 1 ) {
                        throw new InvalidOperationException( "The enumerator is positioned either before the first item or after the last item ." );
                    }

                    return this.Current;
                }
            }

            // Helpers

            /// <inheritdoc />
            private void VerifyArrayUnchanged() {
                if ( this.version != this.array.version ) {
                    throw new InvalidOperationException( "PixArray was modified; enumeration operation may not execute." );
                }
            }

            #endregion Enumerator Implementation

            #region Disposal

            protected override void Dispose( bool disposing ) {
                if ( disposing ) {
                    for ( var i = 0 ; i < this.items.Length ; i++ ) {
                        if ( this.items[ i ] != null ) {
                            this.items[ i ].Dispose();
                            this.items[ i ] = null;
                        }
                    }
                }
            }

            #endregion Disposal
        }

        #endregion Enumerator implementation

        #region Fields

        private readonly int _count;
        private readonly int version;

        /// <summary>
        ///     Gets the handle to the underlying PixA structure.
        /// </summary>
        private HandleRef _handle;

        #endregion Fields

        #region Constructor

        private PixArray( IntPtr handle ) {
            this._handle = new HandleRef( this, handle );
            this.version = 1;

            // These will need to be updated whenever the PixA structure changes (i.e. a Pix is added or removed) though at the moment that isn't a problem.
            this._count = LeptonicaApi.Native.pixaGetCount( this._handle );
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        ///     Gets the number of <see cref="Pix" /> contained in the array.
        /// </summary>
        public int Count {
            get {
                this.VerifyNotDisposed();
                return this._count;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        ///     Returns a <see cref="IEnumerator{Pix}" /> that iterates the the array of <see cref="Pix" />.
        /// </summary>
        /// <remarks>
        ///     When done with the enumerator you must call <see cref="IEnumerator{Pix}.Dispose()" /> to release any unmanaged
        ///     resources.
        ///     However if your using the enumerator in a foreach loop, this is done for you automatically by .Net. This also means
        ///     that any <see cref="Pix" /> returned from the enumerator cannot safely be used outside a foreach loop (or after
        ///     Dispose has been
        ///     called on the enumerator). If you do indeed need the pix after the enumerator has been disposed of you must clone
        ///     it using
        ///     <see cref="Pix.Clone()" />.
        /// </remarks>
        /// <returns>A <see cref="IEnumerator{Pix}" /> that iterates the the array of <see cref="Pix" />.</returns>
        public IEnumerator<Pix> GetEnumerator() => new PixArrayEnumerator( this );

        IEnumerator IEnumerable.GetEnumerator() => new PixArrayEnumerator( this );

        /// <summary>
        ///     Gets the <see cref="Pix" /> located at <paramref name="index" /> using the specified <paramref name="accessType" />
        ///     .
        /// </summary>
        /// <param name="index">The index of the pix (zero based).</param>
        /// <param name="accessType">
        ///     The <see cref="PixArrayAccessType" /> used to retrieve the <see cref="Pix" />, only Clone or
        ///     Copy are allowed.
        /// </param>
        /// <returns>The retrieved <see cref="Pix" />.</returns>
        public Pix GetPix( int index, PixArrayAccessType accessType = PixArrayAccessType.Clone ) {
            Guard.Require( "accessType", accessType == PixArrayAccessType.Clone || accessType == PixArrayAccessType.Copy, "Access type must be either copy or clone but was {0}.", accessType );
            Guard.Require( "index", index >= 0 && index < this.Count, "The index {0} must be between 0 and {1}.", index, this.Count );

            this.VerifyNotDisposed();

            var pixHandle = LeptonicaApi.Native.pixaGetPix( this._handle, index, accessType );
            if ( pixHandle == IntPtr.Zero ) {
                throw new InvalidOperationException( String.Format( "Failed to retrieve pix {0}.", pixHandle ) );
            }
            return Pix.Create( pixHandle );
        }

        protected override void Dispose( bool disposing ) {
            var handle = this._handle.Handle;
            LeptonicaApi.Native.pixaDestroy( ref handle );
            this._handle = new HandleRef( this, handle );
        }

        #endregion Methods
    }
}