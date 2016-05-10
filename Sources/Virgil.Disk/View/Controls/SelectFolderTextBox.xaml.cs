namespace Virgil.Sync.View.Controls
{
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using Infrastructure.Mvvm;
    using Ookii.Dialogs.Wpf;

    /// <summary>
    /// Interaction logic for SelectFolderTextBox.xaml
    /// </summary>
    public partial class SelectFolderTextBox : UserControl
    {
        public SelectFolderTextBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SelectedPathProperty = DependencyProperty.Register(
            "SelectedPath", typeof (string), typeof (SelectFolderTextBox), new PropertyMetadata(default(string)));

        public string SelectedPath
        {
            get { return (string) this.GetValue(SelectedPathProperty); }
            set { this.SetValue(SelectedPathProperty, value); }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog {SelectedPath = this.SelectedPath};
            if (dialog.ShowDialog() == true)
            {
                this.SelectedPath = dialog.SelectedPath;
                this.OnFolderChangedCommand?.Execute(dialog.SelectedPath);
            }
        }

        public static readonly DependencyProperty OnFolderChangedCommandProperty = DependencyProperty.Register(
            "OnFolderChangedCommand", typeof (RelayCommand<string>), typeof (SelectFolderTextBox), new PropertyMetadata(default(RelayCommand<string>)));

        public RelayCommand<string> OnFolderChangedCommand
        {
            get { return (RelayCommand<string>) this.GetValue(OnFolderChangedCommandProperty); }
            set { this.SetValue(OnFolderChangedCommandProperty, value); }
        }

        private void OpenFolderInExplorer(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(this.SelectedPath))
            {
                Process.Start(this.SelectedPath);
            }
        }
    }
}
