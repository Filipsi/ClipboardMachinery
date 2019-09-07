using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Caliburn.Micro;
using ClipboardMachinery.Common.Helpers;
using Microsoft.Xaml.Behaviors;
using Action = System.Action;

namespace ClipboardMachinery.Common.Behaviors {

    internal class FocusAdornerBehavior : Behavior<FrameworkElement>  {

        #region Properties

        #region Content

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            name: nameof(Content),
            propertyType: typeof(IScreen),
            ownerType: typeof(FocusAdornerBehavior),
            typeMetadata: new FrameworkPropertyMetadata(
                defaultValue: null,
                propertyChangedCallback: OnContentChange
            )
        );

        public IScreen Content {
            get => (IScreen)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        #endregion

        #region MaxHeigh

        public static readonly DependencyProperty MaxHeightProperty = DependencyProperty.Register(
            name: nameof(MaxHeigh),
            propertyType: typeof(double),
            ownerType: typeof(FocusAdornerBehavior),
            typeMetadata: new FrameworkPropertyMetadata(
                defaultValue: 256D,
                propertyChangedCallback: MaxHeightChange
            )
        );

        public double MaxHeigh {
            get => (double)GetValue(MaxHeightProperty);
            set => SetValue(MaxHeightProperty, value);
        }

        #endregion

        #region BorderThickness

        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register(
            name: nameof(BorderThickness),
            propertyType: typeof(Thickness),
            ownerType: typeof(FocusAdornerBehavior),
            typeMetadata: new FrameworkPropertyMetadata(
                defaultValue: new Thickness(0)
            )
        );

        public Thickness BorderThickness {
            get => (Thickness)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        #endregion

        #region BorderBrush

        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(
            name: nameof(BorderBrush),
            propertyType: typeof(Brush),
            ownerType: typeof(FocusAdornerBehavior),
            typeMetadata: new FrameworkPropertyMetadata(
                defaultValue: new SolidColorBrush(Color.FromRgb(171, 173, 179))
            )
        );

        public Brush BorderBrush {
            get => (Brush)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        #endregion

        #region IsVisible

        private static readonly DependencyPropertyKey IsVisiblePropertyKey = DependencyProperty.RegisterReadOnly(
            name: nameof(IsVisible),
            propertyType: typeof(bool),
            ownerType: typeof(FocusAdornerBehavior),
            typeMetadata: new PropertyMetadata(
                defaultValue: false,
                propertyChangedCallback: OnIsFocusedChange
            )
        );

        public static readonly DependencyProperty IsVisibleProperty
            = IsVisiblePropertyKey.DependencyProperty;

        public bool IsVisible {
            get => (bool)GetValue(IsVisibleProperty);
            protected set => SetValue(IsVisiblePropertyKey, value);
        }

        #endregion

        #endregion

        #region Fields

        private bool isUnhooked;
        private AdornerContentPresenter adorner;
        private Action HandleClose;

        #endregion

        #region Behavior

        protected override void OnAttached() {
            base.OnAttached();

            HandleClose = ((Action) Close).Debounce();

            if (AssociatedObject.IsLoaded) {
                SetupAdorner();
            } else {
                AssociatedObject.Loaded += OnAssociatedObjectLoaded;
            }

            // Hook events for associated object
            AssociatedObject.Unloaded += OnAssociatedObjectUnloaded;
            AssociatedObject.GotFocus += OnAssociatedObjectGotFocus;
            AssociatedObject.LostFocus += OnAssociatedObjectLostFocus;
        }

        protected override void OnDetaching() {
            if (isUnhooked) {
                return;
            }

            base.OnDetaching();
            isUnhooked = true;

            // Remove search adorer
            AdornerLayer.GetAdornerLayer(AssociatedObject)?.Remove(adorner);

            // Unhook events from associated object
            AssociatedObject.Loaded -= OnAssociatedObjectLoaded;
            AssociatedObject.Unloaded -= OnAssociatedObjectUnloaded;
            AssociatedObject.GotFocus -= OnAssociatedObjectGotFocus;
            AssociatedObject.LostFocus -= OnAssociatedObjectLostFocus;
        }

        #endregion

        #region Handlers

        private void OnAssociatedObjectUnloaded(object sender, RoutedEventArgs e) {
            if (!isUnhooked) {
                OnDetaching();
            }
        }

        private void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e) {
            AssociatedObject.Loaded -= OnAssociatedObjectLoaded;
            SetupAdorner();
        }

        private void OnAssociatedObjectGotFocus(object sender, RoutedEventArgs e) {
            Open();
        }

        private void OnAssociatedObjectLostFocus(object sender, RoutedEventArgs e) {
            HandleClose();
        }

        private static void OnIsFocusedChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            FocusAdornerBehavior behavior = (FocusAdornerBehavior) d;
            AdornerContentPresenter adorner = behavior.adorner;
            if (adorner == null) {
                return;
            }

            bool isVisible = (bool) e.NewValue;
            adorner.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;

            if (isVisible) {
                behavior.Content.ActivateAsync(CancellationToken.None);
            } else {
                behavior.Content.DeactivateAsync(false, CancellationToken.None);
            }
        }

        private static void OnContentChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            FocusAdornerBehavior behavior = (FocusAdornerBehavior)d;
            if (behavior.adorner != null) {
                View.SetModel(behavior.adorner.Content, e.NewValue);
            }
        }

