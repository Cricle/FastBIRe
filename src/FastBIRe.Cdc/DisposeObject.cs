using System;

namespace FastBIRe.Cdc
{
    public abstract class DisposeObject : IDisposable
    {
        private bool isDisposed;

        public bool IsDisposed => isDisposed;

        public void Dispose()
        {
            if (!isDisposed)
            {
                OnDisposed(true);
                isDisposed = true;
            }
        }

        protected virtual void OnDisposed(bool disposing)
        {

        }
    }
}
