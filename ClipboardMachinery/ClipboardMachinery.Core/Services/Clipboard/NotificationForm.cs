using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClipboardMachinery.Core.Services.Clipboard {

    internal class NotificationForm : Form {

        private readonly IClipboardService clipboardService;

        public NotificationForm(IClipboardService clipboardService) {
            this.clipboardService = clipboardService;

            NativeMethods.SetParent(Handle, NativeMethods.HwndMessage);
            NativeMethods.AddClipboardFormatListener(Handle);
        }

        protected override void WndProc(ref Message m) {
            if (m.Msg == NativeMethods.WmClipboardupdate) {
                clipboardService.NotifyOfClipboardChange();
            }

            base.WndProc(ref m);
        }

        internal static class NativeMethods {

            // See http://msdn.microsoft.com/en-us/library/ms649021%28v=vs.85%29.aspx
            public const int WmClipboardupdate = 0x031D;
            public static IntPtr HwndMessage = new IntPtr(-3);

            // See http://msdn.microsoft.com/en-us/library/ms632599%28VS.85%29.aspx#message_only
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AddClipboardFormatListener(IntPtr hwnd);

            // See http://msdn.microsoft.com/en-us/library/ms633541%28v=vs.85%29.aspx
            // See http://msdn.microsoft.com/en-us/library/ms649033%28VS.85%29.aspx
            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        }

    }

}
