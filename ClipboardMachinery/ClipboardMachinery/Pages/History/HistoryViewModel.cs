using System;
using System.Linq;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Core.Data;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.Pages.History {

    public class HistoryViewModel : ClipPageBase, IScreenPage {

        #region IScreenPage

        public string Title
            => "History";

        public string Icon
            => "IconHistory";

        public byte Order
            => 1;

        #endregion

        public HistoryViewModel(IDataRepository dataRepository, IClipViewModelFactory clipVmFactory) : base(15, dataRepository, clipVmFactory) {
        }

        #region Logic

        protected override bool IsAllowedAddClipsFromKeyboard(ClipEvent message) {
            return true;
        }

        protected override bool IsClearingItemsWhenDeactivating() {
            return false;
        }

        #endregion

        #region Handlers

        protected override async void OnKeyboardClipAdded(ClipViewModel newClip) {
            base.OnKeyboardClipAdded(newClip);

            // When new clip is added and user is not scrolling, we try to keep loaded clip count at size of one batch
            // This prevents from having too many items slowing down deactivation and switching between pages
            if (Items.Count <= DataProvider.BatchSize || !(Math.Abs(VerticalScrollOffset) < 4)) {
                return;
            }

            ClipViewModel lastClip = Items.Last();
            await lastClip.TryCloseAsync();
            clipVmFactory.Release(lastClip);
        }

        #endregion

    }

}
