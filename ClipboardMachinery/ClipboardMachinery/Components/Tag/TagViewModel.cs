using Caliburn.Micro;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ClipboardMachinery.Components.DialogOverlay;
using Castle.Core;

namespace ClipboardMachinery.Components.Tag {

    public class TagViewModel : Screen {

        #region Properties

        [DoNotWire]
        public TagModel Model {
            get => model;
            private set {
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

        public bool HasDescription
            => !string.IsNullOrWhiteSpace(Model?.Description);

        public bool IsValueOverflowing
            => MeasureValueString(Model?.Value).Width >= 96;

        public SolidColorBrush BackgroundColor
            => Model?.Color.HasValue == true
                ? new SolidColorBrush(Color.FromArgb(40, Model.Color.Value.R, Model.Color.Value.G, Model.Color.Value.B))
                : Brushes.Transparent;

        #endregion

        #region Fields

        private readonly IDialogOverlayManager dialogOverlayManager;

        private TagModel model;

        #endregion

        public TagViewModel(TagModel tagModel, IDialogOverlayManager dialogOverlayManager) {
            this.dialogOverlayManager = dialogOverlayManager;
            Model = tagModel;
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

        #region Logic

        private static Size MeasureValueString(string candidate) {
            if (string.IsNullOrEmpty(candidate)) {
                return Size.Empty;
            }

            FormattedText formattedText = new FormattedText(
                textToFormat: candidate,
                culture: CultureInfo.CurrentCulture,
                flowDirection: FlowDirection.LeftToRight,
                typeface: new Typeface("Calibri Light"),
                emSize: 14D,
                foreground: Brushes.Black
            );

            return new Size(formattedText.Width, formattedText.Height);
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

    }

}
