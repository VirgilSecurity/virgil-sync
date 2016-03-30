﻿namespace Virgil.Sync.View
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using Infrastructure;
    using Infrastructure.Messages;
    using Infrastructure.Messaging;
    using ViewModels;

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
    public partial class DropBoxSignInView : UserControl, IHandle<DropboxSignOut>
    {
        public DropBoxSignInView()
        {
            this.InitializeComponent();

            ServiceLocator.Resolve<IEventAggregator>().Subscribe(this);
        }

        private DropBoxSignInViewModel Model => this.DataContext as DropBoxSignInViewModel;
        
        private void BrowserNavigating(object sender, NavigatingCancelEventArgs e)
        {
            var handleNavigation = new HandleNavigation(e.Uri);
            this.Model?.HandleNavigating(handleNavigation);
            e.Cancel = handleNavigation.Cancel;
        }

        public void Handle(DropboxSignOut message)
        {
            //_DeleteEveryCookie(new Uri("https://www.dropbox.com"));

            this.Browser.Navigate("https://www.dropbox.com/logout");
            
            //var oldBrowser = this.BrowserHolder.Child as WebBrowser;
            //if (oldBrowser != null)
            //{
            //    oldBrowser.Navigating -= this.BrowserNavigating;
            //    this.BrowserHolder.Child = null;
            //}

            //var webBrowser = new WebBrowser();
            //webBrowser.Navigating += this.BrowserNavigating;
            //webBrowser.SetBinding(WebBrowserUtility.BindableSourceProperty, new Binding("AuthorizeUri"));
            //this.BrowserHolder.Child = webBrowser;
            //this.Browser = webBrowser;
        }

        private static void _DeleteEveryCookie(Uri url)
        {
            string cookie = string.Empty;
            try
            {
                // Get every cookie (Expiration will not be in this response)
                cookie = Application.GetCookie(url);
            }
            catch (Win32Exception)
            {
                // "no more data is available" ... happens randomly so ignore it.
            }
            if (!string.IsNullOrEmpty(cookie))
            {
                // This may change eventually, but seems quite consistent for Facebook.com.
                // ... they split all values w/ ';' and put everything in foo=bar format.
                string[] values = cookie.Split(';');

                foreach (string s in values)
                {
                    if (s.IndexOf('=') > 0)
                    {
                        // Sets value to null with expiration date of yesterday.
                        _DeleteSingleCookie(s.Substring(0, s.IndexOf('=')).Trim(), url);
                    }
                }
            }
        }

        private static void _DeleteSingleCookie(string name, Uri url)
        {
            try
            {
                // Calculate "one day ago"
                DateTime expiration = DateTime.UtcNow - TimeSpan.FromDays(1);
                // Format the cookie as seen on FB.com.  Path and domain name are important factors here.
                string cookie = String.Format("{0}=; expires={1}; path=/; domain=.dropbox.com", name, expiration.ToString("R"));
                // Set a single value from this cookie (doesnt work if you try to do all at once, for some reason)
                Application.SetCookie(url, cookie);
            }
            catch (Exception exc)
            {
            }
        }
    }
}
