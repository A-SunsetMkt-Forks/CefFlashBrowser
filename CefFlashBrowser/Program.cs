﻿using CefFlashBrowser.FlashBrowser;
using CefFlashBrowser.Models.Data;
using CefFlashBrowser.Utils;
using CefSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace CefFlashBrowser
{
    internal static class Program
    {
        private static bool _restart = false;

        [STAThread]
        private static void Main()
        {
            Win32.SetDllDirectory(GlobalData.CefDllPath);
            AppDomain.CurrentDomain.AssemblyResolve += ResolveCefSharpAssembly;

            try
            {
                var app = new App();
                app.InitializeComponent();

                GlobalData.InitData();
                LanguageManager.InitLanguage();

                InitCefFlash();
                InitTheme();
                app.Run();
            }
            catch (Exception e)
            {
                WindowManager.Alert(e.ToString(), LanguageManager.GetString("dialog_error"));
            }
            finally
            {
                OnTerminate();
            }
        }

        private static void InitTheme()
        {
            ThemeManager.ChangeTheme(GlobalData.Settings.FollowSystemTheme ? ThemeManager.GetSystemTheme() : GlobalData.Settings.Theme);
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += UserPreferenceChangedHandler;
        }

        private static void UserPreferenceChangedHandler(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
        {
            if (e.Category == Microsoft.Win32.UserPreferenceCategory.General && GlobalData.Settings.FollowSystemTheme)
            {
                var theme = ThemeManager.GetSystemTheme();
                ThemeManager.ChangeTheme(theme);
            }
        }

        private static Assembly ResolveCefSharpAssembly(object sender, ResolveEventArgs e)
        {
            // Load CefSharp dlls
            string assemblyName = new AssemblyName(e.Name).Name;
            string assemblyPath = Path.Combine(GlobalData.CefDllPath, assemblyName + ".dll");
            return File.Exists(assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null;
        }

        private static void InitCefFlash()
        {
            Environment.SetEnvironmentVariable("ComSpec", GlobalData.EmptyExePath); //Remove black popup window

            var settings = new CefFlashSettings()
            {
                Locale = LanguageManager.GetLocale(LanguageManager.CurrentLanguage),
                LogFile = GlobalData.CefLogPath,
                CachePath = GlobalData.CachesPath,
                PpapiFlashPath = GlobalData.FlashPath,
                EnableSystemFlash = true,
                BrowserSubprocessPath = GlobalData.SubprocessPath
            };

            if (GlobalData.Settings.FakeFlashVersionSetting.Enable)
            {
                settings.PpapiFlashVersion = GlobalData.Settings.FakeFlashVersionSetting.FlashVersion;
            }
            else
            {
                settings.PpapiFlashVersion = FileVersionInfo.GetVersionInfo(GlobalData.FlashPath).FileVersion.Replace(',', '.');
            }

            if (GlobalData.Settings.UserAgentSetting.EnableCustom)
            {
                settings.UserAgent = GlobalData.Settings.UserAgentSetting.UserAgent;
            }

            if (GlobalData.Settings.ProxySettings.EnableProxy)
            {
                var proxySettings = GlobalData.Settings.ProxySettings;
                CefSharpSettings.Proxy = new ProxyOptions(proxySettings.IP, proxySettings.Port, proxySettings.UserName, proxySettings.Password);
            }

#if !DEBUG
            settings.LogSeverity = LogSeverity.Error;
#endif

            settings.CefCommandLineArgs["autoplay-policy"] = "no-user-gesture-required";
            Cef.Initialize(settings);
        }

        private static void OnTerminate()
        {
            GlobalData.SaveData();
            Cef.Shutdown();

            if (_restart)
            {
                Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            }
        }

        public static void Restart()
        {
            _restart = true;
            Application.Current.Shutdown();
        }
    }
}