        private static void MaxHeightChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            FocusAdornerBehavior behavior = (FocusAdornerBehavior)d;
            if (behavior.adorner != null) {
                behavior.adorner.Content.MaxHeight = (double)e.NewValue;
            }
        }

        #endregion

        #region Logic

        private void Open() {
            Application.Current.Dispatcher?.Invoke(() => IsVisible = true);
        }

        private void Close() {
            Application.Current.Dispatcher?.Invoke(() => IsVisible = false);
        }

        private AdornerContentPresenter CreateAdorner() {
            ContentControl contentWrapper = new ContentControl {
                IsHitTestVisible = true,
                MaxHeight = MaxHeigh,
                Template = CreateBorderTemplate()
            };

            View.SetModel(contentWrapper, Content);
            return new AdornerContentPresenter(AssociatedObject, contentWrapper) {
                Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed
            };
        }

        private void SetupAdorner() {
            adorner = CreateAdorner();
            AdornerLayer.GetAdornerLayer(AssociatedObject)?.Add(adorner);
        }

        private ControlTemplate CreateBorderTemplate() {
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(
                type: typeof(Border)
            );

            borderFactory.SetBinding(
                dp: Border.BorderThicknessProperty,
                binding: new Binding {
                    Path = new PropertyPath(BorderThicknessProperty),
                    Mode = BindingMode.OneWay,
                    Source = this
                }
            );

            borderFactory.SetBinding(
                dp: Border.BorderBrushProperty,
                binding: new Binding {
                    Path = new PropertyPath(BorderBrushProperty),
                    Mode = BindingMode.OneWay,
                    Source = this
                }
            );

            FrameworkElementFactory contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            borderFactory.AppendChild(contentPresenterFactory);

            return new ControlTemplate(typeof(ContentControl)) {
                VisualTree = borderFactory
            };
        }

        #endregion

    }

    public class AdornerContentPresenter : Adorner {

        #region Properties

        public ContentControl Content {
            get => (ContentControl)contentPresenter.Content;
            set => contentPresenter.Content = value;
        }

        #endregion

        #region Fields

        private readonly VisualCollection visuals;
        private readonly ContentPresenter contentPresenter;

        #endregion

        public AdornerContentPresenter(FrameworkElement adornedElement, ContentControl content) : this(adornedElement) {
            Content = content;
        }

        public AdornerContentPresenter(FrameworkElement adornedElement) : base(adornedElement) {
            visuals = new VisualCollection(this);
            contentPresenter = new ContentPresenter {
                Width = adornedElement.ActualWidth,
                RenderTransform = new TranslateTransform(0, adornedElement.ActualHeight)
            };

            visuals.Add(contentPresenter);
        }

        #region Logic

        protected override Size MeasureOverride(Size constraint) {
            contentPresenter.Measure(constraint);
            return contentPresenter.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            contentPresenter.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            return contentPresenter.RenderSize;
        }

        protected override Visual GetVisualChild(int index)
            => visuals[index];

        protected override int VisualChildrenCount
            => visuals.Count;

        #endregion
    }

}

