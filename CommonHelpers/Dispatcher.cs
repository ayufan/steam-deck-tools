namespace CommonHelpers
{
    public static class Dispatcher
    {
        public static CancellationTokenSource RunWithDelay(int delayMs, Action action)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            Task.Delay(1000, cancellationTokenSource.Token).ContinueWith(_ =>
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(action);
            });

            return cancellationTokenSource;
        }
    }
}
