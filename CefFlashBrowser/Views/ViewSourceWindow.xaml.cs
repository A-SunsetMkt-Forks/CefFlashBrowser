﻿using CefFlashBrowser.FlashBrowser.Handlers;
using CefFlashBrowser.Models.Data;
using CefFlashBrowser.Utils;
using CefSharp;
using SimpleMvvm.Messaging;
using System.ComponentModel;
using System.Windows;

namespace CefFlashBrowser.Views
{
    /// <summary>
    /// ViewSourceWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ViewSourceWindow : Window
    {
        private class ViewSourceBrowserLifeSpanHandler : LifeSpanHandler
        {
            public override bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
            {
                Application.Current.Dispatcher.Invoke(() => WindowManager.ShowBrowser(targetUrl));
                newBrowser = null;
                return true;
            }
        }


        public string Address
        {
            get { return (string)GetValue(AddressProperty); }
            set { SetValue(AddressProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Address.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddressProperty =
            DependencyProperty.Register("Address", typeof(string), typeof(ViewSourceWindow), new PropertyMetadata(null, (d, e) =>
            {
                ((ViewSourceWindow)d).browser.Address = $"view-source:{e.NewValue}";
            }));


        public ViewSourceWindow()
        {
            InitializeComponent();

            browser.DragHandler = new Utils.Handlers.DisableDragHandler();
            browser.MenuHandler = new Utils.Handlers.ContextMenuHandler();
            browser.JsDialogHandler = new Utils.Handlers.JsDialogHandler();
            browser.DownloadHandler = new Utils.Handlers.DownloadHandler();
            browser.LifeSpanHandler = new ViewSourceBrowserLifeSpanHandler();

            Messenger.Global.Register(MessageTokens.CLOSE_ALL_BROWSERS, CloseBrowserHandler);
            Closed += delegate { Messenger.Global.Unregister(MessageTokens.CLOSE_ALL_BROWSERS, CloseBrowserHandler); };
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!browser.IsDisposed)
            {
                browser.CloseBrowser(true);
            }
            base.OnClosing(e);
        }

        private void CloseBrowserHandler(object msg)
        {
            Close();
        }
    }
}
