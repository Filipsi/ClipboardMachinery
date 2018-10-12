using Caliburn.Micro;
using ClipboardMachinery.Events;
using ClipboardMachinery.ViewModels;
using System;

namespace ClipboardMachinery.Plumbing {

    public interface IShell : IConductor, IHandle<ItemRemoved<ClipViewModel>> {

        IObservableCollection<ClipViewModel> ClipboardItems { get; }

        bool IsVisible { get; set; }

        void SetClipViewFiler(Predicate<object> filter);

    }

}
