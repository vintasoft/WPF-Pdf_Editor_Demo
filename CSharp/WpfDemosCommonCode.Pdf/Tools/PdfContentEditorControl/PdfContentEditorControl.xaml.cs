using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Win32;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Drawing.Gdi;
#if !REMOVE_OFFICE_PLUGIN
using Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction;
#endif
using Vintasoft.Imaging.Pdf.Content;
using Vintasoft.Imaging.Pdf.Drawing;
using Vintasoft.Imaging.Pdf.Drawing.GraphicsFigures;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Fonts;
using Vintasoft.Imaging.Pdf.Wpf.UI;
using Vintasoft.Imaging.Undo;
using Vintasoft.Imaging.Wpf;
using Vintasoft.Imaging.Wpf.UI;
using Vintasoft.Imaging.Wpf.UI.UIElements;
using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;

using WpfDemosCommonCode.CustomControls;
using WpfDemosCommonCode.Imaging;
using WpfDemosCommonCode.Imaging.Codecs;
using WpfDemosCommonCode.Office;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to create and manage graphics figures.
    /// Created graphics figures can be added to the content of PDF page.
    /// </summary>
    public partial class PdfContentEditorControl : UserControl
    {

        #region Fields

        /// <summary>
        /// The text of text box.
        /// </summary>
        string _addText = "";

        /// <summary>
        /// The pen of graphics figure.
        /// </summary>
        PdfPen _pen;

        /// <summary>
        /// The brush of graphics figure.
        /// </summary>
        PdfBrush _brush;

        /// <summary>
        /// The selected mode of higlight pen.
        /// </summary>
        int _selectHighlightPenDialogModeIndex = 0;

        /// <summary>
        /// The open file dialog that allows to open image file.
        /// </summary>
        OpenFileDialog _openImageFileDialog = new OpenFileDialog();

        /// <summary>
        /// The context menu of the image viewer.
        /// </summary>
        ContextMenu _imageViewerContextMenu = null;

        /// <summary>
        /// The context menu of the figure.
        /// </summary>
        ContextMenu _figureViewContextMenu = null;

#if !REMOVE_OFFICE_PLUGIN
        /// <summary>
        /// The last built chart figure.
        /// </summary>
        OfficeDocumentFigure _chartFigure;

        /// <summary>
        /// The last built formatted text figure.
        /// </summary>
        OfficeDocumentFigure _formattedTextFigure;

        /// <summary>
        /// The tooltip that displays text editing errors.
        /// </summary>
        ToolTip _textEditingExceptionToolTip = new ToolTip();
#endif

        /// <summary>
        /// Text timer that closes the tooltip that displays text editing errors.
        /// </summary>
        Timer _tooltipClosingTimer;

        /// <summary>
        /// The location of context menu.
        /// </summary>
        Point _contextMenuLocation;

        /// <summary>
        /// A value indicating whether undo or redo operation is executing.
        /// </summary>
        bool _isUndoRedoExecuting = false;

        /// <summary>
        /// Dictionary: tool strip button => figure content type.
        /// </summary>
        Dictionary<ToolBarButton, GraphicsFigureContentType> buttonToFigureContentType =
            new Dictionary<ToolBarButton, GraphicsFigureContentType>();

        /// <summary>
        /// A value indicating whether the figure content type buttons are updating.
        /// </summary>
        bool _isFigureContentTypeButtonUpdating = false;

        /// <summary>
        /// The undo manager.
        /// </summary>
        UndoManager _undoManager = new UndoManager(20);

        /// <summary>
        /// The undo monitor.
        /// </summary>
        WpfPdfContentEditorToolUndoMonitor _undoMonitor;

        public static RoutedCommand _undoCommand = new RoutedCommand();
        public static RoutedCommand _redoCommand = new RoutedCommand();

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfContentEditorControl"/> class.
        /// </summary>
        public PdfContentEditorControl()
        {
            InitializeComponent();

            CodecsFileFilters.SetFilters(_openImageFileDialog);

            buttonToFigureContentType.Add(textContentToolBarButton, GraphicsFigureContentType.Text);
            buttonToFigureContentType.Add(imageContentToolBarButton, GraphicsFigureContentType.Image);
            buttonToFigureContentType.Add(strokePathContentToolBarButton, GraphicsFigureContentType.StrokePath);
            buttonToFigureContentType.Add(fillPathContentToolBarButton, GraphicsFigureContentType.FillPath);
            buttonToFigureContentType.Add(shadingFillContentToolBarButton, GraphicsFigureContentType.ShadingFill);
            buttonToFigureContentType.Add(clipContentToolBarButton, GraphicsFigureContentType.SetClip);
            buttonToFigureContentType.Add(formContentToolBarButton, GraphicsFigureContentType.Form);

            UpdateUI();

            _imageViewerContextMenu = (ContextMenu)Resources["imageViewerContextMenu"];
            _figureViewContextMenu = (ContextMenu)Resources["figureViewContextMenu"];

            _undoManager.Changed += UndoManager_Changed;

            _tooltipClosingTimer = new Timer(ToolTipClosingCallBack, null, Timeout.Infinite, Timeout.Infinite);
        }

        #endregion



        #region Properties

        WpfPdfContentEditorTool _contentEditorTool = null;
        /// <summary>
        /// Gets or sets the content editor tool.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public WpfPdfContentEditorTool ContentEditorTool
        {
            get
            {
                return _contentEditorTool;
            }
            set
            {
                // if content editor tool is not empty
                if (_contentEditorTool != null)
                {
                    // unsubscribe from events of content editor tool
                    UnsubscribeFromVisualToolEvents(_contentEditorTool);

                    // clear undo manager
                    _undoManager.Clear();

                    if (_undoMonitor != null)
                    {
                        _undoMonitor.Dispose();
                        _undoMonitor = null;
                    }
                }

                // set new content editor tool
                _contentEditorTool = value;

                // if content editor tool is not empty
                if (_contentEditorTool != null)
                {
                    _contentEditorTool.AppendMode = false;
                    _contentEditorTool.FiguresHighlight = true;
                    // set InteractiveContentType
                    _contentEditorTool.InteractiveContentType =
                        GraphicsFigureContentType.Text |
                        GraphicsFigureContentType.Image |
                        GraphicsFigureContentType.Form |
                        GraphicsFigureContentType.StrokePath |
                        GraphicsFigureContentType.FillPath;
                    // subscribe to the events of content editor tool
                    SubscribeToVisualToolEvents(_contentEditorTool);
                    // update the list box
                    UpdateFiguresListBox();

                    // create undo monitor
                    _contentEditorTool.UndoManager = _undoManager;
                    _undoMonitor = new WpfPdfContentEditorToolUndoMonitor(_undoManager, _contentEditorTool);
                }

                UpdateUI();
            }
        }

        /// <summary>
        /// Gets the PdfPage, which is associated with an image focused in image viewer.
        /// </summary>
        private PdfPage FocusedPage
        {
            get
            {
                if (_contentEditorTool != null)
                    return _contentEditorTool.CurrentPage;

                return null;
            }
        }

#if !REMOVE_OFFICE_PLUGIN
        WpfOfficeDocumentVisualEditor _documentVisualEditor;
        /// <summary>
        /// Gets or sets the visual editor for Office document.
        /// </summary>
        WpfOfficeDocumentVisualEditor DocumentVisualEditor
        {
            get
            {
                return _documentVisualEditor;
            }
            set
            {
                if (_documentVisualEditor != null)
                    _documentVisualEditor.EditingException -= VisualEditor_EditingException;

                _documentVisualEditor = value;

                if (_documentVisualEditor != null)
                    _documentVisualEditor.EditingException += VisualEditor_EditingException;
            }
        }
#endif

        #endregion



        #region Methods

        /// <summary>
        /// Updates the user interface of this control.
        /// </summary>
        private void UpdateUI()
        {
            bool isPdfPage = FocusedPage != null;

            mainPanel.IsEnabled = _contentEditorTool != null && isPdfPage;

            if (mainPanel.IsEnabled)
            {
                figuresGroupBox.IsEnabled = isPdfPage;
                renderFiguresButton.IsEnabled = _contentEditorTool != null && _contentEditorTool.GetNonContentFigures().Length > 0;
                removeButton.IsEnabled = _contentEditorTool != null && _contentEditorTool.SelectedFigure != null;
                removeAllButton.IsEnabled = figuresListBox.Items.Count > 0;
                appendModeToolBarButton.IsChecked = _contentEditorTool != null && _contentEditorTool.AppendMode;
                groupContentFiguresToolBarButton.IsChecked = _contentEditorTool != null && _contentEditorTool.GroupContentFigures;
                highlightToolBarButton.IsChecked = _contentEditorTool != null && _contentEditorTool.FiguresHighlight;
                bool graphicalContentFigureSelected = false;
                bool canEditTextFigure = false;
                if (_contentEditorTool != null)
                {
                    if (_contentEditorTool.SelectedFigure is ContentStreamGraphicsFigure)
                        graphicalContentFigureSelected = true;
                    else if (_contentEditorTool.SelectedFigure is ContentStreamGraphicsFigureGroup)
                        graphicalContentFigureSelected = (_contentEditorTool.SelectedFigure.ContentType & GraphicsFigureContentType.Text) == 0;

                    canEditTextFigure = CanEditTextFigure(_contentEditorTool.SelectedFigureView);

                    _isFigureContentTypeButtonUpdating = true;
                    foreach (ToolBarButton toolStripButton in buttonToFigureContentType.Keys)
                    {
                        if (_contentEditorTool.InteractiveContentType == GraphicsFigureContentType.Any ||
                            (_contentEditorTool.InteractiveContentType & buttonToFigureContentType[toolStripButton]) != 0)
                            toolStripButton.IsChecked = true;
                        else
                            toolStripButton.IsChecked = false;
                    }
                    _isFigureContentTypeButtonUpdating = false;
                }
                verticalMirrorToolBarButton.IsEnabled = graphicalContentFigureSelected;
                horizontalMirrorToolBarButton.IsEnabled = graphicalContentFigureSelected;
                rotateClockwiseToolBarButton.IsEnabled = graphicalContentFigureSelected;
                rotateCounterclockwiseToolBarButton.IsEnabled = graphicalContentFigureSelected;
                addRectangleClipToolBarButton.IsEnabled = graphicalContentFigureSelected && (_contentEditorTool.SelectedFigure.ContentType & GraphicsFigureContentType.SetClip) == 0;
                addEllipseClipToolBarButton.IsEnabled = addRectangleClipToolBarButton.IsEnabled;
                replaceResourceToolBarButton.IsEnabled = graphicalContentFigureSelected && GetXObjectFigure(_contentEditorTool.SelectedFigure) != null;
                editTextObjectToolBarButton.IsEnabled = canEditTextFigure;
                contentGraphicsPropertiesToolBarButton.IsEnabled = _contentEditorTool != null && CanEditContentGraphicsProperties(_contentEditorTool.SelectedFigure);

                UpdateUndoUI();
            }
        }

        /// <summary>
        /// Returns a value indicating whether the content graphics properties of specified figure can be edited.
        /// </summary>
        /// <param name="figure">The figure.</param>
        /// <returns>
        /// <b>True</b> if content graphics properties of specified figure can be edited; otherwise, <b>false</b>.
        /// </returns>
        private bool CanEditContentGraphicsProperties(GraphicsFigure figure)
        {
            if (figure != null && figure.IsContentFigure)
            {
                if ((figure.ContentType & GraphicsFigureContentType.FillPath) != 0)
                    return true;
                if ((figure.ContentType & GraphicsFigureContentType.StrokePath) != 0)
                    return true;
                if ((figure.ContentType & GraphicsFigureContentType.Text) != 0)
                    return true;
                if ((figure.ContentType & GraphicsFigureContentType.Image) != 0)
                    return true;
                if ((figure.ContentType & GraphicsFigureContentType.Form) != 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a value indicating whether text of specified graphics figure can be edited.
        /// </summary>
        /// <param name="figureView">The figure view.</param>
        /// <returns>A value indicating whether text of specified graphics figure can be edited.</returns>
        private bool CanEditTextFigure(WpfGraphicsFigureView figureView)
        {
#if !REMOVE_OFFICE_PLUGIN
            if (figureView is WpfOfficeContentStreamGraphicsFigureTextGroupView)
                return true;
            if (figureView is WpfOfficeDocumentFigureView)
                return true;
#endif
            if (figureView is WpfContentStreamGraphicsFigureView &&
                figureView.Figure.ContentType == GraphicsFigureContentType.Text)
                return true;
            return false;
        }

        /// <summary>
        /// Subscribes to the events of content editor tool.
        /// </summary>
        /// <param name="contentEditorTool">The content editor tool.</param>
        private void SubscribeToVisualToolEvents(WpfPdfContentEditorTool contentEditorTool)
        {
            contentEditorTool.Activated += new EventHandler(ContentEditorTool_Activated);
            contentEditorTool.Deactivated += new EventHandler(ContentEditorTool_Deactivated);
            contentEditorTool.SelectedFigureViewChanged += new EventHandler(ContentEditorTool_SelectedFigureViewChanged);
            contentEditorTool.FigureViewCollectionChanged += new EventHandler(ContentEditorTool_FigureViewCollectionChanged);
            contentEditorTool.MouseDoubleClick += new MouseButtonEventHandler(ContentEditorTool_MouseDoubleClick);
            contentEditorTool.FigureBuildingFinished += ContentEditorTool_FigureBuildingFinished;
            contentEditorTool.ActiveInteractionControllerChanged += ContentEditorTool_ActiveInteractionControllerChanged;

            if (contentEditorTool.ImageViewer != null)
                SubscribeToImageViewerEvents(contentEditorTool.ImageViewer);
        }

        /// <summary>
        /// Unsubscribes from the events of content editor tool.
        /// </summary>
        /// <param name="contentEditorTool">The content editor tool.</param>
        private void UnsubscribeFromVisualToolEvents(WpfPdfContentEditorTool contentEditorTool)
        {
            contentEditorTool.Activated -= ContentEditorTool_Activated;
            contentEditorTool.Deactivated -= ContentEditorTool_Deactivated;
            contentEditorTool.SelectedFigureViewChanged += ContentEditorTool_SelectedFigureViewChanged;
            contentEditorTool.FigureViewCollectionChanged -= ContentEditorTool_FigureViewCollectionChanged;
            contentEditorTool.MouseDoubleClick -= ContentEditorTool_MouseDoubleClick;
            contentEditorTool.FigureBuildingFinished -= ContentEditorTool_FigureBuildingFinished;
            contentEditorTool.ActiveInteractionControllerChanged -= ContentEditorTool_ActiveInteractionControllerChanged;

            if (contentEditorTool.ImageViewer != null)
                UnsubscribeFromImageViewerEvents(contentEditorTool.ImageViewer);
        }

        /// <summary>
        /// Subscribes to the events of image viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer.</param>
        private void SubscribeToImageViewerEvents(WpfImageViewer imageViewer)
        {
            imageViewer.FocusedIndexChanged += new PropertyChangedEventHandler<int>(ImageViewer_FocusedIndexChanged);
            imageViewer.FocusedIndexChanging += new PropertyChangedEventHandler<int>(imageViewer_FocusedIndexChanging);
            imageViewer.MouseRightButtonUp += ImageViewer_MouseDown;
            imageViewer.PreviewKeyUp += ImageViewer_PreviewKeyUp;
        }

        /// <summary>
        /// Unsubscribes from the events of image viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer.</param>
        private void UnsubscribeFromImageViewerEvents(WpfImageViewer imageViewer)
        {
            imageViewer.FocusedIndexChanged -= ImageViewer_FocusedIndexChanged;
            imageViewer.FocusedIndexChanging -= imageViewer_FocusedIndexChanging;
            imageViewer.MouseRightButtonUp -= ImageViewer_MouseDown;
            imageViewer.PreviewKeyUp -= ImageViewer_PreviewKeyUp;
        }

        /// <summary>
        /// Focused image is changing.
        /// </summary>
        private void imageViewer_FocusedIndexChanging(object sender, PropertyChangedEventArgs<int> e)
        {
            if (figuresListBox.Items.Count > 0)
                figuresListBox.Items.Clear();
        }

        /// <summary>
        /// Focused image is changed.
        /// </summary>
        private void ImageViewer_FocusedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Renders figures on a PDF page.
        /// </summary>
        private void renderFiguresButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _contentEditorTool.RenderFiguresOnPage();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }


        /// <summary>
        /// Handles the Click event of addRectangleClipToolBarButton object.
        /// </summary>
        private void addRectangleClipToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            WpfGraphicsFigureView figureView = ContentEditorTool.SelectedFigureView;
            using (GdiGraphicsPath clipPath = new GdiGraphicsPath())
            {
                clipPath.AddPolygon(figureView.Figure.GetRegion().ToPolygon());
                ContentEditorTool.AddFigureClip(ContentEditorTool.SelectedFigureView, clipPath);
            }
        }

        /// <summary>
        /// Handles the Click event of addEllipseClipToolBarButton object.
        /// </summary>
        private void addEllipseClipToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            WpfGraphicsFigureView figureView = ContentEditorTool.SelectedFigureView;
            using (GdiGraphicsPath clipPath = new GdiGraphicsPath())
            {
                clipPath.AddEllipse(figureView.Figure.GetRegion().Bounds);
                ContentEditorTool.AddFigureClip(ContentEditorTool.SelectedFigureView, clipPath);
            }
        }

        /// <summary>
        /// Handles the Click event of contentGraphicsPropertiesToolStripMenuItem object.
        /// </summary>
        private void contentGraphicsPropertiesToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            EditContentGraphicsProperties(_contentEditorTool.SelectedFigureView);
        }

        /// <summary>
        /// Handles the Click event of appendModeToolBarButton object.
        /// </summary>
        private void appendModeToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            appendModeToolBarButton.IsChecked = !appendModeToolBarButton.IsChecked;
            if (ContentEditorTool != null)
                ContentEditorTool.AppendMode = appendModeToolBarButton.IsChecked;
        }

        /// <summary>
        /// Handles the Click event of groupContentFiguresToolBarButton object.
        /// </summary>
        private void groupContentFiguresToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            groupContentFiguresToolBarButton.IsChecked = !groupContentFiguresToolBarButton.IsChecked;
            if (ContentEditorTool != null)
                ContentEditorTool.GroupContentFigures = groupContentFiguresToolBarButton.IsChecked;
        }
        /// <summary>
        /// Handles the Click event of highlightToolBarButton object.
        /// </summary>
        private void highlightToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            highlightToolBarButton.IsChecked = !highlightToolBarButton.IsChecked;
            if (ContentEditorTool != null)
                ContentEditorTool.FiguresHighlight = highlightToolBarButton.IsChecked;
        }

        /// <summary>
        /// Handles the Click event of horizontalMirrorToolBarButton object.
        /// </summary>
        private void horizontalMirrorToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            WpfGraphicsFigureView figureView = ContentEditorTool.SelectedFigureView;
            RegionF region = figureView.Figure.GetRegion();
            AffineMatrix transform = AffineMatrix.CreateScaling(-1, 1, region.Center.X, region.Center.Y);
            ContentEditorTool.TransformFigure(figureView, transform);
        }

        /// <summary>
        /// Handles the Click event of verticalMirrorToolBarButton object.
        /// </summary>
        private void verticalMirrorToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            WpfGraphicsFigureView figureView = ContentEditorTool.SelectedFigureView;
            RegionF region = figureView.Figure.GetRegion();
            AffineMatrix transform = AffineMatrix.CreateScaling(1, -1, region.Center.X, region.Center.Y);
            ContentEditorTool.TransformFigure(figureView, transform);
        }

        /// <summary>
        /// Handles the Click event of rotateClockwiseToolBarButton object.
        /// </summary>
        private void rotateClockwiseToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            WpfGraphicsFigureView figureView = ContentEditorTool.SelectedFigureView;
            RegionF region = figureView.Figure.GetRegion();
            AffineMatrix transform = AffineMatrix.CreateRotation(-90, region.Center.X, region.Center.Y);
            ContentEditorTool.TransformFigure(figureView, transform);
        }

        /// <summary>
        /// Handles the Click event of rotateCounterclockwiseToolBarButton object.
        /// </summary>
        private void rotateCounterclockwiseToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            WpfGraphicsFigureView figureView = ContentEditorTool.SelectedFigureView;
            RegionF region = figureView.Figure.GetRegion();
            AffineMatrix transform = AffineMatrix.CreateRotation(90, region.Center.X, region.Center.Y);
            ContentEditorTool.TransformFigure(figureView, transform);
        }

        /// <summary>
        /// Handles the Click event of replaceResourceToolBarButton object.
        /// </summary>
        private void replaceResourceToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            WpfGraphicsFigureView figureView = _contentEditorTool.SelectedFigureView;
            ContentStreamGraphicsFigure figure = GetXObjectFigure(figureView.Figure);
            PdfResourcesViewerWindow form = new PdfResourcesViewerWindow(_contentEditorTool.CurrentPage.Document, true);
            form.DocumentResourceViewer.SelectedResource = figure.Resource;
            if (form.ShowDialog() == true)
            {
                if (form.SelectedResource != null)
                    _contentEditorTool.ReplaceResource(figure, form.SelectedResource);
            }
        }

        /// <summary>
        /// Handles the Click event of contentGraphicsPropertiesToolBarButton object.
        /// </summary>
        private void contentGraphicsPropertiesToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            EditContentGraphicsProperties(_contentEditorTool.SelectedFigureView);
        }

        /// <summary>
        /// Handles the Click event of interactiveContentTypeToolBarButton object.
        /// </summary>
        private void interactiveContentTypeToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            ToolBarButton button = (ToolBarButton)sender;

            button.IsChecked = !button.IsChecked;

            if (_contentEditorTool != null && !_isFigureContentTypeButtonUpdating)
            {
                GraphicsFigureContentType interactiveContentType = GraphicsFigureContentType.NoContent;
                bool isAllButtonsChecked = true;

                // get figure content type

                foreach (ToolBarButton toolStripButton in buttonToFigureContentType.Keys)
                {
                    if (toolStripButton.IsChecked)
                        interactiveContentType |= buttonToFigureContentType[toolStripButton];
                    else
                        isAllButtonsChecked = false;
                }

                if (isAllButtonsChecked)
                    interactiveContentType = GraphicsFigureContentType.Any;

                // if figure content type must be updated
                if (_contentEditorTool.InteractiveContentType != interactiveContentType)
                {
                    // update figure content type
                    _contentEditorTool.InteractiveContentType = interactiveContentType;

                    UpdateFiguresListBox();

                    // updte UI
                    UpdateUI();
                }
            }
        }

        /// <summary>
        /// Handles the MouseDown event of interactiveContentTypeToolBarButton object.
        /// </summary>
        private void interactiveContentTypeToolBarButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // if right mouse button is pressed
            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (_contentEditorTool != null && !_isFigureContentTypeButtonUpdating)
                {
                    // create empty value
                    GraphicsFigureContentType interactiveContentType = GraphicsFigureContentType.NoContent;
                    // set selected figure content type
                    interactiveContentType |= buttonToFigureContentType[(ToolBarButton)sender];
                    // update value in visual tool
                    _contentEditorTool.InteractiveContentType = interactiveContentType;

                    UpdateFiguresListBox();

                    // updte UI
                    UpdateUI();
                }
            }
        }

        #region Graphics Figure List Box

        /// <summary>
        /// Updates the list box of graphics figures.
        /// </summary>
        private void UpdateFiguresListBox()
        {
            figuresListBox.BeginInit();
            try
            {
                // clear list box of graphics figures
                figuresListBox.Items.Clear();

                foreach (WpfGraphicsFigureView view in _contentEditorTool.FigureViewCollection)
                {
                    if (view.IsEnabled)
                    {
                        // add item to the listbox
                        figuresListBox.Items.Add(new GraphicsFigureViewItem(view));
                    }
                }
            }
            finally
            {
                figuresListBox.EndInit();
            }
        }

        /// <summary>
        /// Returns the description of graphics figure.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns>The description of graphics figure.</returns>
        private string GetGraphicsFigureViewDescription(WpfGraphicsFigureView view)
        {
            return view.Figure.GetType().Name;
        }

        /// <summary>
        /// Selected figure in figures list is changed.
        /// </summary>
        private void figuresListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (figuresListBox.SelectedIndex == -1)
                _contentEditorTool.SelectedFigureView = null;
            else
                _contentEditorTool.SelectedFigureView = ((GraphicsFigureViewItem)figuresListBox.SelectedItem).FigureView;
        }

        #endregion


        #region ContentEditorTool

        /// <summary>
        /// Handles the Click event of editTextObjectToolBarButton object.
        /// </summary>
        private void editTextObjectToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            WpfGraphicsFigureView figureView = _contentEditorTool.SelectedFigureView;
