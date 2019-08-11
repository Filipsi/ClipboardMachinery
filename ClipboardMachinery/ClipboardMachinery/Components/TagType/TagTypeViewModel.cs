using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.TagKind;
using ClipboardMachinery.Core.TagKind;
using ClipboardMachinery.DialogOverlays.TagTypeEditor;
using ClipboardMachinery.Plumbing.Factories;

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

        public bool CanEdit {
            get => canEdit;
            private set {
                if (canEdit == value) {
                    return;
                }

                canEdit = value;
                NotifyOfPropertyChange();
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

        #region Fields

        private readonly ITagKindManager tagKindManager;
        private readonly IEventAggregator eventAggregator;
        private readonly IDialogOverlayFactory dialogOverlayFactory;

        private bool isFocused;
        private bool canEdit = true;

        #endregion

        public TagTypeViewModel(TagTypeModel model, ITagKindManager tagKindManager, IEventAggregator eventAggregator, IDialogOverlayFactory dialogOverlayFactory) {
            Model = model;
            this.tagKindManager = tagKindManager;
            this.eventAggregator = eventAggregator;
            this.dialogOverlayFactory = dialogOverlayFactory;
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

        private void OnTagTypeEditorDeactivated(object sender, DeactivationEventArgs e) {
            TagTypeEditorViewModel tagTypeEditor = (TagTypeEditorViewModel)sender;
            tagTypeEditor.Deactivated -= OnTagTypeEditorDeactivated;
            dialogOverlayFactory.Release(tagTypeEditor);
            CanEdit = true;
        }

        #endregion

        #region Actions

        public void Edit() {
            TagTypeEditorViewModel tagTypeEditor = dialogOverlayFactory.CreateTagTypeEditor(Model);
            tagTypeEditor.Deactivated += OnTagTypeEditorDeactivated;
            CanEdit = false;
            eventAggregator.PublishOnCurrentThreadAsync(PopupEvent.Show(tagTypeEditor));
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
