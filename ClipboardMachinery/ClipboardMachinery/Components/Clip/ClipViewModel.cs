﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.Buttons.ToggleButton;
using ClipboardMachinery.Components.Tag;
using Microsoft.Win32;
using static ClipboardMachinery.Common.Events.ClipEvent;
using static ClipboardMachinery.Common.Events.TagEvent;
using Image = System.Windows.Controls.Image;

namespace ClipboardMachinery.Components.Clip {

    public class ClipViewModel : Conductor<TagViewModel>.Collection.AllActive, IHandle<TagEvent> {

        #region Properties

        public ClipModel Model {
            get => model;
            set {
                if (model == value) {
                    return;
                }

                if (model != null) {
                    model.PropertyChanged -= OnModelPropertyChanged;
                    model.Tags.CollectionChanged -= OnModelTagCollectionChanged;
                    OnModelTagCollectionChanged(model.Tags, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }

                model = value;

                if (value != null) {
                    value.PropertyChanged += OnModelPropertyChanged;
                    value.Tags.CollectionChanged += OnModelTagCollectionChanged;
                    OnModelTagCollectionChanged(value.Tags, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value.Tags.ToArray()));
                }

                HandleModelChange();
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => Content);
                NotifyOfPropertyChange(() => Type);
                NotifyOfPropertyChange(() => Icon);
                NotifyOfPropertyChange(() => SelectionColor);
            }
        }

        public object Content
            => Application.Current.Dispatcher.Invoke(() => WrapContentForType(Type, Model?.Content));

