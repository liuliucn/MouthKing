using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Remote.Protocol.Input;
using MouthKing.Core;
using MouthKing.UI.ViewModels;
using MouthKing.UI.Views;
using SharpHook;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouthKing.UI;

public partial class App : Application
{

    public static bool IsHotKeyCapturing { get; set; } = false;

    private TaskPoolGlobalHook _sharpHook;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public override void OnFrameworkInitializationCompleted()
    {
        var config = ConfigurationManager.LoadConfig();

        _sharpHook = new TaskPoolGlobalHook(globalHookType: GlobalHookType.Keyboard, runAsyncOnBackgroundThread: true);
        _sharpHook.KeyPressed += OnKeyReleased;
        _sharpHook.RunAsync();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
            desktop.ShutdownRequested += Desktop_ShutdownRequested;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Desktop_ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        CleanUpBeforeExit();
    }

    private void CleanUpBeforeExit()
    {
        _sharpHook?.Dispose();
    }

    private void SimulateKey(KeyCode key)
    {
        var simulator = new EventSimulator();
        // Press Ctrl+A
        simulator.SimulateKeyPress(KeyCode.VcLeftControl);
        simulator.SimulateKeyPress(key);
        // Release Ctrl+A
        simulator.SimulateKeyRelease(key);
        simulator.SimulateKeyRelease(KeyCode.VcLeftControl);
        System.Threading.Thread.Sleep(100);
    }

    private static readonly Dictionary<KeyCode, string> keyMappings = new()
        { 
            { KeyCode.VcMinus, "-" },
            { KeyCode.VcEquals, "=" },
            { KeyCode.VcComma, "," },
            { KeyCode.VcPeriod, "." },
            { KeyCode.VcSemicolon, ";" },
            { KeyCode.VcSlash, "/" },
            { KeyCode.VcBackQuote, "`" },
            { KeyCode.VcOpenBracket, "[" },
            { KeyCode.VcBackslash, "\\" },
            { KeyCode.VcCloseBracket, "]" },
            { KeyCode.VcQuote, "'" },
             
        };

    private async void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        if (IsHotKeyCapturing)
            return;

        var modifiers = e.RawEvent.Mask;
        var key = e.Data.KeyCode;
        if (modifiers == ModifierMask.None
        || key == KeyCode.VcLeftShift || key == KeyCode.VcRightShift
        || key == KeyCode.VcLeftAlt || key == KeyCode.VcRightAlt
        || key == KeyCode.VcLeftControl || key == KeyCode.VcRightControl
        || key == KeyCode.VcLeftMeta || key == KeyCode.VcRightMeta
       )
        {
            return;
        }

        var hotkey = ConfigurationManager.Config.Hotkey;
        var str = new StringBuilder();
        if ((e.RawEvent.Mask & ModifierMask.Ctrl) > 0)
        {
            str.Append("Ctrl + ");
        }

        if ((e.RawEvent.Mask & ModifierMask.Shift) > 0)
        {
            str.Append("Shift + ");
        }

        if ((e.RawEvent.Mask & ModifierMask.Alt) > 0)
        {
            str.Append("Alt + ");
        }

        if ((e.RawEvent.Mask & ModifierMask.Meta) > 0)
        {
            str.Append(OperatingSystem.IsWindows() ? "Win + " : OperatingSystem.IsMacOS() ? "Command + " : "Super + ");
        }

        if (!keyMappings.TryGetValue(e.Data.KeyCode, out var physicalKey))
            physicalKey = e.Data.KeyCode.ToString().Replace("Vc", "");

        str.Append(physicalKey);

        if (str.ToString() == hotkey)
        {
            SimulateKey(KeyCode.VcA);
            SimulateKey(KeyCode.VcC);

            var text = await GetClipboardTextAsync();

            await SetClipboardTextAsync("[嘴强]翻译中，请等待...");
            SimulateKey(KeyCode.VcA);
            SimulateKey(KeyCode.VcV);

            var settings = new TranslationService.TranslationSettings
            {
                TranslationFrom = "zh",
                TranslationTo = ConfigurationManager.Config.ToLanguage,
                GameScene = ConfigurationManager.Config.Game,
                IsShort = ConfigurationManager.Config.IsShort,
                ModelType = ConfigurationManager.Config.AiModel
            };

            var result = await TranslationService.TranslateWithGpt(settings, text);

            await SetClipboardTextAsync(result);

            SimulateKey(KeyCode.VcA);
            SimulateKey(KeyCode.VcV);
        }
    }

    private async Task SetClipboardTextAsync(string? text)
    {
        // For learning purposes, we opted to directly get the reference
        // for StorageProvider APIs here inside the ViewModel. 

        // For your real-world apps, you should follow the MVVM principles
        // by making service classes and locating them with DI/IoC.

        // See DepInject project for a sample of how to accomplish this.
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.Clipboard is not { } provider)
            throw new NullReferenceException("Missing Clipboard instance.");

        await provider.SetTextAsync(text);
    }

    private async Task<string?> GetClipboardTextAsync()
    {
        // For learning purposes, we opted to directly get the reference
        // for StorageProvider APIs here inside the ViewModel. 

        // For your real-world apps, you should follow the MVVM principles
        // by making service classes and locating them with DI/IoC.

        // See DepInject project for a sample of how to accomplish this.
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.Clipboard is not { } provider)
            throw new NullReferenceException("Missing Clipboard instance.");

        return await provider.GetTextAsync();
    }
}
