using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace ClipboardMachinery.Core.Services.HotKeys {

    public class HotKeyService : IHotKeyService {

        #region Properties

        public IReadOnlyList<HotKey> HotKeys {
            get;
        }

        #endregion

        #region Fields

        private readonly List<HotKey> hotKeys;

        #endregion

        public HotKeyService() {
            hotKeys = new List<HotKey>();
            HotKeys = hotKeys.AsReadOnly();
        }

        #region IHotKeyService

        public HotKey Register(Key key, KeyModifier keyModifiers, Action<HotKey> action) {
            // Bail out when service is disposed, no new key-binds can be registered
            if (disposed) {
                throw new ObjectDisposedException(nameof(HotKeyService));
            }

            // Register new key-bind
            HotKey keyBind = new HotKey(key, keyModifiers, action);
            hotKeys.Add(keyBind);
            return keyBind;
        }

        #endregion

        #region IDisposable

        private bool disposed;

        protected virtual void Dispose(bool disposing) {
            if (disposed) {
                return;
            }

            if (disposing) {
                foreach (HotKey keyBind in HotKeys) {
                    keyBind.Dispose();
                }

                hotKeys.Clear();
            }

            disposed = true;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
