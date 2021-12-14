using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Castle.Core;
using Castle.Core.Logging;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.Buttons.ToggleButton;
using ClipboardMachinery.Components.ContentPresenter;
using ClipboardMachinery.Components.DialogOverlay;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Components.TagLister;
using ClipboardMachinery.Core;
using ClipboardMachinery.Core.DataStorage;
using ClipboardMachinery.Core.Services.Clipboard;
using static ClipboardMachinery.Common.Events.ClipEvent;

namespace ClipboardMachinery.Components.Clip {

    public class ClipViewModel : Conductor<object>.Collection.AllActive {

        #region Properties

        public ClipModel Model {
            get => model;
            private set {
                if (model == value) {
                    return;
                }

                if (model != null) {
                    model.PropertyChanged -= OnModelPropertyChanged;
                    model.Tags.CollectionChanged -= OnModelTagCollectionChanged;
                    OnModelTagCollectionChanged(model.Tags, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }

                model = value;
                Tags.Clip = value;
                ClipContent = null;

                if (value != null) {
                    value.PropertyChanged += OnModelPropertyChanged;
                    value.Tags.CollectionChanged += OnModelTagCollectionChanged;
                    OnModelTagCollectionChanged(value.Tags, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value.Tags.ToArray()));
                }

                UpdateClipContentPresenter(value);
                NotifyOfPropertyChange();
            }
        }

        [DoNotWire]
        public IContentPresenter CurrentPresenter {
            get => currentPresenter;
            set {
                if (currentPresenter == value) {
                    return;
                }

                if (value != null && !CompatibleContentPresenters.Contains(value)) {
                    return;
                }

                currentPresenter = value;
                NotifyOfPropertyChange();

                if (Model?.Id != null && !string.IsNullOrWhiteSpace(currentPresenter?.Id) && Model.Presenter != currentPresenter.Id) {
                    // TODO: Do this asynchronously and block the control until finished
                    dataRepository.UpdateClip(Model.Id, currentPresenter.Id);
                }

                ResetSideControls();
                ClipContent = currentPresenter?.CreateContentScreen(this);
            }
        }

        public ContentScreen ClipContent {
            get => clipContent;
            private set {
                if (clipContent == value) {
                    return;
                }

                DeactivateItemAsync(clipContent, true, CancellationToken.None);
                clipContent = value;
                ActivateItemAsync(value, CancellationToken.None);

                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => Icon);
            }
        }

        public Geometry Icon {
            get {
                string iconKey = ClipContent?.ContentPresenter.Icon;
                return !string.IsNullOrWhiteSpace(iconKey)
                    ? (Geometry)Application.Current.TryFindResource(iconKey)
                    : null;
            }
        }

