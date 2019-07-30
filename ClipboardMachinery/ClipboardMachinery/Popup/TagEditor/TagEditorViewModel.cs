using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.ColorGallery;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Core.Data;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ClipboardMachinery.Popup.Manager;
using static ClipboardMachinery.Common.Events.TagEvent;

namespace ClipboardMachinery.Popup.TagEditor {

    public class TagEditorViewModel : Screen, IPopupExtendedControls {

        #region IPopupExtendedControls

        public BindableCollection<ActionButtonViewModel> ExtensionControls { get; }

        #endregion

        #region Properties

        public TagModel Model {
            get;
        }

        public object Value {
            get => val;
            set {
                if (val == value) {
                    return;
                }

                val = value;
                NotifyOfPropertyChange();
            }
        }

        public ColorGalleryViewModel ColorGallery {
            get;
        }

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;
        private readonly IDataRepository dataRepository;

        private object val;

        #endregion

        public TagEditorViewModel(
            TagModel tagModel, Func<ActionButtonViewModel> actionButtonFactory,
            IEventAggregator eventAggregator, IDataRepository dataRepository, ColorGalleryViewModel colorGalleryVm) {

            Model = tagModel;
            Value = Model.Value;
            ExtensionControls = new BindableCollection<ActionButtonViewModel>();
            this.eventAggregator = eventAggregator;
            this.dataRepository = dataRepository;

            // Setup color gallery
            ColorGallery = colorGalleryVm;
            if(Model.Color.HasValue) {
                ColorGallery.SelectColor(Model.Color.Value);
            }

            // Create extension control buttons
            ActionButtonViewModel removeButton = actionButtonFactory.Invoke();
            removeButton.ToolTip = "Remove";
            removeButton.Icon = (Geometry)Application.Current.FindResource("IconRemove");
            removeButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("DangerousActionBrush");
            removeButton.ClickAction = HandleRemoveClick;
            removeButton.ConductWith(this);
            ExtensionControls.Add(removeButton);

            ActionButtonViewModel saveButton = actionButtonFactory.Invoke();
            saveButton.ToolTip = "Save";
            saveButton.Icon = (Geometry)Application.Current.FindResource("IconSave");
            saveButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("ElementSelectBrush");
            saveButton.ClickAction = HandleSaveClick;
            saveButton.ConductWith(this);
            ExtensionControls.Add(saveButton);
        }

        #region Handlers

        private async Task HandleRemoveClick(ActionButtonViewModel button) {
            await dataRepository.DeleteTag(Model.Id);
            await eventAggregator.PublishOnCurrentThreadAsync(new TagEvent(Model, TagEventType.Remove));
            await eventAggregator.PublishOnCurrentThreadAsync(PopupEvent.Close());
        }

        private async Task HandleSaveClick(ActionButtonViewModel button) {
            // Update value of changed
            if (Model.Value != Value) {
                Model.Value = Value;
                await dataRepository.UpdateTag(Model.Id, Model.Value);
                await eventAggregator.PublishOnCurrentThreadAsync(new TagEvent(Model, TagEventType.ValueChange));
            }

            // Update color if changed
            // ReSharper disable once InvertIf
            if (Model.Color != ColorGallery.SelectedColor) {
                Model.Color = ColorGallery.SelectedColor;
                await dataRepository.UpdateTagType(Model.Name, Model.Color.Value);

                // NOTE: This is needed to change color of all tag types, not just this one.
                await eventAggregator.PublishOnCurrentThreadAsync(new TagEvent(Model, TagEventType.ColorChange));
            }

            await eventAggregator.PublishOnCurrentThreadAsync(PopupEvent.Close());
        }

        #endregion

    }

}
