﻿using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace ClipboardMachinery.Core.Services.HotKeys {

    public interface IHotKeyService {

        IReadOnlyList<HotKey> HotKeys { get; }

        HotKey Register(Key key, KeyModifier keyModifiers, Action<HotKey> action);

    }

}
