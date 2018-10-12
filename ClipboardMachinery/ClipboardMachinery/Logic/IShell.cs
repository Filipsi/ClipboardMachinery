using Caliburn.Micro;
using ClipboardMachinery.Events;
using ClipboardMachinery.Events.Collection;
using ClipboardMachinery.ViewModels;

namespace ClipboardMachinery.Logic {

    internal interface IShell : IConductor, IHandle<ChangeAppVisiblity>, IHandle<PageSelected>{
    }

}
