using Avalonia.Controls;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using NetSparkleUpdater;
using System;

namespace MouthKing.UI.Views;

public partial class MainWindow : Window
{
    private SparkleUpdater _sparkle;

    public MainWindow()
    {
        InitializeComponent();

        var appcastUrl = "https://gitee.com/tsdyy/mouth-king/raw/files/appcast.xml";
        if (OperatingSystem.IsMacOS())
            appcastUrl = "https://gitee.com/tsdyy/mouth-king/raw/files/appcast-macos.xml";
        else if (OperatingSystem.IsLinux())
            appcastUrl = "https://gitee.com/tsdyy/mouth-king/raw/files/appcast-linux.xml";
        // on your main thread...
        _sparkle = new CustomSparkleUpdater(appcastUrl, new Ed25519Checker(SecurityMode.Unsafe, ""))
        {
            UIFactory = new NetSparkleUpdater.UI.Avalonia.UIFactory(Icon), // or null, or choose some other UI factory, or build your own IUIFactory implementation!
            RelaunchAfterUpdate = false, // set to true if needed
        };
        _sparkle.StartLoop(true, true); // will auto-check for updates

    }
}
