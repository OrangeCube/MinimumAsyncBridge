namespace System.Threading
{
    public struct CancellationToken
    {
        private readonly CancellationTokenSource _source;

        internal CancellationToken(CancellationTokenSource source) { _source = source; }

        public bool IsCancellationRequested
        {
            get
            {
                if (_source == null)
                    return false;
                else
                    return _source.IsCancellationRequested;
            }
        }

        public CancellationTokenRegistration Register(Action callback)
        {
            if (_source != null)
            {
                if (_source.IsCancellationRequested) callback();
                _source.Canceled += callback;
            }

            return new CancellationTokenRegistration(_source, callback);
        }

        public static CancellationToken None { get; } = new CancellationToken();

        public void ThrowIfCancellationRequested()
        {
            if (IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }

        public static bool operator ==(CancellationToken x, CancellationToken y) => x._source == y._source;
        public static bool operator !=(CancellationToken x, CancellationToken y) => x._source != y._source;
        public override bool Equals(object obj) => obj is CancellationToken && _source == ((CancellationToken)obj)._source;
        public override int GetHashCode() => _source.GetHashCode();
    }
}
