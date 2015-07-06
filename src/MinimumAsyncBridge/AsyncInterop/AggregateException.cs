#if NET40PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(System.AggregateException))]

# else

using System.Runtime.Serialization;

namespace System
{
    [Serializable]
    internal class AggregateException : Exception
    {
        private Exception ex;
        private Exception exception;

        public AggregateException()
        {
        }

        public AggregateException(string message) : base(message)
        {
        }

        public AggregateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public AggregateException(Exception exception, Exception ex)
        {
            this.exception = exception;
            this.ex = ex;
        }

        protected AggregateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

#endif
