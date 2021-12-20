using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Common.Screen;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.ColorGallery;
using ClipboardMachinery.Components.DialogOverlay;
using ClipboardMachinery.Components.TagKind;
using ClipboardMachinery.Components.TagType;
using ClipboardMachinery.Core;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Core.DataStorage.Validation;
using ClipboardMachinery.Core.TagKind;
using ClipboardMachinery.Plumbing.Factories;

namespace ClipboardMachinery.OverlayDialogs.TagTypeEditor {

    public class TagTypeEditorViewModel : ValidationScreenBase, IOverlayDialog {

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

        public TagTypeModel Model {
            get => model;
            private set {
                if (model == value) {
                    return;
                }

                model = value;
                ValidateProperty(value);
                NotifyOfPropertyChange();
            }
        }

        [Required]
        [StringLength(20)]
        [DataRepositoryCheck(
            nameof(IDataRepository.TagTypeExists),
            InvertResult = true,
            ErrorMessage = "This name is already used by another tag type.")]
        public string Name {
            get => name;
            set {
                if (name == value) {
                    return;
                }

                name = value;
                ValidateProperty(value);
                NotifyOfPropertyChange();
            }
        }

        [StringLength(75)]
        public string Description {
            get => description ?? string.Empty;
            set {
                if (description == value) {
                    return;
                }

                description = value ?? string.Empty;
                ValidateProperty(value);
                NotifyOfPropertyChange();
            }
        }

        public byte Priority {
            get => priority;
            set {
                if (priority == value) {
                    return;
                }

                priority = value;
                NotifyOfPropertyChange();
            }
        }

        public ITagKindSchema SelectedTagKind {
            get => selectedTagKind;
            set {
                if (selectedTagKind == value) {
                    return;
                }

                selectedTagKind = value;
                NotifyOfPropertyChange();
            }
        }

        public IReadOnlyCollection<TagKindViewModel> TagKinds {
            get => tagKinds;
        }

        public ColorGalleryViewModel ColorGallery {
            get;
        }

        public bool IsSystemOwned {
            get;
        }

        public bool IsCreatingNew {
            get;
        }

        #endregion

        #region Fields

        private static IReadOnlyCollection<TagKindViewModel> tagKinds;

        private readonly IEventAggregator eventAggregator;
        private readonly IDataRepository dataRepository;
        private readonly ITagKindManager tagKindManager;
        private readonly ActionButtonViewModel saveButton;

        private bool isOpen;
        private bool areControlsVisible;
        private TagTypeModel model;
        private string name;
        private string description = string.Empty;
        private byte priority;
        private ITagKindSchema selectedTagKind;

        #endregion

        public TagTypeEditorViewModel(
            ColorGalleryViewModel colorGallery, IEventAggregator eventAggregator, IDataRepository dataRepository,
            ITagKindManager tagKindManager, Func<ActionButtonViewModel> actionButtonFactory, ITagKindFactory tagKindFactory)
            : this(null, colorGallery, eventAggregator, dataRepository, tagKindManager, actionButtonFactory, tagKindFactory) {
        }

        public TagTypeEditorViewModel(
            TagTypeModel tagTypeModel, ColorGalleryViewModel colorGallery, IEventAggregator eventAggregator, IDataRepository dataRepository,
            ITagKindManager tagKindManager, Func<ActionButtonViewModel> actionButtonFactory, ITagKindFactory tagKindFactory) {

            if (tagTypeModel == null) {
                IsCreatingNew = true;
                tagTypeModel = new TagTypeModel {
                    Kind = typeof(string)
                };
            }
            else {
                IsCreatingNew = false;
            }

            this.tagKindManager = tagKindManager;
            this.eventAggregator = eventAggregator;
            this.dataRepository = dataRepository;

            // Create static tag kind options
            if (tagKinds == null) {
                tagKinds = Array.AsReadOnly(
                    tagKindManager.Schemas
                        .Select(tagKindFactory.CreateTagKind)
                        .Reverse()
                        .ToArray()
                );
            }

            Model = tagTypeModel;
            Name = Model.Name;
            Description = Model.Description;
            Priority = Model.Priority;
            IsSystemOwned = SystemTagTypes.TagTypes.Any(tt => tt.Name == Model.Name);
            DialogControls = new BindableCollection<ActionButtonViewModel>();

            // If in edit mode, disable validation specific to creation mode
            if (!IsCreatingNew) {
                DisablePropertyValidation(nameof(Name));
            }

            // Setup color gallery
            ColorGallery = colorGallery;
            ColorGallery.SelectColor(IsCreatingNew ? SystemTagTypes.DefaultColor : Model.Color);

            // Create popup controls
            // Do not create remove button if edited tag belongs to the system since system owned tags can't be removed.
            if (!IsSystemOwned && !IsCreatingNew) {
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
            saveButton.Icon = (Geometry) Application.Current.FindResource("IconSave");
            saveButton.HoverColor = (SolidColorBrush) Application.Current.FindResource("ElementSelectBrush");
            saveButton.ClickAction = OnSaveClick;
            saveButton.ConductWith(this);
            DialogControls.Add(saveButton);
        }

        #region Handlers

        protected override Task OnActivateAsync(CancellationToken cancellationToken) {
            SelectedTagKind = tagKindManager.GetSchemaFor(Model.Kind);
            return base.OnActivateAsync(cancellationToken);
        }

        internal override void OnValidationProcessCompleted() {
            base.OnValidationProcessCompleted();
            saveButton.IsEnabled = IsValid;
        }

        private async Task OnRemoveClick(ActionButtonViewModel button) {
            if (IsCreatingNew) {
                return;
            }

            AreControlsVisible = false;
            await dataRepository.DeleteTagType(Model.Name);
            await eventAggregator.PublishOnCurrentThreadAsync(TagEvent.CreateTypeRemovedEvent(Model));
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

            if (IsCreatingNew) {
                Model = await dataRepository.CreateTagType<TagTypeModel>(Name, Description, SelectedTagKind.Kind, Priority, ColorGallery.SelectedColor);
                await eventAggregator.PublishOnCurrentThreadAsync(TagEvent.CreateTypeAddedEvent(Model));

            } else {
                bool isDirty = false;

                // Update description if changed
                if (Model.Description != Description) {
                    Model.Description = Description;
                    await eventAggregator.PublishOnCurrentThreadAsync(TagEvent.CreateTypeDescriptionChangedEvent(Model));
                    isDirty = true;
                }

                // Update priority if changed
                if (Model.Priority != Priority) {
                    Model.Priority = Priority;
                    await eventAggregator.PublishOnCurrentThreadAsync(TagEvent.CreateTypePriorityChangedEvent(Model));
                    isDirty = true;
                }

                // Update color if changed
                if (Model.Color != ColorGallery.SelectedColor) {
                    Model.Color = ColorGallery.SelectedColor;
                    await eventAggregator.PublishOnCurrentThreadAsync(TagEvent.CreateTypeColorChangedEvent(Model));
                    isDirty = true;
                }

                if (isDirty) {
                    await dataRepository.UpdateTagType(Model.Name, Model.Description, model.Priority, Model.Color);
                }
            }

            IsOpen = false;
        }

        #endregion

    }

}
