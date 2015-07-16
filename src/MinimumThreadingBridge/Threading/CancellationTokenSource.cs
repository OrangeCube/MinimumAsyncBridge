namespace System.Threading
{
    public class CancellationTokenSource
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
    }
}
