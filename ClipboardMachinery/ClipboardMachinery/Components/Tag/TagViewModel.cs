using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Plumbing.Factories;
using ClipboardMachinery.Popup.TagEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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
                NotifyOfPropertyChange(() => Text);
            }
        }

        public string Text
            => $"{Model?.Name}: {Model?.Value}";

        public SolidColorBrush BackgroundColor
            => model.Color.HasValue ? new SolidColorBrush(model.Color.Value) : Brushes.Transparent;

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;
        private readonly IPopupFactory popupFactory;

        private TagModel model;

        #endregion

        public TagViewModel(IEventAggregator eventAggregator, IPopupFactory popupFactory) {
            this.eventAggregator = eventAggregator;
            this.popupFactory = popupFactory;
        }

        #region Actions

        public void Edit() {
            TagEditorViewModel tagEditor = popupFactory.CreateTagEditor(model);
            tagEditor.Deactivated += (sender, args) => {
                popupFactory.Release(tagEditor);
            };

            eventAggregator.PublishOnCurrentThreadAsync(PopupEvent.Show(tagEditor));
        }

        #endregion

        #region Handlers

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(TagModel.Name) || e.PropertyName == nameof(TagModel.Value)) {
                NotifyOfPropertyChange(() => Text);
                return;
            }

            if (e.PropertyName == nameof(TagModel.Color)) {
                NotifyOfPropertyChange(() => BackgroundColor);
                return;
            }
        }

        #endregion


    }

}
