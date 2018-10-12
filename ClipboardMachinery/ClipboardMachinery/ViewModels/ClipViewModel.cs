using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using ClipboardMachinery.Events.Collection;
using ClipboardMachinery.Models;
using Newtonsoft.Json;
using Image = System.Windows.Controls.Image;
using Screen = Caliburn.Micro.Screen;

namespace ClipboardMachinery.ViewModels {

    public class ClipViewModel : Screen {

        #region Properties

        public ClipModel Model {
            get => model;
            set {
                if (model == value)
                    return;

                if (model != null) {
                    model.PropertyChanged -= OnModelPropertyChanged;
                }

                if (value != null) {
                    value.PropertyChanged += OnModelPropertyChanged;
                }

                model = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => Content);
                NotifyOfPropertyChange(() => Type);
                NotifyOfPropertyChange(() => Icon);
                NotifyOfPropertyChange(() => FavoriteIcon);
                NotifyOfPropertyChange(() => FavoriteIconColor);
                NotifyOfPropertyChange(() => FavoriteIconColor);
                NotifyOfPropertyChange(() => BackgroundColor);
            }
        }

        public object Content
            => WrapContentForType(Type, Model.RawContent);

        public EntryType Type {
            get {
                if (ImageDataPattern.IsMatch(Model.RawContent)) {
                    return EntryType.Image;
                }

                if (Uri.IsWellFormedUriString(Model.RawContent, UriKind.Absolute)) {
                    return EntryType.Link;
                }

                return EntryType.Text;
            }
        }

        public Geometry Icon
            => Application.Current.FindResource(IconMap[Type]) as Geometry;

        public Geometry FavoriteIcon
            => Application.Current.FindResource(Model.IsFavorite ? "IconStarFull" : "IconStarEmpty") as Geometry;

        public SolidColorBrush FavoriteIconColor
            => Application.Current.FindResource(Model.IsFavorite ? "ElementFavoriteBrush" : "PanelControlBrush") as SolidColorBrush;

        public SolidColorBrush BackgroundColor
            => Application.Current.FindResource(Model.IsFocused ? "ElementSelectBrush" : "BodyBackgroundBrush") as SolidColorBrush;

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

        private readonly IEventAggregator eventBus;

        private ClipModel model;

        #endregion

        #region Enumerations

        public enum EntryType {
            Text,
            Link,
            Image
        }

        #endregion

        public ClipViewModel(IEventAggregator eventAggregator) {
            eventBus = eventAggregator;
        }

        #region Handlers

        private void OnModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            ClipModel model = sender as ClipModel;

            if (e.PropertyName == nameof(ClipModel.RawContent)) {
                NotifyOfPropertyChange(() => Content);
                NotifyOfPropertyChange(() => Type);
                NotifyOfPropertyChange(() => Icon);
                return;
            }

            if (e.PropertyName == nameof(ClipModel.IsFocused)) {
                NotifyOfPropertyChange(() => BackgroundColor);
                return;
            }

            if (e.PropertyName == nameof(ClipModel.IsFavorite)) {
                NotifyOfPropertyChange(() => FavoriteIcon);
                NotifyOfPropertyChange(() => FavoriteIconColor);
                eventBus.PublishOnCurrentThread(new ItemFavoriteChanged<ClipViewModel>(this));
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

        public void Remove()
            => eventBus.PublishOnUIThread(new ItemRemoved<ClipViewModel>(this));

        public void Select()
            => eventBus.PublishOnUIThread(new ItemSelected<ClipViewModel>(this));

        public void ToggleFavorite() {
            Model.IsFavorite = !Model.IsFavorite;
        }

        public void Focus() {
            Model.IsFocused = true;
        }

        public void Unfocus() {
            Model.IsFocused = false;
        }

        #endregion

    }

}
