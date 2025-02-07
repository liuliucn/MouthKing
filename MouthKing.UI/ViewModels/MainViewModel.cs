using Avalonia.Input;
using MouthKing.Core;
using ReactiveUI;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace MouthKing.UI.ViewModels;


public class MainViewModel : ViewModelBase
{

    public MainViewModel()
    {
        var config = ConfigurationManager.Config;
        _currentHotkey = config.Hotkey;
        _selectedLanguage = Languages.FirstOrDefault(x => x.Key == config.ToLanguage);
        _selectedGame = Games.FirstOrDefault(x => x.Key == config.Game);
        _selectedAiModel = AiModels.FirstOrDefault(x => x.Key == config.AiModel);
        _isShort = config.IsShort;
        _isNoTranslate = config.IsNoTranslate;
    }

    private ObservableCollection<KeyValuePair<string, string>> _aiModels = new ObservableCollection<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("deepseek-ai/DeepSeek-V3", "DeepSeek V3"),
        new KeyValuePair<string, string>("deepseek-ai/DeepSeek-R1", "DeepSeek R1"),
    };
    private KeyValuePair<string, string> _selectedAiModel;
    public ObservableCollection<KeyValuePair<string, string>> AiModels
    {
        get => _aiModels;
        set => this.RaiseAndSetIfChanged(ref _aiModels, value);
    }

    public KeyValuePair<string, string> SelectedAiModel
    {
        get => _selectedAiModel;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedAiModel, value);
            // 处理选择变化
            HandleAiModelSelection(value);
        }
    }

    private static void HandleAiModelSelection(KeyValuePair<string, string> value)
    {
        ConfigurationManager.Config.AiModel = value.Key;
        ConfigurationManager.SaveConfig();
    }

    private ObservableCollection<KeyValuePair<string, string>> _languages = new ObservableCollection<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("英语", "英语"),
        new KeyValuePair<string, string>("东南亚英语", "东南亚英语"),
        new KeyValuePair<string, string>("中文", "中文"),
    };
    private KeyValuePair<string, string> _selectedLanguage;
    public ObservableCollection<KeyValuePair<string, string>> Languages
    {
        get => _languages;
        set => this.RaiseAndSetIfChanged(ref _languages, value);
    }

    public KeyValuePair<string, string> SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
            // 处理选择变化
            HandleLanguageSelection(value);
        }
    }

    private static void HandleLanguageSelection(KeyValuePair<string, string> value)
    {
        ConfigurationManager.Config.ToLanguage = value.Key;
        ConfigurationManager.SaveConfig();
    }

    private ObservableCollection<KeyValuePair<string, string>> _games = new ObservableCollection<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("Dota2", "Dota2"),
        new KeyValuePair<string, string>("lol", "英雄联盟"),
        new KeyValuePair<string, string>("csgo", "CS:GO"),
        new KeyValuePair<string, string>("World of warcraft", "WoW"),

    };
    private KeyValuePair<string, string> _selectedGame;
    public ObservableCollection<KeyValuePair<string, string>> Games
    {
        get => _games;
        set => this.RaiseAndSetIfChanged(ref _games, value);
    }

    public KeyValuePair<string, string> SelectedGame
    {
        get => _selectedGame;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedGame, value);
            // 处理选择变化
            HandleGameSelection(value);
        }
    }

    private static void HandleGameSelection(KeyValuePair<string, string> value)
    {
        ConfigurationManager.Config.Game = value.Key;
        ConfigurationManager.SaveConfig();
    }

    public string HotkeyPlaceholder = "按下新的快捷键组合";

    private string _currentHotkey;

    public string CurrentHotkey
    {
        get => _currentHotkey;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentHotkey, value);
            if (value == HotkeyPlaceholder) return;
            ConfigurationManager.Config.Hotkey = value;
            ConfigurationManager.SaveConfig();
        }
    }

    private bool _isShort;

    public bool IsShort
    {
        get => _isShort;
        set
        {
            this.RaiseAndSetIfChanged(ref _isShort, value);
            ConfigurationManager.Config.IsShort = value;
            ConfigurationManager.SaveConfig();
        }
    }

    private bool _isNoTranslate;

    public bool IsNoTranslate
    {
        get => _isNoTranslate;
        set
        {
            this.RaiseAndSetIfChanged(ref _isNoTranslate, value);
            ConfigurationManager.Config.IsNoTranslate = value;
            ConfigurationManager.SaveConfig();
        }
    }

    public void SetHotKey(KeyEventArgs e)
    {
        var key = e.Key;
        var modifiers = e.KeyModifiers;

        if (key == Key.Escape)
        {
            CurrentHotkey = ConfigurationManager.Config.Hotkey;
        }

        if (modifiers == KeyModifiers.None || key == Key.None
            || key == Key.LeftShift || key == Key.RightShift
            || key == Key.LeftAlt || key == Key.RightAlt
            || key == Key.LeftCtrl || key == Key.RightCtrl
            || key == Key.LWin || key == Key.RWin
            )
        {
            return;
        }

        var hotKeyString = "";

        if (modifiers.HasFlag(KeyModifiers.Control))
            hotKeyString += "Ctrl + ";
        if (modifiers.HasFlag(KeyModifiers.Shift))
            hotKeyString += "Shift + ";
        if (modifiers.HasFlag(KeyModifiers.Alt))
            hotKeyString += "Alt + ";
        if (modifiers.HasFlag(KeyModifiers.Meta))
            hotKeyString += OperatingSystem.IsWindows() ? "Win + " : OperatingSystem.IsMacOS() ? "Command + " : "Super + ";

        if (!keyMappings.TryGetValue(e.PhysicalKey, out var physicalKey))
            physicalKey = e.PhysicalKey.ToString();
        hotKeyString += physicalKey;

        CurrentHotkey = hotKeyString;
    }

    private static readonly Dictionary<PhysicalKey, string> keyMappings = new()
    {
         { PhysicalKey.Digit0, "0" },
         { PhysicalKey.Digit1, "1" },
         { PhysicalKey.Digit2, "2" },
         { PhysicalKey.Digit3, "3" },
         { PhysicalKey.Digit4, "4" },
         { PhysicalKey.Digit5, "5" },
         { PhysicalKey.Digit6, "6" },
         { PhysicalKey.Digit7, "7" },
         { PhysicalKey.Digit8, "8" },
         { PhysicalKey.Digit9, "9" },
         { PhysicalKey.Minus, "-" },
         { PhysicalKey.Equal, "=" },
         { PhysicalKey.Backquote, "`" },
         { PhysicalKey.BracketLeft, "[" },
         { PhysicalKey.BracketRight, "]" },
         { PhysicalKey.Semicolon, ";" },
         { PhysicalKey.Quote, "'" },
         { PhysicalKey.Comma, "," },
         { PhysicalKey.Period, "." },
         { PhysicalKey.Slash, "/" },
         { PhysicalKey.Backslash, "\\" },
    };
}