        public bool IsFocused {
            get => isFocused;
            set {
                if (isFocused == value) {
                    return;
                }

                isFocused = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => SelectionColor);
            }
        }

        public ILogger Logger {
            get;
            set;
        }

        public SolidColorBrush SelectionColor {
            get {
                return FocusColors[IsFocused ? 1 : 0];
            }
        }

        public TagListerViewModel Tags {
            get;
        }

        public BindableCollection<ActionButtonViewModel> SideControls {
            get;
        }

        public ActionButtonViewModel AddTagButton {
            get;
        }

        public BindableCollection<IContentPresenter> CompatibleContentPresenters {
            get;
        }

        #endregion

        #region Fields

        private readonly IReadOnlyList<SolidColorBrush> FocusColors = Array.AsReadOnly(
            new [] {
                Application.Current.FindResource("PanelControlBrush") as SolidColorBrush,
                Application.Current.FindResource("ElementSelectBrush") as SolidColorBrush
            }
        );

        private readonly IEventAggregator eventAggregator;
        private readonly IDataRepository dataRepository;
        private readonly IDialogOverlayManager dialogOverlayManager;
        private readonly IContentDisplayResolver clipContentResolver;
        private readonly IClipboardService clipboardService;
        private readonly ActionButtonViewModel removeButton;
        private readonly ToggleButtonViewModel favoriteButton;

        private ClipModel model;
        private bool isFocused;
        private ContentScreen clipContent;
        private IContentPresenter currentPresenter;

        #endregion

        public ClipViewModel(
            ClipModel model, ActionButtonViewModel removeButton, ActionButtonViewModel addTagButton, ToggleButtonViewModel favoriteButton,
            TagListerViewModel tagLister, IEventAggregator eventAggregator, IDataRepository dataRepository,
            IDialogOverlayManager dialogOverlayManager, IContentDisplayResolver clipContentResolver, IClipboardService clipboardService) : base(false) {

            this.eventAggregator = eventAggregator;
            this.dataRepository = dataRepository;
            this.dialogOverlayManager = dialogOverlayManager;
            this.clipContentResolver = clipContentResolver;
            this.clipboardService = clipboardService;

            Logger = NullLogger.Instance;
            CompatibleContentPresenters = new BindableCollection<IContentPresenter>();
            SideControls = new BindableCollection<ActionButtonViewModel>();
            SideControls.CollectionChanged += OnSideControlsCollectionChanged;

            // Setup remove button
            this.removeButton = removeButton;
            this.removeButton.ToolTip = "Remove";
            this.removeButton.Icon = (Geometry)Application.Current.FindResource("IconRemove");
            this.removeButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("DangerousActionBrush");
            this.removeButton.ClickAction = Remove;

            // Setup favorite button
            this.favoriteButton = favoriteButton;
            this.favoriteButton.ToolTip = "Favorite";
            this.favoriteButton.Icon = (Geometry)Application.Current.FindResource("IconStarEmpty");
            this.favoriteButton.ToggledIcon = (Geometry)Application.Current.FindResource("IconStarFull");
            this.favoriteButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("ElementFavoriteBrush");
            this.favoriteButton.DisabledColor = (SolidColorBrush)Application.Current.FindResource("ElementFavoriteDisabledBrush");
            this.favoriteButton.ToggleColor = (SolidColorBrush)Application.Current.FindResource("ElementFavoriteBrush");
            this.favoriteButton.ClickAction = ToggleFavorite;

            // Setup add tag button
            AddTagButton = addTagButton;
            AddTagButton.ToolTip = "Add tag";
            AddTagButton.Icon = (Geometry)Application.Current.FindResource("IconAddTag");
            AddTagButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("ElementSelectBrush");
            AddTagButton.ClickAction = AddTag;

            ResetSideControls();

            // Set backing clip model
            Tags = tagLister;
            Model = model;
        }

        #region Lifecycle

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            if (close) {
                SideControls.Clear();
                SideControls.CollectionChanged -= OnSideControlsCollectionChanged;
            }

            await base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion

        #region Handlers

        private async void OnSideControlsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
            case NotifyCollectionChangedAction.Add:
                    foreach (ActionButtonViewModel newButton in e.NewItems.OfType<ActionButtonViewModel>()) {
                        await ActivateItemAsync(newButton, CancellationToken.None);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (ActionButtonViewModel oldButton in e.OldItems.OfType<ActionButtonViewModel>()) {
                        await DeactivateItemAsync(oldButton, true, CancellationToken.None);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (ActionButtonViewModel oldButton in Items.OfType<ActionButtonViewModel>().Where(btn => !SideControls.Contains(btn)).ToArray()) {
                        await DeactivateItemAsync(oldButton, true, CancellationToken.None);
                    }

                    foreach (ActionButtonViewModel newButton in SideControls) {
                        if (!newButton.IsActive || !Items.OfType<ActionButtonViewModel>().Contains(newButton)) {
                            await ActivateItemAsync(newButton, CancellationToken.None);
                        }
                    }
                    break;
            }
        }

        private void OnModelTagCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            favoriteButton.IsToggled = Model != null && Model.Tags.Any(tag => tag.TypeName == SystemTagTypes.CategoryTagType.Name);
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName != nameof(ClipModel.Content)) {
                return;
            }

            NotifyOfPropertyChange(() => ClipContent);
            NotifyOfPropertyChange(() => Icon);
        }

        #endregion

        #region Logic

        private void UpdateClipContentPresenter(ClipModel model) {
            CompatibleContentPresenters.Clear();

            if (model == null) {
                return;
            }

            CompatibleContentPresenters.AddRange(clipContentResolver.GetCompatiblePresenters(model.Content));

            if (string.IsNullOrWhiteSpace(model.Presenter)) {
                CurrentPresenter = clipContentResolver.GetDefaultPresenter(model.Content);
                Logger.Error($"Clip with Id={model.Id} does not specify any presenter, this might be old entry format! Using default presenter based on content Id={CurrentPresenter.Id}.");
                return;
            }

            if (CompatibleContentPresenters.Count == 0) {
                Logger.Error($"Clip with Id={model.Id} does not have any compatible presenter, content won't be rendered.");
                CurrentPresenter = null;
                return;
            }

            if (!clipContentResolver.TryGetPresenter(model.Presenter, out IContentPresenter contentPresenter)) {
                Logger.Error($"Clip with Id={model.Id} specifies a Presenter={Model.Presenter}, but there is no available presenter with required Id, content won't be rendered.");
                CurrentPresenter = null;
                return;
            }

            CurrentPresenter = contentPresenter;
        }

        private void ResetSideControls() {
            SideControls.IsNotifying = false;
            SideControls.Clear();
            SideControls.Add(removeButton);
            SideControls.Add(favoriteButton);
            SideControls.IsNotifying = true;
            SideControls.Refresh();
        }

        #endregion

        #region Actions

        public async Task Remove(ActionButtonViewModel source) {
            await eventAggregator.PublishOnCurrentThreadAsync(new ClipEvent(model, ClipEventType.Remove));
        }

        public async Task Select() {
            clipboardService.SetClipboardContent(ClipContent?.GetClipboardString() ?? Model.Content);
            await eventAggregator.PublishOnCurrentThreadAsync(new ClipEvent(model, ClipEventType.Select));
        }

        public Task ToggleFavorite(ActionButtonViewModel source) {
            TagModel[] categoryTags = Model.Tags.Where(tag => tag.TypeName == SystemTagTypes.CategoryTagType.Name).ToArray();

            return Task.Run(async () => {
                if (categoryTags.Length == 0) {
                    TagModel favoriteTag = await dataRepository.CreateTag<TagModel>(
                        clipId: Model.Id,
                        tagType: SystemTagTypes.CategoryTagType.Name,
                        value: "favorite"
                    );

                    await eventAggregator.PublishOnUIThreadAsync(TagEvent.CreateTagAddedEvent(Model.Id, favoriteTag));
                } else {
                    foreach (TagModel categoryTag in categoryTags) {
                        await dataRepository.DeleteTag(categoryTag.Id);
                        await eventAggregator.PublishOnUIThreadAsync(TagEvent.CreateTagRemovedEvent(categoryTag));
                    }
                }
            });
        }

        private async Task AddTag(ActionButtonViewModel button) {
            await dialogOverlayManager.OpenDialog(
                () => dialogOverlayManager.Factory.CreateTagEditor(Model),
                tagEditor => dialogOverlayManager.Factory.Release(tagEditor)
            );
        }

        public void Focus() {
            IsFocused = true;
        }

        public void Unfocus() {
            IsFocused = false;
        }

        #endregion

    }

}
