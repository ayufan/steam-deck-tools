namespace CommonHelpers
{
    public class MyLock : IDisposable
    {
        private object context;

        public MyLock(object context)
        {
            this.context = context;
            Monitor.Enter(this.context);
        }

        public MyLock(object lockObj, int millisecondsTimeout)
        {
            this.context = lockObj;
            if (!Monitor.TryEnter(this.context, millisecondsTimeout))
                RaiseTimeout();
        }

        public MyLock(object lockObj, TimeSpan timeout)
        {
            this.context = lockObj;
            if (!Monitor.TryEnter(this.context, timeout))
                RaiseTimeout();
        }

        public void Dispose()
        {
            Monitor.Exit(context);
        }

        private void RaiseTimeout()
        {
            var message = String.Format("Lock took too long for: {0}\n{1}",
                this.context, Environment.StackTrace);

            Log.TraceError("RaiseTimeout: {0}", message);

            MessageBox.Show(message, "Lock timeout");
            throw new TimeoutException();
        }
    }
}
