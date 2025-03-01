﻿using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.UI.Avalonia.Controls;
using NetSparkleUpdater.UI.Avalonia.Interfaces;
using NetSparkleUpdater.UI.Avalonia.ViewModels;

namespace NetSparkleUpdater.UI.Avalonia
{
    /// <summary>
    /// Interaction logic for DownloadProgressWindow.xaml.
    /// 
    /// Window that shows while SparkleUpdater is downloading the update
    /// for the user.
    /// </summary>
    public partial class DownloadProgressWindow : BaseWindow, IDownloadProgress, IBackgroundColorChanger
    {
        private bool _didCallDownloadProcessCompletedHandler = false;

        private DownloadProgressWindowViewModel? _dataContext;

        private Button? _actionButton;

        /// <summary>
        /// Base constructor for DownloadProgressWindow. Initializes the window
        /// and sets up the Closing event.
        /// </summary>
        public DownloadProgressWindow()
        {
            this.InitializeComponent();
            _actionButton = this.FindControl<Button>("ActionButton");
            Closing += DownloadProgressWindow_Closing;
        }

        /// <summary>
        /// Constructor for DownloadProgressWindow that takes an initialized
        /// <see cref="DownloadProgressWindowViewModel"/> view model for use
        /// </summary>
        /// <param name="viewModel"><see cref="DownloadProgressWindowViewModel"/> view model that
        /// this window will bind to as its DataContext</param>
        /// <param name="iconBitmap">Bitmap to use for the app's logo/graphic</param>
        public DownloadProgressWindow(DownloadProgressWindowViewModel viewModel, Bitmap? iconBitmap)
        {
            InitializeComponent();
            _actionButton = this.FindControl<Button>("ActionButton");
            Closing += DownloadProgressWindow_Closing;
            DataContext = _dataContext = viewModel;
            _dataContext.BackgroundColorChanger = this;
            var imageControl = this.FindControl<Image>("AppIcon");
            if (imageControl != null)
            {
                imageControl.Source = iconBitmap;
            }
        }

        /// <summary>
        /// Event to fire when the download UI is complete; tells you 
        /// if the install process should happen or not
        /// </summary>
        public event DownloadInstallEventHandler? DownloadProcessCompleted;

        private void DownloadProgressWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_didCallDownloadProcessCompletedHandler)
            {
                _didCallDownloadProcessCompletedHandler = true;
                DownloadProcessCompleted?.Invoke(this, new DownloadInstallEventArgs(false));
            }
            Closing -= DownloadProgressWindow_Closing;
            if (!_hasInitiatedShutdown)
            {
                _hasInitiatedShutdown = true;
            }
        }

        bool IDownloadProgress.DisplayErrorMessage(string errorMessage)
        {
            if (_dataContext != null)
            {
                _dataContext.ErrorMessageText = errorMessage;
                _dataContext.IsErrorMessageVisible = true;
            }
            return true;
        }

        void IDownloadProgress.FinishedDownloadingFile(bool isDownloadedFileValid)
        {
            _dataContext?.SetFinishedDownloading(isDownloadedFileValid);
        }

        void IDownloadProgress.Close()
        {
            CloseWindow();
        }

        /// <summary>
        /// Event called when the client download progress changes
        /// </summary>
        private void OnDownloadProgressChanged(object sender, long bytesReceived, long totalBytesToReceive, int percentage)
        {
            _dataContext?.UpdateProgress(bytesReceived, totalBytesToReceive, percentage);
        }

        void IDownloadProgress.OnDownloadProgressChanged(object sender, ItemDownloadProgressEventArgs e)
        {
            OnDownloadProgressChanged(sender, e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
        }

        void IDownloadProgress.SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled)
        {
            if (_actionButton != null)
            {
                _actionButton.IsEnabled = shouldBeEnabled;
            }
        }

        void IDownloadProgress.Show()
        {
            ShowWindow();
        }

        /// <summary>
        /// Action that is invoked when the action button is clicked (e.g. for canceling or installing)
        /// </summary>
        public void ActionButton_Click()
        {
            _didCallDownloadProcessCompletedHandler = true;
            DownloadProcessCompleted?.Invoke(this, new DownloadInstallEventArgs(
                (_dataContext?.DidDownloadFail ?? false)
                ? false 
                : !(_dataContext?.IsDownloading ?? true)));
        }

        /// <inheritdoc/>
        public void ChangeBackgroundColor(Brush color)
        {
            Background = color;
        }
    }
}
