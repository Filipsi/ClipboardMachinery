﻿using ClipboardMachinery.Common.Helpers;
using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace ClipboardMachinery.Common.Behaviors {

    public class FocusPopupBehavior : Behavior<Popup> {

        #region Properties

        public static readonly DependencyProperty TriggerElementProperty = DependencyProperty.Register(
            name: nameof(TriggerElement),
            propertyType: typeof(FrameworkElement),
            ownerType: typeof(FocusPopupBehavior),
            typeMetadata: new PropertyMetadata(HandleTriggerElementChange)
        );

        public FrameworkElement TriggerElement {
            get => (FrameworkElement)GetValue(TriggerElementProperty);
            set => SetValue(TriggerElementProperty, value);
        }

        #endregion

        #region Fields

        private bool isHookedUp;

        #endregion

        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.Loaded += OnPopupLoaded;
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            Cleanup();
        }

        private void Setup() {
            // Make sure that we won't setup again after it was already processed
            if (isHookedUp) {
                return;
            }

            // Mark setup as processed
            isHookedUp = true;

            // Set default properties to the pop-up
            AssociatedObject.PopupAnimation = PopupAnimation.Slide;
            AssociatedObject.AllowsTransparency = true;
            AssociatedObject.StaysOpen = true;

            // Add hooks from pop-up element
            AssociatedObject.LostKeyboardFocus += OnPopupLostKeyboardFocus;
            AssociatedObject.Unloaded += OnPopupUnloaded;

            // Bind the two elements together
            BindPopupToTrigger(TriggerElement);
        }

        private void Cleanup() {
            // Make sure that we won't clean if there we are not set up
            if (!isHookedUp) {
                return;
            }

            // Mark cleanup as processed
            isHookedUp = false;

            // Remove hooks from pop-up element
            AssociatedObject.LostKeyboardFocus -= OnPopupLostKeyboardFocus;
            AssociatedObject.Unloaded -= OnPopupUnloaded;

            // Clear bound element in order to remove hooks
            TriggerElement = null;
        }

        private void BindPopupToTrigger(FrameworkElement triggetElement) {
            if (AssociatedObject == null) {
                return;
            }

            // Bind bound element as pop-up placement target
            BindingOperations.ClearBinding(AssociatedObject, Popup.PlacementTargetProperty);
            if (triggetElement != null) {
                AssociatedObject.SetBinding(Popup.PlacementTargetProperty, new Binding() { Source = triggetElement });
            }

            // Bind pop-up with to bound element width, this makes it look like suggestion box
            BindingOperations.ClearBinding(AssociatedObject, FrameworkElement.WidthProperty);
            if (triggetElement != null) {
                AssociatedObject.SetBinding(FrameworkElement.WidthProperty, new Binding("ActualWidth") { Source = triggetElement });
            }
        }

        #region Handlers

        private void OnPopupLoaded(object sender, RoutedEventArgs e) {
            Setup();
        }

        private void OnPopupUnloaded(object sender, RoutedEventArgs e) {
            Cleanup();
        }

        private static void HandleTriggerElementChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is FocusPopupBehavior behavior)) {
                throw new ArgumentException($"Expected {nameof(FocusPopupBehavior)} as property change source!", nameof(d));
            }

            // Remove hooks from previous bound element
            if (e.OldValue is FrameworkElement oldBoundElement) {
                oldBoundElement.GotFocus -= behavior.OnBoundElementGotFocus;
                oldBoundElement.LostKeyboardFocus -= behavior.OnBoundElementLostKeyboardFocus;
            }

            // Add hooks to new bound element
            if (e.NewValue is FrameworkElement newBoundElement) {
                newBoundElement.GotFocus += behavior.OnBoundElementGotFocus;
                newBoundElement.LostKeyboardFocus += behavior.OnBoundElementLostKeyboardFocus;
            }

            behavior.BindPopupToTrigger(e.NewValue as FrameworkElement);
        }

        private void OnBoundElementGotFocus(object sender, RoutedEventArgs e) {
            AssociatedObject.IsOpen = true;
        }

        private void OnBoundElementLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            // If the focused element is a child of the window, it can't be the child of the pop-up
            if (!(e.NewFocus is DependencyObject newFocusedElement) || newFocusedElement.FindParent<Window>() != null) {
                AssociatedObject.IsOpen = false;
            }
        }

        private void OnPopupLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            AssociatedObject.IsOpen = e.NewFocus == TriggerElement;
        }

        #endregion

    }

}
