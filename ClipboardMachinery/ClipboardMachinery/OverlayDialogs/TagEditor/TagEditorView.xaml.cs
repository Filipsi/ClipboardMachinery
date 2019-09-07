using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ClipboardMachinery.Common.Behaviors;
using ClipboardMachinery.Common.Helpers;
using ClipboardMachinery.Common.Screen;

namespace ClipboardMachinery.OverlayDialogs.TagEditor {

    public partial class TagEditorView : UserControl {

        #region Fields

        private DependencyPropertyDescriptor descriptor;

        #endregion

        public TagEditorView() {
            InitializeComponent();
        }

        #region Handlers

        private void OnInitialized(object sender, EventArgs e) {
            descriptor = DependencyPropertyDescriptor.FromProperty(
                dependencyProperty: FocusAdornerBehavior.IsVisibleProperty,
                targetType: typeof(FocusAdornerBehavior)
            );

            descriptor.AddValueChanged(
                component: TagTypeAdorner,
                handler: OnTagTypeAdornerVisibilityChange
            );
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            descriptor.RemoveValueChanged(
                component: TagTypeAdorner,
                handler: OnTagTypeAdornerVisibilityChange
            );
        }

        private void OnTagTypeAdornerVisibilityChange(object sender, EventArgs e) {
            if (!(DataContext is ValidationScreenBase validationScreen)) {
                return;
            }

            if (TagTypeAdorner.IsVisible) {
                validationScreen.ClearErrors();
            } else {
                validationScreen.Validate();
            }
        }

        #endregion

    }

}
