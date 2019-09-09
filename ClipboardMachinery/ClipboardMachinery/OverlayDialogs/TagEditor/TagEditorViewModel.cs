using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Castle.Core;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Common.Screen;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.DialogOverlay;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Components.TagKind;
using ClipboardMachinery.Components.TagType;
using ClipboardMachinery.Components.TagTypeLister;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Core.DataStorage.Validation;
using ClipboardMachinery.Core.TagKind;
using Color = System.Windows.Media.Color;
// ReSharper disable SuggestBaseTypeForParameter

namespace ClipboardMachinery.OverlayDialogs.TagEditor {

    public class TagEditorViewModel : ValidationScreenBase, IOverlayDialog {

        #region Properties

        public BindableCollection<ActionButtonViewModel> DialogControls {
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

        public TagModel Model {
            get;
        }

        public IScreen TagTypeLister {
            get;
        }

        private ITagKindManager TagKindManager {
            get;
        }

        [DoNotWire]
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

        public Color Color {
            get => color;
            private set {
                if (color == value) {
                    return;
                }

                color = value;
                NotifyOfPropertyChange();
            }
        }

        [Required]
        [DataRepositoryCheck(nameof(IDataRepository.TagTypeExists))]
        public string Tag {
            get => tag;
            set {
                if (val == value) {
                    return;
                }

                tag = value;

                Task.Run(async () => {
                    TagTypeModel tagType = await dataRepository.FindTagType<TagTypeModel>(Tag);
                    if (tagType != null) {
                        Color = tagType.Color;
                        ITagKindSchema tagKindSchema = TagKindManager.GetSchemaFor(tagType.Kind);
                        TagKind = TagKindManager.TagKinds.FirstOrDefault(vm => vm.Schema == tagKindSchema);
                    } else {
                        TagKind = null;
                    }

                    NotifyOfPropertyChange();
                });
            }
        }

        [Required]
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
        private string tag;
        private string val;
        private TagKindViewModel tagKind;
        private Color color;

        #endregion

        public TagEditorViewModel(
            ClipModel clipModel, Func<ActionButtonViewModel> actionButtonFactory, IEventAggregator eventAggregator,
            IDataRepository dataRepository, ITagKindManager tagKindManager, TagTypeListerViewModel tagTypeLister)
            : this(null, eventAggregator, actionButtonFactory, dataRepository, tagKindManager, tagTypeLister) {

            targetClip = clipModel;
        }

        public TagEditorViewModel(
            TagModel tagModel, IEventAggregator eventAggregator, Func<ActionButtonViewModel> actionButtonFactory,
            IDataRepository dataRepository, ITagKindManager tagKindManager, TagTypeListerViewModel tagTypeLister) {

            if (tagModel == null) {
                IsCreatingNew = true;
                tagModel = new TagModel();
            } else {
                IsCreatingNew = false;
            }

            this.eventAggregator = eventAggregator;
            this.dataRepository = dataRepository;

            DialogControls = new BindableCollection<ActionButtonViewModel>();
            TagKindManager = tagKindManager;
            TagTypeLister = tagTypeLister;
            TagTypeLister.ConductWith(this);
            TagTypeLister.PropertyChanged += OnTagTypeListerPropertyChanged;

            if (!string.IsNullOrWhiteSpace(tagModel.TypeName)) {
                Tag = tagModel.TypeName;
            }

            if (!string.IsNullOrWhiteSpace(tagModel.Value)) {
                Value = tagModel.Value;
                ITagKindSchema tagKindSchema = TagKindManager.GetSchemaFor(tagModel.ValueKind);
                TagKind = TagKindManager.TagKinds.FirstOrDefault(vm => vm.Schema == tagKindSchema);
            }

            Color = tagModel.Color.GetValueOrDefault();

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

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            TagTypeLister.PropertyChanged -= OnTagTypeListerPropertyChanged;
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        private void OnTagTypeListerPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(TagTypeListerViewModel.SelectedItem):
                    TagTypeListerViewModel tagTypeLister = (TagTypeListerViewModel)sender;
                    Tag = tagTypeLister.SelectedItem.Name;
                    break;
            }
        }

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

            if (!TagKindManager.TryParse(TagKind.Schema.Kind, Value, out object newValue)) {
                return;
            }

            string persistentValue = TagKind.Schema.ToPersistentValue(newValue);

            // Create new tag or update values if changed
            if (IsCreatingNew) {
                TagModel newModel = await dataRepository.CreateTag<TagModel>(targetClip.Id, Tag, persistentValue);
                await eventAggregator.PublishOnCurrentThreadAsync(TagEvent.CreateTagAddedEvent(targetClip.Id, newModel));

            } else {
                if (Model.Value != persistentValue) {
                    Model.Value = persistentValue;
                    await dataRepository.UpdateTag(Model.Id, persistentValue);
                    await eventAggregator.PublishOnCurrentThreadAsync(TagEvent.CreateTagValueChangedEvent(Model));
                }
            }

            IsOpen = false;
        }

        #endregion

    }

}
