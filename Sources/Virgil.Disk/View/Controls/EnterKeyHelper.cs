namespace Virgil.Sync.View.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public static class EnterKeyHelper
    {
        public static ICommand GetEnterKeyCommand(DependencyObject target)
        {
            return (ICommand)target.GetValue(EnterKeyCommandProperty);
        }

        public static void SetEnterKeyCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(EnterKeyCommandProperty, value);
        }

        public static readonly DependencyProperty EnterKeyCommandProperty =
            DependencyProperty.RegisterAttached(
                "EnterKeyCommand",
                typeof(ICommand),
                typeof(EnterKeyHelper),
                new PropertyMetadata(null, OnEnterKeyCommandChanged));


        public static object GetEnterKeyCommandParam(DependencyObject target)
        {
            return (object)target.GetValue(EnterKeyCommandParamProperty);
        }

        public static void SetEnterKeyCommandParam(DependencyObject target, object value)
        {
            target.SetValue(EnterKeyCommandParamProperty, value);
        }

        public static readonly DependencyProperty EnterKeyCommandParamProperty =
            DependencyProperty.RegisterAttached(
                "EnterKeyCommandParam",
                typeof(object),
                typeof(EnterKeyHelper),
                new PropertyMetadata(null));

        static void OnEnterKeyCommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ICommand command = (ICommand)e.NewValue;
            Control control = (Control)target;
            control.KeyDown += (s, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    var dps = new[]
                    {
                        control.GetBindingExpression(TextBox.TextProperty),
                        control.GetBindingExpression(ConfirmationCodeEdit.ConfirmationCodeProperty),
                        control.GetBindingExpression(SelectFolderTextBox.SelectedPathProperty),
                        control.GetBindingExpression(TransparentTextBox.TextProperty),
                        control.GetBindingExpression(TransparentTextBox.PasswordProperty)
                    };

                    foreach (var bindingExpression in dps)
                    {
                        if (bindingExpression != null)
                        {
                            bindingExpression.UpdateSource();
                        }
                    }
                    
                    object commandParameter = GetEnterKeyCommandParam(target);
                    command.Execute(commandParameter);
                }
            };
        }
    }
}
