using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using ClipboardMachinery.Components.TagKind;
using ClipboardMachinery.Core.TagKind;

namespace ClipboardMachinery.Components.TagType {

    public class TagTypeViewModel : Screen {

        #region Properties

        public TagTypeModel Model {
            get;
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

        public SolidColorBrush SelectionColor
            => Application.Current.FindResource(IsFocused ? "ElementSelectBrush" : "PanelControlBrush") as SolidColorBrush;

        public string KindLabel {
            get {
                ITagKindSchema tagKind = tagKindManager.GetSchemaFor(Model.Kind);
                return tagKind == null ? string.Empty : $"with {tagKind.Name.ToLowerInvariant()} value";
            }
        }

        #endregion

        public event EventHandler Selected;

        #region Fields

        private readonly ITagKindManager tagKindManager;

        private bool isFocused;

        #endregion

        public TagTypeViewModel(TagTypeModel model, ITagKindManager tagKindManager) {
            Model = model;
            this.tagKindManager = tagKindManager;
        }

        #region Handlers

        protected override Task OnActivateAsync(CancellationToken cancellationToken) {
            Model.PropertyChanged += OnModelPropertyChanged;
            return base.OnActivateAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            Model.PropertyChanged -= OnModelPropertyChanged;
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(TagTypeModel.Kind)) {
                NotifyOfPropertyChange(() => KindLabel);
            }
        }

        #endregion

        #region Actions

        public void Select() {
            Selected?.Invoke(this, EventArgs.Empty);
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
