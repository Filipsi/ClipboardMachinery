using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.Components.DialogOverlay {

    public interface IDialogOverlayManager {

        IScreen Portal { get; }

        IDialogOverlayFactory Factory { get; }

        Task OpenDialog(IOverlayDialog dialog);

        Task OpenDialog<T>(Func<T> createFn, Action<T> releaseFn) where T : IOverlayDialog;

        Task CloseDialog();

    }

}
