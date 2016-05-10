namespace Virgil.Sync.View
{
    using System;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for Container.xaml
    /// </summary>
    public partial class ContainerWindow : Window
    {
        private readonly double AspectRatio;

        public static ContainerWindow CurrentInstance { get; private set; } = null;

        public ContainerWindow() : base()
        {
            InitializeComponent();
            this.MouseDown += this.OnMouseDown;
            CurrentInstance = this;

            this.AspectRatio = this.Width / this.Height;
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
            CurrentInstance = null;
        }

        private void Minimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        public void PositionWindowOnScreen(double horizontalShift = 0, double verticalShift = 0)
        {
            this.Left = 0 + (SystemParameters.PrimaryScreenWidth - this.ActualWidth) / 2 + horizontalShift;
            this.Top = 0 + (SystemParameters.PrimaryScreenHeight - this.ActualHeight) / 2 + verticalShift;
        }

        private void Thumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            this.Height = Math.Round(Math.Max(this.Height + e.VerticalChange, this.MinHeight));
            this.Width = Math.Round(this.Height * this.AspectRatio);
        }

        private void Thumb_OnDragDeltaTop(object sender, DragDeltaEventArgs e)
        {
            var newHeight = Math.Max(this.Height - e.VerticalChange, this.MinHeight);
            var newWidth = this.Height * this.AspectRatio;

            var heightDelta = this.Height - newHeight;
            var widthDelta = this.Width - newWidth;

            if ((int) (heightDelta - this.Height) != 0)
            {
                this.Top = Math.Round(this.Top + heightDelta);
                this.Left = Math.Round(this.Left + widthDelta);

                this.Height = Math.Round(newHeight);
                this.Width = Math.Round(newWidth);
            }
        }
    }
}
