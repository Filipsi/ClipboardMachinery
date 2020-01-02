﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
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
                CompatibleContentPresenters.Clear();

                if (value != null) {
                    value.PropertyChanged += OnModelPropertyChanged;
                    value.Tags.CollectionChanged += OnModelTagCollectionChanged;
                    OnModelTagCollectionChanged(value.Tags, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value.Tags.ToArray()));

                    CompatibleContentPresenters.AddRange(
                        clipContentResolver.GetCompatiblePresenters(value.Content)
                    );

                    if (string.IsNullOrWhiteSpace(value.Presenter)) {
                        Logger.Error($"Clip with Id={value.Id} does not specify any presenter, content won't be rendered.");
                    } else if (CompatibleContentPresenters.Count == 0) {
                        Logger.Error($"Clip with Id={value.Id} does not have any compatible presenter, content won't be rendered.");
                    } else if (!CompatibleContentPresenters.Any(cp => cp.Id == value.Presenter)) {
                        Logger.Error($"Clip with Id={value.Id} specifies a Presenter={value.Presenter}, but there is no available presenter with required Id, content won't be rendered.");
                    } else {
                        Content = clipContentResolver.GetPresenter(value.Presenter).CreateContentScreen(this);
                    }
                } else {
                    Content = null;
                }

                NotifyOfPropertyChange();
            }
        }

        public ContentScreen Content {
            get => content;
            private set {
                if (content == value) {
                    return;
                }

                DeactivateItemAsync(content, true, CancellationToken.None);
                content = value;
                ActivateItemAsync(value, CancellationToken.None);

                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => Icon);
            }
        }

        public Geometry Icon {
            get {
                string iconKey = Content?.ContentPresenter.Icon;
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

        public ActionButtonViewModel AddTagButton {
            get;
        }

        public BindableCollection<ActionButtonViewModel> SideControls {
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
        private readonly ToggleButtonViewModel favoriteButton;

        private ClipModel model;
        private bool isFocused;
        private ContentScreen content;

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

            // Setup add tag button
            AddTagButton = addTagButton;
            AddTagButton.ToolTip = "Add tag";
            AddTagButton.Icon = (Geometry)Application.Current.FindResource("IconAddTag");
            AddTagButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("ElementSelectBrush");
            AddTagButton.ClickAction = AddTag;

            // Setup remove button
            removeButton.ToolTip = "Remove";
            removeButton.Icon = (Geometry)Application.Current.FindResource("IconRemove");
            removeButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("DangerousActionBrush");
            removeButton.ClickAction = Remove;
            SideControls.Add(removeButton);

            // Setup favorite button
            this.favoriteButton = favoriteButton;
            this.favoriteButton.ToolTip = "Favorite";
            this.favoriteButton.Icon = (Geometry)Application.Current.FindResource("IconStarEmpty");
            this.favoriteButton.ToggledIcon = (Geometry)Application.Current.FindResource("IconStarFull");
            this.favoriteButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("ElementFavoriteBrush");
            this.favoriteButton.DisabledColor = (SolidColorBrush)Application.Current.FindResource("ElementFavoriteDisabledBrush");
            this.favoriteButton.ToggleColor = (SolidColorBrush)Application.Current.FindResource("ElementFavoriteBrush");
            this.favoriteButton.ClickAction = ToggleFavorite;
            SideControls.Add(this.favoriteButton);

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
            if (Model == null) {
                favoriteButton.IsToggled = false;
            } else {
                favoriteButton.IsToggled = Model.Tags.Any(
                    tag => tag.TypeName == SystemTagTypes.CategoryTagType.Name && tag.Value.ToString() == "favorite"
                );
            }
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(ClipModel.Content)) {
                NotifyOfPropertyChange(() => Content);
                NotifyOfPropertyChange(() => Icon);
            }
        }

        #endregion

        #region Actions

        public async Task Remove(ActionButtonViewModel source) {
            await eventAggregator.PublishOnCurrentThreadAsync(new ClipEvent(model, ClipEventType.Remove));
        }

        public async Task Select() {
            clipboardService.SetClipboardContent(Content?.GetClipboardString() ?? Model.Content);
            await eventAggregator.PublishOnCurrentThreadAsync(new ClipEvent(model, ClipEventType.Select));
        }

        public Task ToggleFavorite(ActionButtonViewModel source) {
            TagModel favoriteTag = Model.Tags.FirstOrDefault(
                tag => tag.TypeName == SystemTagTypes.CategoryTagType.Name && tag.Value == "favorite"
            );

            return Task.Run(async () => {
                if (favoriteTag == null) {
                    favoriteTag = await dataRepository.CreateTag<TagModel>(
                        clipId: Model.Id,
                        tagType: SystemTagTypes.CategoryTagType.Name,
                        value: "favorite"
                    );

                    await eventAggregator.PublishOnUIThreadAsync(TagEvent.CreateTagAddedEvent(Model.Id, favoriteTag));
                } else {
                    await dataRepository.DeleteTag(favoriteTag.Id);
                    await eventAggregator.PublishOnUIThreadAsync(TagEvent.CreateTagRemovedEvent(favoriteTag));
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
