using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Common.Helpers;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.ColorGallery;
using ClipboardMachinery.Components.DialogOverlay;
using ClipboardMachinery.Components.TagKind;
using ClipboardMachinery.Components.TagType;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Core.DataStorage.Validation;
using ClipboardMachinery.Core.TagKind;

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
            get => description;
            set {
                if (description == value) {
                    return;
                }

                description = value;
                ValidateProperty(value);
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

        public ITagKindManager TagKindManager {
            get;
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

        private readonly IEventAggregator eventAggregator;
        private readonly IDataRepository dataRepository;
        private readonly ActionButtonViewModel saveButton;

        private bool isOpen;
        private bool areControlsVisible;
        private TagTypeModel model;
        private string name;
        private string description;
        private ITagKindSchema selectedTagKind;

        #endregion

        public TagTypeEditorViewModel(
            TagTypeModel tagTypeModel, bool isCreatingNew, ColorGalleryViewModel colorGallery, IEventAggregator eventAggregator, IDataRepository dataRepository,
            ITagKindManager tagKindManager, Func<ActionButtonViewModel> actionButtonFactory) {

            Model = tagTypeModel;
            Name = Model.Name;
            Description = Model.Description;
            TagKindManager = tagKindManager;
            IsSystemOwned = SystemTagTypes.TagTypes.Any(tt => tt.Name == Model.Name);
            DialogControls = new BindableCollection<ActionButtonViewModel>();
            this.eventAggregator = eventAggregator;
            this.dataRepository = dataRepository;
            IsCreatingNew = isCreatingNew;

            // If in edit mode, disable validation specific to creation mode
            if (!IsCreatingNew) {
                DisablePropertyValidation(nameof(Name));
            }

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
            SelectedTagKind = TagKindManager.GetSchemaFor(Model.Kind);
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
                Model = await dataRepository.CreateTagType<TagTypeModel>(Name, Description, SelectedTagKind.Type, ColorGallery.SelectedColor);
                await eventAggregator.PublishOnCurrentThreadAsync(TagEvent.CreateTypeAddedEvent(Model));

            } else {
                // Update description if changed
                if (Model.Description != Description) {
                    Model.Description = Description;
                    await dataRepository.UpdateTagType(Model.Name, Description);
                    await eventAggregator.PublishOnCurrentThreadAsync(TagEvent.CreateTypeDescriptionChangedEvent(Model));
                }

                // Update color if changed
                if (Model.Color != ColorGallery.SelectedColor) {
                    Model.Color = ColorGallery.SelectedColor;
                    await dataRepository.UpdateTagType(Model.Name, Model.Color);
                    await eventAggregator.PublishOnCurrentThreadAsync(TagEvent.CreateTypeColorChangedEvent(Model));
                }
            }

            IsOpen = false;
        }

        #endregion

    }

}