#if !REMOVE_OFFICE_PLUGIN
            if (figureView is WpfOfficeContentStreamGraphicsFigureTextGroupView)
            {
                WpfOfficeDocumentFigureView view = ((WpfOfficeContentStreamGraphicsFigureTextGroupView)_contentEditorTool.SelectedFigureView).ConvertToOfficeDocumentFigure(_contentEditorTool.ImageViewer);
                view.DocumentVisualEditor.EnableEditing(null);
            }
            else if (figureView is WpfOfficeDocumentFigureView)
            {
                ((WpfOfficeDocumentFigureView)figureView).DocumentVisualEditor.EnableEditing(null);
            }
            else
#endif
            if (figureView is WpfContentStreamGraphicsFigureView && figureView.Figure.ContentType == GraphicsFigureContentType.Text)
            {
                ((WpfContentStreamGraphicsFigureView)figureView).EnableTextEditing(_contentEditorTool.ImageViewer);
            }
        }

        /// <summary>
        /// Handles the FigureBuildingFinished event of the ContentEditorTool.
        /// </summary>
        private void ContentEditorTool_FigureBuildingFinished(object sender, EventArgs e)
        {
#if !REMOVE_OFFICE_PLUGIN
            // if figure is chart figure
            if (_contentEditorTool.SelectedFigure == _chartFigure)
            {
                // show dialog that allows to edit chart data
                _contentEditorTool.SelectedFigureView.InteractionController = _contentEditorTool.SelectedFigureView.Transformer;
                Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction.WpfOfficeDocumentVisualEditor visualEditor =
                    WpfCompositeInteractionController.FindInteractionController<Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction.WpfOfficeDocumentVisualEditor>(_contentEditorTool.SelectedFigureView.InteractionController);
                if (visualEditor != null)
                {
                    OpenXmlDocumentChartDataWindow chartForm = new OpenXmlDocumentChartDataWindow();
                    chartForm.WindowStartupLocation = WindowStartupLocation.Manual;
                    chartForm.Left = Window.GetWindow(this).Left;
                    chartForm.Top = Window.GetWindow(this).Top;
                    chartForm.VisualEditor = visualEditor;
                    chartForm.ShowDialog();
                }
            }
            // if figure is formatted text figure
            else if (_contentEditorTool.SelectedFigure == _formattedTextFigure)
            {
                // start edit of formatted text
                _contentEditorTool.SelectedFigureView.InteractionController = _contentEditorTool.SelectedFigureView.Transformer;
                Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction.WpfOfficeDocumentVisualEditor visualEditor =
                    WpfCompositeInteractionController.FindInteractionController<Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction.WpfOfficeDocumentVisualEditor>(_contentEditorTool.SelectedFigureView.InteractionController);
                if (visualEditor != null)
                {
                    visualEditor.EnableEditing(null);
                }
            }
#endif
        }

        /// <summary>
        /// The content editor tool is activated.
        /// </summary>
        private void ContentEditorTool_Activated(object sender, EventArgs e)
        {
            // update list box
            UpdateFiguresListBox();

            // get the content editor tool
            WpfPdfContentEditorTool visualTool = (WpfPdfContentEditorTool)sender;
            // subscribe to events of content editor tool
            SubscribeToImageViewerEvents(visualTool.ImageViewer);

            // update user interface
            UpdateUI();
        }

        /// <summary>
        /// The content editor tool tool is deactivated.
        /// </summary>
        private void ContentEditorTool_Deactivated(object sender, EventArgs e)
        {
            // clear list box
            figuresListBox.Items.Clear();

            // get the content editor tool
            WpfPdfContentEditorTool visualTool = (WpfPdfContentEditorTool)sender;
            // unsubscribe from events of content editor tool
            UnsubscribeFromImageViewerEvents(visualTool.ImageViewer);

            // update user interface
            UpdateUI();
        }

        /// <summary>
        /// Selected view of figure is changed.
        /// </summary>
        private void ContentEditorTool_SelectedFigureViewChanged(object sender, EventArgs e)
        {
            // if control is enabled
            if (IsEnabled && mainPanel.IsEnabled)
            {
                WpfGraphicsFigureView selectedView = _contentEditorTool.SelectedFigureView;
                foreach (GraphicsFigureViewItem item in figuresListBox.Items)
                {
                    if (item.FigureView == selectedView)
                    {
                        figuresListBox.SelectedItem = item;
                        break;
                    }
                }
                UpdateUI();
            }
        }

        /// <summary>
        /// Figure view collection is changed.
        /// </summary>
        private void ContentEditorTool_FigureViewCollectionChanged(object sender, EventArgs e)
        {
            UpdateFiguresListBox();
            UpdateUI();
        }

        /// <summary>
        /// Key is up.
        /// </summary>
        private void ImageViewer_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Z)
                {
                    Undo();
                    e.Handled = true;
                }
                else if (e.Key == Key.Y)
                {
                    Redo();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Mouse is down.
        /// </summary>
        private void ImageViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // if control is enabled and the right mouse button is pressed
            if (IsEnabled && mainPanel.IsEnabled && e.ChangedButton == MouseButton.Right)
            {
                // get image viewer
                WpfImageViewer viewer = ContentEditorTool.ImageViewer;
                // get focused page
                PdfPage page = FocusedPage;

                // convert point to the image space
                System.Windows.Point point = e.GetPosition(viewer);

                // update location of context menu
                _contextMenuLocation = WpfPointAffineTransform.TransformPoint(viewer.GetTransformFromControlToVisualToolSpace(), e.GetPosition(viewer));

                // transform point from viewer to the image space
                point = viewer.PointFromControlToImage(point);

                // transform point from image to the PDF page space
                point = WpfPointAffineTransform.TransformPoint(page.GetTransformFromImageSpaceToPageSpace(viewer.Image.Resolution), point);

                // get selected view
                WpfGraphicsFigureView view = _contentEditorTool.SelectedFigureView;
                // if view is empty 
                if (view == null || !view.IsPointOnObject(point))
                {
                    view = null;
                    // find view
                    foreach (WpfGraphicsFigureView currentView in _contentEditorTool.FigureViewCollection)
                    {
                        if (currentView.IsPointOnObject(point))
                        {
                            view = currentView;
                            break;
                        }
                    }
                }

                // update selected figure
                _contentEditorTool.SelectedFigureView = view;

                // if view is empty
                if (view == null)
                    // show context menu for image viewer
                    _imageViewerContextMenu.IsOpen = true;
                else
                    // show context menu for graphics figure
                    _figureViewContextMenu.IsOpen = true;
            }
        }

        /// <summary>
        /// Mouse is double clicked.
        /// </summary>
        private void ContentEditorTool_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // if control is enabled and figure is selected
            if (IsEnabled && mainPanel.IsEnabled && _contentEditorTool.SelectedFigureView != null)
            {
#if !REMOVE_OFFICE_PLUGIN
                if (_contentEditorTool.SelectedFigureView is WpfOfficeDocumentFigureView)
                    return;
#endif

                // if figure is building
                if (_contentEditorTool.SelectedFigureView.Builder != null &&
                     _contentEditorTool.SelectedFigureView.Builder.IsInteracting)
                    return;

                ContentStreamGraphicsFigureTextGroup textGroup = _contentEditorTool.SelectedFigureView.Figure as ContentStreamGraphicsFigureTextGroup;
                ContentStreamGraphicsFigure contentFigure = _contentEditorTool.SelectedFigureView.Figure as ContentStreamGraphicsFigure;
                ContentStreamGraphicsFigure contentXObjectFigure = GetXObjectFigure(_contentEditorTool.SelectedFigureView.Figure);

                // if content figure is form or image
                if (contentXObjectFigure != null)
                {
                    PdfResourcesViewerWindow form = new PdfResourcesViewerWindow(_contentEditorTool.CurrentPage.Document, true);
                    form.DocumentResourceViewer.SelectedResource = contentXObjectFigure.Resource;
                    if (form.ShowDialog() == true)
                    {
                        if (form.SelectedResource != null)
                            _contentEditorTool.ReplaceResource(contentXObjectFigure, form.SelectedResource);
                    }
                }
                // if content figure is text
                else if (textGroup != null || (contentFigure != null && contentFigure.IsText))
                {
                    // text editor is shown
                }
                else if (CanEditContentGraphicsProperties(_contentEditorTool.SelectedFigure) &&
                    _contentEditorTool.SelectedFigure is ContentStreamGraphicsFigure)
                {
                    // edit content graphics properties
                    EditContentGraphicsProperties(_contentEditorTool.SelectedFigureView);
                }
                else
                {
                    // show property grid of selected figure
                    ShowCurrentFigureSettingDialog();
                }
            }
        }

        /// <summary>
        /// Edits the content graphics properties.
        /// </summary>
        /// <param name="figure">The figure.</param>
        private void EditContentGraphicsProperties(WpfGraphicsFigureView figureView)
        {
            ContentStreamGraphicsFigure contentFigure = figureView.Figure as ContentStreamGraphicsFigure;
            if (contentFigure != null && contentFigure.Resource != null)
            {
                PdfResourceGraphicsPropertiesWindow dialog = new PdfResourceGraphicsPropertiesWindow();
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dialog.Owner = Window.GetWindow(this);
                dialog.GraphicsProperties = new PdfContentGraphicsProperties(contentFigure);
                if (dialog.ShowDialog() == true)
                {
                    ContentEditorTool.SetContentGraphicsProperties(figureView, dialog.GraphicsProperties);
                }
            }
            else
            {
                PdfContentGraphicsPropertiesWindow dialog = new PdfContentGraphicsPropertiesWindow();
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dialog.Owner = Window.GetWindow(this);
                if (figureView.Figure is ContentStreamGraphicsFigure)
                    dialog.GraphicsProperties = new PdfContentGraphicsProperties((ContentStreamGraphicsFigure)figureView.Figure);
                if (dialog.ShowDialog() == true)
                {
                    ContentEditorTool.SetContentGraphicsProperties(figureView, dialog.GraphicsProperties);
                }
            }
        }

        /// <summary>
        /// Handles the ActiveInteractionControllerChanged event of ContentEditorTool object.
        /// </summary>
        private void ContentEditorTool_ActiveInteractionControllerChanged(object sender, PropertyChangedEventArgs<IWpfInteractionController> e)
        {
#if !REMOVE_OFFICE_PLUGIN
            DocumentVisualEditor = WpfCompositeInteractionController.FindInteractionController<WpfOfficeDocumentVisualEditor>(e.NewValue);
#endif
        }

