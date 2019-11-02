using Caliburn.Micro;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ClipboardMachinery.Components.DialogOverlay;

namespace ClipboardMachinery.Components.Tag {

    public class TagViewModel : Screen {

        #region Properties

        public TagModel Model {
            get => model;
            set {
                if (model == value) {
                    return;
                }

                if (model != null) {
                    model.PropertyChanged -= OnModelPropertyChanged;
                }

                if (value != null) {
                    value.PropertyChanged += OnModelPropertyChanged;
                }

                model = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => HasDescription);
                NotifyOfPropertyChange(() => IsValueOverflowing);
            }
        }

        private bool HasModel
            => Model != null;

        public bool HasDescription
            => HasModel && !string.IsNullOrWhiteSpace(Model.Description);

        public bool IsValueOverflowing
            => HasModel && MeasureValueString(Model.Value).Width >= 96;

        public SolidColorBrush BackgroundColor
            => model.Color.HasValue
                ? new SolidColorBrush(Color.FromArgb(40, model.Color.Value.R, model.Color.Value.G, model.Color.Value.B))
                : Brushes.Transparent;

        #endregion

        #region Fields

        private readonly IDialogOverlayManager dialogOverlayManager;

        private TagModel model;

        #endregion

        public TagViewModel(IDialogOverlayManager dialogOverlayManager) {
            this.dialogOverlayManager = dialogOverlayManager;
        }

        #region Handlers

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(TagModel.Color):
                    NotifyOfPropertyChange(() => BackgroundColor);
                    break;

                case nameof(TagModel.Description):
                    NotifyOfPropertyChange(() => HasDescription);
                    break;

                case nameof(TagModel.Value):
                    NotifyOfPropertyChange(() => IsValueOverflowing);
                    break;
            }
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            if (close) {
                Model = null;
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Actions

        public void Edit() {
            dialogOverlayManager.OpenDialog(
                () => dialogOverlayManager.Factory.CreateTagEditor(Model),
                tagEditor => dialogOverlayManager.Factory.Release(tagEditor)
            );
        }

        #endregion

        #region Helpers

        private static Size MeasureValueString(string candidate) {
            // Bail out when candidate text is empty
            if (string.IsNullOrEmpty(candidate)) {
                return Size.Empty;
            }

            FormattedText formattedText = new FormattedText(
                textToFormat: candidate,
                culture: CultureInfo.CurrentCulture,
                flowDirection: FlowDirection.LeftToRight,
                typeface: new Typeface("Calibri Light"),
                emSize: 14D,
                foreground: Brushes.Black,
                numberSubstitution: new NumberSubstitution(),
                textFormattingMode: TextFormattingMode.Display
            );

            return new Size(formattedText.Width, formattedText.Height);
        }

        #endregion

    }

}
