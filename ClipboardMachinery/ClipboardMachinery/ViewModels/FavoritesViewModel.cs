using System.Linq;
using Caliburn.Micro;
using ClipboardMachinery.Core.Services.Clipboard;
using ClipboardMachinery.Events;
using ClipboardMachinery.Plumbing;

namespace ClipboardMachinery.ViewModels {

    public class FavoritesViewModel : HistoryViewModel, IPage, IHandle<ItemFavoriteChanged<ClipViewModel>> {

        #region Properties

        public new bool ErrorMessageIsVisible
           => !shell.ClipboardItems.Any(vm => vm.Model.IsFavorite);

        #endregion

        #region IPage

        public new string Title { get; } = "Favorites";

        public new string Icon { get; } = "IconStarFull";

        #endregion

        public FavoritesViewModel(IClipboardService clipboardService) : base(clipboardService) {
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            ApplyItemFilter();
        }

        #region Handlers

        private void ApplyItemFilter()
            => shell.SetClipViewFiler(vm => ((ClipViewModel)vm).Model.IsFavorite);

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