#if !REMOVE_OFFICE_PLUGIN
        /// <summary>
        /// Handles the EditingException event of VisualEditor object.
        /// </summary>
        private void VisualEditor_EditingException(object sender, ExceptionEventArgs e)
        {
            if (_contentEditorTool != null && _documentVisualEditor != null)
            {
                WpfImageViewer imageViewer = _contentEditorTool.ImageViewer;

                // get text caret position in text space
                System.Drawing.PointF caretLocationInTextSpace = _documentVisualEditor.TextTool.FocusedTextSymbol.Region.LeftTop;

                // get transformation from text space to the image viewer space
                AffineMatrix fromTextToImage = _documentVisualEditor.TextTool.FocusedTextRegion.GetTransformFromTextToImageSpace(imageViewer.Image.Resolution);
                AffineMatrix fromImageToViewer = _contentEditorTool.ImageViewer.GetTransformFromImageToControl(imageViewer.Image);
                AffineMatrix fromTextToViewer = AffineMatrix.Multiply(fromTextToImage, fromImageToViewer);

                // apply transform to the caret location point
                System.Drawing.PointF caretLocationInViewerSpace = PointFAffineTransform.TransformPoint(fromTextToViewer, caretLocationInTextSpace);
                Point toolTipLocation = new Point((int)caretLocationInViewerSpace.X, (int)caretLocationInViewerSpace.Y);

                // set tooltip properties

                _textEditingExceptionToolTip.Content = e.Exception.Message;
                ToolTipService.SetHorizontalOffset(_textEditingExceptionToolTip, toolTipLocation.X);
                ToolTipService.SetVerticalOffset(_textEditingExceptionToolTip, toolTipLocation.Y);
                ToolTipService.SetPlacementTarget(_textEditingExceptionToolTip, _contentEditorTool.ImageViewer);
                ToolTipService.SetPlacement(_textEditingExceptionToolTip, System.Windows.Controls.Primitives.PlacementMode.RelativePoint);

                // bind tooltip to the selected content object
                ToolTipService.SetToolTip(_contentEditorTool.SelectedFigureView, _textEditingExceptionToolTip);

                // show tooltip
                _textEditingExceptionToolTip.IsOpen = true;

                // start the timer that closes the tooltip that displays text editing error
                _tooltipClosingTimer.Change(3000, Timeout.Infinite);
            }
        }
