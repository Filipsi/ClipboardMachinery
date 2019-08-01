using System;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Plumbing.Factories;
using ClipboardMachinery.Popup.TagTypeEditor;

namespace ClipboardMachinery.Components.TagType {

    public class TagTypeViewModel : Screen {

        #region Properties

        public TagTypeModel Model {
            get => model;
            private set {
                if (model == value) {
                    return;
                }

                model = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsFocused {
            get => isFocused;
            set {
                if (isFocused == value) {
                    return;
                }

                isFocused = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => SelectionColor);
            }
        }

        public bool CanEdit {
            get => canEdit;
            private set {
                if (canEdit == value) {
                    return;
                }

                canEdit = value;
                NotifyOfPropertyChange();
            }
        }

        public SolidColorBrush SelectionColor
            => Application.Current.FindResource(IsFocused ? "ElementSelectBrush" : "PanelControlBrush") as SolidColorBrush;

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;
        private readonly IPopupFactory popupFactory;

        private TagTypeModel model;
        private bool isFocused;
        private bool canEdit = true;

        #endregion

        public TagTypeViewModel(TagTypeModel model, IEventAggregator eventAggregator, IPopupFactory popupFactory) {
            Model = model;
            this.eventAggregator = eventAggregator;
            this.popupFactory = popupFactory;
        }

        #region Handlers

        private void OnTagTypeEditorDeactivated(object sender, DeactivationEventArgs e) {
            TagTypeEditorViewModel tagTypeEditor = (TagTypeEditorViewModel)sender;
            tagTypeEditor.Deactivated -= OnTagTypeEditorDeactivated;
            popupFactory.Release(tagTypeEditor);
            CanEdit = true;
        }

        #endregion

        #region Actions

        public void Edit() {
            TagTypeEditorViewModel tagTypeEditor = popupFactory.CreateTagTypeEditor(Model);
            tagTypeEditor.Deactivated += OnTagTypeEditorDeactivated;
            CanEdit = false;
            eventAggregator.PublishOnCurrentThreadAsync(PopupEvent.Show(tagTypeEditor));
        }

        public void Focus() {
            IsFocused = true;
        }

        public void Unfocus() {
            IsFocused = false;
        }

        #endregion

    }

}
