namespace Virgil.Sync.View
{
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    /// <summary>
    /// Interaction logic for CreateAccountView.xaml
    /// </summary>
    public partial class CreateAccountView : UserControl
    {
        public CreateAccountView()
        {
            InitializeComponent();
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            var navigateUri = (sender as Hyperlink)?.NavigateUri?.ToString();
            if (navigateUri != null)
                Process.Start(navigateUri);
        }
    }
}