#endif

        /// <summary>
        /// Invokes the method that closes the tooltip that displays text editing error.
        /// </summary>
        private void ToolTipClosingCallBack(object state)
        {
            Dispatcher.Invoke(new Action(CloseTextEditingExceptionToolTip));
        }

        /// <summary>
        /// Closes the tooltip that displays text editing error.
        /// </summary>
        private void CloseTextEditingExceptionToolTip()
        {
#if !REMOVE_OFFICE_PLUGIN
            _textEditingExceptionToolTip.IsOpen = false;
#endif
        }

        /// <summary>
        /// Returns the XObject content figure (form or image).
        /// </summary>
        /// <param name="figure">The figure.</param>
        private ContentStreamGraphicsFigure GetXObjectFigure(GraphicsFigure figure)
        {
            if (figure == null)
                return null;
            if (figure is ContentStreamGraphicsFigureTextGroup)
                return null;
            ContentStreamGraphicsFigureGroup contentFigureGroup = figure as ContentStreamGraphicsFigureGroup;
            if (contentFigureGroup != null)
            {
                foreach (ContentStreamGraphicsFigure groupItem in contentFigureGroup)
                {
                    ContentStreamGraphicsFigure result = GetXObjectFigure(groupItem);
                    if (result != null)
                        return result;
                }
                return null;
            }
            ContentStreamGraphicsFigure contentFigure = figure as ContentStreamGraphicsFigure;
            if (contentFigure == null)
                return null;
            if (contentFigure.Resource != null)
                return contentFigure;
            return null;
        }

        #endregion


        #region Set figure settings

        /// <summary>
        /// Sets the pen settings.
        /// </summary>
        private bool SetPenSettings()
        {
            // if pen is empty
            if (_pen == null)
            {
                // create pen
                _pen = new PdfPen(System.Drawing.Color.Black);
                _pen.LineJoinStyle = GraphicsStateLineJoinStyle.RoundJoin;
            }
            else
                _pen = (PdfPen)_pen.Clone();

            // create dialog that allows to view and change pen settings
            PropertyGridWindow form = new PropertyGridWindow(_pen, "Set properties of pen", true);
            return form.ShowDialog().Value;
        }

        /// <summary>
        /// Sets the settings of highlight pen.
        /// </summary>
        private bool SetHighlightPenSettings()
        {
            // if pen is empty
            if (_pen == null)
            {
                // create pen
                _pen = new PdfPen(System.Drawing.Color.Black, 10);
                _pen.LineJoinStyle = GraphicsStateLineJoinStyle.RoundJoin;
            }
            else
                _pen = (PdfPen)_pen.Clone();

            // create dialog that allows to view and change highlighting of pen

            SelectHighlightWindow dlg = new SelectHighlightWindow(_pen);
            // set highlight mode of pen
            dlg.ModeIndex = _selectHighlightPenDialogModeIndex;
            if (dlg.ShowDialog().Value)
            {
                // save highlight mode of pen
                _selectHighlightPenDialogModeIndex = dlg.ModeIndex;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the settings of highlight brush.
        /// </summary>
        private bool SetHighlightBrushSettings()
        {
            // if brush is empty
            if (_brush == null)
                // create brush
                _brush = new PdfBrush(System.Drawing.Color.Black);
            else
                _brush = (PdfBrush)_brush.Clone();

            // create dialog that allows to view and change highlighting of brush
            SelectHighlightWindow dlg = new SelectHighlightWindow(_brush);
            // set highlight mode of brush
            dlg.ModeIndex = _selectHighlightPenDialogModeIndex;
            if (dlg.ShowDialog().Value)
            {
                // save highlight mode of brush
                _selectHighlightPenDialogModeIndex = dlg.ModeIndex;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the settings of brush.
        /// </summary>
        private bool SetBrushSettings()
        {
            // if brush is empty
            if (_brush == null)
                // create brush
                _brush = new PdfBrush(System.Drawing.Color.FromArgb(70, System.Drawing.Color.FromArgb(255, 255, 0)));
            else
                _brush = (PdfBrush)_brush.Clone();

            // create dialog that allows to view and change brush settings
            PropertyGridWindow form = new PropertyGridWindow(_brush, "Set properties of brush", true);
            return form.ShowDialog().Value;
        }

        #endregion


        #region Add Figures

        /// <summary>
        /// Adds the text line.
        /// </summary>
        private void addTextButton_Click(object sender, RoutedEventArgs e)
        {
            // get fonts of PDF document
            IList<PdfFont> fonts = FocusedPage.Document.GetFonts();

            // create "Add text" dialog
            AddTextWindow dialog = new AddTextWindow(FocusedPage.Document, fonts, _addText);
            // show dialog
            if (dialog.ShowDialog().Value)
            {
                // set text
                _addText = dialog.TextToAdd;
                // create PDF brush of text line
                PdfBrush textBrush = new PdfBrush(dialog.TextColor);
                // build of text line
                _contentEditorTool.StartBuildTextLine(_addText, dialog.TextFont, dialog.TextSize, textBrush);
            }
        }

        /// <summary>
        /// Adds the text box.
        /// </summary>
        private void addTextBoxButton_Click(object sender, RoutedEventArgs e)
        {
            // get fonts of PDF document
            IList<PdfFont> fonts = FocusedPage.Document.GetFonts();
            // create form of text box
            AddTextWindow addTextDialog = new AddTextWindow(FocusedPage.Document, fonts, _addText);
            // show dialog
            if (addTextDialog.ShowDialog().Value)
            {
                // set pen of text box
                PdfPen pen = null;
                if (SetPenSettings())
                    pen = _pen;
                // set brush of text box
                PdfBrush brush = null;
                if (SetBrushSettings())
                    brush = _brush;
                // set text
                _addText = addTextDialog.TextToAdd;
                // create figure
                TextBoxFigure textBox = new TextBoxFigure(new PdfBrush(addTextDialog.TextColor),
                    _addText, addTextDialog.TextFont, addTextDialog.TextSize);
                textBox.CanRotate = true;
                // set pen
                textBox.Pen = pen;
                // set brush
                textBox.Brush = brush;

                // create dialog that allows to edit setting of text box figure
                PropertyGridWindow dialog = new PropertyGridWindow(textBox, "Set properties of TextBoxFigure", true);
                // show dialog
                if (dialog.ShowDialog().Value)
                {
                    // build of text box
                    _contentEditorTool.StartBuildFigure(textBox);
                }
            }
        }

        /// <summary>
        /// Adds the image figure based on PDF image-resource.
        /// </summary>
        private void drawImageButton_Click(object sender, RoutedEventArgs e)
        {
            // create dialog that allows to select resource
            PdfResourcesViewerWindow dialog = new PdfResourcesViewerWindow(FocusedPage.Document);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = Window.GetWindow(this.Parent);
            dialog.ShowFormResources = false;
            dialog.ShowImageResources = true;
            dialog.CanAddResources = true;

            // show dialog
            if (dialog.ShowDialog() == true)
            {
                // build of image
                _contentEditorTool.StartBuildImage((PdfImageResource)dialog.SelectedResource);
            }
        }

        /// <summary>
        /// Adds the image figure based on PDF form-resource.
        /// </summary>
        private void formXObjectButton_Click(object sender, RoutedEventArgs e)
        {
            PdfResourcesViewerWindow dialog = new PdfResourcesViewerWindow(FocusedPage.Document, true);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowFormResources = true;
            dialog.ShowImageResources = false;
            dialog.CanAddResources = true;

            if (dialog.ShowDialog() == true && (dialog.SelectedResource is PdfFormXObjectResource))
            {
                _contentEditorTool.StartBuildFormXObject(null, null, (PdfFormXObjectResource)dialog.SelectedResource);
            }
        }

        /// <summary>
        /// Adds the image figure based on VintasoftImage.
        /// </summary>
        private void drawVintasoftImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // if image is selected
                if (_openImageFileDialog.ShowDialog().Value)
                {

                    // select image from file
                    VintasoftImage image = SelectImageWindow.SelectImageFromFile(_openImageFileDialog.FileName);

                    // if image is not selected
                    if (image == null)
                        return;

                    // create figure
                    VintasoftImageFigure figure = new VintasoftImageFigure();
                    figure.Image = image;
                    figure.CanRotate = true;

                    // create dialog with figure properties
                    PropertyGridWindow dialog = new PropertyGridWindow(figure, "VintasoftImageFigure properties");
                    if (dialog.ShowDialog().Value)
                        // build of the figure
                        _contentEditorTool.StartBuildFigure(figure);
                }
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Adds the rectangle.
        /// </summary>
        private void drawRectangleButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetPenSettings() && SetBrushSettings())
                // build the rectangle
                _contentEditorTool.StartBuildRectangle(_pen, _brush);
        }

        /// <summary>
        /// Adds the filled rectangle.
        /// </summary>
        private void fillRectangleButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetBrushSettings())
                // build the rectangle
                _contentEditorTool.StartBuildRectangle(null, _brush);
        }

        /// <summary>
        /// Adds the highlighted rectangle.
        /// </summary>
        private void fillRectangleUseBlendingModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetHighlightBrushSettings())
                // build the rectangle
                _contentEditorTool.StartBuildRectangle(null, _brush);
        }

        /// <summary>
        /// Adds the freehand lines.
        /// </summary>
        private void drawLinesUseBlendingModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetHighlightPenSettings())
                // build the freehand lines
                _contentEditorTool.StartBuildFreehandLines(_pen);
        }

        /// <summary>
        /// Adds the ellipse.
        /// </summary>
        private void drawEllipseButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetPenSettings() && SetBrushSettings())
                // build ellipse
                _contentEditorTool.StartBuildEllipse(_pen, _brush);
        }

        /// <summary>
        /// Adds the lines.
        /// </summary>
        private void drawLinesButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetPenSettings())
                // build the lines
                _contentEditorTool.StartBuildLines(_pen);
        }

        /// <summary>
        /// Adds the freehand lines.
        /// </summary>
        private void freeHandLineButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetPenSettings())
                // build the freehand lines
                _contentEditorTool.StartBuildFreehandLines(_pen);
        }

        /// <summary>
        /// Adds the curves.
        /// </summary>
        private void drawCurvesButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetPenSettings())
                // build curves
                _contentEditorTool.StartBuildCurves(_pen);
        }

        /// <summary>
        /// Adds the polygon.
        /// </summary>
        private void drawPolygonButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetPenSettings() && SetBrushSettings())
                // build the polygon
                _contentEditorTool.StartBuildPolygon(_pen, _brush);
        }

        /// <summary>
        /// Adds the closed curves.
        /// </summary>
        private void drawClosedCurvesButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetPenSettings() && SetBrushSettings())
                // build the closed curves
                _contentEditorTool.StartBuildClosedCurves(_pen, _brush);
        }

        /// <summary>
        /// "Add Office document" button is clicked.
        /// </summary>
        private void addOfficeDocumentButton_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_OFFICE_PLUGIN
            try
            {
                // select Office document
                Stream documentStream = OfficeDemosTools.SelectOfficeDocument();

                // if document is selected
                if (documentStream != null)
                {
                    // create Office document figure
                    OfficeDocumentFigure figure = new OfficeDocumentFigure(_contentEditorTool.CurrentPage.Document);

                    figure.SetDocumentStream(documentStream, true);

                    // build the figure
                    _contentEditorTool.StartBuildFigure(figure);
                }
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
#endif
        }

        /// <summary>
        /// "Add formatted text" button is clicked.
        /// </summary>
        private void addFormattedTextButton_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_OFFICE_PLUGIN
            // create Office document figure
            OfficeDocumentFigure figure = new OfficeDocumentFigure(_contentEditorTool.CurrentPage.Document);
            figure.SetDocumentStream(DemosResourcesManager.GetResourceAsStream("EmptyDocument.docx"), true);
            figure.Brush = null;

            _formattedTextFigure = figure;

            // build the figure
            _contentEditorTool.StartBuildFigure(figure);
