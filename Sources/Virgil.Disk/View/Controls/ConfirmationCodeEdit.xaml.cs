namespace Virgil.Sync.View.Controls
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for ConfirmationCode.xaml
    /// </summary>
    public partial class ConfirmationCodeEdit : UserControl
    {
        private readonly TextBox[] textBoxes;

        public ConfirmationCodeEdit()
        {
            this.InitializeComponent();

            var transparentTextBoxes = this.Controls.Children.OfType<TransparentTextBox>().ToArray();
            this.textBoxes = transparentTextBoxes.Select(it => it.TextBox).ToArray();
            
            for (var i = 0; i < this.textBoxes.Length; i++)
            {
                var control = this.textBoxes[i];
                control.CharacterCasing = CharacterCasing.Upper;
                control.MaxLength = 1;

                var next = this.textBoxes.ElementAtOrDefault(i + 1);
                var prev = this.textBoxes.ElementAtOrDefault(i - 1);
                    
                control.KeyUp += (sender, e) =>
                {
                    if (Keyboard.Modifiers == ModifierKeys.None)
                    {
                        if (e.Key == Key.Back)
                        {
                            control.Clear();
                            this.UpdateCode();
                            if (prev != null)
                            {
                                prev.Focus();
                                prev.CaretIndex = 2;
                            }
                        }

                        if (e.Key == Key.Delete)
                        {
                            control.Clear();
                            this.UpdateCode();
                            if (next != null)
                            {
                                next.Focus();
                                next.CaretIndex = 2;
                            }
                        }

                        if (e.Key == Key.Home)
                        {
                            var first = this.textBoxes[0];
                            first.Focus();
                            first.CaretIndex = 2;

                        }

                        if (e.Key == Key.End)
                        {
                            var last = this.textBoxes.Last();
                            last.Focus();
                            last.CaretIndex = 2;
                        }

                        if (e.Key == Key.Left)
                        {
                            if (prev != null)
                            {
                                prev.Focus();
                                prev.CaretIndex = 2;
                            }
                        }

                        if (e.Key == Key.Right)
                        {
                            if (next != null)
                            {
                                next.Focus();
                                next.CaretIndex = 2;
                            }
                        }

                        if ((e.Key >= Key.A && e.Key <= Key.Z) ||
                            (e.Key >= Key.D0 && e.Key <= Key.D9) ||
                            (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
                        {
                            control.Text = e.Key.ToString().Last().ToString();
                            if (next != null)
                            {
                                next.Focus();
                                next.CaretIndex = 2;
                            }
                            this.UpdateCode();
                        }
                    }
                };

                control.LostFocus += (sender, e) =>
                {
                    this.IsTextBoxFocused = transparentTextBoxes.Any(it => it.IsTextBoxFocused);
                };

                control.GotFocus += (sender, e) =>
                {
                    this.IsTextBoxFocused = transparentTextBoxes.Any(it => it.IsTextBoxFocused);
                };

                DataObject.AddPastingHandler(control, (sender, e) =>
                {
                    var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
                    if (!isText) return;

                    var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;

                    this.SetValues(text);
                    this.UpdateCode();
                    this.textBoxes.LastOrDefault()?.Focus();
                });
            }
        }

        private void UpdateCode()
        {
            this.ConfirmationCode = this.textBoxes.Aggregate("", (code, tb) => code + tb.Text);
        }

        private void SetValues(string text)
        {
            var substring = text?.Trim().ToUpperInvariant() ?? "";
            
            for (int idx = 0; idx < this.textBoxes.Length; idx++)
            {
                var elementAtOrDefault = substring.ElementAtOrDefault(idx);
                if (elementAtOrDefault == default(char))
                {
                    this.textBoxes[idx].Text = "";
                }
                else
                {
                    this.textBoxes[idx].Text = elementAtOrDefault.ToString();
                }
            }
        }

        public static readonly DependencyProperty ConfirmationCodeProperty = DependencyProperty.Register(
            "ConfirmationCode", typeof (string), typeof (ConfirmationCodeEdit), new PropertyMetadata("", (o, args) =>
            {
                var @this = (ConfirmationCodeEdit) o;
                var text = args.NewValue?.ToString() ?? "";
                if (text == "")
                {
                    @this.SetValues(text);
                }
            }));

        public string ConfirmationCode
        {
            get
            {
                return (string) this.GetValue(ConfirmationCodeProperty);
            }
            set
            {
                this.SetValue(ConfirmationCodeProperty, value);
            }
        }

        public static readonly DependencyProperty IsTextBoxFocusedProperty = DependencyProperty.Register(
            "IsTextBoxFocused", typeof (bool), typeof (ConfirmationCodeEdit), new PropertyMetadata(default(bool)));

        public bool IsTextBoxFocused
        {
            get { return (bool) this.GetValue(IsTextBoxFocusedProperty); }
            set { this.SetValue(IsTextBoxFocusedProperty, value); }
        }
    }
}
