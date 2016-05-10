namespace Virgil.Sync.View.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Interactivity;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    public class SacleWindowBehavior : Behavior<FrameworkElement>
    {

        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            "Width", typeof (double), typeof (SacleWindowBehavior), new PropertyMetadata(default(double)));

        public double Width
        {
            get { return (double) this.GetValue(WidthProperty); }
            set { this.SetValue(WidthProperty, value); }
        }

        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(
            "Height", typeof (double), typeof (SacleWindowBehavior), new PropertyMetadata(default(double)));

        public double Height
        {
            get { return (double) this.GetValue(HeightProperty); }
            set { this.SetValue(HeightProperty, value); }
        }

        private static DependencyObject GetParent(DependencyObject obj)
        {
            if (obj == null)
                return null;

            ContentElement ce = obj as ContentElement;
            if (ce != null)
            {
                DependencyObject parent = ContentOperations.GetParent(ce);
                if (parent != null)
                    return parent;

                FrameworkContentElement fce = ce as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            return VisualTreeHelper.GetParent(obj);
        }

        private static T FindAncestorOrSelf<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                T objTest = obj as T;
                if (objTest != null)
                    return objTest;
                obj = GetParent(obj);
            }

            return null;
        }

        protected override void OnAttached()
        {
            var window = FindAncestorOrSelf<Window>(this.AssociatedObject);

            window.Width = this.Width;
            window.Height = this.Height;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }

    public class EntranceTransitionBehavior : Behavior<FrameworkElement>
    {
        static readonly Stopwatch stopwatch = new Stopwatch();
        private static readonly List<SiblingData> internalSiblingData = new List<SiblingData>(100);

        private TimeSpan lastUnloadTime;
        private List<Storyboard> storyboards;
        private bool hasAnimated;

        private TranslateTransform cachedRenderTransform;

        private class SiblingData
        {
            public WeakReference<FrameworkElement> Parent;
            public TimeSpan LastStartOffset;
            public TimeSpan LastAnimationStart;
        }

        private void PruneSiblingData()
        {
            var deadItems = new List<SiblingData>();

            foreach (var siblingData in internalSiblingData)
            {
                FrameworkElement element;
                if (!siblingData.Parent.TryGetTarget(out element))
                {
                    deadItems.Add(siblingData);
                }
            }

            deadItems.ForEach(e => internalSiblingData.Remove(e));
        }

        SiblingData GetSiblingData(FrameworkElement parent)
        {
            this.PruneSiblingData();

            foreach (var siblingData in internalSiblingData)
            {
                FrameworkElement element;
                if (siblingData.Parent.TryGetTarget(out element))
                {
                    if (ReferenceEquals(element, parent))
                    {
                        return siblingData;
                    }
                }
            }

            var newData = new SiblingData { Parent = new WeakReference<FrameworkElement>(parent) };
            internalSiblingData.Add(newData);

            return newData;
        }

        static EntranceTransitionBehavior()
        {
            stopwatch.Start();
        }
        
        #region FromHorizontalOffset

        public static readonly DependencyProperty FromHorizontalOffsetProperty =
            DependencyProperty.Register("FromHorizontalOffset", typeof(double), typeof(EntranceTransitionBehavior),
                new FrameworkPropertyMetadata(100.0,
                    FrameworkPropertyMetadataOptions.None));

        public double FromHorizontalOffset
        {
            get { return (double)this.GetValue(FromHorizontalOffsetProperty); }
            set { this.SetValue(FromHorizontalOffsetProperty, value); }
        }

        #endregion

        #region FromVerticalOffset

        public static readonly DependencyProperty FromVerticalOffsetProperty =
            DependencyProperty.Register("FromVerticalOffset", typeof(double), typeof(EntranceTransitionBehavior),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.None));

        public double FromVerticalOffset
        {
            get { return (double)this.GetValue(FromVerticalOffsetProperty); }
            set { this.SetValue(FromVerticalOffsetProperty, value); }
        }

        #endregion


        #region IsStaggeringEnabled

        public static readonly DependencyProperty IsStaggeringEnabledProperty =
            DependencyProperty.Register("IsStaggeringEnabled", typeof(bool), typeof(EntranceTransitionBehavior),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.None));

        public bool IsStaggeringEnabled
        {
            get { return (bool)this.GetValue(IsStaggeringEnabledProperty); }
            set { this.SetValue(IsStaggeringEnabledProperty, value); }
        }

        #endregion

        #region Duration

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(TimeSpan), typeof(EntranceTransitionBehavior),
                new FrameworkPropertyMetadata(TimeSpan.FromMilliseconds(500),
                        FrameworkPropertyMetadataOptions.None));

        public TimeSpan Duration
        {
            get { return (TimeSpan)this.GetValue(DurationProperty); }
            set { this.SetValue(DurationProperty, value); }
        }

        #endregion

        #region StaggardOverlapRatio

        public static readonly DependencyProperty StaggerdOverlapRatioProperty =
            DependencyProperty.Register("StaggerdOverlapRatio", typeof(double), typeof(EntranceTransitionBehavior),
                new FrameworkPropertyMetadata(0.1,
                    FrameworkPropertyMetadataOptions.None));

        public double StaggerdOverlapRatio
        {
            get { return (double)this.GetValue(StaggerdOverlapRatioProperty); }
            set { this.SetValue(StaggerdOverlapRatioProperty, value); }
        }

        #endregion

        #region IsEnabled

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(EntranceTransitionBehavior),
                new FrameworkPropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject d)
        {
            return (bool)d.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject d, bool value)
        {
            d.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool newIsEnabled = (bool)d.GetValue(IsEnabledProperty);

            var uie = d as UIElement;
            if (uie != null)
            {
                var behColl = Interaction.GetBehaviors(uie);
                var existingBehavior = behColl.OfType<EntranceTransitionBehavior>().FirstOrDefault();
                if (newIsEnabled == false && existingBehavior != null)
                {
                    behColl.Remove(existingBehavior);
                }
                else if (newIsEnabled && existingBehavior == null)
                {
                    behColl.Add(new EntranceTransitionBehavior { IsStaggeringEnabled = true });
                }
            }
        }

        #endregion


        private TranslateTransform RenderTransform
        {
            get
            {
                if (this.cachedRenderTransform != null && ReferenceEquals(this.cachedRenderTransform, this.AssociatedObject.RenderTransform))
                {
                    return this.cachedRenderTransform;
                }
                this.RenderTransform = new TranslateTransform();
                return this.cachedRenderTransform;
            }
            set
            {
                if (this.cachedRenderTransform == value)
                {
                    return;
                }
                this.cachedRenderTransform = value;
                this.AssociatedObject.RenderTransform = value;
            }
        }

        internal void EnsureTransform()
        {
            var transform = this.RenderTransform;
            if (transform == null || transform.IsFrozen)
            {
                this.RenderTransform = transform == null ? new TranslateTransform() : new TranslateTransform(transform.X, transform.Y);
            }
            this.AssociatedObject.RenderTransformOrigin = new Point(0.0, 0.0);
        }

        private void StartStaggardAnimation()
        {
            var parent = VisualTreeHelper.GetParent(this.AssociatedObject);

            var siblingData = this.GetSiblingData(parent as FrameworkElement);

            if (stopwatch.Elapsed - siblingData.LastAnimationStart > TimeSpan.FromMilliseconds(60))
            {
                siblingData.LastStartOffset = TimeSpan.Zero;
            }

            siblingData.LastAnimationStart = stopwatch.Elapsed;
            this.StartAnimation(siblingData.LastStartOffset, this.Duration);
            siblingData.LastStartOffset += (TimeSpan.FromMilliseconds(this.Duration.TotalMilliseconds * this.StaggerdOverlapRatio));
        }


        private void StopAnimation()
        {
            if (this.storyboards == null)
            {
                this.storyboards = new List<Storyboard>();
                return;
            }

            foreach (var storyboard in this.storyboards)
            {
                storyboard.SkipToFill();
            }

            this.storyboards.Clear();
        }

        private void StartAnimation(TimeSpan startOffsetTime, TimeSpan duration)
        {
            if (this.hasAnimated)
            {
                return;
            }
            this.hasAnimated = true;
            this.StopAnimation();
            this.EnsureTransform();
            var horizontalAnimation = new DoubleAnimation(this.FromHorizontalOffset, 0, duration)
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var storyboard = new Storyboard();
            storyboard.Children.Add(horizontalAnimation);
            Storyboard.SetTarget(horizontalAnimation, this.AssociatedObject);
            Storyboard.SetTargetProperty(horizontalAnimation, new PropertyPath("RenderTransform.X"));
            storyboard.BeginTime = startOffsetTime;
            storyboard.Begin();
            this.storyboards.Add(storyboard);
            var veritcalAnimation = new DoubleAnimation(this.FromVerticalOffset, 0, duration)
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            storyboard = new Storyboard();
            storyboard.Children.Add(veritcalAnimation);
            Storyboard.SetTarget(veritcalAnimation, this.AssociatedObject);
            Storyboard.SetTargetProperty(veritcalAnimation, new PropertyPath("RenderTransform.Y"));
            storyboard.BeginTime = startOffsetTime;
            storyboard.Begin();
            this.storyboards.Add(storyboard);

            var opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, TimeSpan.Zero));
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, startOffsetTime, new CubicEase { EasingMode = EasingMode.EaseOut }));
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, duration + startOffsetTime + TimeSpan.FromMilliseconds(this.Duration.TotalMilliseconds / 2), new CubicEase { EasingMode = EasingMode.EaseOut }));

            storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);
            Storyboard.SetTarget(opacityAnimation, this.AssociatedObject);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Begin();
            this.storyboards.Add(storyboard);
        }

        protected override void OnAttached()
        {
            this.AssociatedObject.Loaded += this.AssociatedObject_Loaded;
            this.AssociatedObject.Unloaded += this.AssociatedObject_Unloaded;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.Loaded -= this.AssociatedObject_Loaded;
            this.AssociatedObject.Unloaded -= this.AssociatedObject_Unloaded;

            base.OnDetaching();
        }

        void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(this.AssociatedObject);
            if (parent != null)
            {
                this.hasAnimated = false;
            }

            this.StopAnimation();

            this.lastUnloadTime = stopwatch.Elapsed;
        }

        void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            if (stopwatch.Elapsed - this.lastUnloadTime < TimeSpan.FromMilliseconds(30))
            {
                this.StopAnimation();
                return;
            }
            this.lastUnloadTime = stopwatch.Elapsed;
            this.StopAnimation();
            if (this.IsStaggeringEnabled)
            {
                this.StartStaggardAnimation();
            }
            else
            {
                this.StartAnimation(TimeSpan.Zero, this.Duration);
            }
        }
    }
}