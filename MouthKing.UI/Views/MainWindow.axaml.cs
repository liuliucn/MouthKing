using Avalonia.Controls;
using System.Diagnostics;

namespace MouthKing.UI.Views;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        var assemblyName = Process.GetCurrentProcess().MainModule.FileName;
        var fvi = FileVersionInfo.GetVersionInfo(assemblyName);
         
        this.Title = "嘴强 v" + fvi.ProductVersion;

        InitializeComponent();
    }
}
