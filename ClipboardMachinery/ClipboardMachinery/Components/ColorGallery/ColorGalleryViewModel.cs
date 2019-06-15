using Caliburn.Micro;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.ColorGallery.Presets;
using ClipboardMachinery.Plumbing.Factories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ClipboardMachinery.Components.ColorGallery {

    public class ColorGalleryViewModel : Screen {

        #region Properties

        public IColorPalette SelectedPreset {
            get => selectedPreset;
            private set {
                if (selectedPreset == value) {
                    return;
                }

                selectedPreset = value;
                NotifyOfPropertyChange();
            }
        }

        public Color SelectedColor {
            get => selectedColor;
            private set {
                if (selectedColor == value) {
                    return;
                }

                selectedColor = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<IColorPalette> Presets {
            get;
        }

        public ActionButtonViewModel PreviousPageButton {
            get;
        }

        public ActionButtonViewModel NextPageButton {
            get;
        }

        #endregion

        #region Fields

        private readonly IColorGalleryFactory colorGalleryFactory;

        private IColorPalette selectedPreset;
        private Color selectedColor;

        #endregion

        public ColorGalleryViewModel(IColorGalleryFactory colorGalleryFactory, Func<ActionButtonViewModel> actionButtonFactory) {
            this.colorGalleryFactory = colorGalleryFactory;

            Presets = new BindableCollection<IColorPalette>(colorGalleryFactory.GetAllPresets());
            SelectedPreset = Presets.FirstOrDefault();

            // Create control buttons
            PreviousPageButton = actionButtonFactory.Invoke();
            PreviousPageButton.ToolTip = "Previous preset";
            PreviousPageButton.Icon = (Geometry)Application.Current.FindResource("IconLeftChevron");
            PreviousPageButton.ClickAction = HandlePreviousPresetClick;

            NextPageButton = actionButtonFactory.Invoke();
            NextPageButton.ToolTip = "Next preset";
            NextPageButton.Icon = (Geometry)Application.Current.FindResource("IconRightChevron");
            NextPageButton.ClickAction = HandleNextPresetClick;
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            if (close) {
                foreach (IColorPalette preset in Presets) {
                    colorGalleryFactory.Release(preset);
                }
            }

            await base.OnDeactivateAsync(close, cancellationToken);
        }

        #region Actions

        public void SelectColor(Color newColor) {
            if (!SelectedPreset.Colors.Contains(newColor)) {
                IColorPalette palette = Presets.FirstOrDefault(preset => preset.Colors.Contains(newColor));
                if (palette != null) {
                    SelectedPreset = palette;
                }
            }

            SelectedColor = newColor;
        }

        #endregion

        #region Handlers

        private Task HandlePreviousPresetClick(ActionButtonViewModel obj) {
            if (SelectedPreset == null) {
                if (Presets.Count > 0) {
                    SelectedPreset = Presets.LastOrDefault();
                }

                return Task.CompletedTask;
            }

            int currentIndex = Presets.IndexOf(SelectedPreset);
            if (currentIndex - 1 < 0) {
                currentIndex = Presets.Count - 1;
            } else {
                currentIndex--;
            }

            SelectedPreset = Presets[currentIndex];
            return Task.CompletedTask;
        }

        private Task HandleNextPresetClick(ActionButtonViewModel source) {
            if (SelectedPreset == null) {
                if (Presets.Count > 0) {
                    SelectedPreset = Presets.FirstOrDefault();
                }

                return Task.CompletedTask;
            }

            int currentIndex = Presets.IndexOf(SelectedPreset);
            if (currentIndex + 1 > Presets.Count - 1) {
                currentIndex = 0;
            } else {
                currentIndex++;
            }

            SelectedPreset = Presets[currentIndex];
            return Task.CompletedTask;
        }

        #endregion

    }

}
