﻿using Caliburn.Micro;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.ColorGallery.Presets;
using ClipboardMachinery.Plumbing.Factories;
using System;
using System.Linq;
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

        protected override void OnDeactivate(bool close) {
            base.OnDeactivate(close);

            if (close) {
                foreach(IColorPalette preset in Presets) {
                    colorGalleryFactory.Release(preset);
                }
            }
        }

        #region Actions

        public void SelectColor(Color selectedColor) {
            if (!SelectedPreset.Colors.Contains(selectedColor)) {
                IColorPalette palette = Presets.FirstOrDefault(preset => preset.Colors.Contains(selectedColor));
                if (palette != null) {
                    SelectedPreset = palette;
                }
            }

            SelectedColor = selectedColor;
        }

        #endregion

        #region Handlers

        private void HandlePreviousPresetClick(ActionButtonViewModel obj) {
            if (SelectedPreset == null) {
                if (Presets.Count > 0) {
                    SelectedPreset = Presets.LastOrDefault();
                }
                return;
            }

            int currentIndex = Presets.IndexOf(SelectedPreset);
            if (currentIndex - 1 < 0) {
                currentIndex = Presets.Count - 1;
            } else {
                currentIndex--;
            }

            SelectedPreset = Presets[currentIndex];
        }

        private void HandleNextPresetClick(ActionButtonViewModel obj) {
            if (SelectedPreset == null) {
                if (Presets.Count > 0) {
                    SelectedPreset = Presets.FirstOrDefault();
                }
                return;
            }

            int currentIndex = Presets.IndexOf(SelectedPreset);
            if (currentIndex + 1 > Presets.Count - 1) {
                currentIndex = 0;
            } else {
                currentIndex++;
            }

            SelectedPreset = Presets[currentIndex];
        }

        #endregion

    }

}
