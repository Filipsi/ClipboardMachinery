using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using Castle.Core.Logging;

namespace ClipboardMachinery.Core.Services.HotKeys {

    public class HotKey : IDisposable {

        #region Properties

        public ILogger Logger {
            get;
            set;
        }

        public Key Key {
            get;
        }

        public KeyModifier KeyModifiers {
            get;
        }

        public Action<HotKey> Action {
            get;
        }

        public int Id {
            get;
            set;
        }

        #endregion

        #region Fields

        private static Dictionary<int, HotKey> callbackMap;
        private bool disposed;

        #endregion

        internal HotKey(Key key, KeyModifier keyModifiers, Action<HotKey> action, bool register = true) {
            Logger = NullLogger.Instance;
            Key = key;
            KeyModifiers = keyModifiers;
            Action = action;

            if (register) {
                Register();
            }
        }

        #region Logic

        public bool Register() {
            int virtualKeyCode = KeyInterop.VirtualKeyFromKey(Key);
            Id = virtualKeyCode + (int)KeyModifiers * 0x10000;
            bool result = NativeMethods.RegisterHotKey(IntPtr.Zero, Id, (uint)KeyModifiers, (uint)virtualKeyCode);

            if (callbackMap == null) {
                callbackMap = new Dictionary<int, HotKey>();
                ComponentDispatcher.ThreadFilterMessage += ComponentDispatcherThreadFilterMessage;
            }

            callbackMap.Add(Id, this);

            Logger.Info($"Resisting hot-key hook for '{Key}': Result={result}, HookId={Id}, KeyCode={virtualKeyCode}");
            return result;
        }

        public void Unregister() {
            if (!callbackMap.TryGetValue(Id, out _)) {
                return;
            }

            Logger.Info($"Removing hot-key hook with id '{Id}'.");
            NativeMethods.UnregisterHotKey(IntPtr.Zero, Id);
        }

        private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled) {
            if (handled) return;
            if (msg.message != NativeMethods.WmHotKey) return;
            if (!callbackMap.TryGetValue((int)msg.wParam, out HotKey hotKey)) return;

            hotKey.Action?.Invoke(hotKey);
            handled = true;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposed) {
                return;
            }

            if (disposing) {
                Unregister();
            }

            disposed = true;
        }

        #endregion

        private static class NativeMethods {
            // Refer to https://msdn.microsoft.com/en-us/library/windows/desktop/ms646279(v=vs.85).aspx
            public const int WmHotKey = 0x0312;

            // Refer to https://msdn.microsoft.com/en-us/library/windows/desktop/ms646309(v=vs.85).aspx
            [DllImport("user32.dll")]
            public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vlc);

            // Refer to https://msdn.microsoft.com/en-us/library/windows/desktop/ms646327(v=vs.85).aspx
            [DllImport("user32.dll")]
            public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        }

    }

}
