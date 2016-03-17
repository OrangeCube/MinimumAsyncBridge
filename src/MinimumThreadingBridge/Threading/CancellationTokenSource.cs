namespace System.Threading
{
    public class CancellationTokenSource : IDisposable
    {
        public CancellationTokenSource()
        {
            Token = new CancellationToken(this);
        }

        public CancellationToken Token { get; }

        public bool IsCancellationRequested { get; private set; }

        public void Cancel()
        {
            if (IsCancellationRequested) return;
            IsCancellationRequested = true;

            var d = _canceled;
            if (d != null) d();
            d = null;

        }

        internal event Action Canceled
        {
            add
            {
                if (IsCancellationRequested)
                {
                    if(value != null)
                        value();
                }
                else
                {
                    _canceled += value;
                }
            }
            remove { _canceled -= value; }
        }
        private Action _canceled;

        public void CancelAfter(TimeSpan delay) => CancelAfter((int)delay.TotalMilliseconds);

        public void CancelAfter(int millisecondsDelay)
        {
            if (millisecondsDelay <= 0)
            {
                Cancel();
                return;
            }

            Timer t = null;
            t = new Timer(_ =>
            {
                t.Dispose();
                Cancel();
                t = null;
            }, null, millisecondsDelay, Timeout.Infinite);
        }

        public void Dispose()
        {
            _canceled = null;
        }
    }
}
