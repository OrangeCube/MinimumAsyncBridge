namespace System.Threading
{
    public struct CancellationTokenRegistration : IEquatable<CancellationTokenRegistration>, IDisposable
    {
        private readonly CancellationTokenSource _source;
        private readonly Action _callback;

        public CancellationTokenRegistration(CancellationTokenSource source, Action callback)
        {
            _source = source;
            _callback = callback;
        }

        public void Dispose()
        {
            _source.Canceled -= _callback;
        }
        public bool Equals(CancellationTokenRegistration other) => _source == other._source && _callback == other._callback;
        public override bool Equals(object obj) => obj is CancellationTokenRegistration && Equals((CancellationTokenRegistration)obj);
        public override int GetHashCode() => _source.GetHashCode() ^ _callback.GetHashCode();

        public static bool operator ==(CancellationTokenRegistration left, CancellationTokenRegistration right) => left._callback == right._callback && left._source == right._source;
        public static bool operator !=(CancellationTokenRegistration left, CancellationTokenRegistration right) => left._callback != right._callback || left._source != right._source;
    }
}
