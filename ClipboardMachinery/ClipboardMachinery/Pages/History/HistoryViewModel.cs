using System.Linq;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Core.Repository;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.Pages.History {

    public class HistoryViewModel : LazyClipPage, IScreenPage {

        #region IPage

        public string Title
            => "History";

        public string Icon
            => "IconHistory";

        public byte Order
            => 1;

        #endregion

        public HistoryViewModel(IDataRepository dataRepository, IClipViewModelFactory clipVmFactory) : base(15, dataRepository, clipVmFactory) {
        }

        #region Handlers

        protected override void OnKeyboardClipAdded(ClipViewModel newClip) {
            base.OnKeyboardClipAdded(newClip);

            // When new clip is added and user is not scrolling, we try to keep loaded clip count at size of one batch
            // This prevents from having too many items slowing down deactivation and switching between pages
            if (Items.Count > batchSize && VerticalScrollOffset == 0) {
                ClipViewModel lastClip = Items.Last();
                Items.Remove(lastClip);
                clipVmFactory.Release(lastClip);
            }
        }

        #endregion

    }

}
