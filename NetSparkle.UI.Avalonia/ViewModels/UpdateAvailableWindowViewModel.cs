﻿using NetSparkleUpdater.Enums;
using NetSparkleUpdater.UI.Avalonia.Helpers;
using NetSparkleUpdater.UI.Avalonia.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using NetSparkleUpdater.Interfaces;

namespace NetSparkleUpdater.UI.Avalonia.ViewModels
{
    /// <summary>
    /// View model for views that detail updates that are available for a given user
    /// </summary>
    public class UpdateAvailableWindowViewModel : ChangeNotifier
    {
        private List<AppCastItem> _updates;

        private CancellationToken? _cancellationToken;
        private CancellationTokenSource? _cancellationTokenSource;

        private string? _titleHeaderText;
        private string? _infoText;

        private bool _isRemindMeLaterEnabled;
        private bool _isSkipEnabled;

        private bool _areReleaseNotesVisible;
        private bool _isRemindMeLaterVisible;
        private bool _isSkipVisible;

        private UpdateAvailableResult _userResponse;

        /// <summary>
        /// Initialize the view model with release notes, remind me later,
        /// and skip button being visible by default.
        /// </summary>
        public UpdateAvailableWindowViewModel()
        {
            _updates = new List<AppCastItem>();
            _areReleaseNotesVisible = true;
            _isRemindMeLaterVisible = true;
            _isSkipVisible = true;
            _userResponse = UpdateAvailableResult.None;
        }

        /// <summary>
        /// Get the default release notes HTML template. Use this if you're
        /// creating your own subclass and you want things to be styled the same
        /// as the default.
        /// </summary>
        /// <returns>The default release notes HTML template</returns>
        public static string GetDefaultReleaseNotesTemplate()
        {
            return
                "<div style=\"border: #ccc 1px solid;\">" +
                    "<div style=\"background: {3}; background-color: {3}; font-size: 16px; padding: 5px; padding-top: 4px; padding-bottom: 0;\">" +
                        "{0} ({1})" +
                "</div><div style=\"padding: 5px; font-size: 12px;\">{2}</div></div>";
        }

        /// <summary>
        /// Get the default additional header HTML CSS. Use this if you're
        /// creating your own subclass and you want things to be styled the same
        /// as the default.
        /// </summary>
        /// <returns>The default additional header HTML CSS.</returns>
        public static string GetDefaultAdditionalHeaderHTML()
        {
            return @"
                <style>
                    body { position: fixed; padding-left: 8px; padding-right: 0px; margin-right: 0; padding-top: 14px; padding-bottom: 0px; } 
                    h1, h2, h3, h4, h5 { margin: 4px; margin-top: 8px; } 
                    li, li li { margin: 4px; } 
                    li, p { font-size: 18px; } 
                    li p, li ul { margin-top: 0px; margin-bottom: 0px; }
                    ul { margin-top: 2px; margin-bottom: 2px; }
                </style>";
        }

        /// <summary>
        /// Interface object for the object that will be displaying the release notes
        /// </summary>
        public IReleaseNotesDisplayer? ReleaseNotesDisplayer { get; set; }

        /// <summary>
        /// Interface object for the object that can handle user responses to the update that
        /// is being shown to the user
        /// </summary>
        public IUserRespondedToUpdateCheck? UserRespondedHandler { get; set; }

        /// <summary>
        /// Object responsible for downloading and formatting markdown release notes for display in HTML
        /// </summary>
        public ReleaseNotesGrabber? ReleaseNotesGrabber { get; set; }

