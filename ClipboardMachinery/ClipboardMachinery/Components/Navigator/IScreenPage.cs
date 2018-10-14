using Caliburn.Micro;

namespace ClipboardMachinery.Components.Navigator {

    public interface IScreenPage : IScreen {

        string Title { get; }

        string Icon { get; }

        byte Order { get; }

    }

}
