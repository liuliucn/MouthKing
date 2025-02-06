using Avalonia.Controls;
using Avalonia.Input;
using System.Text;
using MouthKing.UI.ViewModels;
using SharpHook;
using MouthKing.Core;
using SharpHook.Native;
using System.Diagnostics;

namespace MouthKing.UI.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void TextBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        App.IsHotKeyCapturing = false;
        if (DataContext is MainViewModel vm)
        {
            vm.SetHotKey(e);
            e.Handled = true;
        }
    }

    private void TextBox_GotFocus(object? sender, Avalonia.Input.GotFocusEventArgs e)
    {
        App.IsHotKeyCapturing = true;
        if (DataContext is MainViewModel vm)
        {
            vm.CurrentHotkey = vm.HotkeyPlaceholder;
            e.Handled = true;
        }
    }

    private void TextBox_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        App.IsHotKeyCapturing = false;
        if (DataContext is MainViewModel vm)
        {
            vm.CurrentHotkey = ConfigurationManager.Config.Hotkey;
            e.Handled = true;
        }
    }
}