#endif
        }

        /// <summary>
        /// "Add chart" button is clicked.
        /// </summary>
        private void addChartButton_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_OFFICE_PLUGIN
            Stream chartStream = OfficeDemosTools.SelectChartResource();
            if (chartStream != null)
            {
                // create Office document figure
                OfficeDocumentFigure figure = new OfficeDocumentFigure();
                figure.UseGraphicObjectRelativeSize = true;
                figure.SetDocumentStream(chartStream, true);
                _chartFigure = figure;

                // build the figure
                _contentEditorTool.StartBuildFigure(figure);
            }
#endif
        }

        #endregion


        #region Context Menu Strip

        /// <summary>
        /// Context menu strip of figure is opening.
        /// </summary>
        private void figureViewContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = (ContextMenu)sender;

            ((MenuItem)menu.Items[0]).IsEnabled = _contentEditorTool.CutAction.IsEnabled;
            ((MenuItem)menu.Items[1]).IsEnabled = _contentEditorTool.CopyAction.IsEnabled;
            ((MenuItem)menu.Items[2]).IsEnabled = _contentEditorTool.PasteAction.IsEnabled;
            ((MenuItem)menu.Items[3]).IsEnabled = _contentEditorTool.DeleteAction.IsEnabled;
            ((MenuItem)menu.Items[7]).IsEnabled = _contentEditorTool.BringToFrontAction.IsEnabled;
            ((MenuItem)menu.Items[8]).IsEnabled = _contentEditorTool.BringToBackAction.IsEnabled;

            MenuItem selectFigureMenuItem = (MenuItem)menu.Items[5];

            WpfGraphicsFigureView[] figures = ContentEditorTool.FindFigures(_contextMenuLocation);
            if (figures.Length > 0)
            {
                selectFigureMenuItem.Items.Clear();
                selectFigureMenuItem.IsEnabled = true;
                foreach (WpfGraphicsFigureView figure in figures)
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = GraphicsFigureViewItem.GetDescription(figure);
                    menuItem.Tag = figure;
                    menuItem.Click += SelectFigureMenuItem_Click;
                    selectFigureMenuItem.Items.Add(menuItem);
                }
            }
            else
            {
                selectFigureMenuItem.IsEnabled = false;
            }

            ((MenuItem)menu.Items[10]).IsEnabled = _contentEditorTool != null && CanEditContentGraphicsProperties(_contentEditorTool.SelectedFigure);
        }

        /// <summary>
        /// Selects the figure (context menu "Select Figure").
        /// </summary>
        private void SelectFigureMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WpfGraphicsFigureView figure = (WpfGraphicsFigureView)((MenuItem)sender).Tag;
            ContentEditorTool.SelectedFigureView = figure;
        }

        /// <summary>
        /// Context menu strip of image viewer is opening.
        /// </summary>
        private void imageViewerContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = (ContextMenu)sender;

            ((MenuItem)menu.Items[0]).IsEnabled = _contentEditorTool.PasteAction.IsEnabled;
            ((MenuItem)menu.Items[1]).IsEnabled = _contentEditorTool.FigureViewCollection.Count > 0;
        }

        /// <summary>
        /// Cuts the figure.
        /// </summary>
        private void cutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _contentEditorTool.CutAction.Execute();
        }

        /// <summary>
        /// Copies the figure.
        /// </summary>
        private void copyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _contentEditorTool.CopyAction.Execute();
        }

        /// <summary>
        /// Pastes the figure.
        /// </summary>
        private void pasteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _contentEditorTool.PasteAction.Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Removes selected figure.
        /// </summary>
        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedFigure();
        }

        /// <summary>
        /// Removes all figures.
        /// </summary>
        private void removeAllButton_Click(object sender, RoutedEventArgs e)
        {
            List<WpfGraphicsFigureView> figures = GetCurrentFigures();
            if (figures.Count == _contentEditorTool.FigureViewCollection.Count)
                _contentEditorTool.RemoveAllFigures();
            else
                _contentEditorTool.RemoveFigures(figures);
        }

        /// <summary>
        /// Returns collection that contains current figure list.
        /// </summary>
        private List<WpfGraphicsFigureView> GetCurrentFigures()
        {
            List<WpfGraphicsFigureView> result = new List<WpfGraphicsFigureView>(figuresListBox.Items.Count);
            foreach (GraphicsFigureViewItem figureViewItem in figuresListBox.Items)
                result.Add(figureViewItem.FigureView);
            return result;
        }

        /// <summary>
        /// Shows properties of figure.
        /// </summary>
        private void propertiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowCurrentFigureSettingDialog();
        }

        /// <summary>
        /// "Bring To Front" menu is selected in context menu.
        /// </summary>
        private void bringToFrontMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _contentEditorTool.BringToFrontAction.Execute();
        }

        /// <summary>
        /// "Bring To Back" menu is selected in context menu.
        /// </summary>
        private void bringToBackMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _contentEditorTool.BringToBackAction.Execute();
        }

        /// <summary>
        /// Shows the property grid of selected graphics figure.
        /// </summary>
        private void ShowCurrentFigureSettingDialog()
        {
            // get selected graphics figure
            WpfGraphicsFigureView view = _contentEditorTool.SelectedFigureView;

            // create dialog that displays property grid
            PropertyGridWindow dialog = new PropertyGridWindow(view.Figure, GetGraphicsFigureViewDescription(view));
            // show dialog
            dialog.ShowDialog();

            // update graphics figure
            _contentEditorTool.InvalidateItem(view);
        }

        /// <summary>
        /// Deletes the selected figure.
        /// </summary>
        private void DeleteSelectedFigure()
        {
            _contentEditorTool.DeleteAction.Execute();
        }

        #endregion


        #region Undo/Redo

        /// <summary>
        /// Handles the Changed event of the UndoManager.
        /// </summary>
        private void UndoManager_Changed(object sender, UndoManagerChangedEventArgs e)
        {
            UpdateUndoUI();
        }

        /// <summary>
        /// Adds the <see cref="MenuItem"/> to the specified menu item.
        /// </summary>
        /// <param name="toolBarMenu">Parent menu item.</param>
        /// <param name="actions">The actions.</param>
        /// <param name="clickHandler">The <see cref="MenuItem.Click"/> handler.</param>
        private void AddSubMenuItems(
            MenuItem toolBarMenu,
            IEnumerable<Vintasoft.Imaging.Undo.UndoAction> actions,
            RoutedEventHandler clickHandler)
        {
            // clear action lists
            toolBarMenu.Items.Clear();

            foreach (Vintasoft.Imaging.Undo.UndoAction action in actions)
            {
                MenuItem menuItem = new MenuItem();
                menuItem.Header = action.ToString();
                menuItem.Click += clickHandler;

                toolBarMenu.Items.Add(menuItem);
            }
        }

        /// <summary>
        /// Updates the UI state of undo / redo.
        /// </summary>
        private void UpdateUndoUI()
        {
            // if undo manager contains undo actions and undo action is not executing now
            if (_undoManager.UndoCount > 0 && !_isUndoRedoExecuting)
            {
                // get undo manager actions
                ReadOnlyCollection<Vintasoft.Imaging.Undo.UndoAction> actions = _undoManager.GetActions();

                // get undo actions
                List<Vintasoft.Imaging.Undo.UndoAction> undoActions = new List<Vintasoft.Imaging.Undo.UndoAction>();

                // from current action to the first action
                for (int i = _undoManager.CurrentActionIndex; i >= 0; i--)
                    undoActions.Add(actions[i]);

                // create menu items
                AddSubMenuItems(undoToolBarMenu, undoActions, undoToolBarMenu_ItemClick);

                // update button properties
                undoToolBarButton.IsEnabled = true;
                undoToolBarButton.ToolTip = string.Format("Undo (Ctrl+Z): {0}", _undoManager.UndoDescription).Trim();
            }
            else
            {
                // update button properties
                undoToolBarButton.IsEnabled = false;
                undoToolBarButton.ToolTip = "Undo";
            }
            undoToolBarMenu.IsEnabled = undoToolBarButton.IsEnabled;


            // if undo manager contains redo actions and redo action is not executing now
            if (_undoManager.RedoCount > 0 && !_isUndoRedoExecuting)
            {
                // get undo manager actions
                ReadOnlyCollection<Vintasoft.Imaging.Undo.UndoAction> actions = _undoManager.GetActions();

                // get redo actions
                List<Vintasoft.Imaging.Undo.UndoAction> redoActions = new List<Vintasoft.Imaging.Undo.UndoAction>();

                // from next action to the last action
                for (int i = _undoManager.CurrentActionIndex + 1; i < actions.Count; i++)
                    redoActions.Add(actions[i]);

                // create menu items
                AddSubMenuItems(redoToolBarMenu, redoActions, redoToolBarMenu_ItemClick);

                // update button properties
                redoToolBarButton.IsEnabled = true;
                redoToolBarButton.ToolTip = string.Format("Redo (Ctrl+Y): {0}", _undoManager.RedoDescription).Trim();
            }
            else
            {
                // update button properties
                redoToolBarButton.IsEnabled = false;
                redoToolBarButton.ToolTip = "Redo";
            }
            redoToolBarMenu.IsEnabled = redoToolBarButton.IsEnabled;
        }

        /// <summary>
        /// Undoes the changes in figures.
        /// </summary>
        private void Undo()
        {
            // start undo execution
            _isUndoRedoExecuting = true;
            // execute a single undo operation
            _undoManager.Undo();
            // end undo execution
            _isUndoRedoExecuting = false;

            UpdateUI();
        }

        /// <summary>
        /// Redos the changes in figures.
        /// </summary>
        private void Redo()
        {
            // start redo execution
            _isUndoRedoExecuting = true;
            // execute a single redo operation
            _undoManager.Redo();
            // end redo execution
            _isUndoRedoExecuting = false;

            UpdateUI();
        }

        /// <summary>
        /// Handles the CanExecute event of undoCommandBinding object.
        /// </summary>
        private void undoCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = undoToolBarButton.IsEnabled;
        }

        /// <summary>
        /// Handles the Click event of undoToolBarButton object.
        /// </summary>
        private void undoToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            Undo();
        }

        /// <summary>
        /// Handles the CanExecute event of redoCommandBinding object.
        /// </summary>
        private void redoCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = redoToolBarButton.IsEnabled;
        }

        /// <summary>
        /// Handles the Click event of redoToolBarButton object.
        /// </summary>
        private void redoToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            Redo();
        }

        /// <summary>
        /// Handles the ItemClick event of undoToolBarMenu object.
        /// </summary>
        private void undoToolBarMenu_ItemClick(object sender, RoutedEventArgs e)
        {
            // start undo execution
            _isUndoRedoExecuting = true;

            // get clicked action index
            int undoItemIndex = ((MenuItem)((MenuItem)sender).Parent).Items.IndexOf(sender);

            // execute undo
            _undoManager.Undo(undoItemIndex + 1);

            // end undo execution
            _isUndoRedoExecuting = false;

            UpdateUI();
        }

        /// <summary>
        /// Handles the ItemClick event of redoToolBarMenu object.
        /// </summary>
        private void redoToolBarMenu_ItemClick(object sender, RoutedEventArgs e)
        {
            // start redo execution
            _isUndoRedoExecuting = true;

            // get clicked action index
            int redoItemIndex = ((MenuItem)((MenuItem)sender).Parent).Items.IndexOf(sender);

            // execute redo
            _undoManager.Redo(redoItemIndex + 1);

            // end redo execution
            _isUndoRedoExecuting = false;

            UpdateUI();
        }

        #endregion

        #endregion

    }
}
