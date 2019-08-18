using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Common.Helpers;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.DialogOverlay;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Components.TagKind;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Core.TagKind;

namespace ClipboardMachinery.OverlayDialogs.TagEditor {

    public class TagEditorViewModel : ValidationScreenBase, IOverlayDialog {

        #region Properties

        public BindableCollection<ActionButtonViewModel> DialogControls {
            get;
        }

        public TagModel Model {
            get;
        }

        public TagKindViewModel TagKind {
            get;
        }

        private ITagKindManager TagKindManager {
            get;
        }

        public bool IsOpen {
            get => isOpen;
            set {
                if (isOpen == value) {
                    return;
                }

                isOpen = value;
                NotifyOfPropertyChange();
            }
        }

        public bool AreControlsVisible {
            get => areControlsVisible;
            set {
                if (areControlsVisible == value) {
                    return;
                }

                areControlsVisible = value;
                NotifyOfPropertyChange();
            }
        }

        [StringLength(20)]
        [CustomValidation(typeof(TagEditorViewModel), nameof(ValidateTagValue))]
        public string Value {
            get => val;
            set {
                if (val == value) {
                    return;
                }

                val = value;
                NotifyOfPropertyChange();
                ValidateProperty(value);
            }
        }

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;
        private readonly IDataRepository dataRepository;
        private readonly ActionButtonViewModel saveButton;

        private bool isOpen;
        private bool areControlsVisible;
        private string val;

        #endregion

        public TagEditorViewModel(
            TagModel tagModel, Func<ActionButtonViewModel> actionButtonFactory,
            IEventAggregator eventAggregator, IDataRepository dataRepository, ITagKindManager tagKindManager) {

            Model = tagModel;
            TagKindManager = tagKindManager;
            DialogControls = new BindableCollection<ActionButtonViewModel>();
            this.eventAggregator = eventAggregator;
            this.dataRepository = dataRepository;

            if (Model.Value != null) {
                Value = Model.Value;
                ITagKindSchema tagKindSchema = tagKindManager.GetSchemaFor(Model.ValueKind);
                TagKind = tagKindManager.TagKinds.FirstOrDefault(vm => vm.Schema == tagKindSchema);
            }

            // Create extension control buttons
            ActionButtonViewModel removeButton = actionButtonFactory.Invoke();
            removeButton.ToolTip = "Remove";
            removeButton.Icon = (Geometry)Application.Current.FindResource("IconRemove");
            removeButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("DangerousActionBrush");
            removeButton.ClickAction = OnRemoveClick;
            removeButton.ConductWith(this);
            DialogControls.Add(removeButton);

            saveButton = actionButtonFactory.Invoke();
            saveButton.ToolTip = "Save";
            saveButton.Icon = (Geometry)Application.Current.FindResource("IconSave");
            saveButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("ElementSelectBrush");
            saveButton.ClickAction = OnSaveClick;
            saveButton.ConductWith(this);
            DialogControls.Add(saveButton);
        }

        #region Logic

        public static ValidationResult ValidateTagValue(string newTagValue, ValidationContext context) {
            TagEditorViewModel editor = (TagEditorViewModel)context.ObjectInstance;
            return editor.TagKindManager.IsValid(editor.Model.ValueKind, newTagValue)
                ? ValidationResult.Success
                : new ValidationResult($"Tag value is not valid, expected a {editor.TagKind.Schema.Name.ToLowerInvariant()} value.", new[] { nameof(Value) });
        }

        #endregion

        #region Handlers

        internal override void OnValidationProcessCompleted() {
            saveButton.IsEnabled = IsValid;
            base.OnValidationProcessCompleted();
        }

        private async Task OnRemoveClick(ActionButtonViewModel button) {
            await dataRepository.DeleteTag(Model.Id);
            await eventAggregator.PublishOnCurrentThreadAsync(TagEvent.CreateTagRemovedEvent(Model));
            IsOpen = false;
        }

        private async Task OnSaveClick(ActionButtonViewModel button) {
            // Prevent saving if validation is still running
            if (ValidationProcess?.IsNotCompleted == true) {
                return;
            }

            // Validate all properties
            await Validate();

            // Prevent saving changes if there are data errors
            if (!IsValid) {
                return;
            }

            // Update value of changed
            if (TagKindManager.TryParse(TagKind.Schema.Kind, Value, out object newValue)) {
                string displayValue = TagKind.Schema.ToDisplayValue(newValue);

                if (Model.Value != displayValue) {
                    Model.Value = displayValue;
                    await dataRepository.UpdateTag(Model.Id, displayValue);
                    await eventAggregator.PublishOnCurrentThreadAsync(TagEvent.CreateTagValueChangedEvent(Model));
                }
            }

            IsOpen = false;
        }

        #endregion

    }

}
