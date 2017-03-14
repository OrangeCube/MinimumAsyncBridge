#if !NET40PLUS

namespace System.Runtime.CompilerServices
{
    /// <summary>Represents an asynchronous method builder.</summary>
    internal interface IAsyncMethodBuilder
    {
        object ObjectIdForDebugger
        {
            get;
        }

        void PreBoxInitialization<TStateMachine>(ref TStateMachine stateMachine);
    }
}

#endif
