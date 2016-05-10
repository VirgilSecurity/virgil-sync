namespace Virgil.Sync.View
{
    using System.Windows;
    using System.Windows.Controls;
    using ViewModels;

    /// <summary>
    /// Interaction logic for FolderSettings.xaml
    /// </summary>
    public partial class FolderSettingsView : UserControl
    {
        public FolderSettingsView()
        {
            this.InitializeComponent();

            this.UpdateAnimation(false);
        }

        public static readonly DependencyProperty SettingsStageProperty = DependencyProperty.Register(
            "SettingsStage", typeof (SettingsStage), typeof (FolderSettingsView), new PropertyMetadata(SettingsStage.None,
                PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            var @this = (FolderSettingsView) source;
            var state = (SettingsStage) args.NewValue;

            switch (state)
            {
                case SettingsStage.IntroStart:
                    VisualStateManager.GoToState(@this, "Initial", false);
                    break;
                case SettingsStage.IntroContinue:
                    VisualStateManager.GoToState(@this, "Second", false);
                    break;
                case SettingsStage.FullView:
                    VisualStateManager.GoToState(@this, "Third", false);
                    break;
            }
        }

        public SettingsStage SettingsStage
        {
            get { return (SettingsStage) this.GetValue(SettingsStageProperty); }
            set { this.SetValue(SettingsStageProperty, value); }
        }

        public static readonly DependencyProperty UseAnimationProperty = DependencyProperty.Register(
            "UseAnimation", typeof (bool), typeof (FolderSettingsView), new PropertyMetadata(false, ChangedCallback));

        const int SpeedRatio = 1;

        private static void ChangedCallback(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            var @this = (FolderSettingsView)source;
            var useAnimation = (bool)args.NewValue;

            @this.UpdateAnimation(useAnimation);
        }

        private void UpdateAnimation(bool useAnimation)
        {
            if (useAnimation)
            {
                this.StoryboardSecond.SpeedRatio = SpeedRatio;
                this.StoryboardThird.SpeedRatio = SpeedRatio;
            }
            else
            {
                this.StoryboardSecond.SpeedRatio = double.MaxValue;
                this.StoryboardThird.SpeedRatio = double.MaxValue;
            }
        }

        public bool UseAnimation
        {
            get { return (bool) this.GetValue(UseAnimationProperty); }
            set { this.SetValue(UseAnimationProperty, value); }
        }
    }
}
