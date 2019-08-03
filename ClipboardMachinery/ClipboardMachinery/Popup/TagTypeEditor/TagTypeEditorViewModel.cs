using System;
using System.Linq;
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

        public bool IsSystemOwned {
            get;
        }

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;
        private readonly IDataRepository dataRepository;
        private readonly bool isCreatingNew;

        #endregion

        public TagTypeEditorViewModel(
            TagTypeModel tagTypeModel, bool isCreatingNew, ColorGalleryViewModel colorGallery, IEventAggregator eventAggregator, IDataRepository dataRepository,
            Func<ActionButtonViewModel> actionButtonFactory) {

            Model = tagTypeModel;
            IsSystemOwned = SystemTagTypes.TagTypes.Any(tt => tt.Name == Model.Name);
            PopupControls = new BindableCollection<ActionButtonViewModel>();
            this.eventAggregator = eventAggregator;
            this.dataRepository = dataRepository;
            this.isCreatingNew = isCreatingNew;

            // Setup color gallery
            ColorGallery = colorGallery;
            ColorGallery.SelectColor(isCreatingNew ? SystemTagTypes.DefaultColor : Model.Color);

            // Create popup controls
            // Do not create remove button if edited tag belongs to the system since system owned tags can't be removed.
            if (!IsSystemOwned && !isCreatingNew) {
                ActionButtonViewModel removeButton = actionButtonFactory.Invoke();
                removeButton.ToolTip = "Remove";
                removeButton.Icon = (Geometry) Application.Current.FindResource("IconRemove");
                removeButton.HoverColor = (SolidColorBrush) Application.Current.FindResource("DangerousActionBrush");
                removeButton.ClickAction = OnRemoveClick;
                removeButton.ConductWith(this);
                PopupControls.Add(removeButton);
            }

            ActionButtonViewModel saveButton = actionButtonFactory.Invoke();
            saveButton.ToolTip = "Save";
            saveButton.Icon = (Geometry) Application.Current.FindResource("IconSave");
            saveButton.HoverColor = (SolidColorBrush) Application.Current.FindResource("ElementSelectBrush");
            saveButton.ClickAction = OnSaveClick;
            saveButton.ConductWith(this);
            PopupControls.Add(saveButton);
        }

        #region Handlers

        private Task OnRemoveClick(ActionButtonViewModel button) {
            return Task.CompletedTask;
        }

        private async Task OnSaveClick(ActionButtonViewModel button) {
            if (isCreatingNew) {
                // TODO: Implement this
            } else {
                // Update color if changed
                if (Model.Color != ColorGallery.SelectedColor) {
                    Model.Color = ColorGallery.SelectedColor;
                    await dataRepository.UpdateTagType(Model.Name, Model.Color);
                    await eventAggregator.PublishOnCurrentThreadAsync(new TagEvent(TagEventType.ColorChange, Model.Name, ColorGallery.SelectedColor));
                }
            }


            await eventAggregator.PublishOnCurrentThreadAsync(PopupEvent.Close());
        }

        #endregion

    }

}
