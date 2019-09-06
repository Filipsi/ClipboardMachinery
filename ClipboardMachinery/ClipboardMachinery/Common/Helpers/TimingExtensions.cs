using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardMachinery.Common.Helpers {

    public static class TimingExtensions {

        public static Action<T> Debounce<T>(this Action<T> func, int milliseconds = 300) {
            int lastInvoke = 0;

            return arg => {
                int currentInvoke = Interlocked.Increment(ref lastInvoke);
                Task.Delay(milliseconds).ContinueWith(task => {
                    if (currentInvoke == lastInvoke) {
                        func(arg);
                    }

                    task.Dispose();
                });
            };
        }

        public static Action Debounce(this Action func, int milliseconds = 300) {
            int lastInvoke = 0;

            return () => {
                int currentInvoke = Interlocked.Increment(ref lastInvoke);
                Task.Delay(milliseconds).ContinueWith(task => {
                    if (currentInvoke == lastInvoke) {
                        func();
                    }

                    task.Dispose();
                });
            };
        }

    }

}
