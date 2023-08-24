using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Text;
using Vintasoft.Imaging.Wpf.UI;
using Vintasoft.Imaging.Wpf.UI.VisualTools;

namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// Tool strip that allows to search text in a PDF document.
    /// </summary>
    public partial class FindTextToolBar : ToolBar
    {

        #region Fields

        /// <summary>
        /// Find text dialog.
        /// </summary>
        string _fastFindText = null;

        /// <summary>
        /// Determines that text search is restarted.
        /// </summary>
        bool _fastFindAgain;

        /// <summary>
        /// Determines that text searching can be stopped.
        /// </summary>
        bool _fastFindStop;

        /// <summary>
        /// Indicates that the case sensitivity should be ignored for search of text.
        /// </summary>
        bool _matchCase = false;

        /// <summary>
        /// Text search mode.
        /// </summary>
        TextSearchMode _findIn = TextSearchMode.AllPages;

        /// <summary>
        /// Indicates that searching text must be searched
        /// from current position in document to the beginning of document/page.
        /// </summary>
        bool _searchUp = false;

        Thread _thread = null;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FindTextToolStrip"/> class.
        /// </summary>
        public FindTextToolBar()
        {
            InitializeComponent();

            IsEnabledChanged += new DependencyPropertyChangedEventHandler(FindTextToolBar_IsEnabledChanged);

            UpdateUIAsync();
        }

        #endregion



        #region Properties

        #region PUBLIC

        WpfTextSelectionTool _textSelectionTool = null;
        /// <summary>
        /// Gets or sets the text selection tool.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public WpfTextSelectionTool TextSelectionTool
        {
            get
            {
                return _textSelectionTool;
            }
            set
            {
                if (_textSelectionTool != null)
                {
                    UnsubscribeFromVisualToolEvents(_textSelectionTool);
                    _isActivated = false;
                    _isTextSearching = false;
                    _isFindDialogShow = false;
                }

                _textSelectionTool = value;

                if (_textSelectionTool != null)
                {
                    SubscribeToVisualToolEvents(_textSelectionTool);
                    _isActivated = _textSelectionTool.ImageViewer != null;
                }

                UpdateUIAsync();
            }
        }

        bool _isTextSearching = false;
        /// <summary>
        /// Gets a value indicating whether text is searching.
        /// </summary>
        public bool IsTextSearching
        {
            get
            {
                return _isTextSearching;
            }
        }

        #endregion


        #region PRIVATE

        bool _isActivated = false;
        /// <summary>
        /// Gets or sets a value indicating whether visual tool is activated.
        /// </summary>
        private bool IsActivated
        {
            get
            {
                return _isActivated;
            }
            set
            {
                _isActivated = value;
                UpdateUIAsync();
            }
        }

        bool _isFindDialogShow = false;
        /// <summary>
        /// Gets or sets a value indicating whether the find dialog is shown.
        /// </summary>
        private bool IsFindDialogShow
        {
            get
            {
                return _isFindDialogShow;
            }
            set
            {
                _isFindDialogShow = value;
                UpdateUIAsync();
            }
        }

        #endregion

        #endregion



        #region Methods

        #region UI state

        /// <summary>
        /// Update UI safely.
        /// </summary>
        private void UpdateUIAsync()
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
                Dispatcher.Invoke(new UpdateUIDelegate(UpdateUI));
            else
                UpdateUI();
        }

        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            bool isEnabled = IsEnabled && IsActivated && TextSelectionTool != null && TextSelectionTool.FocusedImageHasTextRegion;
            bool isFindText = IsTextSearching;
            bool isFindDialogShow = IsFindDialogShow;

            findTextButton.IsEnabled = isEnabled && !isFindText && !isFindDialogShow;
            fastFindComboBox.IsEnabled = isEnabled && !isFindText && !isFindDialogShow;
            fastFindNextButton.IsEnabled = isEnabled && !isFindDialogShow;
            fastFindNextButton.Visibility = isFindText ? Visibility.Collapsed : Visibility.Visible;
            stopFastFindButton.IsEnabled = isEnabled && !isFindDialogShow;
            stopFastFindButton.Visibility = isFindText ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// The Enabled property is changed.
        /// </summary>
        private void FindTextToolBar_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateUIAsync();
        }

        #endregion


        #region Image viewer

        /// <summary>
        /// Subscribes to the events of image viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer.</param>
        private void SubscribeToImageViewerEvents(WpfImageViewer imageViewer)
        {
            imageViewer.FocusedIndexChanged += new PropertyChangedEventHandler<int>(imageViewer_FocusedIndexChanged);
        }

        /// <summary>
        /// Unsubscribes from the events of image viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer.</param>
        private void UnsubscribeFromImageViewerEvents(WpfImageViewer imageViewer)
        {
            imageViewer.FocusedIndexChanged -= imageViewer_FocusedIndexChanged;
        }

        /// <summary>
        /// Focused image is changed in image viewer.
        /// </summary>
        private void imageViewer_FocusedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            UpdateUI();
        }

        #endregion


        #region Text selection tool

        /// <summary>
        /// Subscribes to the events of visual tool.
        /// </summary>
        /// <param name="textSelectionTool">The text selection tool.</param>
        private void SubscribeToVisualToolEvents(WpfTextSelectionTool textSelectionTool)
        {
            textSelectionTool.Activated += new EventHandler(textSelectionTool_Activated);
            textSelectionTool.Deactivated += new EventHandler(textSelectionTool_Deactivated);
            textSelectionTool.TextSearchingProgress +=
                new EventHandler<TextSearchingProgressEventArgs>(textSelectionTool_TextSearchingProgress);
            textSelectionTool.TextSearched += new EventHandler<TextSearchedEventArgs>(textSelectionTool_TextSearched);

            if (textSelectionTool.ImageViewer != null)
                SubscribeToImageViewerEvents(textSelectionTool.ImageViewer);
        }

        /// <summary>
        /// Unsubscribes from the events of visual tool.
        /// </summary>
        /// <param name="textSelectionTool">The text selection tool.</param>
        private void UnsubscribeFromVisualToolEvents(WpfTextSelectionTool textSelectionTool)
        {
            textSelectionTool.Activated -= textSelectionTool_Activated;
            textSelectionTool.Deactivated -= textSelectionTool_Deactivated;
            textSelectionTool.TextSearchingProgress -= textSelectionTool_TextSearchingProgress;
            textSelectionTool.TextSearched -= textSelectionTool_TextSearched;

            if (textSelectionTool.ImageViewer != null)
                UnsubscribeFromImageViewerEvents(textSelectionTool.ImageViewer);
        }

        /// <summary>
        /// Text selection tool is activated.
        /// </summary>
        private void textSelectionTool_Activated(object sender, EventArgs e)
        {
            IsActivated = true;
            SubscribeToImageViewerEvents(((WpfTextSelectionTool)sender).ImageViewer);
        }

        /// <summary>
        /// Text selection tool is deactivated.
        /// </summary>
        private void textSelectionTool_Deactivated(object sender, EventArgs e)
        {
            if (_thread != null)
            {
                _fastFindStop = true;
                _isTextSearching = false;
                _isFindDialogShow = false;
                _thread = null;
            }

            IsActivated = false;
            UnsubscribeFromImageViewerEvents(((WpfTextSelectionTool)sender).ImageViewer);
        }

        /// <summary>
        /// Progress of text searching is changed.
        /// </summary>
        private void textSelectionTool_TextSearchingProgress(object sender, TextSearchingProgressEventArgs e)
        {
            if (TextSelectionTool.ImageViewer == null || IsTextSearching && !e.Cancel && _fastFindStop)
                // stop search
                e.Cancel = true;
        }

        /// <summary>
        /// Text is searched.
        /// </summary>
        private void textSelectionTool_TextSearched(object sender, TextSearchedEventArgs e)
        {
            if (IsTextSearching)
            {
                if (e.FoundTextRegion == null)
                {
                    // text was not found
                    if (!e.Canceled)
                        MessageBox.Show(string.Format("The following specified text was not found: {0}", e.SearchEngine),
                            "Find text", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    FindTextWindow.SelectSearchedText((WpfTextSelectionTool)sender, e);
                }

                OnTextSearchingStarted(false);
                fastFindComboBox.Focus();
            }
        }

        #endregion


        /// <summary>
        /// Find text button is clicked.
        /// </summary>
        private void FindTextButton_Click(object sender, RoutedEventArgs e)
        {
            FindTextWindow findTextDialog = new FindTextWindow(_textSelectionTool,
                _matchCase, _findIn, _searchUp);
            findTextDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            findTextDialog.Owner = Window.GetWindow(this);
            if (_textSelectionTool.HasSelectedText)
                findTextDialog.FindWhat = _textSelectionTool.SelectedText;

            IsFindDialogShow = true;
            findTextDialog.ShowDialog();
            _matchCase = findTextDialog.MatchCase;
            _findIn = findTextDialog.FindIn;
            _searchUp = findTextDialog.SearchUp;
            IsFindDialogShow = false;
        }

        /// <summary>
        /// "Enter" key button is pressed.
        /// </summary>
        private void fastFindComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (fastFindComboBox.IsEnabled && e.Key == Key.Enter)
            {
                FastFind();
            }
        }

        /// <summary>
        /// "Fast find" button is clicked.
        /// </summary>
        private void FastFindNextButton_Click(object sender, RoutedEventArgs e)
        {
            FastFind();
        }

        /// <summary>
        /// "Stop text search" button is clicked.
        /// </summary>
        private void StopFastFindButton_Click(object sender, RoutedEventArgs e)
        {
            _fastFindStop = true;
        }

        /// <summary>
        /// Text searching is started.
        /// </summary>
        private void OnTextSearchingStarted(bool value)
        {
            _isTextSearching = value;
            UpdateUIAsync();
        }

        /// <summary>
        /// Runs the text search asynchronously.
        /// </summary>
        private void FastFind()
        {
            OnTextSearchingStarted(true);

            // get a search text
            string text = fastFindComboBox.Text;
            if (text == "")
            {
                OnTextSearchingStarted(false);
                return;
            }

            fastFindComboBox.BeginInit();
            // add text to fastFindToolStripComboBox
            if (fastFindComboBox.Items.Contains(text))
                fastFindComboBox.Items.Remove(text);
            fastFindComboBox.Items.Insert(0, text);
            fastFindComboBox.SelectedIndex = 0;
            fastFindComboBox.EndInit();

            _fastFindAgain = text != _fastFindText;
            _fastFindText = text;
            _fastFindStop = false;

            _thread = new Thread(new ThreadStart(FastFindThread));
            _thread.IsBackground = true;
            _thread.Start();
        }

        /// <summary>
        /// Searches text in the background thread.
        /// </summary>
        private void FastFindThread()
        {
            TextSearchEngine finder = TextSearchEngine.Create(_fastFindText, !_matchCase);

            // search text
            _textSelectionTool.FindText(finder, _findIn, _searchUp, _fastFindAgain);

            _thread = null;
            _fastFindStop = false;
        }

        #endregion



        #region Delegates

        /// <summary>
        /// Delegate for updating the user interface.
        /// </summary>
        private delegate void UpdateUIDelegate();

        #endregion

    }
}
