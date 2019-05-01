using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ClipboardMachinery.Core.Services.Clipboard {

    internal static class WindowHelper {

        public static string GetActiveProcessName() {
            IntPtr hwnd = NativeMethods.GetForegroundWindow();
            NativeMethods.GetWindowThreadProcessId(hwnd, out uint pid);
            return Process.GetProcessById((int)pid).ProcessName;
        }

        private static class NativeMethods {

            // See https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-getwindowthreadprocessid
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

            // https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-getforegroundwindow
            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

        }

    }
}
