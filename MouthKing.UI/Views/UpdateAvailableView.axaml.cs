using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;
using Velopack;

namespace MouthKing.UI.Views;

public partial class UpdateAvailableView : UserControl
{
    public UpdateAvailableView()
    {
        InitializeComponent();

        CheckForUpdates();

    }

    private async Task CheckForUpdates()
    {
        try
        {
            var mgr = new UpdateManager(new GiteeSource("https://gitee.com/tsdyy/mouth-king", null, false));
            var newVersion = await mgr.CheckForUpdatesAsync().ConfigureAwait(true);
            if (newVersion != null)
            {
                this.IsVisible = true;
                text.Content = $"发现当前有新版本{newVersion.TargetFullRelease.Version}可用";
            }
        }
        catch (Exception ex)
        {

        }

    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.IsVisible = false;
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
                    text.Content = $"下载中: {i}%";
                    if (i == 100)
                    {
                        text.Content = "下载完成，正在重启应用";
                    }
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