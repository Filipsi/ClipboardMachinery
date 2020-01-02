using System;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClipboardMachinery.Components.Clip;

namespace ClipboardMachinery.Components.ContentPresenter {

    public abstract class ContentScreen : Screen {

        #region Properties

        public IContentPresenter ContentPresenter { get; }

        public ClipViewModel Clip { get; }

        #endregion

        #region Fields

        private readonly Action<ContentScreen> releaseFn;

        #endregion

        protected ContentScreen(IContentPresenter creator, ClipViewModel owner, Action<ContentScreen> releaseFn) {
            ContentPresenter = creator;
            Clip = owner;

            this.releaseFn = releaseFn;
            this.ConductWith(owner);
        }

        #region Lifecycle

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            if (close) {
                releaseFn?.Invoke(this);
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion

    }

}
