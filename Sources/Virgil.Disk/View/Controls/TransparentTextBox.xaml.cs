namespace Virgil.Sync.View.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class TransparentTextBox : UserControl
    {
        public TransparentTextBox()
        {
            this.InitializeComponent();
            this.TextBox.SelectionChanged += (sender, e) => this.MoveCustomCaret();
            this.TextBox.LostFocus += (sender, e) =>
            {
                this.Caret.Visibility = Visibility.Collapsed;
                this.IsTextBoxFocused = false;
            };

            this.TextBox.GotFocus += (sender, e) =>
            {
                this.Caret.Visibility = Visibility.Visible;
                this.IsTextBoxFocused = true;
            };

            this.Loaded += (sender, args) =>
            {
                if (this.IsPassword)
                {
                    this.TextBox.TextChanged += this.OnTextChanged;
                }
            };
        }
      

        public bool IsPassword { get; set; }

        public static readonly DependencyProperty IsTextBoxFocusedProperty = DependencyProperty.Register(
            "IsTextBoxFocused", typeof (bool), typeof (TransparentTextBox), new PropertyMetadata(default(bool)));

        public bool IsTextBoxFocused
        {
            get { return (bool) this.GetValue(IsTextBoxFocusedProperty); }
            set { this.SetValue(IsTextBoxFocusedProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof (string), typeof (TransparentTextBox), new PropertyMetadata(default(string)));

        public string Text
        {
            get { return (string) this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
            "Password", typeof (string), typeof (TransparentTextBox), new PropertyMetadata(default(string)));

        public string Password
        {
            get { return (string) this.GetValue(PasswordProperty); }
            set
            {
                this.SetValue(PasswordProperty, value);
                this.BaseText = new string(this.PWD_CHAR, value.Length);
            }
        }

        public static readonly DependencyProperty HintProperty = DependencyProperty.Register(
            "Hint", typeof (string), typeof (TransparentTextBox), new PropertyMetadata(default(string)));

        public string Hint
        {
            get { return (string) this.GetValue(HintProperty); }
            set { this.SetValue(HintProperty, value); }
        }

        public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register(
            "MaxLength", typeof (int), typeof (TransparentTextBox), new PropertyMetadata(0));

        public int MaxLength
        {
            get { return (int) this.GetValue(MaxLengthProperty); }
            set { this.SetValue(MaxLengthProperty, value); }
        }
       
        /// <summary>
        /// Moves the custom caret on the canvas.
        /// </summary>
        private void MoveCustomCaret()
        {
            var caretLocation = this.TextBox.GetRectFromCharacterIndex(this.TextBox.CaretIndex).Location;

            if (!double.IsInfinity(caretLocation.X))
            {
                Canvas.SetLeft(this.Caret, caretLocation.X);
            }

            if (!double.IsInfinity(caretLocation.Y))
            {
                Canvas.SetTop(this.Caret, caretLocation.Y + 5);
            }
        }

        // Fake char to display in Visual Tree
        private char PWD_CHAR = '*';

        /// <summary>
        /// Only copy of real password
        /// </summary>
        /// <remarks>For more security use System.Security.SecureString type instead</remarks>
        private string _password = string.Empty;

        /// <summary>
        /// TextChanged event handler for secure storing of password into Visual Tree,
        /// text is replaced with PWD_CHAR chars, clean text is keept into
        /// Text property (CLR property not snoopable without mod)   
        /// </summary>
        protected void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.dirtyBaseText == true)
                return;

            string currentText = this.BaseText;

            int selStart = this.TextBox.SelectionStart;
            if (currentText.Length < this._password.Length)
            {
                // Remove deleted chars          
                this._password = this._password.Remove(selStart, this._password.Length - currentText.Length);
            }
            if (!string.IsNullOrEmpty(currentText))
            {
                for (int i = 0; i < currentText.Length; i++)
                {
                    if (currentText[i] != this.PWD_CHAR)
                    {
                        // Replace or insert char
                        if (this.BaseText.Length == this._password.Length)
                        {
                            this._password = this._password.Remove(i, 1).Insert(i, currentText[i].ToString());
                        }
                        else
                        {
                            this._password = this._password.Insert(i, currentText[i].ToString());
                        }
                    }
                }
                this.BaseText = new string(this.PWD_CHAR, this._password.Length);
                this.TextBox.SelectionStart = selStart;
            }

            this.Password = this._password;
        }

        // flag used to bypass OnTextChanged
        private bool dirtyBaseText;

        /// <summary>
        /// Provide access to base.Text without call OnTextChanged 
        /// </summary>
        private string BaseText
        {
            get { return this.TextBox.Text; }
            set
            {
                this.dirtyBaseText = true;
                this.TextBox.Text = value;
                this.dirtyBaseText = false;
            }
        }
    }
}
