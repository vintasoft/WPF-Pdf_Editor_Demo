using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Text;
using Vintasoft.Imaging.Wpf.UI;
using Vintasoft.Imaging.Wpf;
using Vintasoft.Imaging.Wpf.UI.VisualTools;

namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A window that allows to find a text on PDF page.
    /// </summary>
    public partial class FindTextWindow : Window
    {

        #region Fields

        /// <summary>
        /// The left coordinate of the window.
        /// </summary>
        static double _formLeft = double.NaN;
        /// <summary>
        /// The top coordinate of the window.
        /// </summary>
        static double _formTop = double.NaN;


        /// <summary>
        /// Indicates whether find text window is shown.
        /// </summary>
        bool _isWindowShown = false;


        /// <summary>
        /// The visual tool that allows to search and select text on PDF page.
        /// </summary>
        WpfTextSelectionTool _pdfTextSelectionTool;

        /// <summary>
        /// The text, which must be searched.
        /// </summary>
        string _textToSearch;

        /// <summary>
        /// The text search mode of the text selection tool.
        /// </summary>
        TextSearchMode _textSearchMode;

        /// <summary>
        /// Determines whether text searching must be stoped.
        /// </summary>
        bool _stopTextSearch = false;

        /// <summary>
        /// Indicates whether text searching must be continued.
        /// </summary>
        bool _continueTextSearch = true;

        /// <summary>
        /// Indicates whether the case sensitive search must be made.
        /// </summary>
        bool _doCaseSensitiveSearch;

        /// <summary>
        /// Indicates whether regular expression should be used.
        /// </summary>
        bool _useRegexInTextSearch;

        #endregion



        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FindTextWindow"/> class.
        /// </summary>
        /// <param name="viewerTool">The text selection tool.</param>
        /// <param name="matchCase">Determines whether the case sensitive search must be made.</param>
        /// <param name="findIn">The text search mode of the text selection tool.</param>
        /// <param name="searchUp">Indicates whether the text must be searched from the current point
        /// to the top of the page/document.</param>
        public FindTextWindow(WpfTextSelectionTool viewerTool,
            bool matchCase, TextSearchMode findIn, bool searchUp)
        {
            InitializeComponent();

            // init text selection tool
            _pdfTextSelectionTool = viewerTool;
            _pdfTextSelectionTool.TextSearchingProgress += new EventHandler<TextSearchingProgressEventArgs>(_viewerTool_FindTextAtPage);
            _pdfTextSelectionTool.TextSearched += new EventHandler<TextSearchedEventArgs>(_viewerTool_TextSearched);
            lookInComboBox.SelectedIndex = (int)findIn;

            matchCaseCheckBox.IsChecked = matchCase;
            _searchUp = searchUp;
            searchUpCheckBox.IsChecked = _searchUp;

            Closing += new System.ComponentModel.CancelEventHandler(FindTextWindow_Closing);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FindTextWindow"/> class.
        /// </summary>
        /// <param name="viewerTool">The text selection tool.</param>
        public FindTextWindow(WpfTextSelectionTool viewerTool)
            :this(viewerTool, false, TextSearchMode.AllPages, false)
        {
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether search is in progress.
        /// </summary>
        private bool SearchInProgress
        {
            get
            {
                return !findWhatComboBox.IsEnabled;
            }
            set
            {
                SetSearchInProgress(value);
            }
        }

        /// <summary>
        /// Gets or sets a text to find.
        /// </summary>
        public string FindWhat
        {
            get
            {
                return findWhatComboBox.Text;
            }
            set
            {
                if (value.Contains(Environment.NewLine))
                    value = value.Substring(0, value.IndexOf(Environment.NewLine));
                findWhatComboBox.Text = value;
                findWhatComboBox_KeyUp(findWhatComboBox, null);
            }
        }

        bool _searchUp;
        /// <summary>
        /// Gets a value indicating whether the text must be searched from the current point
        /// to the top of the page/document.
        /// </summary>
        public bool SearchUp
        {
            get
            {
                return _searchUp;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the case sensitive search must be made.
        /// </summary>
        public bool MatchCase
        {
            get
            {
                return matchCaseCheckBox.IsChecked.Value;
            }
        }

        /// <summary>
        /// Gets the selected text search mode.
        /// </summary>
        public TextSearchMode FindIn
        {
            get
            {
                return (TextSearchMode)lookInComboBox.SelectedIndex;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Selects searched text.
        /// </summary>
        /// <param name="tool">The text selection tool.</param>
        /// <param name="args">The text searched event args.</param>
        public static void SelectSearchedText(WpfTextSelectionTool tool, TextSearchedEventArgs args)
        {
            // set image where text is found
            tool.ImageViewer.SetFocusedIndexSync(args.ImageIndex);

            // select the region of searched text
            tool.SelectedRegion = args.FoundTextRegion;

            // scroll to the searched text
            tool.ScrollToSelectedRegion();
        }        

        /// <summary>
        /// This dialog is closing.
        /// </summary>
        private void FindTextWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = SearchInProgress;
        }

        /// <summary>
        /// This dialog is loaded.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isWindowShown = true;
            _continueTextSearch = true;
            // if window location is not empty
            if (!double.IsNaN(_formLeft) && !double.IsNaN(_formTop))
            {
                // set window location
                Left = _formLeft;
                Top = _formTop;
            }
        }

        /// <summary>
        /// This dialog is closed.
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            _isWindowShown = false;
            // get window location
            _formLeft = this.Left;
            _formTop = this.Top;
        }

        /// <summary>
        /// Text to find is changed.
        /// </summary>
        private void findWhatComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            findNextButton.IsEnabled = findWhatComboBox.Text != "";
        }

        /// <summary>
        /// "Find Next" button is clicked.
        /// </summary>
        private void findNextButton_Click(object sender, RoutedEventArgs e)
        {
            // if image in image viewer is empty
            if (_pdfTextSelectionTool.ImageViewer.Image == null)
                return;

            // get searching text
            string text = findWhatComboBox.Text;

            _doCaseSensitiveSearch = matchCaseCheckBox.IsChecked.Value;
            _useRegexInTextSearch = regexCheckBox.IsChecked.Value;

            // if regular expression must be used
            if (_useRegexInTextSearch)
            {
                // check regular expression
                try
                {
                    CreateRegex(text, _doCaseSensitiveSearch);
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                    return;
                }
            }

            // get value which indicates that the text must be searched 
            // from current point to the top of the page.
            _searchUp = searchUpCheckBox.IsChecked.Value == true;

            if (findWhatComboBox.Items.Contains(text))
                findWhatComboBox.Items.Remove(text);
            findWhatComboBox.Items.Insert(0, text);

            // get text search mode
            TextSearchMode findMode = FindIn;
            // if text search mode is all pages
            if (findMode == TextSearchMode.AllPages)
                SearchInProgress = true;
            _stopTextSearch = false;

            _textToSearch = text;
            _textSearchMode = findMode;
            // begin search async
            ThreadPool.QueueUserWorkItem(new WaitCallback(FindNextAsync));
        }

        /// <summary>
        /// Creates the regular expression.
        /// </summary>
        /// <param name="text">The pattern of regular expression.</param>
        /// <param name="matchCase">Indicates whether the case sensitive search must be made.</param>
        private Regex CreateRegex(string text, bool matchCase)
        {
            // get regular expression
            RegexOptions options = RegexOptions.None;
            if (!matchCase)
                options |= RegexOptions.IgnoreCase;
            // create text search engine
            return new Regex(text, options);
        }
    
        /// <summary>
        /// Starts the asynchronous search of the next occurrence of the text.
        /// </summary>
        private void FindNextAsync(object state)
        {
            TextSearchEngine finder;
            // if regular expression must be used
            if (_useRegexInTextSearch)
            {
                // create text search engine use regular expression
                finder = TextSearchEngine.Create(CreateRegex(_textToSearch, _doCaseSensitiveSearch));
            }
            else
            {
                // create text search engine
                finder = TextSearchEngine.Create(_textToSearch, !_doCaseSensitiveSearch);
            }
            // search text
            _pdfTextSelectionTool.FindText(finder, _textSearchMode, _searchUp, _continueTextSearch);
            _continueTextSearch = false;

            if (_textSearchMode == TextSearchMode.AllPages)
                SearchInProgress = false;
        }

        /// <summary>
        /// Text searching is in progress.
        /// </summary>
        private void _viewerTool_FindTextAtPage(object sender, Vintasoft.Imaging.Text.TextSearchingProgressEventArgs e)
        {
            // if text searching must be stoped
            if (_stopTextSearch)
                e.Cancel = true;
        }

        /// <summary>
        /// Text searching is finished.
        /// </summary>
        private void _viewerTool_TextSearched(object sender, TextSearchedEventArgs e)
        {
            // if form is shown
            if (_isWindowShown)
            {
                // if text is found
                if (e.FoundTextRegion == null)
                {
                    // if text searching is not canceled
                    if (!e.Canceled)
                    {
                        MessageBox.Show(string.Format("The following specified text was not found: {0}", e.SearchEngine),
                            "Find text", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                // if text is not found
                else
                {
                    // select searched text
                    SelectSearchedText((WpfTextSelectionTool)sender, e);
                }
            }
        }

        /// <summary>
        /// Cancels the text search.
        /// </summary>
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            _stopTextSearch = true;
        }

        /// <summary>
        /// A key is pressed in dialog.
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                DialogResult = false;
        }

        /// <summary>
        /// Enables or disables the search up function.
        /// </summary>
        private void searchUpCheckBox_Click(object sender, RoutedEventArgs e)
        {
            _searchUp = searchUpCheckBox.IsChecked.Value == true;
        }

        /// <summary>
        /// Updates user interface of the form.
        /// </summary>
        /// <param name="isSearchInProgress">Determines whether search is in progress.</param>
        private void SetSearchInProgress(bool isSearchInProgress)
        {
            if (Dispatcher.Thread == Thread.CurrentThread)
            {
                findWhatComboBox.IsEnabled = !isSearchInProgress;
                lookInComboBox.IsEnabled = !isSearchInProgress;
                findOptionsGroupBox.IsEnabled = !isSearchInProgress;
                findNextButton.Visibility = isSearchInProgress ? Visibility.Collapsed : Visibility.Visible;
                stopButton.Visibility = isSearchInProgress ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                Dispatcher.Invoke(new SetSearchInProgressDelegate(SetSearchInProgress), isSearchInProgress);
            }
        }
         
        #endregion



        #region Delegates

        /// <summary>
        /// Represents the method that updates user interface of the form.
        /// </summary>
        /// <param name="isSearchInProgress">Determines whether search in progress.</param>
        delegate void SetSearchInProgressDelegate(bool isSearchInProgress);

        #endregion

    }
}
