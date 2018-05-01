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
using Bibliotheque.Machine;
using Caliburn.Micro;
using ClipboardMachinery.Events.Collection;
using Ninject;
using Image = System.Windows.Controls.Image;
using Screen = Caliburn.Micro.Screen;

namespace ClipboardMachinery.ViewModels {

    internal class ClipViewModel : Screen {

        [Inject]
        public IEventAggregator Events { set; get; }

        public string RawContent {
            get => _rawContent;
            set {
                if (value == _rawContent) return;

                _rawContent = value;
                Type = DeterminateType(_rawContent);
                Content = WrapContentForType(Type, _rawContent);
                Icon = Application.Current.FindResource(GetIconFromType(Type)) as Geometry;

                NotifyOfPropertyChange(() => RawContent);
                NotifyOfPropertyChange(() => Content);
                NotifyOfPropertyChange(() => Type);
                NotifyOfPropertyChange(() => Icon);
            }
        }

        public string Timestamp {
            get => _timestamp;
            set {
                if (value == _timestamp) return;
                _timestamp = value;
                NotifyOfPropertyChange(() => Timestamp);
            }
        }

        public bool IsFavorite {
            get => _isFavorite;
            private set {
                if (value == _isFavorite) return;
                _isFavorite = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => FavoriteIcon);
                NotifyOfPropertyChange(() => FavoriteIconColor);
            }
        }

        public bool IsFocused {
            get => _isFocused;
            set {
                if (value == _isFocused) return;
                _isFocused = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => BackgroundColor);
            }
        }

        public object Content { private set; get; }

        public Geometry Icon { private set; get; }

        public EntryType Type { private set; get; }

        public Geometry FavoriteIcon
            => Application.Current.FindResource(IsFavorite ? "IconStarFull" : "IconStarEmpty") as Geometry;

        public SolidColorBrush FavoriteIconColor
            => Application.Current.FindResource(IsFavorite ? "ElementFavoriteBrush" : "PanelControlBrush") as SolidColorBrush;

        public SolidColorBrush BackgroundColor
            => Application.Current.FindResource(IsFocused ? "ElementSelectBrush" : "BodyBackgroundBrush") as SolidColorBrush;

        private bool _isFavorite;
        private bool _isFocused;
        private string _timestamp;
        private string _rawContent;

        private static readonly Regex ImageDataPattern =
            new Regex(@"^data\:(?<visiblityChangeType>image\/(png|tiff|jpg|gif));base64,(?<data>[A-Z0-9\+\/\=]+)$",
                RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        public enum EntryType {
            Text,
            Link,
            Image
        }

        private static EntryType DeterminateType(string content) {
            if (ImageDataPattern.IsMatch(content)) {
                return EntryType.Image;
            }

            if (Uri.IsWellFormedUriString(content, UriKind.Absolute)) {
                return EntryType.Link;
            }

            return EntryType.Text;
        }

        private static string GetIconFromType(EntryType type) {
            switch (type) {
                case EntryType.Link:
                    return "IconLink";

                case EntryType.Image:
                    return "IconPicture";

                default:
                    return "IconTextFile";
            }
        }

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

        #region Actions

        public void Remove() {
            Events.PublishOnCurrentThread(new ItemRemoved<ClipViewModel>(this));
        }

        public void Select() {
            Events.PublishOnCurrentThread(new ItemSelected<ClipViewModel>(this));
        }

        public void ToggleFavorite() {
            IsFavorite = !IsFavorite;
            Events.PublishOnCurrentThread(new ItemFavoriteChanged<ClipViewModel>(this));
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
