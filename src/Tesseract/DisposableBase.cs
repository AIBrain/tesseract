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

        public event EventHandler<EventArgs> Disposed;

        public bool IsDisposed {
            get;
            private set;
        }

        public void Dispose() {
            this.Dispose( true );

            this.IsDisposed = true;
            GC.SuppressFinalize( this );

            this.Disposed?.Invoke( this, EventArgs.Empty );
        }

        protected abstract void Dispose( bool disposing );

        protected virtual void VerifyNotDisposed() {
            if ( this.IsDisposed )
                throw new ObjectDisposedException( this.ToString() );
        }
    }
}