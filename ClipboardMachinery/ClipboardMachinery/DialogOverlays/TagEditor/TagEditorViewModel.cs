using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.DialogOverlay;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Core.DataStorage;

namespace ClipboardMachinery.DialogOverlays.TagEditor {

    public class TagEditorViewModel : Screen, IDialogOverlayControlsProvider {

        #region Properties

        public BindableCollection<ActionButtonViewModel> DialogControls {
            get;
        }

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

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;
        private readonly IDataRepository dataRepository;

        private object val;

        #endregion

        public TagEditorViewModel(
            TagModel tagModel, Func<ActionButtonViewModel> actionButtonFactory,
            IEventAggregator eventAggregator, IDataRepository dataRepository) {

            Model = tagModel;
            Value = Model.Value;
            DialogControls = new BindableCollection<ActionButtonViewModel>();
            this.eventAggregator = eventAggregator;
            this.dataRepository = dataRepository;

            // Create extension control buttons
            ActionButtonViewModel removeButton = actionButtonFactory.Invoke();
            removeButton.ToolTip = "Remove";
            removeButton.Icon = (Geometry)Application.Current.FindResource("IconRemove");
            removeButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("DangerousActionBrush");
            removeButton.ClickAction = OnRemoveClick;
            removeButton.ConductWith(this);
            DialogControls.Add(removeButton);

            ActionButtonViewModel saveButton = actionButtonFactory.Invoke();
            saveButton.ToolTip = "Save";
            saveButton.Icon = (Geometry)Application.Current.FindResource("IconSave");
            saveButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("ElementSelectBrush");
            saveButton.ClickAction = OnSaveClick;
            saveButton.ConductWith(this);
            DialogControls.Add(saveButton);
        }

        #region Handlers

        private async Task OnRemoveClick(ActionButtonViewModel button) {
            await dataRepository.DeleteTag(Model.Id);
            await eventAggregator.PublishOnCurrentThreadAsync(TagEvent.CreateTagRemovedEvent(Model));
            await eventAggregator.PublishOnCurrentThreadAsync(DialogOverlayEvent.Close());
        }

        private async Task OnSaveClick(ActionButtonViewModel button) {
            // Update value of changed
            if (Model.Value != Value) {
                Model.Value = Value;
                await dataRepository.UpdateTag(Model.Id, Value);
                await eventAggregator.PublishOnCurrentThreadAsync(TagEvent.CreateTagValueChangedEvent(Model));
            }

            await eventAggregator.PublishOnCurrentThreadAsync(DialogOverlayEvent.Close());
        }

        #endregion

    }

}
