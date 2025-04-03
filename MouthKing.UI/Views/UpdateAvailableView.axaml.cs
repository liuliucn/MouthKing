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

    private UpdateManager updateManager = new UpdateManager(new GiteeSource("https://gitee.com/tsdyy/mouth-king", null, false));
    private UpdateInfo? newVersion;

    private async Task CheckForUpdates()
    {
        try
        {
             newVersion = await updateManager.CheckForUpdatesAsync().ConfigureAwait(true);
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
        try
        {
            // download new version
            await updateManager.DownloadUpdatesAsync(newVersion, i =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // update progress bar
                    text.Content = $"下载中: {i}%";
                    if (i >= 99)
                    {
                        text.Content = "下载完成，正在重启应用";
                    }
                });
            });

            // install new version and restart app
            updateManager.ApplyUpdatesAndRestart(newVersion);
        }
        catch (System.Exception ex)
        {
            text.Content = ex.Message;
        }

    }
}