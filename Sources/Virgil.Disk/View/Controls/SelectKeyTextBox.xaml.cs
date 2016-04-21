using System.Windows;

namespace Virgil.Sync.View.Controls
{
    using System.Windows.Forms;
    using Infrastructure.Mvvm;
    using Ookii.Dialogs.Wpf;
    using UserControl = System.Windows.Controls.UserControl;

    /// <summary>
    /// Interaction logic for SelectKeyTextBox.xaml
    /// </summary>
    public partial class SelectKeyTextBox : UserControl
    {
        public SelectKeyTextBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SelectedPathProperty = DependencyProperty.Register(
            "SelectedPath", typeof(string), typeof(SelectKeyTextBox), new PropertyMetadata(default(string)));

        public string SelectedPath
        {
            get { return (string)GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); }
        }

        public static readonly DependencyProperty OnFileChangedCommandProperty = DependencyProperty.Register(
            "OnFileChangedCommand", typeof(RelayCommand<string>), typeof(SelectKeyTextBox), new PropertyMetadata(default(RelayCommand<string>)));

        public RelayCommand<string> OnFileChangedCommand
        {
            get { return (RelayCommand<string>)GetValue(OnFileChangedCommandProperty); }
            set { SetValue(OnFileChangedCommandProperty, value); }
        }

        private void ChoseKeyPairOnDisk(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaOpenFileDialog()
            {
                Title = "Select Virgil Card",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                ReadOnlyChecked = true,
                DefaultExt = "*.vcard",
                Filter = "All files (*.*)|*.*|Virgil Card Files (*.vcard)|*.vcard",
                FilterIndex = 2
            };

            if (dialog.ShowDialog() == true)
            {
                this.SelectedPath = dialog.FileName;
                this.OnFileChangedCommand?.Execute(dialog.FileName);
            }
        }
    }
}
