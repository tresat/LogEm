using System;
using System.Threading;

namespace LogEm
{
    internal sealed class SynchronousAsyncResult : IAsyncResult
    {
        private ManualResetEvent _waitHandle;
        private readonly string _syncMethodName;
        private readonly object _asyncState;
        private readonly object _result;
        private readonly Exception _exception;
        private bool _ended;

        public static SynchronousAsyncResult OnSuccess(string syncMethodName, object asyncState, object result)
        {
            return new SynchronousAsyncResult(syncMethodName, asyncState, result, null);
        }

        public static SynchronousAsyncResult OnFailure(string syncMethodName, object asyncState, Exception e)
        {
            Debug.Assert(e != null);

            return new SynchronousAsyncResult(syncMethodName, asyncState, null, e);
        }

        private SynchronousAsyncResult(string syncMethodName, object asyncState, object result, Exception e)
        {
            Debug.AssertStringNotEmpty(syncMethodName);

            _syncMethodName = syncMethodName;
            _asyncState = asyncState;
            _result = result;
            _exception = e;
        }

        public bool IsCompleted
        {
            get { return true; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                //
                // Create the async handle on-demand, assuming the caller
                // insists on having it even though CompletedSynchronously and
                // IsCompleted should make this redundant.
                //

                if (_waitHandle == null)
                    _waitHandle = new ManualResetEvent(true);

                return _waitHandle;
            }
        }

        public object AsyncState
        {
            get { return _asyncState; }
        }

        public bool CompletedSynchronously
        {
            get { return true; }
        }

        public object End()
        {
            if (_ended)
                throw new InvalidOperationException(string.Format("End{0} can only be called once for each asynchronous operation.", _syncMethodName));

            _ended = true;

            if (_exception != null)
                throw _exception;

            return _result;
        }
    }
}
