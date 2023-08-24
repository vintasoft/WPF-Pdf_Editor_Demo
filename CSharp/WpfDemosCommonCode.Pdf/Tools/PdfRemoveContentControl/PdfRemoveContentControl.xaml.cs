using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Fonts;
using Vintasoft.Imaging.Pdf.Wpf.UI;
using Vintasoft.Imaging.Wpf.UI;
using Vintasoft.Imaging.Wpf.UI.VisualTools;
using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;
using Vintasoft.Imaging.Wpf.Utils;

using WpfDemosCommonCode.Imaging;
using WpfDemosCommonCode.Pdf.Security;
using Vintasoft.Imaging.Wpf;
using Vintasoft.Imaging.Text;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to create, manage and apply redaction marks to a PDF page.
    /// </summary>
    public partial class PdfRemoveContentControl : UserControl
    {

        #region Fields

        /// <summary>
        /// Indicates that this is the first build of redaction mark.
        /// </summary>
        bool _isFirstBuild = true;

        ContextMenu _imageViewerContextMenu = null;

        ContextMenu _redactionMarkContextMenu = null;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRemoveContentControl"/> class.
        /// </summary>
        public PdfRemoveContentControl()
        {
            InitializeComponent();

            _imageViewerContextMenu = (ContextMenu)Resources["imageViewerContextMenu"];
            _redactionMarkContextMenu = (ContextMenu)Resources["redactionMarkContextMenu"];

            removeAllButton.Tag = PdfRedactionMarkType.RemoveAll;
            removeAnnotationsButton.Tag = PdfRedactionMarkType.RemoveAnnotations;
            removeRasterGraphicsButton.Tag = PdfRedactionMarkType.RemoveRasterGraphics;
            removeTextButton.Tag = PdfRedactionMarkType.RemoveText;
            removeVectorGraphicsButton.Tag = PdfRedactionMarkType.RemoveVectorGraphics;
        }

        #endregion



        #region Properties

        WpfPdfRemoveContentTool _removeContentTool = null;
        /// <summary>
        /// Gets or sets the remove content tool.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public WpfPdfRemoveContentTool RemoveContentTool
        {
            get
            {
                return _removeContentTool;
            }
            set
            {
                if (_removeContentTool != null)
                    UnsubscribeFromVisualToolEvents(_removeContentTool);

                _removeContentTool = value;

                if (_removeContentTool != null)
                    SubscribeToVisualToolEvents(_removeContentTool);

                UpdateUI();
            }
        }

        WpfTextSelectionTool _textSelectionTool = null;
        /// <summary>
        /// Gets or sets the text selection tool.
        /// </summary>
        public WpfTextSelectionTool TextSelectionTool
        {
            get
            {
                return _textSelectionTool;
            }
            set
            {
                _textSelectionTool = value;
            }
        }

        WpfThumbnailViewer _thumbnailViewer = null;
        /// <summary>
        /// Gets or sets the thumbnail viewer.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public WpfThumbnailViewer ThumbnailViewer
        {
            get
            {
                return _thumbnailViewer;
            }
            set
            {
                _thumbnailViewer = value;
                markSelectedPagesFrmRedactionButton.Visibility = _thumbnailViewer != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the pack dialog must be shown after the marks was applied.
        /// </summary>
        public bool ShowPackDialogAfterMarkApplied
        {
            get
            {
                return showPackDialogAfterMarkAppliesCheckBox.IsChecked.Value;
            }
        }

        /// <summary>
        /// Gets the current image of image viewer.
        /// </summary>
        private VintasoftImage CurrentImage
        {
            get
            {
                if (_removeContentTool == null ||
                    _removeContentTool.ImageViewer == null)
                    return null;

                return _removeContentTool.ImageViewer.Image;
            }
        }

        /// <summary>
        /// Gets the current PDF page.
        /// </summary>
        private PdfPage CurrentPage
        {
            get
            {
                VintasoftImage image = CurrentImage;
                if (image != null)
                    return PdfDocumentController.GetPageAssociatedWithImage(image);

                return null;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the user interface of this control.
        /// </summary>
        private void UpdateUI()
        {
            mainPanel.IsEnabled =
                _removeContentTool != null &&
                _removeContentTool.ImageViewer != null &&
                _removeContentTool.ImageViewer.Image != null &&
                PdfDocumentController.GetPageAssociatedWithImage(_removeContentTool.ImageViewer.Image) != null;

            if (mainPanel.IsEnabled)
            {
                bool redactionMarkIsSelected = false;
                bool containsRedactionMarks = false;
                bool containsImage = CurrentImage != null;
                bool containsAppearance = false;

                if (_removeContentTool != null)
                {
                    redactionMarkIsSelected = _removeContentTool.FocusedSelectionItem != null;
                    containsRedactionMarks = _removeContentTool.Selection.Count > 0;
                    containsAppearance = _removeContentTool.RedactionMarkAppearance != null;
                }

                addRedactionMarkGroupBox.IsEnabled = containsImage;

                applyRedactionMarksButton.IsEnabled = containsRedactionMarks && containsImage;
                deleteSelectedRedactionMarkButton.IsEnabled = redactionMarkIsSelected;
                redactionMarkAppearancePropertiesButton.IsEnabled = containsImage && containsAppearance;
            }
        }


        /// <summary>
        /// Subscribes to the events of visual tool.
        /// </summary>
        /// <param name="removeContentTool">The remove content tool.</param>
        private void SubscribeToVisualToolEvents(WpfPdfRemoveContentTool removeContentTool)
        {
            removeContentTool.Activated += new EventHandler(RemoveContentTool_Activated);
            removeContentTool.Deactivated += new EventHandler(RemoveContentTool_Deactivated);
            removeContentTool.SelectionChanged += new EventHandler(RemoveContentTool_SelectionChanged);
            removeContentTool.FocusedRectangleChanged += new PropertyChangedEventHandler<WpfRedactionMark>(RemoveContentTool_FocusedRectangleChanged);
            removeContentTool.MouseDown += new MouseButtonEventHandler(RemoveContentTool_MouseDown);

            if (removeContentTool.ImageViewer != null)
                SubscribeToImageViewerEvents(removeContentTool.ImageViewer);
        }

        /// <summary>
        /// Unsubscribes from the events of visual tool.
        /// </summary>
        /// <param name="removeContentTool">The remove content tool.</param>
        private void UnsubscribeFromVisualToolEvents(WpfPdfRemoveContentTool removeContentTool)
        {
            removeContentTool.Activated -= RemoveContentTool_Activated;
            removeContentTool.Deactivated -= RemoveContentTool_Deactivated;
            removeContentTool.SelectionChanged -= RemoveContentTool_SelectionChanged;
            removeContentTool.FocusedRectangleChanged -= RemoveContentTool_FocusedRectangleChanged;
            removeContentTool.MouseDown -= RemoveContentTool_MouseDown;

            if (removeContentTool.ImageViewer != null)
                UnsubscribeFromVisualToolEvents(removeContentTool.ImageViewer);
        }

        /// <summary>
        /// Subscribes to the events of viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer.</param>
        private void SubscribeToImageViewerEvents(WpfImageViewer imageViewer)
        {
            imageViewer.FocusedIndexChanged += new PropertyChangedEventHandler<int>(imageViewer_FocusedIndexChanged);
        }

        /// <summary>
        /// Unsubscribes from the events of viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer.</param>
        private void UnsubscribeFromVisualToolEvents(WpfImageViewer imageViewer)
        {
            imageViewer.FocusedIndexChanged -= imageViewer_FocusedIndexChanged;
        }


        #region 'Main' buttons

        /// <summary>
        /// Applies the redaction marks to the current PDF document.
        /// </summary>
        private void applyRedactionMarksButton_Click(object sender, RoutedEventArgs e)
        {
            // determines that the temporary redaction mark appearance must be reset
            bool createTemporaryAppearance = _removeContentTool.RedactionMarkAppearance == null;
            try
            {
                // get current PDF page
                PdfPage pdfPage = CurrentPage;
                // get current PDF document
                PdfDocument pdfDocument = pdfPage.Document;

                // if temporary redaction mark appearance must be created
                if (createTemporaryAppearance)
                    // create redaction mark appearance
                    _removeContentTool.RedactionMarkAppearance = new RedactionMarkAppearanceGraphicsFigure();

                // get redaction mark appearance
                RedactionMarkAppearanceGraphicsFigure appearance =
                    (RedactionMarkAppearanceGraphicsFigure)_removeContentTool.RedactionMarkAppearance;

                // determines that the redaction mark appearance text is drawn
                bool drawText = !string.IsNullOrEmpty(appearance.Text);

                // create the redaction mark appearance font must be created
                if (drawText && (appearance.Font == null || appearance.Font.Document != pdfDocument))
                    // create the redaction mark appearance font
                    appearance.Font = pdfDocument.FontManager.GetStandardFont(PdfStandardFontType.TimesRoman);

                // create a dialog that will show process progress
                ActionProgressWindow progressWindow = new ActionProgressWindow(
                    _removeContentTool.ApplyRedactionMarks, 2, "Remove content and blackout");
                Window parentWindow = Window.GetWindow(this);
                // apply the redaction marks to the current PDF document
                if (!progressWindow.RunAndShowDialog(parentWindow).Value)
                {
                    return;
                }

                if (RedactionMarkApplied != null)
                    // raise the RedactionMarkApplied event
                    RedactionMarkApplied(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
            finally
            {
                // if temporary redaction mark appearance was created
                if (createTemporaryAppearance)
                    // remove the temporary redaction mark appearance
                    _removeContentTool.RedactionMarkAppearance = null;
                // update UI
                UpdateUI();
            }
        }

        /// <summary>
        /// Marks the selected pages for redaction.
        /// </summary>
        private void markSelectedPagesFrmRedactionToolStripButton_Click(object sender, RoutedEventArgs e)
        {
            WpfImageViewer imageViewer = _removeContentTool.ImageViewer;
            if (imageViewer.Images.Count == 0)
                return;

            SelectedThumbnailImageItemCollection selectedThumbnails = _thumbnailViewer.SelectedThumbnails;
            // get selected images
            VintasoftImage[] selectedImages = new VintasoftImage[selectedThumbnails.Count];
            for (int i = 0; i < selectedThumbnails.Count; i++)
                selectedImages[i] = selectedThumbnails[i].Source;
            // if images are NOT selected
            if (selectedImages.Length == 0)
                // get current image as selected image
                selectedImages = new VintasoftImage[] { imageViewer.Image };
            // for each selected image
            foreach (VintasoftImage image in selectedImages)
            {
                // get redaction marks of image
                IList<WpfRedactionMark> redactionMarks = _removeContentTool.GetSelection(image);
                // remove redaction marks
                redactionMarks.Clear();
                // create new redaction mark for the whole image
                redactionMarks.Add(new WpfPageRedactionMark(image));
                // if image is focused in image viewer
                if (image == imageViewer.Image)
                {
                    redactionMarks[0].Invalidate(imageViewer);
                    // set focus in viewer to the first redaction mark of image
                    _removeContentTool.FocusedSelectionItem = redactionMarks[0];
                }
            }
        }

        /// <summary>
        /// Deletes the selected redaction mark.
        /// </summary>
        private void deleteSelectedRedactionMarkToolStripButton_Click(object sender, RoutedEventArgs e)
        {
            _removeContentTool.DeleteAction.Execute();
        }

        /// <summary>
        /// Shows a form that allows to edit appearance of redaction mark.
        /// </summary>
        private void redactionMarkAppearancePropertiesToolStripButton_Click(object sender, RoutedEventArgs e)
        {
            RedactionMarkAppearanceEditor editorForm =
                new RedactionMarkAppearanceEditor(CurrentPage.Document,
                    (RedactionMarkAppearanceGraphicsFigure)_removeContentTool.RedactionMarkAppearance);
            editorForm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            editorForm.Owner = Window.GetWindow(this);
            editorForm.ShowDialog();
        }

        #endregion


        #region 'Add Redaction Mark' group

        /// <summary>
        /// Adds the redaction mark to a PDF page.
        /// </summary>
        private void AddRedactionMarkButton_Click(object sender, RoutedEventArgs e)
        {
            // get button
            Button button = (Button)sender;
            // get type of mark
            PdfRedactionMarkType markType = (PdfRedactionMarkType)button.Tag;

            bool setSelectedTextToRedactionMark =
               markType == PdfRedactionMarkType.RemoveText && TextSelectionTool != null && !string.IsNullOrEmpty(TextSelectionTool.SelectedText);

            if (_isFirstBuild && !setSelectedTextToRedactionMark)
            {
                string message =
                    "1. Select the area on page." + Environment.NewLine +
                    "2. Press \"Apply Redaction Marks\" when the redaction marks must be applied to the page.";
                MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                _isFirstBuild = false;
            }

            try
            {
                // if need add text to redaction then 
                if (setSelectedTextToRedactionMark)
                {
                    // add selected text to redaction marks
                    VintasoftImage[] imagesWithSelection = TextSelectionTool.GetSelectionImages();
                    foreach (VintasoftImage image in imagesWithSelection)
                    {
                        TextRegion selectedText = TextSelectionTool.GetSelectedRegion(image);
                        if (!selectedText.IsEmpty)
                        {
                            RemoveContentTool.Add(image, new WpfTextRedactionMark(image, selectedText));
                        }
                    }
                    TextSelectionTool.ClearSelection();
                }
                else
                {
                    // create new mark
                    WpfRedactionMark mark = new WpfRedactionMark(CurrentImage);
                    mark.MarkType = markType;
                    // add and build mark
                    RemoveContentTool.AddAndBuild(mark);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Image is not PDF page.")
                    MessageBox.Show("Current image is not PDF page. Save document and try again.", "Information",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Marks a content of selected page for removal.
        /// </summary>
        private void removeSelectedPageToolStripButton_Click(object sender, RoutedEventArgs e)
        {
            // get current image
            VintasoftImage image = CurrentImage;
            // get redaction marks of image
            IList<WpfRedactionMark> redactionMarks = _removeContentTool.GetSelection(image);
            // clear redation marks
            redactionMarks.Clear();
            // create redaction mark of page
            redactionMarks.Add(new WpfPageRedactionMark(image));
            redactionMarks[0].Invalidate(_removeContentTool.ImageViewer);
            // select new mark
            _removeContentTool.FocusedSelectionItem = redactionMarks[0];
        }

        #endregion


        #region Redaction mark list box

        /// <summary>
        /// Updates the list box of redaction marks.
        /// </summary>
        private void UpdateRedactionMarksListBox()
        {
            redactionMarksListBox.BeginInit();
            try
            {
                redactionMarksListBox.Items.Clear();

                if (_removeContentTool != null)
                {
                    foreach (WpfRedactionMark mark in _removeContentTool.Selection)
                    {
                        ListBoxItem item = new ListBoxItem();
                        item.Content = GetRedactionMarkDescription(mark);
                        redactionMarksListBox.Items.Add(item);
                    }
                }
            }
            finally
            {
                redactionMarksListBox.EndInit();
            }
        }

        /// <summary>
        /// Selected index of list box is changed.
        /// </summary>
        private void redactionMarksListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // get selected index
            int selectedIndex = redactionMarksListBox.SelectedIndex;
            // selected redaction mark
            WpfRedactionMark selectedRedactionMark = null;

            // if redaction mark is selected
            if (selectedIndex != -1)
                // get selected mark
                selectedRedactionMark = _removeContentTool.Selection[selectedIndex];

            // set focused mark
            _removeContentTool.FocusedSelectionItem = selectedRedactionMark;
        }

        #endregion


        #region Visual Tool

        /// <summary>
        /// Visual tool is activated.
        /// </summary>
        private void RemoveContentTool_Activated(object sender, EventArgs e)
        {
            // get visual tool
            WpfVisualTool visualTool = (WpfVisualTool)sender;
            // subscribe to the events of visual tool
            SubscribeToImageViewerEvents(visualTool.ImageViewer);
            UpdateUI();
        }

        /// <summary>
        /// Visual tool is deactivated.
        /// </summary>
        private void RemoveContentTool_Deactivated(object sender, EventArgs e)
        {
            // get visual tool
            WpfVisualTool visualTool = (WpfVisualTool)sender;
            // unsubscribe from events of visual tool
            UnsubscribeFromVisualToolEvents(visualTool.ImageViewer);

            mainPanel.IsEnabled = false;
        }

        /// <summary>
        /// Visual tool selection is changed.
        /// </summary>
        private void RemoveContentTool_SelectionChanged(object sender, EventArgs e)
        {
            UpdateRedactionMarksListBox();
            UpdateUI();
        }

        /// <summary>
        /// Focused rectangle of visual tool is changed.
        /// </summary>
        private void RemoveContentTool_FocusedRectangleChanged(
            object sender,
            PropertyChangedEventArgs<WpfRedactionMark> e)
        {
            // find focused mark
            int index = _removeContentTool.Selection.IndexOf(e.NewValue);

            // if selected index is changed
            if (redactionMarksListBox.SelectedIndex != index)
                redactionMarksListBox.SelectedIndex = index;

            // update user interface
            UpdateUI();
        }

        /// <summary>
        /// Mouse is down.
        /// </summary>
        private void RemoveContentTool_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // if visual tool is enabled and the right mouse button is pressed
            if (IsEnabled && mainPanel.IsEnabled && e.ChangedButton == MouseButton.Right)
            {
                WpfRedactionMark selectedMark = null;
                // get image viewer
                WpfImageViewer imageViewer = _removeContentTool.ImageViewer;
                IList<WpfRedactionMark> selection = _removeContentTool.Selection;
                if (selection.Count > 0)
                {
                    Point location = e.GetPosition(imageViewer);
                    IWpfInteractiveObject interactiveObject;
                    for (int i = selection.Count - 1; i >= 0; i--)
                    {
                        interactiveObject = (IWpfInteractiveObject)selection[i];
                        WpfPointTransform transform = interactiveObject.GetPointTransform(imageViewer, imageViewer.Image).GetInverseTransform();
                        Point point = transform.TransformPoint(location);
                        if (interactiveObject.IsPointOnObject(point))
                        {
                            selectedMark = selection[i];
                            break;
                        }
                    }
                }

                ContextMenu contextMenu = _redactionMarkContextMenu;
                if (selectedMark == null)
                    contextMenu = _imageViewerContextMenu;

                _removeContentTool.FocusedSelectionItem = selectedMark;
                contextMenu.IsOpen = true;
                e.Handled = true;
            }
        }

        #endregion


        #region Context Menu

        #region Remove content control

        /// <summary>
        /// Context menu strip of redaction mark is opening.
        /// </summary>
        private void RedactionMarkContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = (ContextMenu)sender;

            MenuItem cutMenuItem = (MenuItem)menu.Items[0];
            MenuItem copyMenuItem = (MenuItem)menu.Items[1];
            MenuItem pasteMenuItem = (MenuItem)menu.Items[2];
            MenuItem deleteMenuItem = (MenuItem)menu.Items[4];

            cutMenuItem.IsEnabled = _removeContentTool.CutAction.IsEnabled;
            copyMenuItem.IsEnabled = _removeContentTool.CopyAction.IsEnabled;
            pasteMenuItem.IsEnabled = _removeContentTool.PasteAction.IsEnabled;
            deleteMenuItem.IsEnabled = _removeContentTool.DeleteAction.IsEnabled;
        }

        /// <summary>
        /// Cuts redaction mark.
        /// </summary>
        private void cutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _removeContentTool.CutAction.Execute();
        }

        /// <summary>
        /// Copies redaction mark.
        /// </summary>
        private void copyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _removeContentTool.CopyAction.Execute();
        }

        /// <summary>
        /// Pastes redaction mark.
        /// </summary>
        private void pasteMenuItem1_Click(object sender, RoutedEventArgs e)
        {
            _removeContentTool.PasteAction.Execute();
        }

        /// <summary>
        /// Deletes redaction mark.
        /// </summary>
        private void deleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _removeContentTool.DeleteAction.Execute();
        }

        #endregion


        #region Image viewer

        /// <summary>
        /// Focused image is changed.
        /// </summary>
        private void imageViewer_FocusedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Context menu of image viewer is opening.
        /// </summary>
        private void ImageViewerContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = (ContextMenu)sender;

            MenuItem pasteMenuItem = (MenuItem)menu.Items[0];
            MenuItem removeAllMenuItem = (MenuItem)menu.Items[1];

            pasteMenuItem.IsEnabled = _removeContentTool.PasteAction.IsEnabled;
            removeAllMenuItem.IsEnabled = redactionMarksListBox.Items.Count > 0;
        }

        /// <summary>
        /// Pastes a redaction mark.
        /// </summary>
        private void imageViewerPasteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _removeContentTool.PasteAction.Execute();
        }

        /// <summary>
        /// Removes all redaction marks.
        /// </summary>
        private void imageViewerRemoveAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _removeContentTool.Selection.Clear();
        }

        #endregion

        #endregion


        #region Common

        /// <summary>
        /// Returns the description of redaction mark.
        /// </summary>
        /// <param name="mark">The mark.</param>
        private string GetRedactionMarkDescription(WpfRedactionMark mark)
        {
            return string.Format("{0}", mark.MarkType);
        }

        #endregion

        #endregion



        #region Events

        /// <summary>
        /// Occurs when redaction mark is applied.
        /// </summary>
        public event EventHandler RedactionMarkApplied;

        #endregion

    }
}
