using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.ColorGallery;
using ClipboardMachinery.Components.TagType;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Popup.Manager;
using static ClipboardMachinery.Common.Events.TagEvent;

namespace ClipboardMachinery.Popup.TagTypeEditor {

    public class TagTypeEditorViewModel : Screen, IPopupExtendedControls {

        #region Properties

        public BindableCollection<ActionButtonViewModel> PopupControls {
            get;
        }

        public TagTypeModel Model {
            get;
        }

        public ColorGalleryViewModel ColorGallery {
            get;
        }

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;
        private readonly IDataRepository dataRepository;

        #endregion

        public TagTypeEditorViewModel(
            TagTypeModel tagTypeModel, ColorGalleryViewModel colorGallery, IEventAggregator eventAggregator, IDataRepository dataRepository,
            Func<ActionButtonViewModel> actionButtonFactory) {

            Model = tagTypeModel;
            PopupControls = new BindableCollection<ActionButtonViewModel>();
            this.eventAggregator = eventAggregator;
            this.dataRepository = dataRepository;

            // Setup color gallery
            ColorGallery = colorGallery;
            ColorGallery.SelectColor(Model.Color);

            // Create popup controls
            ActionButtonViewModel removeButton = actionButtonFactory.Invoke();
            removeButton.ToolTip = "Remove";
            removeButton.Icon = (Geometry)Application.Current.FindResource("IconRemove");
            removeButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("DangerousActionBrush");
            removeButton.ClickAction = OnRemoveClick;
            removeButton.ConductWith(this);
            PopupControls.Add(removeButton);

            ActionButtonViewModel saveButton = actionButtonFactory.Invoke();
            saveButton.ToolTip = "Save";
            saveButton.Icon = (Geometry)Application.Current.FindResource("IconSave");
            saveButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("ElementSelectBrush");
            saveButton.ClickAction = OnSaveClick;
            saveButton.ConductWith(this);
            PopupControls.Add(saveButton);
        }

        private Task OnRemoveClick(ActionButtonViewModel button) {
            return Task.CompletedTask;
        }

        private async Task OnSaveClick(ActionButtonViewModel button) {
            // Update color if changed
            if (Model.Color != ColorGallery.SelectedColor) {
                Model.Color = ColorGallery.SelectedColor;
                await dataRepository.UpdateTagType(Model.Name, Model.Color);
                await eventAggregator.PublishOnCurrentThreadAsync(new TagEvent(TagEventType.ColorChange, Model.Name, ColorGallery.SelectedColor));
            }

            await eventAggregator.PublishOnCurrentThreadAsync(PopupEvent.Close());
        }

    }

}
