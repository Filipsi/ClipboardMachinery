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
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.DialogOverlay;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Components.TagKind;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Core.DataStorage.Schema;
using ClipboardMachinery.Core.DataStorage.Validation;
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

        private ITagKindManager TagKindManager {
            get;
        }

        public TagKindViewModel TagKind {
            get => tagKind;
            private set {
                if (tagKind == value) {
                    return;
                }

                tagKind = value;
                NotifyOfPropertyChange();
            }
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

        [Required]
        [DataRepositoryCheck(nameof(IDataRepository.TagTypeExists))]
        public string TypeName {
            get => typeName;
            set {
                if (val == value) {
                    return;
                }

                typeName = value;

                Task.Run(async () => {
                    TagType tagType = await dataRepository.FindTagType<TagType>(TypeName);
                    if (tagType != null) {
                        ITagKindSchema tagKindSchema = TagKindManager.GetSchemaFor(tagType.Kind);
                        TagKind = TagKindManager.TagKinds.FirstOrDefault(vm => vm.Schema == tagKindSchema);
                    }

                    NotifyOfPropertyChange();
                    await ValidateProperty(value);
                    await ValidateProperty(Value, nameof(Value));
                });
            }
        }

        [Required]
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

        public bool IsCreatingNew {
            get;
        }

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;
        private readonly IDataRepository dataRepository;
        private readonly ActionButtonViewModel saveButton;
        private readonly ClipModel targetClip;

        private bool isOpen;
        private bool areControlsVisible;
        private string typeName;
        private string val;
        private TagKindViewModel tagKind;

        #endregion

        public TagEditorViewModel(
            ClipModel clipModel, Func<ActionButtonViewModel> actionButtonFactory, IEventAggregator eventAggregator, IDataRepository dataRepository, ITagKindManager tagKindManager)
            : this(new TagModel(), actionButtonFactory, eventAggregator, dataRepository, tagKindManager) {

            IsCreatingNew = true;
            targetClip = clipModel;
        }

        public TagEditorViewModel(
            TagModel tagModel, Func<ActionButtonViewModel> actionButtonFactory,
            IEventAggregator eventAggregator, IDataRepository dataRepository, ITagKindManager tagKindManager) {

            this.eventAggregator = eventAggregator;
            this.dataRepository = dataRepository;

            TagKindManager = tagKindManager;
            DialogControls = new BindableCollection<ActionButtonViewModel>();

            if (!string.IsNullOrWhiteSpace(tagModel.TypeName)) {
                TypeName = tagModel.TypeName;
            }

            if (!string.IsNullOrWhiteSpace(tagModel.Value)) {
                Value = tagModel.Value;
            }

            // Create extension control buttons
            if (!IsCreatingNew) {
                ActionButtonViewModel removeButton = actionButtonFactory.Invoke();
                removeButton.ToolTip = "Remove";
                removeButton.Icon = (Geometry) Application.Current.FindResource("IconRemove");
                removeButton.HoverColor = (SolidColorBrush) Application.Current.FindResource("DangerousActionBrush");
                removeButton.ClickAction = OnRemoveClick;
                removeButton.ConductWith(this);
                DialogControls.Add(removeButton);
            }

            saveButton = actionButtonFactory.Invoke();
            saveButton.ToolTip = "Save";
            saveButton.Icon = (Geometry)Application.Current.FindResource("IconSave");
            saveButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("ElementSelectBrush");
            saveButton.ClickAction = OnSaveClick;
            saveButton.ConductWith(this);
            DialogControls.Add(saveButton);

            Model = tagModel;
        }

        #region Logic

        public static ValidationResult ValidateTagValue(string newTagValue, ValidationContext context) {
            TagEditorViewModel editor = (TagEditorViewModel)context.ObjectInstance;

            if (editor.TagKind == null) {
                return ValidationResult.Success;
            }

            return editor.TagKindManager.IsValid(editor.TagKind.Schema.Kind, newTagValue)
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

            // Create new tag or update values if changed
            if (IsCreatingNew) {
                TagModel newModel = await dataRepository.CreateTag<TagModel>(targetClip.Id, TypeName, Value);
                await eventAggregator.PublishOnCurrentThreadAsync(TagEvent.CreateTagAddedEvent(targetClip.Id, newModel));

            } else if (TagKindManager.TryParse(TagKind.Schema.Kind, Value, out object newValue)) {
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