        public EntryType Type {
            get {
                if (Model == null) {
                    return EntryType.Empty;
                }

                if (imageDataPattern.IsMatch(Model.Content)) {
                    return EntryType.Image;
                }

                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (Uri.IsWellFormedUriString(Model.Content, UriKind.Absolute)) {
                    return EntryType.Link;
                }

                return EntryType.Text;
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

        public Geometry Icon
            => (Geometry)Application.Current.TryFindResource(iconMap[Type]);

        public SolidColorBrush SelectionColor
            => Application.Current.FindResource(IsFocused ? "ElementSelectBrush" : "PanelControlBrush") as SolidColorBrush;

        public BindableCollection<ActionButtonViewModel> Controls {
            get;
        }

        #endregion

        #region Fields

        private static readonly Regex imageDataPattern = new Regex(
            pattern: @"^data\:(?<visiblityChangeType>image\/(png|tiff|jpg|gif));base64,(?<data>[A-Z0-9\+\/\=]+)$",
            options: RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
        );

        private static readonly Dictionary<EntryType, string> iconMap = new Dictionary<EntryType, string> {
            { EntryType.Empty, string.Empty   },
            { EntryType.Link,  "IconLink"     },
            { EntryType.Image, "IconPicture"  },
            { EntryType.Text,  "IconTextFile" }
        };

        private readonly IEventAggregator eventAggregator;
        private readonly Func<ActionButtonViewModel> actionButtonFactory;
        private readonly Func<TagViewModel> tagVmFactory;
        private readonly ToggleButtonViewModel favoriteButton;

        private ClipModel model;
        private bool isFocused;

        #endregion

        #region Enumerations

        public enum EntryType {
            Empty,
            Text,
            Link,
            Image
        }

        #endregion

        public ClipViewModel(
            ClipModel model, IEventAggregator eventAggregator,
            Func<TagViewModel> tagVmFactory, Func<ActionButtonViewModel> actionButtonFactory, Func<ToggleButtonViewModel> toggleButtonFactory) {

            this.tagVmFactory = tagVmFactory;
            this.eventAggregator = eventAggregator;
            this.actionButtonFactory = actionButtonFactory;

            Controls = new BindableCollection<ActionButtonViewModel>();

            // Create controls
            ActionButtonViewModel removeButton = actionButtonFactory.Invoke();
            removeButton.ToolTip = "Remove";
            removeButton.Icon = (Geometry)Application.Current.FindResource("IconRemove");
            removeButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("DangerousActionBrush");
            removeButton.ClickAction = Remove;
            removeButton.ConductWith(this);
            Controls.Add(removeButton);

            favoriteButton = toggleButtonFactory.Invoke();
            favoriteButton.ToolTip = "Favorite";
            favoriteButton.Icon = (Geometry)Application.Current.FindResource("IconStarEmpty");
            favoriteButton.ToggledIcon = (Geometry)Application.Current.FindResource("IconStarFull");
            favoriteButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("ElementFavoriteBrush");
            favoriteButton.DisabledColor = (SolidColorBrush)Application.Current.FindResource("ElementFavoriteDisabledBrush");
            favoriteButton.ToggleColor = (SolidColorBrush)Application.Current.FindResource("ElementFavoriteBrush");
            favoriteButton.ClickAction = ToggleFavorite;
            favoriteButton.ConductWith(this);
            Controls.Add(favoriteButton);

            Model = model;
        }

        #region Handlers

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            if (close) {
                Model = null;
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        private async void OnModelTagCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch(e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach(TagModel tagModel in e.NewItems) {
                        TagViewModel vm = tagVmFactory.Invoke();
                        vm.Model = tagModel;
                        await ActivateItemAsync(vm, CancellationToken.None);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (TagModel tagModel in e.OldItems) {
                        foreach (TagViewModel vm in Items.Where(vm => vm.Model == tagModel).ToArray()) {
                            await DeactivateItemAsync(vm, true, CancellationToken.None);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    if (Items?.Count > 0) {
                        foreach (TagViewModel vm in Items.ToArray()) {
                            await DeactivateItemAsync(vm, true, CancellationToken.None);
                        }
                    }
                    break;
            }

            UpdateControlsState();
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            // ReSharper disable once InvertIf
            if (e.PropertyName == nameof(ClipModel.Content)) {
                NotifyOfPropertyChange(() => Content);
                NotifyOfPropertyChange(() => Type);
                NotifyOfPropertyChange(() => Icon);
                // ReSharper disable once RedundantJumpStatement
                return;
            }
        }

        public async Task HandleAsync(TagEvent message, CancellationToken cancellationToken) {
            switch(message.EventType) {
                case TagEventType.Remove:
                    foreach(TagViewModel tagToRemove in Items.Where(vm => vm.Model.Id == message.Source.Id).ToArray()) {
                        await DeactivateItemAsync(tagToRemove, true, cancellationToken);
                    }
                    break;

                case TagEventType.ColorChange:
                    foreach (TagViewModel vm in Items.Where(vm => vm.Model.Name == message.Source.Name)) {
                        vm.Model.Color = message.Source.Color;
                    }
                    break;

                case TagEventType.ValueChange:
                    if (Items.Any(tag => tag.Model == message.Source)) {
                        UpdateControlsState();
                    }
                    break;
            }
        }

        private void HandleModelChange() {
            if (Type == EntryType.Image) {
                ActionButtonViewModel exportImageButton = actionButtonFactory.Invoke();
                exportImageButton.ToolTip = "Export as *.png";
                exportImageButton.Icon = (Geometry)Application.Current.FindResource("IconExport");
                exportImageButton.ClickAction = ExportImage;
                exportImageButton.ConductWith(this);
                Controls.Add(exportImageButton);
            } else {
                ActionButtonViewModel exportImageButton = Controls.FirstOrDefault(control => control.ToolTip == "Export as *.png");
                if (exportImageButton != null) {
                    Controls.Remove(exportImageButton);
                }
            }
        }

        #endregion

        #region Logic

        private static object WrapContentForType(EntryType type, string content) {
            switch (type) {
                case EntryType.Empty:
                    return string.Empty;

                case EntryType.Link:
                    Hyperlink link = new Hyperlink(new Run(content)) {
                        NavigateUri = new Uri(content)
                    };

                    // FIXME: Unhook this
                    link.RequestNavigate += (sender, args) => Process.Start(new ProcessStartInfo(args.Uri.AbsoluteUri));

                    TextBlock block = new TextBlock() {
                        TextWrapping = TextWrapping.Wrap
                    };
                    block.Inlines.Add(link);
                    return block;

                case EntryType.Image:
                    string imageData = imageDataPattern.Match(content).Groups[2].Value;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
                    bitmapImage.StreamSource = new MemoryStream(Convert.FromBase64String(imageData));
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    Image image = new Image {
                        Source = bitmapImage,
                        Height = bitmapImage.Height,
                        MaxHeight = 128D,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        UseLayoutRounding = true
                    };

                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                    return image;

                default:
                    return new TextBlock {
                        Background = Brushes.Transparent,
                        TextWrapping = TextWrapping.Wrap,
                        Text = content
                    };
            }
        }

        private void UpdateControlsState() {
            if (Model == null) {
                favoriteButton.IsToggled = false;
            } else {
                favoriteButton.IsToggled = Model.Tags.Any(
                    tag => tag.Name == "category" && tag.Value.ToString() == "favorite"
                );
            }
        }

        #endregion

        #region Actions

        public async Task Remove(ActionButtonViewModel source) {
            await eventAggregator.PublishOnCurrentThreadAsync(new ClipEvent(model, ClipEventType.Remove));
        }

        public void Select() {
            eventAggregator.PublishOnCurrentThreadAsync(new ClipEvent(model, ClipEventType.Select));
        }

        public async Task ToggleFavorite(ActionButtonViewModel source) {
            await eventAggregator.PublishOnCurrentThreadAsync(new ClipEvent(model, ClipEventType.ToggleFavorite));
        }

        private Task ExportImage(ActionButtonViewModel source) {
            if (Type != EntryType.Image) {
                return Task.CompletedTask;
            }

            if (!(((Image) Content).Source is BitmapImage bitmapImage)) {
                return Task.CompletedTask;
            }

            SaveFileDialog dialog = new SaveFileDialog {
                FileName = $"{Model.Id}-{Model.Created.ToFileTimeUtc()}",
                Filter = "Portable Network Graphics (*.png)|*.png|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true) {
                return Task.CompletedTask;
            }

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (FileStream fileStream = new FileStream(dialog.FileName, FileMode.Create)) {
                encoder.Save(fileStream);
            }

            return Task.CompletedTask;
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
