using System.Windows.Input;
using Bibliotheque.Machine;
using Caliburn.Micro;
using ClipboardMachinery.Events;

namespace ClipboardMachinery.Logic.HotKeyHandler {

    public class HotKeyHandler : IHotKeyHandler {

        public HotKey[] HotKeys { get; }

        private readonly IEventAggregator _eventAggregator;

        public HotKeyHandler(IEventAggregator eventAggregator) {
            _eventAggregator = eventAggregator;

            HotKeys = new[] {
                new HotKey(Key.H, HotKey.KeyModifier.Ctrl, OnVisiblityHotKeyPress)
            };
        }

        private void OnVisiblityHotKeyPress(HotKey hotKey) {
            _eventAggregator.PublishOnUIThread(new ChangeAppVisiblity(VisiblityChangeType.Toggle));
        }

    }

}
