using System;
using Caliburn.Micro;
using ClipboardMachinery.Events;
using ClipboardMachinery.Events.Collection;
using ClipboardMachinery.ViewModels;

namespace ClipboardMachinery.Logic.ClipboardItemsProvider {

    public interface IClipboardItemsProvider : IHandle<SetViewFilter>, IHandle<ItemRemoved<ClipViewModel>> {

        BindableCollection<ClipViewModel> Items { get; }

        void SetFilter(Predicate<object> filter);

    }

}
