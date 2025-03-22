using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using Velopack;

namespace MouthKing.UI.Views;

public partial class UpdateAvailableView : UserControl
{
    public UpdateAvailableView()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        text.Content = "sss";
    }

    private async void Button_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var mgr = new UpdateManager(new GiteeSource("https://gitee.com/tsdyy/mouth-king", null, false));
        var newVersion = await mgr.CheckForUpdatesAsync();
        if (newVersion == null)
            return; // no update available

        try
        {
            // download new version
            await mgr.DownloadUpdatesAsync(newVersion, i =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // update progress bar
                    text.Content = $"Downloading: {i}%";
                });
            });

            // install new version and restart app
            mgr.ApplyUpdatesAndRestart(newVersion);
        }
        catch (System.Exception ex)
        {
            text.Content = ex.Message;
        }
       
    }
}