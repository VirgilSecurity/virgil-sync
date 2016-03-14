namespace Virgil.Disk.View
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Windows;
    using ViewModels;
    using System.Windows.Controls;
    using System.Windows.Navigation;


    public static class WebBrowserUtility
    {
        public static readonly DependencyProperty BindableSourceProperty =
            DependencyProperty.RegisterAttached("BindableSource", typeof(string), typeof(WebBrowserUtility), new UIPropertyMetadata(null, BindableSourcePropertyChanged));

        public static string GetBindableSource(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableSourceProperty);
        }

        public static void SetBindableSource(DependencyObject obj, string value)
        {
            obj.SetValue(BindableSourceProperty, value);
        }

        public static void BindableSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser browser = o as WebBrowser;
            if (browser != null)
            {
                string uri = e.NewValue as string;
                if (uri != null)
                {
                    browser.Navigate(new Uri(uri));
                }
            }
        }

        public static void SetZoom(this WebBrowser webBrowser, double zoom)
        {
            mshtml.IHTMLDocument2 doc = webBrowser.Document as mshtml.IHTMLDocument2;
            doc?.parentWindow.execScript("document.body.style.zoom=" + zoom.ToString(CultureInfo.InvariantCulture).Replace(",", ".") + ";");
        }

    }

    /// <summary>
    /// Interaction logic for DropBoxSignIn.xaml
    /// </summary>
    public partial class DropBoxSignInView : UserControl
    {
        public DropBoxSignInView()
        {
            this.InitializeComponent();
        }

        private DropBoxSignInViewModel Model => this.DataContext as DropBoxSignInViewModel;
        
        private void BrowserNavigating(object sender, NavigatingCancelEventArgs e)
        {
            var handleNavigation = new HandleNavigation(e.Uri);
            this.Model?.HandleNavigating(handleNavigation);
            e.Cancel = handleNavigation.Cancel;
        }
    }
}
