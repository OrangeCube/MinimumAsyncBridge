using System.Collections.ObjectModel;
using System.Linq;

namespace System
{
    public class AggregateException : Exception
    {
        public ReadOnlyCollection<Exception> InnerExceptions { get; }

        public AggregateException(params Exception[] exceptions) : base("", exceptions.FirstOrDefault())
        {
            InnerExceptions = new ReadOnlyCollection<Exception>(exceptions);
        }
    }
}
