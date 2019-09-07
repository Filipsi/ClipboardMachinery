using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ClipboardMachinery.Common.Helpers;
using Microsoft.Xaml.Behaviors;

namespace ClipboardMachinery.Common.Behaviors {

    internal class CollapseOverflowBehavior : Behavior<ItemsControl> {

        #region Properties

        public static readonly DependencyProperty OverflowIndicatorElementProperty = DependencyProperty.Register(
            name: nameof(OverflowIndicatorElement),
            propertyType: typeof(FrameworkElement),
            ownerType: typeof(CollapseOverflowBehavior),
            typeMetadata: new FrameworkPropertyMetadata(
                defaultValue: null,
                propertyChangedCallback: OnOverflowIndicatorElementChange
            )
        );

        public FrameworkElement OverflowIndicatorElement {
            get => (FrameworkElement)GetValue(OverflowIndicatorElementProperty);
            set => SetValue(OverflowIndicatorElementProperty, value);
        }

        public static readonly DependencyProperty OverflowIndicatorOffsetProperty = DependencyProperty.Register(
            name: nameof(OverflowIndicatorOffset),
            propertyType: typeof(double),
            ownerType: typeof(CollapseOverflowBehavior),
            typeMetadata: new FrameworkPropertyMetadata(
                defaultValue: 0D,
                propertyChangedCallback: OverflowIndicatorOffsetChange
            )
        );

        public double OverflowIndicatorOffset {
            get => (double)GetValue(OverflowIndicatorOffsetProperty);
            set => SetValue(OverflowIndicatorOffsetProperty, value);
        }

        public static readonly DependencyProperty OverflowElementsCountProperty = DependencyProperty.Register(
            name: nameof(OverflowElementsCount),
            propertyType: typeof(int),
            ownerType: typeof(CollapseOverflowBehavior)
        );

        public int OverflowElementsCount {
            get => (int)GetValue(OverflowElementsCountProperty);
            set => SetValue(OverflowElementsCountProperty, value);
        }

        #endregion

        #region Fields

        private readonly List<FrameworkElement> trackedContainers = new List<FrameworkElement>();
        private readonly Action debouncedSizeChange;
        private bool isUnhooked;

        #endregion

        public CollapseOverflowBehavior() {
            Action dispatchSizeChange = () => {
                Application.Current.Dispatcher?.Invoke(HandleSizeChange);
            };

            debouncedSizeChange = dispatchSizeChange.Debounce(500);
        }

        #region Behavior

        protected override void OnAttached() {
            base.OnAttached();

            // Hook events for associated object
            AssociatedObject.ItemContainerGenerator.StatusChanged += OnItemsGeneratorStatusChanged;
            AssociatedObject.Unloaded += OnAssociatedObjectUnloaded;
        }

        protected override void OnDetaching() {
            if (isUnhooked) {
                return;
            }

            base.OnDetaching();
            isUnhooked = true;

            // Unhook events from associated object
            AssociatedObject.Unloaded -= OnAssociatedObjectUnloaded;
            AssociatedObject.ItemContainerGenerator.StatusChanged -= OnItemsGeneratorStatusChanged;

            // Unhook events from tracked containers
            foreach (FrameworkElement container in trackedContainers.ToArray()) {
                StopTrackingItemContainer(container);
            }
        }

        #endregion

        #region Handlers

        private void OnAssociatedObjectUnloaded(object sender, RoutedEventArgs e) {
            if (!isUnhooked) {
                OnDetaching();
            }
        }

        private void OnItemContainerUnloaded(object sender, RoutedEventArgs e) {
            StopTrackingItemContainer((FrameworkElement)sender);
        }

        private void OnContainerSizeChanged(object sender, SizeChangedEventArgs e) {
            debouncedSizeChange();
        }

        private void OnItemsGeneratorStatusChanged(object sender, EventArgs e) {
            ItemContainerGenerator generator = (ItemContainerGenerator)sender;
            if (generator.Status != GeneratorStatus.ContainersGenerated) {
                return;
            }

            if (generator.Items.Count == 0) {
                return;
            }

            IEnumerable<FrameworkElement> containers = generator.Items
                .Select(item => generator.ContainerFromItem(item))
                .Where(container => container is FrameworkElement)
                .Cast<FrameworkElement>();

            foreach (FrameworkElement container in containers) {
                StartTrackingItemContainer(container);
            }
        }

        private static void OnOverflowIndicatorElementChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            CollapseOverflowBehavior behavior = (CollapseOverflowBehavior) d;
            behavior.OverflowIndicatorElement.Visibility = Visibility.Hidden;
            behavior.OverflowIndicatorElement.DataContext = behavior;
        }


        private static void OverflowIndicatorOffsetChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            CollapseOverflowBehavior behavior = (CollapseOverflowBehavior)d;

            if (behavior.AssociatedObject == null) {
                return;
            }

            int fittedContainers = behavior.GetNumberOfFittingContainers(behavior.AssociatedObject.ActualWidth);
            behavior.UpdateOverflowIndicatorOffset(fittedContainers);
        }

        #endregion

        #region Logic

        private void StartTrackingItemContainer(FrameworkElement container) {
            // Make sure that the element we want to add is not already tracked container
            if (trackedContainers.Contains(container)) {
                return;
            }

            container.Unloaded += OnItemContainerUnloaded;
            container.SizeChanged += OnContainerSizeChanged;
            trackedContainers.Add(container);
        }

        private void StopTrackingItemContainer(FrameworkElement container) {
            // We shouldn't tract the overflow indicator
            if (Equals(container, OverflowIndicatorElement)) {
                return;
            }

            // Make sure that the element we want to remove is a tracked container
            if (!trackedContainers.Contains(container)) {
                return;
            }

            container.Unloaded -= OnItemContainerUnloaded;
            container.SizeChanged -= OnContainerSizeChanged;
            trackedContainers.Remove(container);
        }

        private int GetNumberOfFittingContainers(double wrapperWidth) {;
            double oveflowIndicatorWidth = OverflowIndicatorElement?.ActualWidth ?? 0;
            double currentWidth = oveflowIndicatorWidth;
            int fittedContainers = 0;

            for (int i = 0; i < trackedContainers.Count; i++) {
                FrameworkElement container = trackedContainers[i];
                double containerWidth = container.ActualWidth;

                if (currentWidth + containerWidth > wrapperWidth) {
                    if (i == trackedContainers.Count - 1) {
                        if (currentWidth + containerWidth - oveflowIndicatorWidth > wrapperWidth) {
                            break;
                        }
                    } else {
                        break;
                    }
                }

                fittedContainers++;
                currentWidth += containerWidth;
            }

            return fittedContainers;
        }

        private void HandleSizeChange() {
            // Calculate sizes of wrapper and items
            double wrapperWidth = AssociatedObject.ActualWidth;
            double itemsWidth = trackedContainers
                .Select(container => container.ActualWidth)
                .Sum();

            // If items are overflowing the container
            if (itemsWidth <= wrapperWidth) {
                if (!(trackedContainers.Any(container => container.Visibility != Visibility.Visible))) {
                    return;
                }
            }

            // Update visibility depending of number of containers that can fit the wrapper
            int fittedContainers = GetNumberOfFittingContainers(wrapperWidth);
            for (int index = 0; index < trackedContainers.Count; index++) {
                trackedContainers[index].Visibility = index >= fittedContainers ? Visibility.Hidden : Visibility.Visible;
            }

            if (OverflowIndicatorElement == null) {
                return;
            }

            // Update overflow indicator
            if (fittedContainers != trackedContainers.Count) {
                UpdateOverflowIndicatorOffset(fittedContainers);
                OverflowElementsCount = trackedContainers.Count - fittedContainers;
                OverflowIndicatorElement.Visibility = Visibility.Visible;
            } else {
                OverflowIndicatorElement.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateOverflowIndicatorOffset(int fittedContainers) {
            double fittedItemsWidth = trackedContainers
                .Take(Math.Max(0, fittedContainers - 1))
                .Select(container => container.ActualWidth + container.Margin.Left + container.Margin.Right)
                .Sum();

            FrameworkElement lastFittedContainer = (FrameworkElement)AssociatedObject.ItemContainerGenerator.ContainerFromIndex(Math.Max(0, fittedContainers - 1));
            OverflowIndicatorElement.RenderTransform = new TranslateTransform(fittedItemsWidth + lastFittedContainer.ActualWidth + OverflowIndicatorOffset, 0);
        }

        #endregion

    }

}
