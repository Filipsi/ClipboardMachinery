using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using ClipboardMachinery.Components.ActionButton;
using ClipboardMachinery.Components.Tag;
using Image = System.Windows.Controls.Image;
using Screen = Caliburn.Micro.Screen;

namespace ClipboardMachinery.Components.Clip {

    public class ClipViewModel : Screen {

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

                if (value != null) {
                    value.PropertyChanged += OnModelPropertyChanged;
                    value.Tags.CollectionChanged += OnModelTagCollectionChanged;
                    OnModelTagCollectionChanged(value.Tags, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value.Tags));
                }

                model = value;
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

        #region Events

        internal event EventHandler RemovalRequest;

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

        private readonly Func<TagViewModel> tagVmFactory;

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

        public ClipViewModel(Func<TagViewModel> tagVmFactory, Func<ActionButtonViewModel> buttonVmFactory) {
            this.tagVmFactory = tagVmFactory;
            Tags = new BindableCollection<TagViewModel>();
            Controls = new BindableCollection<ActionButtonViewModel>();

            // Create controls
            ActionButtonViewModel removeButton = buttonVmFactory.Invoke();
            removeButton.Icon = (Geometry)Application.Current.FindResource("IconRemove");
            removeButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("DangerousActionBrush");
            removeButton.ClickAction = Remove;
            Controls.Add(removeButton);
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
                        foreach(TagViewModel vm in Tags.Where(vm => vm.Model == model)) {
                            Tags.Remove(vm);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    Tags.Clear();
                    break;
            }
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            ClipModel model = sender as ClipModel;

            if (e.PropertyName == nameof(ClipModel.Content)) {
                NotifyOfPropertyChange(() => Content);
                NotifyOfPropertyChange(() => Type);
                NotifyOfPropertyChange(() => Icon);
                return;
            }
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

        #endregion

        #region Actions

        public void Remove(ActionButtonViewModel source) {
            RemovalRequest?.Invoke(this, EventArgs.Empty);
        }

        public void Select() {

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
