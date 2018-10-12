using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Bibliotheque.Machine;
using Caliburn.Micro;
using ClipboardMachinery.Events;
using ClipboardMachinery.Events.Collection;
using ClipboardMachinery.FileSystem;
using ClipboardMachinery.Models;
using ClipboardMachinery.ViewModels;

namespace ClipboardMachinery.Logic.ClipboardItemsProvider {

    public class ClipboardItemsProvider : IClipboardItemsProvider {

        #region Properties

        public BindableCollection<ClipViewModel> Items {
            get;
        }

        public ICollectionView ItemsView {
            get;
        }

        #endregion

        #region Fields

        private readonly Func<ClipViewModel> clipVmFactory;
        private readonly ClipFile storage;

        #endregion

        public ClipboardItemsProvider(ClipFile clipFile, Func<ClipViewModel> clipViewModelFactory) {
            storage = clipFile;
            clipVmFactory = clipViewModelFactory;

            Items = new BindableCollection<ClipViewModel>();
            foreach (ClipModel model in storage.Favorites) {
                ClipViewModel vm = clipViewModelFactory.Invoke();
                vm.Model = model;
                Items.Add(vm);
            }

            ItemsView = CollectionViewSource.GetDefaultView(Items);

            ClipboardObserver.ClipboardChanged += OnClipboardChanged;
        }

        #region IClipboardItemsProvider

        public void SetFilter(Predicate<object> filter) {
            ItemsView.Filter = filter;
        }

        #endregion

        #region Handlers

        private void OnClipboardChanged(object sender, Bibliotheque.Machine.Event.ClipboardEventArgs e) {
            if (e.Payload == string.Empty || e.Payload == Items.FirstOrDefault()?.Model.RawContent) {
                return;
            }

            ClipViewModel vm = clipVmFactory.Invoke();
            vm.Model = new ClipModel {
                Created = DateTime.UtcNow,
                RawContent = e.Payload
            };

            Items.Insert(0, vm);
        }

        public void Handle(SetViewFilter message)
            => SetFilter(message.Filter);

        public void Handle(ItemRemoved<ClipViewModel> message)
            => Items.Remove(message.Item);

        #endregion

    }

}
