using System;
using System.Diagnostics;

namespace Tesseract {

    public abstract class DisposableBase : IDisposable {
        private static readonly TraceSource Trace = new TraceSource( "Tesseract" );

        protected DisposableBase() {
            this.IsDisposed = false;
        }

        ~DisposableBase() {
            this.Dispose( false );
            Trace.TraceEvent( TraceEventType.Warning, 0, "{0} was not disposed off.", this );
        }

        public void Dispose() {
            this.Dispose( true );

            this.IsDisposed = true;
            GC.SuppressFinalize( this );

            if ( this.Disposed != null ) {
                this.Disposed( this, EventArgs.Empty );
            }
        }

        public bool IsDisposed {
            get;
            private set;
        }

        public event EventHandler<EventArgs> Disposed;

        protected virtual void VerifyNotDisposed() {
            if ( this.IsDisposed )
                throw new ObjectDisposedException( this.ToString() );
        }

        protected abstract void Dispose( bool disposing );
    }
}