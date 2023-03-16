using System;
using System.Threading.Tasks.Sources;

namespace WalletConnectSharp.Core.Utils
{
    public abstract class DisposableBase : IDisposable
    {
        private bool _isDisposed;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DisposableBase() { }

        /// <summary>
        /// dispose managed state (managed objects). This must be implemented.
        /// </summary>
        protected abstract void DisposeManaged();

        /// <summary>
        /// free unmanaged resources (unmanaged objects) and override finalizer
        /// set large fields to null
        /// </summary>
        protected virtual void DisposeUnmanaged() { }

        /// <summary>
        /// Dispose Managed and Unmanaged resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    DisposeManaged();
                }
                DisposeUnmanaged();
                _isDisposed = true;
            }
        }

        /// <summary>
        /// override finalizer only if 'Dispose(bool disposing)'
        /// has code to free unmanaged resources
        /// </summary>
        ~DisposableBase()
        {
            // Do not change this code.
            // Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
            // Do not change this code.
            // Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
