using System;
using System.Linq;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Common.Screen;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Core.DataStorage;
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

        public HistoryViewModel(IDataRepository dataRepository, IEventAggregator eventAggregator, IClipFactory vmFactory) : base(15, dataRepository, eventAggregator, vmFactory) {
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
            vmFactory.Release(lastClip);
        }

        #endregion

    }

}
