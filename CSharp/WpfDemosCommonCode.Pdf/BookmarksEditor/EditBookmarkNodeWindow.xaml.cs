using System.Windows;
using System.Windows.Media;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Wpf;
using Vintasoft.Imaging.Wpf.UI;

using WpfDemosCommonCode.CustomControls;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view and edit parameters of PDF bookmark.
    /// </summary>
    public partial class EditBookmarkNodeWindow : Window
    {

        #region Fields

        PdfDocument _document;
        WpfImageViewerBase _viewer;
        PdfAction _action;
        bool _init = true;
        Color _outlineColor = Colors.Transparent;

        #endregion



        #region Constructor

        private EditBookmarkNodeWindow()
        {
            InitializeComponent();
        }

        public EditBookmarkNodeWindow(int initialPageIndex, PdfBookmark initialOutline, bool addToRoot)
            : this(null, initialPageIndex, initialOutline, addToRoot)
        {
        }

        public EditBookmarkNodeWindow(WpfImageViewerBase viewer, int initialPageIndex, PdfBookmark initialOutline, bool addToRoot)
            : this()
        {
            _viewer = viewer;
            addToRootCheckBox.Visibility = addToRoot ? Visibility.Visible : Visibility.Hidden;
            _outline = initialOutline;
            _document = initialOutline.Document;
            if (_viewer == null)
                pageNumberNumericUpDown.Maximum = _document.Pages.Count;
            else
                pageNumberNumericUpDown.Maximum = _viewer.Images.Count;
            if (initialPageIndex < 0)
                initialPageIndex = 0;
            pageNumberNumericUpDown.Value = initialPageIndex + 1;
            outlineTitle.Text = _outline.Title;
            outlineExpandedCheckBox.IsChecked = _outline.IsExpanded;
            outlineBoldCheckBox.IsChecked = (_outline.Flags & PdfBookmarkFlags.Bold) != 0;
            outlineItalicCheckBox.IsChecked = (_outline.Flags & PdfBookmarkFlags.Italic) != 0;
            _outlineColor = WpfObjectConverter.CreateWindowsColor(_outline.Color);
            _action = _outline.Action;
            if (_action != null && _outline.Destination == null)
            {
                actionRadioButton.IsChecked = true;
                actionRadioButton_Click(actionRadioButton, null);
            }
            else
                destRadioButton.IsChecked = true;
            _init = false;
        }

        #endregion



        #region Properties

        PdfBookmark _outline;
        public PdfBookmark Bookmark
        {
            get
            {
                return _outline;
            }
        }

        public bool AddToRoot
        {
            get
            {
                return addToRootCheckBox.IsChecked.Value == true;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of OkButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            _outline.Title = outlineTitle.Text;
            _outline.IsExpanded = outlineExpandedCheckBox.IsChecked.Value == true;
            _outline.Flags = PdfBookmarkFlags.None;
            if (outlineBoldCheckBox.IsChecked.Value == true)
                _outline.Flags |= PdfBookmarkFlags.Bold;
            if (outlineItalicCheckBox.IsChecked.Value == true)
                _outline.Flags |= PdfBookmarkFlags.Italic;
            _outline.Color = WpfObjectConverter.CreateDrawingColor(_outlineColor);
            if (destRadioButton.IsChecked.Value == true)
            {
                if (pageNumberNumericUpDown.Value > 0)
                {
                    PdfPage page = FindPage((int)pageNumberNumericUpDown.Value - 1);
                    if (page != null)
                    {
                        try
                        {
                            _outline.Destination = new PdfDestinationFit(_document, page);
                        }
                        catch (System.Exception ex)
                        {
                            DemosTools.ShowErrorMessage(ex);
                            return;
                        }
                    }
                    else
                    {
                        DemosTools.ShowWarningMessage("Bookmarks",
                            string.Format("Page {0} is not from this PDF document. Save document and try again.",
                            pageNumberNumericUpDown.Value));
                        return;
                    }
                }
            }
            else
            {
                _outline.Action = _action;
            }
            DialogResult = true;
        }

        private PdfPage FindPage(int index)
        {
            if (_viewer == null)
                return _document.Pages[index];
            ImageSourceInfo info = _viewer.Images[index].SourceInfo;
            if (info.Stream == _document.SourceStream)
                return _document.Pages[info.PageIndex];
            return null;
        }

        /// <summary>
        /// Handles the Click event of CancelButton object.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Handles the Click event of ColorButton object.
        /// </summary>
        private void colorButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerDialog _colorDialog = new ColorPickerDialog();
            _colorDialog.StartingColor = _outlineColor;
            if (_colorDialog.ShowDialog().Value)
                _outlineColor = _colorDialog.SelectedColor;

        }

        /// <summary>
        /// Handles the Click event of ActionRadioButton object.
        /// </summary>
        private void actionRadioButton_Click(object sender, RoutedEventArgs e)
        {
            editActionButton.IsEnabled = actionRadioButton.IsChecked.Value == true;
            pageNumberNumericUpDown.IsEnabled = actionRadioButton.IsChecked.Value == false;
            if (actionRadioButton.IsChecked.Value == true)
            {
                if (!_init)
                {
                    if (!EditAction())
                    {
                        destRadioButton.IsChecked = true;
                        actionRadioButton_Click(actionRadioButton, null);
                    }
                }
            }
        }

        private bool EditAction()
        {
            ImageCollection images = null;
            if (_viewer != null)
                images = _viewer.Images;
            PdfActionEditorWindow window = new PdfActionEditorWindow(_document, _action, images);
            window.Owner = this;
            if (window.ShowDialog() == true)
            {
                _action = window.Action;
                return _action != null;
            }
            return false;
        }

        /// <summary>
        /// Handles the Click event of EditActionButton object.
        /// </summary>
        private void editActionButton_Click(object sender, RoutedEventArgs e)
        {
            EditAction();
            if (_action == null)
            {
                destRadioButton.IsChecked = true;
                actionRadioButton_Click(actionRadioButton, null);
            }
        }

        #endregion

    }
}
