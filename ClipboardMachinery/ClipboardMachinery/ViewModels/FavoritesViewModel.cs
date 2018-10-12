using System.Linq;
using Caliburn.Micro;
using ClipboardMachinery.Core.Services.Clipboard;
using ClipboardMachinery.Events;
using ClipboardMachinery.Plumbing;

namespace ClipboardMachinery.ViewModels {

    public class FavoritesViewModel : HistoryViewModel, IHandle<ItemFavoriteChanged<ClipViewModel>> {

        public new bool ErrorMessageIsVisible
            => !shell.ClipboardItems.Any(vm => vm.Model.IsFavorite);

        public FavoritesViewModel(IEventAggregator eventAggregator, IClipboardService clipboardService, IShell shellVm) : base(eventAggregator, clipboardService, shellVm) {
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            ApplyItemFilter();
        }

        private void ApplyItemFilter()
            => shell.SetClipViewFiler(vm => ((ClipViewModel)vm).Model.IsFavorite);

        #region Event Handlers

        public void Handle(ItemFavoriteChanged<ClipViewModel> message) {
            if (!IsActive) return;
            if (message.Item.Model.IsFavorite) return;

            ApplyItemFilter();
            if (shell.ClipboardItems.Count > 0) {
                NotifyOfPropertyChange(() => ErrorMessageIsVisible);
            }
        }

        #endregion

    }

}
