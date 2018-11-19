using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CustomAwaiter
{
    public class CustomAwaitable
    {
        public CustomAwaiter GetAwaiter()
        {
            return new CustomAwaiter("Custom GetAwaiter");
        }
    }


    public class CustomAwaiter : INotifyCompletion
    {
        #region Fields

        private string mResult;

        #endregion


        #region Properties

        public static bool RunAsync { get; set; } = false;


        public bool IsCompleted
        {
            get
            {
                return !RunAsync;
            }
        }

        #endregion


        public CustomAwaiter(string result)
        {
            this.mResult = result;
        }


        void INotifyCompletion.OnCompleted(Action continuation)
        {
            var sc = SynchronizationContext.Current;
            var t = new Timer(
                s => sc.Post(s1 => continuation.Invoke(), s),
                null, 1000, Timeout.Infinite);
        }


        public string GetResult()
        {
            return mResult;
        }
    }
}
