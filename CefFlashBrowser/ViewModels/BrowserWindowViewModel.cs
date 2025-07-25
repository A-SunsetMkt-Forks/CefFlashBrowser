﻿using CefFlashBrowser.Models.Data;
using CefFlashBrowser.Utils;
using CefSharp;
using IWshRuntimeLibrary;
using SimpleMvvm;
using SimpleMvvm.Command;
using SimpleMvvm.Messaging;
using System;
using System.Diagnostics;
using System.IO;

namespace CefFlashBrowser.ViewModels
{
    public class BrowserWindowViewModel : ViewModelBase
    {
        public DelegateCommand ShowMainWindowCommand { get; set; }
        public DelegateCommand ViewSourceCommand { get; set; }
        public DelegateCommand OpenInDefaultBrowserCommand { get; set; }
        public DelegateCommand CreateShortcutCommand { get; set; }
        public DelegateCommand AddFavoriteCommand { get; set; }
        public DelegateCommand ReloadOrStopCommand { get; set; }
        public DelegateCommand OpenInSwfPlayerCommand { get; set; }
        public DelegateCommand NewBrowserWindowCommand { get; set; }
        public DelegateCommand ToggleDevToolsCommand { get; set; }

        private string _address = "about:blank";
        public string Address
        {
            get => _address;
            set => UpdateValue(ref _address, value);
        }

        private IntPtr _devtoolsHandle = IntPtr.Zero;
        public IntPtr DevToolsHandle
        {
            get => _devtoolsHandle;
            set => UpdateValue(ref _devtoolsHandle, value);
        }

        public void ShowMainWindow()
        {
            WindowManager.ShowMainWindow();
        }

        public void ViewSource(string url)
        {
            WindowManager.ShowViewSourceWindow(url);
        }

        public void OpenInDefaultBrowser(string url)
        {
            Process.Start(url);
        }

        private static string GetWebBrowserTitle(IWebBrowser browser)
        {
            try
            {
                if (browser is WinformCefSharp4WPF.IWpfWebBrowser b)
                {
                    return b.Title;
                }
                else
                {
                    var frame = browser.GetBrowser().MainFrame;
                    var result = frame.EvaluateScriptAsync("document.title", timeout: TimeSpan.FromSeconds(1)).Result;
                    return result.Result.ToString();
                }
            }
            catch (Exception e)
            {
                LogHelper.LogError("Failed to get web browser title", e);
                return string.Empty;
            }
        }

        public void CreateShortcut(IWebBrowser browser)
        {
            var title = GetWebBrowserTitle(browser);

            foreach (var item in Path.GetInvalidFileNameChars())
            {
                title = title.Replace(item, '_');
            }

            var sfd = new Microsoft.Win32.SaveFileDialog()
            {
                FileName = title,
                Filter = $"{LanguageManager.GetString("common_shortcut")}|*.lnk",
            };

            if (sfd.ShowDialog() == true)
            {
                var path = GetType().Assembly.Location;
                var arg = browser.Address;
                var fileName = sfd.FileName;

                try
                {
                    WshShortcut shortcut = new WshShell().CreateShortcut(fileName);
                    shortcut.TargetPath = path;
                    shortcut.Arguments = arg;
                    shortcut.Save();
                }
                catch (Exception e)
                {
                    LogHelper.LogError($"Failed to create shortcut: {fileName}", e);
                    WindowManager.ShowError(e.Message);
                }
            }
        }

        public void AddFavorite(IWebBrowser browser)
        {
            WindowManager.ShowAddFavoriteDialog(GetWebBrowserTitle(browser), browser.Address);
        }

        public void ReloadOrStop(IWebBrowser browser)
        {
            if (browser.IsLoading)
            {
                browser.Stop();
            }
            else
            {
                browser.Reload();
            }
        }

        public void OpenInSwfPlayer(string url)
        {
            WindowManager.ShowSwfPlayer(url);
        }

        public void NewBrowserWindow(string url)
        {
            WindowManager.ShowBrowser(url ?? "about:blank");
        }

        public void ToggleDevTools(IWebBrowser browser)
        {
            if (browser != null)
            {
                if (DevToolsHandle == IntPtr.Zero
                    && HwndHelper.FindNotIntegratedDevTools(browser) == IntPtr.Zero)
                {
                    browser.ShowDevTools();
                    Messenger.Global.Send(MessageTokens.DEVTOOLS_OPENED, browser);
                }
                else
                {
                    DevToolsHandle = IntPtr.Zero;
                    browser.CloseDevTools();
                    Messenger.Global.Send(MessageTokens.DEVTOOLS_CLOSED, browser);
                }
            }
        }

        public BrowserWindowViewModel()
        {
            ShowMainWindowCommand = new DelegateCommand(ShowMainWindow);
            ViewSourceCommand = new DelegateCommand<string>(ViewSource);
            OpenInDefaultBrowserCommand = new DelegateCommand<string>(OpenInDefaultBrowser);
            CreateShortcutCommand = new DelegateCommand<IWebBrowser>(CreateShortcut);
            AddFavoriteCommand = new DelegateCommand<IWebBrowser>(AddFavorite);
            ReloadOrStopCommand = new DelegateCommand<IWebBrowser>(ReloadOrStop);
            OpenInSwfPlayerCommand = new DelegateCommand<string>(OpenInSwfPlayer);
            NewBrowserWindowCommand = new DelegateCommand<string>(NewBrowserWindow);
            ToggleDevToolsCommand = new DelegateCommand<IWebBrowser>(ToggleDevTools);
        }
    }
}
