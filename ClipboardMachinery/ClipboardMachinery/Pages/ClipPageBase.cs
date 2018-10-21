using Caliburn.Micro;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Pages {

    public abstract class ClipPageBase : Screen {

        #region Properties

        public bool ErrorMessageIsVisible
            => ClipboardItems.Count == 0;

        public BindableCollection<ClipViewModel> ClipboardItems {
            get;
        }

        #endregion

        protected readonly IDataRepository dataRepository;

        public ClipPageBase(IDataRepository dataRepository) {
            this.dataRepository = dataRepository;

            ClipboardItems = new BindableCollection<ClipViewModel>();
            ClipboardItems.CollectionChanged += OnClipboardItemsCollectionChanged;
        }

        #region Handlers

        private async void OnClipRemovalRequest(object sender, EventArgs e) {
            ClipViewModel clipVm = sender as ClipViewModel;
            ClipboardItems.Remove(clipVm);
            await dataRepository.DeleteClip(clipVm.Model.Id);
        }

        private void OnClipboardItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    NotifyOfPropertyChange(() => ErrorMessageIsVisible);
                    foreach (ClipViewModel clipVm in e.NewItems) {
                        clipVm.RemovalRequest += OnClipRemovalRequest;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    NotifyOfPropertyChange(() => ErrorMessageIsVisible);
                    foreach (ClipViewModel clipVm in e.OldItems) {
                        clipVm.RemovalRequest -= OnClipRemovalRequest;
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    NotifyOfPropertyChange(() => ErrorMessageIsVisible);
                    foreach (ClipViewModel clipVm in sender as ICollection<ClipViewModel>) {
                        clipVm.RemovalRequest -= OnClipRemovalRequest;
                    }
                    break;
            }
        }

        #endregion

    }

}
