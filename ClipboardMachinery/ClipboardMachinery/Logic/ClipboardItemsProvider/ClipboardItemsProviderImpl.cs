using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Bibliotheque.Machine;
using Caliburn.Micro;
using ClipboardMachinery.Events.Collection;
using ClipboardMachinery.FileSystem;
using ClipboardMachinery.Logic.ViewModelFactory;
using ClipboardMachinery.ViewModels;
using Ninject;

namespace ClipboardMachinery.Logic.ClipboardItemsProvider {

    internal class ClipboardItemsProviderImpl : IClipboardItemsProvider {

        public BindableCollection<ClipViewModel> Items     { get; }
        public ICollectionView                   ItemsView { get; }

        private readonly IViewModelFactory _viewModelFactory;

        public ClipboardItemsProviderImpl(IViewModelFactory viewModelFactory) {
            _viewModelFactory = viewModelFactory;

            Items = new BindableCollection<ClipViewModel>(ClipFile.Instance.Favorites);
            ItemsView = CollectionViewSource.GetDefaultView(Items);

            ClipboardObserver.ClipboardChanged += OnClipboardChanged;
        }

        private void OnClipboardChanged(object sender, Bibliotheque.Machine.Event.ClipboardEventArgs e) {
            if (e.Payload == string.Empty || e.Payload == Items.FirstOrDefault()?.RawContent) {
                return;
            }

            ClipViewModel model = _viewModelFactory.Create<ClipViewModel>();

            model.Created = DateTime.UtcNow;
            model.RawContent = e.Payload;

            Items.Insert(0, model);
        }

        public void SetFilter(Predicate<object> filter) {
            ItemsView.Filter = filter;
        }

    }

}
