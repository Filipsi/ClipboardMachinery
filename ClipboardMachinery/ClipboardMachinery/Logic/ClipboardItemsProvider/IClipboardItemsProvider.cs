using System;
using Caliburn.Micro;
using ClipboardMachinery.ViewModels;

namespace ClipboardMachinery.Logic.ClipboardItemsProvider {

    internal interface IClipboardItemsProvider {

        BindableCollection<ClipViewModel> Items { get; }

        void SetFilter(Predicate<object> filter);

    }

}
