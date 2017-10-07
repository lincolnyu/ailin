using System.Threading;

namespace WebKit
{
    public class Canceller
    {
        private CancellationTokenSource _source = new CancellationTokenSource();
        public CancellationToken Token => _source.Token;
        public void Cancel()
        {
            _source.Cancel();
            _source = new CancellationTokenSource();
        }
    }
}
