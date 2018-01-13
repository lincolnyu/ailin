using System;
using System.Threading.Tasks;

namespace WebKit
{
    public class AsyncLock
    {
        public class Handle : IDisposable
        {
            public AsyncLock Owner { get; private set; }
            public Handle(AsyncLock owner)
            {
                Owner = owner;
            }

            public void Dispose()
            {
                if (Owner != null)
                {
                    Owner.Release();
                    Owner = null;
                }
            }
        }

        public bool InUse { get; private set; }

        public async Task<Handle> Wait(int granularityInMilliseconds = 100)
            => await Wait(TimeSpan.FromMilliseconds(granularityInMilliseconds));

        public async Task<Handle> Wait(TimeSpan grandularity)
        {
            while (InUse)
            {
                await Task.Delay(grandularity);
            }
            InUse = true;
            return new Handle(this);
        }

        public void Release()
        {
            InUse = false;
        }
    }
}
