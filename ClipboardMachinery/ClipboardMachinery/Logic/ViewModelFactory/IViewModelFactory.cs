using System;
using Caliburn.Micro;

namespace ClipboardMachinery.Logic.ViewModelFactory {

    internal interface IViewModelFactory {

        T Create<T>() where T : IScreen;

        IScreen Create(Type type);

    }

}
