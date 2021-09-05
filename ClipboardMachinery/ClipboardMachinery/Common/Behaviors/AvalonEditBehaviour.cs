using ICSharpCode.AvalonEdit;
using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;

namespace ClipboardMachinery.Common.Behaviors {

    internal sealed class AvalonEditBehaviour : Behavior<TextEditor> {

        #region Properties

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            name: nameof(Text),
            propertyType: typeof(string),
            ownerType: typeof(AvalonEditBehaviour),
            typeMetadata: new FrameworkPropertyMetadata(
                defaultValue: default(string),
                flags: FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                propertyChangedCallback: OnTextPropertyChange
            )
        );

        public string Text {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        #endregion

        #region Behavior

        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.TextChanged += OnTextChanged;
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            AssociatedObject.TextChanged -= OnTextChanged;
        }

        #endregion

        #region Handlers

        private void OnTextChanged(object sender, EventArgs eventArgs) {
            if (!(sender is TextEditor textEditor)) {
                return;
            }

            Text = textEditor?.Document.Text;
        }

        private static void OnTextPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is AvalonEditBehaviour behavior)) {
                return;
            }

            if (!(behavior.AssociatedObject is TextEditor textEditor)) {
                return;
            }

            if (textEditor.Document == null) {
                return;
            }

            string newText = e.NewValue?.ToString() ?? string.Empty;
            int caretOffset = textEditor.CaretOffset;
            textEditor.Document.Text = newText;
            textEditor.CaretOffset = caretOffset > newText.Length ? newText.Length : caretOffset;
        }

        #endregion

    }

}
