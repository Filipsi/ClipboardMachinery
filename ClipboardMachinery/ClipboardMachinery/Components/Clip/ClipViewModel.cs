using System;
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
using static ClipboardMachinery.Common.Events.ClipEvent;
using static ClipboardMachinery.Common.Events.TagEvent;
using Image = System.Windows.Controls.Image;
using Screen = Caliburn.Micro.Screen;

namespace ClipboardMachinery.Components.Clip {

    public class ClipViewModel : Screen, IHandle<TagEvent> {

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
                    OnModelTagCollectionChanged(value.Tags, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value.Tags));
                }

                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => Content);
                NotifyOfPropertyChange(() => Type);
                NotifyOfPropertyChange(() => Icon);
                NotifyOfPropertyChange(() => SelectionColor);
            }
        }

        public object Content
            => WrapContentForType(Type, Model.Content);

        public EntryType Type {
            get {
                if (ImageDataPattern.IsMatch(Model.Content)) {
                    return EntryType.Image;
                }

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
            => Application.Current.FindResource(IconMap[Type]) as Geometry;

        public SolidColorBrush SelectionColor
            => Application.Current.FindResource(IsFocused ? "ElementSelectBrush" : "PanelControlBrush") as SolidColorBrush;

        public BindableCollection<TagViewModel> Tags {
            get;
        }

        public BindableCollection<ActionButtonViewModel> Controls {
            get;
        }

        #endregion

        #region Fields

        private static readonly Regex ImageDataPattern = new Regex(
            pattern: @"^data\:(?<visiblityChangeType>image\/(png|tiff|jpg|gif));base64,(?<data>[A-Z0-9\+\/\=]+)$",
            options: RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
        );

        private static readonly Dictionary<EntryType, string> IconMap = new Dictionary<EntryType, string> {
            { EntryType.Link,  "IconLink"     },
            { EntryType.Image, "IconPicture"  },
            { EntryType.Text,  "IconTextFile" }
        };

        private readonly IEventAggregator eventAggregator;
        private readonly Func<TagViewModel> tagVmFactory;
        private readonly ToggleButtonViewModel favoriteButton;

        private ClipModel model;
        private bool isFocused;

        #endregion

        #region Enumerations

        public enum EntryType {
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

            Tags = new BindableCollection<TagViewModel>();
            Controls = new BindableCollection<ActionButtonViewModel>();

            // Create controls
            ActionButtonViewModel removeButton = actionButtonFactory.Invoke();
            removeButton.ToolTip = "Remove";
            removeButton.Icon = (Geometry)Application.Current.FindResource("IconRemove");
            removeButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("DangerousActionBrush");
            removeButton.ClickAction = Remove;
            Controls.Add(removeButton);

            favoriteButton = toggleButtonFactory.Invoke();
            favoriteButton.ToolTip = "Favorite";
            favoriteButton.Icon = (Geometry)Application.Current.FindResource("IconStarEmpty");
            favoriteButton.ToggledIcon = (Geometry)Application.Current.FindResource("IconStarFull");
            favoriteButton.ToggleColor = (SolidColorBrush)Application.Current.FindResource("ElementFavoriteBrush");
            favoriteButton.ClickAction = ToggleFavorite;
            Controls.Add(favoriteButton);

            Model = model;
        }

        #region Handlers

        private void OnModelTagCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch(e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach(TagModel model in e.NewItems) {
                        TagViewModel vm = tagVmFactory.Invoke();
                        vm.Model = model;
                        Tags.Add(vm);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (TagModel model in e.OldItems) {
                        foreach (TagViewModel vm in Tags.Where(vm => vm.Model == model).ToArray()) {
                            Tags.Remove(vm);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    Tags.Clear();
                    break;
            }

            UpdateControlsState();
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(ClipModel.Content)) {
                NotifyOfPropertyChange(() => Content);
                NotifyOfPropertyChange(() => Type);
                NotifyOfPropertyChange(() => Icon);
                return;
            }
        }

        public Task HandleAsync(TagEvent message, CancellationToken cancellationToken) {
            switch(message.EventType) {
                case TagEventType.Remove:
                    Tags.RemoveRange(Tags.Where(vm => vm.Model.Id == message.Source.Id).ToArray());
                    break;

                case TagEventType.ColorChange:
                    foreach (TagViewModel vm in Tags.Where(vm => vm.Model.Name == message.Source.Name)) {
                        vm.Model.Color = message.Source.Color;
                    }
                    break;

                case TagEventType.ValueChange:
                    if (Tags.Any(tag => tag.Model == message.Source)) {
                        UpdateControlsState();
                    }
                    break;
            }
            return Task.CompletedTask;
        }

        #endregion

        #region Logic

        private static object WrapContentForType(EntryType type, string content) {
            switch (type) {
                case EntryType.Link:
                    Hyperlink link = new Hyperlink(new Run(content)) {
                        NavigateUri = new Uri(content)
                    };
                    link.RequestNavigate += (sender, args) => Process.Start(new ProcessStartInfo(args.Uri.AbsoluteUri));

                    TextBlock block = new TextBlock() {
                        TextWrapping = TextWrapping.Wrap
                    };
                    block.Inlines.Add(link);
                    return block;

                case EntryType.Image:
                    string imageData = ImageDataPattern.Match(content).Groups[2].Value;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new MemoryStream(Convert.FromBase64String(imageData));
                    bitmapImage.EndInit();

                    return new Image {
                        Source = bitmapImage,
                        Height = bitmapImage.Height,
                        MaxHeight = 128D,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };

                default:
                    return new TextBlock {
                        Background = Brushes.Transparent,
                        TextWrapping = TextWrapping.Wrap,
                        Text = content
                    };
            }
        }

        private void UpdateControlsState() {
            favoriteButton.IsToggled = Model.Tags.Any(
                tag => tag.Name == "category" && tag.Value.ToString() == "favorite"
            );
        }

        #endregion

        #region Actions

        public void Remove(ActionButtonViewModel source) {
            eventAggregator.PublishOnCurrentThreadAsync(new ClipEvent(model, ClipEventType.Remove));
        }

        public void Select() {
            eventAggregator.PublishOnCurrentThreadAsync(new ClipEvent(model, ClipEventType.Select));
        }

        public void ToggleFavorite(ActionButtonViewModel source) {
            eventAggregator.PublishOnCurrentThreadAsync(new ClipEvent(model, ClipEventType.ToggleFavorite));
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
