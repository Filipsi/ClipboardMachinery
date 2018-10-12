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

        public void Register(Key key, KeyModifier keyModifiers, Action<HotKey> action)
            => hotKeys.Add(new HotKey(key, keyModifiers, action));

        #endregion

    }

}
