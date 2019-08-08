using System;
using System.Threading;
using System.Threading.Tasks;

namespace CadExtract.Library
{
    public static class ActionExtensions
    {
        // FROM: https://stackoverflow.com/a/50461641/567524
        public static void InvokeWithTimeout(this Action action, int timeoutMs)
        {
            var src = new CancellationTokenSource();
            var task = Task.Factory.StartNew(action, src.Token);
            task.Wait(timeoutMs);
            src.Cancel();
        }
    }
}