        /// <summary>
        /// Header text to show to the user. Usually something along the lines of
        /// "A new version of {software} is available!"
        /// </summary>
        public string TitleHeaderText
        {
            get => _titleHeaderText ?? "";
            set { _titleHeaderText = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Some additional text to show to the user underneath the header. This is text like
        /// "Version {version} is now available. Do you want to download it?"
        /// </summary>
        public string InfoText
        {
            get => _infoText ?? "";
            set { _infoText = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Whether or not the "Remind Me Later" button is enabled
        /// </summary>
        public bool IsRemindMeLaterEnabled
        {
            get => _isRemindMeLaterEnabled;
            set { _isRemindMeLaterEnabled = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Whether or not the "Skip this Update" button is enabled
        /// </summary>
        public bool IsSkipEnabled
        {
            get => _isSkipEnabled;
            set { _isSkipEnabled = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// The list of updates that are available for downloading by the user
        /// </summary>
        public List<AppCastItem> Updates
        {
            get => _updates;
        }

        /// <summary>
        /// Whether or not the release notes for updates are shown to the user
        /// </summary>
        public bool AreReleaseNotesVisible
        {
            get => _areReleaseNotesVisible;
            set { _areReleaseNotesVisible = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Whether or not the "Remind Me Later" button is visible
        /// </summary>
        public bool IsRemindMeLaterVisible
        {
            get => _isRemindMeLaterVisible;
            set { _isRemindMeLaterVisible = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Whether or not the "Skip this Update" button is visible
        /// </summary>
        public bool IsSkipVisible
        {
            get => _isSkipVisible;
            set { _isSkipVisible = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// The response that the user gave to the update
        /// </summary>
        public UpdateAvailableResult UserResponse
        {
            get => _userResponse;
        }

        /// <summary>
        /// Command to execute when the user wants to skip this update
        /// </summary>
        public ICommand Skip => new RelayCommand(PerformSkip);
        /// <summary>
        /// Command to execute when the user wants to be reminded about this update later
        /// </summary>
        public ICommand RemindMeLater => new RelayCommand(PerformRemindMeLater);
        /// <summary>
        /// Command to execute when the user wants to download and install the update right away
        /// </summary>
        public ICommand DownloadInstall => new RelayCommand(PerformDownloadInstall);

        /// <summary>
        /// Skip updating the software
        /// </summary>
        public void PerformSkip()
        {
            SendResponse(UpdateAvailableResult.SkipUpdate);
        }

        /// <summary>
        /// Hide the update window and show it to the user at another time
        /// </summary>
        public void PerformRemindMeLater()
        {
            SendResponse(UpdateAvailableResult.RemindMeLater);
        }

        /// <summary>
        /// Start downloading the update so that the user can install it
        /// </summary>
        public void PerformDownloadInstall()
        {
            SendResponse(UpdateAvailableResult.InstallUpdate);
        }

        private void SendResponse(UpdateAvailableResult response)
        {
            _userResponse = response;
            UserRespondedHandler?.UserRespondedToUpdateCheck(response);
            Cancel();
        }

        /// <summary>
        /// Initialize this view model with its necessary items so that it can display things
        /// to the user properly
        /// </summary>
        /// <param name="signatureVerifier">The <seealso cref="ISignatureVerifier"/> for verifying release note signatures</param>
        /// <param name="items">The list of <seealso cref="AppCastItem"/> updates that are available for the user</param>
        /// <param name="isUpdateAlreadyDownloaded">Whether or not the update is already downloaded ot the user's computer</param>
        /// <param name="releaseNotesHTMLTemplate">The HTML string template to show for the release notes</param>
        /// <param name="additionalReleaseNotesHeaderHTML">The HTML string to add into the head element of the HTML for the release notes</param>
        /// <param name="releaseNotesDateFormat">Date format for release notes</param>
        /// <param name="appNameTitle">Title for application</param>
        /// <param name="installedVersion">Currently installed version of application</param>
        public void Initialize(List<AppCastItem> items, ISignatureVerifier? signatureVerifier, bool isUpdateAlreadyDownloaded = false,
            string releaseNotesHTMLTemplate = "", string additionalReleaseNotesHeaderHTML = "", string releaseNotesDateFormat = "D",
            string appNameTitle = "本应用", string installedVersion = "")
        {
            _updates = items;
            if (ReleaseNotesGrabber == null)
            {
                var defaultReleaseNotesAvaloniaTemplate = GetDefaultReleaseNotesTemplate();
                if (string.IsNullOrWhiteSpace(additionalReleaseNotesHeaderHTML))
                {
                    additionalReleaseNotesHeaderHTML = UpdateAvailableWindowViewModel.GetDefaultAdditionalHeaderHTML();
                }
                if (string.IsNullOrWhiteSpace(releaseNotesHTMLTemplate))
                {
                    releaseNotesHTMLTemplate = defaultReleaseNotesAvaloniaTemplate;
                }
                ReleaseNotesGrabber = new ReleaseNotesGrabber(releaseNotesHTMLTemplate, additionalReleaseNotesHeaderHTML, signatureVerifier)
                {
                    DateFormat = releaseNotesDateFormat
                };
            }

            AppCastItem? item = items.FirstOrDefault();

            // TODO: string translations
            TitleHeaderText = string.Format("检测到 嘴强 有新版本。", appNameTitle);
            var downloadInstallText = isUpdateAlreadyDownloaded ? "安装" : "下载";
            if (item != null)
            {
                InfoText = string.Format("{0} {1} 可以升级 (已安装 {2})。 你想现在就 {3} 吗?", appNameTitle, item.Version, installedVersion, downloadInstallText);
            }
            else
            {
                InfoText = string.Format("你想现在就 {0} 吗?", downloadInstallText);
            }

            bool isUserMissingCriticalUpdate = items.Any(x => x.IsCriticalUpdate);
            IsRemindMeLaterEnabled = isUserMissingCriticalUpdate == false;
            IsSkipEnabled = isUserMissingCriticalUpdate == false;

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            ReleaseNotesDisplayer?.ShowReleaseNotes(ReleaseNotesGrabber.GetLoadingText());
            LoadReleaseNotes(items);
        }

        private async void LoadReleaseNotes(List<AppCastItem> items)
        {
            AppCastItem? latestVersion = items.OrderByDescending(p => p.Version).FirstOrDefault();
            string releaseNotes = ReleaseNotesGrabber != null 
                ? await ReleaseNotesGrabber.DownloadAllReleaseNotes(
                    items, 
                    latestVersion ?? new AppCastItem(), 
                    _cancellationToken ?? new CancellationTokenSource().Token) 
                : "";
            ReleaseNotesDisplayer?.ShowReleaseNotes(releaseNotes);
        }

        /// <summary>
        /// Cancel the download of release notes or other asynchronous operations taking place
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
