using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Reflection;
using Microsoft.Win32;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Codecs.Decoders;
using Vintasoft.Imaging.Codecs.Encoders;
using Vintasoft.Imaging.Codecs.ImageFiles;
using Vintasoft.Imaging.Processing;
using Vintasoft.Imaging.UIActions;

#if !REMOVE_ANNOTATION_PLUGIN
using Vintasoft.Imaging.Annotation.Comments;
using Vintasoft.Imaging.Annotation.Comments.Pdf;
using Vintasoft.Imaging.Annotation.Wpf.UI.Comments;
using Vintasoft.Imaging.Annotation.Wpf.UI.VisualTools;
#endif

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Drawing;
using Vintasoft.Imaging.Pdf.Drawing.GraphicsFigures;
using Vintasoft.Imaging.Pdf.Processing;
using Vintasoft.Imaging.Pdf.Processing.Analyzers;
using Vintasoft.Imaging.Pdf.Processing.BasicTypes;
using Vintasoft.Imaging.Pdf.Processing.PdfA;
using Vintasoft.Imaging.Pdf.Processing.Fonts;
using Vintasoft.Imaging.Pdf.Processing.Images;
using Vintasoft.Imaging.Pdf.Security;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.Fonts;
using Vintasoft.Imaging.Pdf.Tree.FileAttachments;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;
using Vintasoft.Imaging.Pdf.JavaScriptApi;
using Vintasoft.Imaging.Pdf.JavaScript;
using Vintasoft.Imaging.Pdf.Wpf.UI;
using Vintasoft.Imaging.Pdf.Wpf.UI.Annotations;
#if !REMOVE_OCR_PLUGIN
using Vintasoft.Imaging.Pdf.Ocr;
#endif
using Vintasoft.Imaging.Print;
using Vintasoft.Imaging.UI;
using Vintasoft.Imaging.Utils;
using Vintasoft.Imaging.Text;
using Vintasoft.Imaging.Wpf.Print;
using Vintasoft.Imaging.Wpf.UI.VisualTools;
using Vintasoft.Imaging.Fonts;
using Vintasoft.Imaging.ImageProcessing.Color;
using Vintasoft.Imaging.ImageProcessing.Info;
#if !REMOVE_OCR_PLUGIN
using Vintasoft.Imaging.Ocr.Tesseract;
using Vintasoft.Imaging.Ocr;
#endif

using WpfDemosCommonCode;
using WpfDemosCommonCode.Imaging;
using WpfDemosCommonCode.Imaging.Codecs;
using WpfDemosCommonCode.Annotation;
using WpfDemosCommonCode.Pdf;
using WpfDemosCommonCode.Pdf.Security;
using WpfDemosCommonCode.Pdf.JavaScript;
using WpfDemosCommonCode.Imaging.Codecs.Dialogs;
using WpfDemosCommonCode.Ocr;

namespace WpfPdfEditorDemo
{
    /// <summary>
    /// Main form of PDF Editor Demo.
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Fields

        /// <summary>
        /// Title format string of main form.
        /// </summary>
        string _titleFormatString = "VintaSoft WPF PDF Editor Demo v" + ImagingGlobalSettings.ProductVersion + " {0}";

        /// <summary>
        /// A value indicating whether updating of User Interface is enabled.
        /// </summary>
        bool _enableUpdateUI = true;

        /// <summary>
        /// Selected "View - Image scale mode" menu item.
        /// </summary>
        MenuItem _imageScaleSelectedMenuItem;

        /// <summary>
        /// Opened PDF document.
        /// </summary>
        PdfDocument _document;

        /// <summary>
        /// The name of file, which stores a PDF document.
        /// </summary>
        string _pdfFileName;

        /// <summary>
        /// The stream, which stores a PDF document.
        /// </summary>
        Stream _pdfFileStream;

        /// <summary>
        /// The stream, where a PDF document must be saved.
        /// </summary>
        Stream _newFileStream;

        /// <summary>
        /// A value indicating whether PDF file stream must be switched to the new file stream after save.
        /// </summary>
        bool _switchPdfFileStreamToNewStream = false;

        /// <summary>
        /// PDF encoder settings.
        /// </summary>
        PdfEncoderSettings _pdfEncoderSettings;

        /// <summary>
        /// A value indicating whetherthe encoder setting, which are specified in _pdfEncoderSettings field, must be applied to all saving images.
        /// </summary>
        bool _usePdfEncoderSettingsForAllImages = false;

        /// <summary>
        /// Action name.
        /// </summary>
        string _actionName = "";

        /// <summary>
        /// The start time of action.
        /// </summary>
        DateTime _actionStartTime;

        /// <summary>
        /// The visual tool for searching and selecting of text on PDF page.
        /// </summary>
        WpfTextSelectionTool _textSelectionTool;

        /// <summary>
        /// The visual tool for text markup (highlight, undeline, strikeout...).
        /// </summary>
        WpfPdfTextMarkupTool _textMarkupTool;

        /// <summary>
        /// The visual tool for cropping a PDF page or an image in image viewer.
        /// </summary>
        WpfPdfCropSelectionTool _cropSelectionTool;

        /// <summary>
        /// The visual tool for editing content of PDF page.
        /// </summary>
        WpfPdfContentEditorTool _contentEditorTool;

        /// <summary>
        /// The composite visual tool for editing content of PDF page.
        /// </summary>
        WpfCompositeVisualTool _contentEditorToolComposition;

        /// <summary>
        /// The visual tool for removing and blacking out content of PDF page.
        /// </summary>
        WpfPdfRemoveContentTool _removeContentTool;

        /// <summary>
        /// The visual tool for redaction content of PDF page.
        /// </summary>
        WpfCompositeVisualTool _redactionTool;

        /// <summary>
        /// The visual tool for viewing, filling and editing PDF annotations and PDF interactive fields.
        /// </summary>
        WpfPdfAnnotationTool _annotationTool;

        /// <summary>
        /// The composite visual tool that combines functionality of annotation tool and
        /// text selection tool.
        /// </summary>
        WpfCompositeVisualTool _defaultAnnotationTool;

        /// <summary>
        /// The visual tool for panning of image.
        /// </summary>
        WpfPanTool _panTouchTool;

        /// <summary>
        /// The visual tool for zooming of image.
        /// </summary>
        WpfZoomTool _zoomTouchTool;

        /// <summary>
        /// Size of empty PDF page.
        /// </summary>
        System.Drawing.SizeF _emptyPageSize;

        /// <summary>
        /// Units of empty PDF page.
        /// </summary>
        UnitOfMeasure _emptyPageUnits = UnitOfMeasure.Centimeters;

        /// <summary>
        /// ThumbnailViewer print manager.
        /// </summary>
        WpfImagePrintManager _thumbnailViewerPrintManager;

        /// <summary>
        /// The signature appearance.
        /// </summary>
        SignatureAppearanceGraphicsFigure _signatureAppearance = new SignatureAppearanceGraphicsFigure();

        /// <summary>
        /// The redaction mark appearance.
        /// </summary>
        RedactionMarkAppearanceGraphicsFigure _redactionMarkAppearance;

        /// <summary>
        /// The PDF content custom renderer.
        /// </summary>
        CustomContentRenderer _pdfCustomContentRenderer = new CustomContentRenderer();

        /// <summary>
        /// The text encoding obfuscator.
        /// </summary>
        PdfTextEncodingObfuscator _textEncodingObfuscator = new PdfTextEncodingObfuscator();

        /// <summary>
        /// The selected PDF pages for text encoding obfuscation.
        /// </summary>
        PdfPage[] _pagesForObfuscation;

        /// <summary>
        /// The JavaScript debugger window.
        /// </summary>
        PdfJavaScriptDebuggerWindow _debuggerWindow = null;

        /// <summary>
        /// A value indicating whether demo shown an error message with information about missing CJK font.
        /// </summary>
        bool _isCJKFontMissingErrorMessageShown = false;

        /// <summary>
        /// Default image viewer display mode.
        /// </summary>
        ImageViewerDisplayMode _defaultImageViewerDisplayMode;

        /// <summary>
        /// The commentate visual tool.
        /// </summary>
#if !REMOVE_ANNOTATION_PLUGIN
        WpfCommentVisualTool _commentTool;
#endif

#if !REMOVE_OCR_PLUGIN        
        /// <summary>
        /// The tesseract OCR.
        /// </summary>
        TesseractOcr _tesseract;

        /// <summary>
        /// Dictionary: <see cref="PdfPageCreationMode"/> => <see cref="SearchablePdfGenerator"/>.
        /// </summary>
        Dictionary<PdfPageCreationMode, SearchablePdfGenerator> _pdfPageCreationModeToGenerator =
            new Dictionary<PdfPageCreationMode, SearchablePdfGenerator>();
#endif

        SaveFileDialog _saveFileDialog;
        SaveFileDialog _saveImageFileDialog;
        SaveFileDialog _convertToFileDialog;
        OpenFileDialog _openPdfFileDialog;
        OpenFileDialog _openImageFileDialog;

        public static RoutedCommand _newCommand = new RoutedCommand();
        public static RoutedCommand _openCommand = new RoutedCommand();
        public static RoutedCommand _closeCommand = new RoutedCommand();
        public static RoutedCommand _addPagesCommand = new RoutedCommand();
        public static RoutedCommand _saveAsCommand = new RoutedCommand();
        public static RoutedCommand _printCommand = new RoutedCommand();
        public static RoutedCommand _documentInformationCommand = new RoutedCommand();
        public static RoutedCommand _findTextCommand = new RoutedCommand();
        public static RoutedCommand _aboutCommand = new RoutedCommand();

        public static RoutedCommand _cutCommand = new RoutedCommand();
        public static RoutedCommand _copyCommand = new RoutedCommand();
        public static RoutedCommand _pasteCommand = new RoutedCommand();
        public static RoutedCommand _deleteCommand = new RoutedCommand();
        public static RoutedCommand _selectAllCommand = new RoutedCommand();

        public static RoutedCommand _rotateClockwiseCommand = new RoutedCommand();
        public static RoutedCommand _rotateCounterclockwiseCommand = new RoutedCommand();

        #endregion



        #region Constructors


        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            // register the evaluation license for VintaSoft Imaging .NET SDK
            Vintasoft.Imaging.ImagingGlobalSettings.Register("REG_USER", "REG_EMAIL", "EXPIRATION_DATE", "REG_CODE");

            InitializeComponent();

            Jbig2AssemblyLoader.Load();
            Jpeg2000AssemblyLoader.Load();
            DocxAssemblyLoader.Load();

            ImagingTypeEditorRegistrator.Register();

#if !REMOVE_OFFICE_PLUGIN
            PdfOfficeWpfUIAssembly.Init();
#endif

            visualToolsToolBar.ImageViewer = imageViewer1;
            visualToolsToolBar.VisualToolsMenuItem = visualToolsMenuItem;

            thumbnailViewer1.MasterViewer = imageViewer1;
            viewerToolBar.ImageViewer = imageViewer1;
            documentBookmarks.Viewer = imageViewer1;

            // specify that exceptions of visual tools must be catched
            DemosTools.CatchVisualToolExceptions(imageViewer1);

            // enable PDF password dialog
            PdfAuthenticateTools.EnableAuthenticateRequest = true;
            // set CustomFontProgramsController for all opened PDF documents
            PdfDemosTools.EnableUsageOfDefaultFontProgramsController();
            // generate interactive form fields appearance if need
            PdfDemosTools.NeedGenerateInteractiveFormFieldsAppearance = true;

            // init "View => Image Display Mode" menu
            singlePageMenuItem.Tag = ImageViewerDisplayMode.SinglePage;
            twoColumnsMenuItem.Tag = ImageViewerDisplayMode.TwoColumns;
            singleContinuousRowMenuItem.Tag = ImageViewerDisplayMode.SingleContinuousRow;
            singleContinuousColumnMenuItem.Tag = ImageViewerDisplayMode.SingleContinuousColumn;
            twoContinuousRowsMenuItem.Tag = ImageViewerDisplayMode.TwoContinuousRows;
            twoContinuousColumnsMenuItem.Tag = ImageViewerDisplayMode.TwoContinuousColumns;

            // init "View => Image Scale Mode" menu
            normalImageMenuItem.Tag = ImageSizeMode.Normal;
            bestFitMenuItem.Tag = ImageSizeMode.BestFit;
            fitToWidthMenuItem.Tag = ImageSizeMode.FitToWidth;
            fitToHeightMenuItem.Tag = ImageSizeMode.FitToHeight;
            pixelToPixelMenuItem.Tag = ImageSizeMode.PixelToPixel;
            scaleMenuItem.Tag = ImageSizeMode.Zoom;
            scale25MenuItem.Tag = 25;
            scale50MenuItem.Tag = 50;
            scale100MenuItem.Tag = 100;
            scale200MenuItem.Tag = 200;
            scale400MenuItem.Tag = 400;
            _imageScaleSelectedMenuItem = bestFitMenuItem;
            _imageScaleSelectedMenuItem.IsChecked = true;

            // init menu
            InitMenu();

            // init dialogs
            InitDialogs();

            // init visual tools
            InitVisualTools();

            // init PDF action executors
            InitPdfActionExecutors();

#if !REMOVE_ANNOTATION_PLUGIN
            MeasurementVisualToolActionFactory.CreateActions(visualToolsToolBar);
#endif
            ZoomVisualToolActionFactory.CreateActions(visualToolsToolBar);
            // initialize visual tool actions
            InitVisualToolActions();

            // set "Pages" tab as selected tab
            toolsTabControl.SelectedItem = pagesTabItem;

            // set ThumbnailFlowStyle to SingleColumn
            thumbnailViewer1.ThumbnailFlowStyle = ThumbnailFlowStyle.SingleColumn;

            // set ThumbnailSize
            thumbnailViewer1.ThumbnailSize = new Size(128, 128);

            // set ThumbnailCaption properties
            thumbnailViewer1.ThumbnailCaption.Anchor = AnchorType.Bottom;
            thumbnailViewer1.ThumbnailCaption.CaptionFormat = "{PageLabel}";
            thumbnailViewer1.ThumbnailCaption.IsVisible = true;
            thumbnailViewer1.ThumbnailCaption.Padding = new Thickness(0, 0, 0, 3);

            // add event handles to image collection
            imageViewer1.Images.ImageCollectionSavingProgress += new EventHandler<ProgressEventArgs>(Images_ImageCollectionSavingProgress);


            // create PDF rendering settings
            PdfRenderingSettings renderingSettings = new PdfRenderingSettings();
            renderingSettings.DrawPdfAnnotations = true;
            renderingSettings.DrawVintasoftAnnotations = true;
#if REMOVE_OFFICE_PLUGIN
            // set PDF rendering settings as image viewer settings
            imageViewer1.ImageRenderingSettings = renderingSettings;
#else
            imageViewer1.ImageRenderingSettings = new CompositeRenderingSettings(
                renderingSettings,
                new DocxRenderingSettings());
#endif

            // set the default image viewer display mode
            _defaultImageViewerDisplayMode = imageViewer1.DisplayMode;

            // image viewer must use the image appearances in single-page and multi-page modes
            imageViewer1.UseImageAppearancesInSinglePageMode = true;

            // set default params
            resolutionToolBar.ImageViewer = imageViewer1;
            resolutionToolBar.UseDynamicRendering = true;

            // specify the default size for new (empty) page
            _emptyPageSize = new System.Drawing.SizeF();
            _emptyPageSize.Width = PdfPage.ConvertFromUnitOfMeasureToUserUnits(210, UnitOfMeasure.Millimeters);
            _emptyPageSize.Height = PdfPage.ConvertFromUnitOfMeasureToUserUnits(297, UnitOfMeasure.Millimeters);


            // create PDF encoder settings
            _pdfEncoderSettings = new PdfEncoderSettings();

            // create the print manager
            _thumbnailViewerPrintManager = new WpfImagePrintManager();
            _thumbnailViewerPrintManager.Images = thumbnailViewer1.Images;
            _thumbnailViewerPrintManager.PrintScaleMode = PrintScaleMode.BestFit;

            InteractionAreaAppearanceManager.IsEnabledFormFieldSpellChecking = enableFormFieldsSpellCheckingMenuItem.IsChecked;
            InteractionAreaAppearanceManager.IsEnabledAnnotationsSpellChecking = enableAnnotationsSpellCheckingMenuItem.IsChecked;

#if REMOVE_ANNOTATION_PLUGIN
            commentsTabItem.Visibility = Visibility.Collapsed;
#endif

#if REMOVE_PDFVISUALEDITOR_PLUGIN
            toolsTabControl.Items.Remove(contentEditorTabItem);
            addSelectedTextToRedactionMarksMenuItem.Visibility = Visibility.Collapsed;
#endif

            // initialize color management
            ColorManagementHelper.EnableColorManagement(imageViewer1);

            // update UI
            UpdateUI();

            imageViewer1.Images.ImageSavingException += Images_ImageSavingException;
        }

        #endregion



        #region Properties

        string _tesseractOcrDllDirectory = null;
        /// <summary>
        /// Gets a directory where Tesseract5.Vintasoft.xXX.dll is located.
        /// </summary>
        public string TesseractOcrDllDirectory
        {
            get
            {
                if (_tesseractOcrDllDirectory == null)
                {
                    // Tesseract OCR dll filename
                    string dllFilename;
                    // if is 64-bit system then
                    if (IntPtr.Size == 8)
                        dllFilename = "Tesseract5.Vintasoft.x64.dll";
                    else
                        dllFilename = "Tesseract5.Vintasoft.x86.dll";

                    string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);

                    // search directories
                    string[] directories = new string[]
                    {
                        "",
                        @"TesseractOCR\",
                        @"Debug\net6.0-windows\TesseractOCR\",
                        @"Release\net6.0-windows\TesseractOCR\",
                        @"Debug\net7.0-windows\TesseractOCR\",
                        @"Release\net7.0-windows\TesseractOCR\",
                        @"Debug\net8.0-windows\TesseractOCR\",
                        @"Release\net8.0-windows\TesseractOCR\",
                    };

                    // search tesseract dll
                    foreach (string dir in directories)
                    {
                        string dllDirectory = Path.Combine(currentDirectory, dir);
                        if (File.Exists(Path.Combine(dllDirectory, dllFilename)))
                        {
                            _tesseractOcrDllDirectory = dllDirectory;
                            break;
                        }
                    }
                    if (_tesseractOcrDllDirectory == null)
                        _tesseractOcrDllDirectory = currentDirectory;
                    else
                        _tesseractOcrDllDirectory = Path.GetFullPath(_tesseractOcrDllDirectory);
                }
                return _tesseractOcrDllDirectory;
            }
        }

        /// <summary>
        /// Gets the PDF page associated with image loaded in image viewer.
        /// </summary>
        internal PdfPage FocusedPage
        {
            get
            {
                if (imageViewer1.Image == null)
                    return null;
                return PdfDocumentController.GetPageAssociatedWithImage(imageViewer1.Image);
            }
        }

        bool _isDocumentChanged = false;
        /// <summary>
        /// Gets or sets a value indicating whether PDF document is changed.
        /// </summary>
        internal bool IsDocumentChanged
        {
            get
            {
                return _isDocumentChanged;
            }
            set
            {
                _isDocumentChanged = value;
                UpdateUIAsync();
            }
        }

        /// <summary>
        /// Gets or sets a filename of PDF document.
        /// </summary>
        internal string Filename
        {
            get
            {
                return _pdfFileName;
            }
            set
            {
                _pdfFileName = value;
                UpdateUI();
            }
        }

        /// <summary>
        /// Gets or sets a current visual tool.
        /// </summary>
        internal WpfVisualTool CurrentTool
        {
            get
            {
                return imageViewer1.VisualTool;
            }
            set
            {
                imageViewer1.VisualTool = value;
            }
        }

        bool _isPdfFileOpening = false;
        /// <summary>
        /// Gets or sets a value indicating whether PDF document is opening.
        /// </summary>
        internal bool IsPdfFileOpening
        {
            get
            {
                return _isPdfFileOpening;
            }
            set
            {
                _isPdfFileOpening = value;

                UpdateUI();
            }
        }

        bool _isPdfFileSaving = false;
        /// <summary>
        /// Gets or sets a value indicating whether PDF document is saving.
        /// </summary>
        internal bool IsPdfFileSaving
        {
            get
            {
                return _isPdfFileSaving;
            }
            set
            {
                _isPdfFileSaving = value;
                UpdateUIAsync();
            }
        }

        bool _isPdfFileReadOnlyMode = false;
        /// <summary>
        /// Gets or sets a value indicating whether PDF document is opened in read-only mode.
        /// </summary>
        internal bool IsPdfFileReadOnlyMode
        {
            get
            {
                return _isPdfFileReadOnlyMode;
            }
            set
            {
                _isPdfFileReadOnlyMode = value;
                UpdateUI();
            }
        }

        bool _isTextSearching = false;
        /// <summary>
        /// Gets or sets a value indicating whether text search is in progress.
        /// </summary>
        internal bool IsTextSearching
        {
            get
            {
                return _isTextSearching;
            }
            set
            {
                _isTextSearching = value;
                UpdateUI();
            }
        }

        WpfPdfInteractionAreaAppearanceManager _interactionAreaAppearanceManager = null;
        /// <summary>
        /// The interaction area manager.
        /// </summary>
        internal WpfPdfInteractionAreaAppearanceManager InteractionAreaAppearanceManager
        {
            get
            {
                if (_interactionAreaAppearanceManager == null)
                    _interactionAreaAppearanceManager = new WpfPdfInteractionAreaAppearanceManager();

                return _interactionAreaAppearanceManager;
            }
        }

        #endregion



        #region Methods

        #region PROTECTED

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.ContentRendered" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

#if !REMOVE_OFFICE_PLUGIN && !REMOVE_PDFVISUALEDITOR_PLUGIN
            WpfDemosCommonCode.Office.OfficeDocumentVisualEditorWindow documentVisualEditorForm = new WpfDemosCommonCode.Office.OfficeDocumentVisualEditorWindow();
            documentVisualEditorForm.Owner = this;
            documentVisualEditorForm.AddVisualTool(_contentEditorTool);
            documentVisualEditorForm.AddVisualTool(_annotationTool);
#endif
        }

        /// <summary>
        /// Form is closing.
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!e.Cancel)
            {
                ClosePdfDocument();

                if (_debuggerWindow != null)
                    _debuggerWindow.Owner = this;

                if (_interactionAreaAppearanceManager != null)
                    _interactionAreaAppearanceManager.Dispose();
            }

#if !REMOVE_OCR_PLUGIN
            if (_tesseract != null)
            {
                _tesseract.Dispose();
                _tesseract = null;
            }
#endif
        }

        #endregion


        #region PRIVATE

        #region Main Form

        /// <summary>
        /// Main window is loaded.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // process command line of the application
            string[] appArgs = Environment.GetCommandLineArgs();
            if (appArgs.Length > 1)
            {
                if (appArgs.Length == 2)
                {
                    try
                    {
                        OpenPdfDocument(appArgs[1]);
                    }
                    catch
                    {
                        ClosePdfDocument();
                    }
                }

                // update the UI
                UpdateUI();
            }
        }

        #endregion


        #region Init

        /// <summary>
        /// Initializes the menu.
        /// </summary>
        private void InitMenu()
        {
            // add available PdfDocumentViewMode to "Document" menu
            foreach (string name in Enum.GetNames(typeof(PdfDocumentViewMode)))
                documentViewModeComboBox.Items.Add(name);

            // add available PdfDocumentViewMode to "Document" menu
            foreach (string name in Enum.GetNames(typeof(PdfDocumentPageLayoutMode)))
                viewerPageLayoutComboBox.Items.Add(name);
        }

        /// <summary>
        /// Initializes the dialogs.
        /// </summary>
        private void InitDialogs()
        {
            _saveFileDialog = new SaveFileDialog();
            _saveFileDialog.Filter = "PDF Files|*.pdf";
            _saveFileDialog.DefaultExt = "pdf";
            _saveImageFileDialog = new SaveFileDialog();
            _saveImageFileDialog.DefaultExt = "png";
            _convertToFileDialog = new SaveFileDialog();
            _convertToFileDialog.DefaultExt = "pdf";
            _openPdfFileDialog = new OpenFileDialog();
            _openPdfFileDialog.Filter = "PDF Files|*.pdf";
            _openPdfFileDialog.DefaultExt = "pdf";
            _openImageFileDialog = new OpenFileDialog();
            _openImageFileDialog.Filter = "PDF Files|*.pdf";
            _openImageFileDialog.Multiselect = true;

            // set filters in open dialog
            CodecsFileFilters.SetFilters(_openImageFileDialog);

            // set filters in save dialog
            CodecsFileFilters.SetFilters(_saveImageFileDialog, false);

            // set the initial directory in open file dialog
            DemosTools.SetTestFilesFolder(_openImageFileDialog);
            DemosTools.SetTestFilesFolder(_openPdfFileDialog);
        }

        /// <summary>
        /// Initializes the visual tools.
        /// </summary>
        private void InitVisualTools()
        {
            // create pan tool
            _panTouchTool = new WpfPanTool();
            _panTouchTool.ProcessMouseEvents = false;
            _panTouchTool.ProcessTouchEvents = false;
            _panTouchTool.Enabled = false;

            // create zoom tool
            _zoomTouchTool = new WpfZoomTool();
            _zoomTouchTool.ProcessMouseEvents = false;
            _zoomTouchTool.ProcessTouchEvents = false;
            _zoomTouchTool.Enabled = false;


            // create PdfTextSelectionTool
            _textSelectionTool = new WpfTextSelectionTool(new SolidColorBrush(Color.FromArgb(56, 0, 0, 255)));
            _textSelectionTool.TextSearchingProgress += new EventHandler<TextSearchingProgressEventArgs>(TextSelectionTool_TextSearchingProgress);
            _textSelectionTool.SelectionChanged += new EventHandler(TextSelectionTool_SelectionChanged);
            _textSelectionTool.IsMouseSelectionEnabled = true;
            _textSelectionTool.IsKeyboardSelectionEnabled = true;
            textSelectionControl.TextSelectionTool = _textSelectionTool;
            findTextToolBar.TextSelectionTool = _textSelectionTool;

            // create PdfTextMarkupTool
            _textMarkupTool = new WpfPdfTextMarkupTool();
            _textMarkupTool.Enabled = false;
            _textMarkupTool.MarkupAnnotationCreated += TextMarkupTool_MarkupAnnotationCreated;
            _textMarkupTool.MarkupAnnotationAdded += TextMarkupTool_MarkupAnnotationAdded;

#if !REMOVE_PDFVISUALEDITOR_PLUGIN
            // create PdfContentEditorTool
            _contentEditorTool = new WpfPdfContentEditorTool();
            _contentEditorTool.RenderFiguresWhenImageIndexChanging = false;
            _contentEditorTool.RenderFiguresWhenDeactivating = true;
            pdfContentEditorControl.ContentEditorTool = _contentEditorTool;
            _contentEditorToolComposition = new WpfCompositeVisualTool(
#if !REMOVE_OFFICE_PLUGIN
                new Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction.WpfOfficeDocumentVisualEditorTextTool(),
#endif
                _contentEditorTool
);

            // create PdfRemoveContentTool
            _removeContentTool = new WpfPdfRemoveContentTool();
            _redactionMarkAppearance = new RedactionMarkAppearanceGraphicsFigure();
            _redactionMarkAppearance.FillColor = System.Drawing.Color.Black;
            _redactionMarkAppearance.TextColor = System.Drawing.Color.Red;
            _redactionMarkAppearance.AutoFontSize = true;
            _redactionMarkAppearance.Text = null;
            _removeContentTool.RedactionMarkAppearance = _redactionMarkAppearance;
            pdfRemoveContentControl.RemoveContentTool = _removeContentTool;
            pdfRemoveContentControl.TextSelectionTool = _textSelectionTool;
            pdfRemoveContentControl.ThumbnailViewer = thumbnailViewer1;
            pdfRemoveContentControl.RedactionMarkApplied += new EventHandler(pdfRemoveContentControl_RedactionMarkApplied);
            _redactionTool = new WpfCompositeVisualTool(_removeContentTool, _textSelectionTool);
#endif

            // create PDF annotation tool
#if !REMOVE_PDFVISUALEDITOR_PLUGIN
            _annotationTool = new WpfPdfAnnotationTool(PdfJavaScriptManager.JsApp, true);
            _annotationTool.InteractionMode = WpfPdfAnnotationInteractionMode.Markup;
#else
            _annotationTool = new WpfPdfAnnotationTool(PdfJavaScriptManager.JsApp, false);
            _annotationTool.InteractionMode = WpfPdfAnnotationInteractionMode.View;
#endif
            _annotationTool.ChangeFocusedItemBeforeInteraction = true;
            _annotationTool.HoveredAnnotationChanged += new EventHandler<PdfAnnotationEventArgs>(AnnotationTool_HoveredAnnotationChanged);
            _annotationTool.InteractiveFormEditorManager.UseUniqueAnnotationName = true;
            _annotationTool.InteractiveFormEditorManager.UseUniqueFieldName = true;
            _annotationTool.InteractionModeChanged += new PropertyChangedEventHandler<WpfPdfAnnotationInteractionMode>(AnnotationTool_InteractionModeChanged);
            _annotationTool.IsNavigationLoopedOnFocusedPage = false;
            annotationToolControl.AnnotationTool = _annotationTool;
            InteractionAreaAppearanceManager.VisualTool = _annotationTool;

#if !REMOVE_ANNOTATION_PLUGIN
            // create commentate visual tool
            WpfImageViewerCommentController commentController =
                new WpfImageViewerCommentController(new ImageCollectionPdfAnnotationCommentController());
            _commentTool = new WpfCommentVisualTool(commentController, new CommentControlFactory());

            annotationToolControl.CommentTool = _commentTool;

            commentsControl.CommentController = commentController;
            commentsControl.CommentTool = _commentTool;
            commentsControl.AnnotationTool = _annotationTool;
            commentsControl.ImageViewer = imageViewer1;

            // create composite tool: Comment tool + PDF Annotation tool + Text Selection tool + PDF Text Markup Tool
            _defaultAnnotationTool = new WpfCompositeVisualTool(
                _panTouchTool, _zoomTouchTool,
#if !REMOVE_OFFICE_PLUGIN
                new Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction.WpfOfficeDocumentVisualEditorTextTool(),
#endif
                _annotationTool, _commentTool, _textSelectionTool, _textMarkupTool);
#else
            // create composite tool: PDF Annotation tool + Text Selection tool
            _defaultAnnotationTool = new WpfCompositeVisualTool(
                _panTouchTool, _zoomTouchTool, 
#if !REMOVE_OFFICE_PLUGIN
                new Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction.WpfOfficeDocumentVisualEditorTextTool(),
#endif
                _annotationTool, _textSelectionTool, _textMarkupTool);
#endif

#if !REMOVE_PDFVISUALEDITOR_PLUGIN
            // create PDF crop selection tool
            _cropSelectionTool = new WpfPdfCropSelectionTool();
            _cropSelectionTool.SelectionOnlyOnImage = false;
#endif
        }


        /// <summary>
        /// Initializes the PDF action executors.
        /// </summary>
        private void InitPdfActionExecutors()
        {
            // enable JavaScript
            PdfJavaScriptManager.IsJavaScriptEnabled = true;
            enableJavaScriptExecutingMenuItem.IsChecked = PdfJavaScriptManager.IsJavaScriptEnabled;
            // register image viewer in JavaScript manager
            PdfJavaScriptManager.JsApp.RegisterImageViewer(imageViewer1);
            PdfJavaScriptManager.JavaScriptActionExecutor.StatusChanged +=
                new EventHandler<PdfJavaScriptActionStatusChangedEventArgs>(JavaScriptActionExecutor_StatusChanged);

            // initialize global action executor, using "Print" and "SaveAs" action handlers
            PdfActionExecutorManager.Initialize(
                imageViewer1,
                _annotationTool,
                new PdfViewerNamedAction("Print", PrintPdfDocument),
                new PdfViewerNamedAction("SaveAs", SavePdfDocumentAs));

            // create document-level actions executor
            PdfDocumentLevelActionsExecutor documentLevelActionsExecutor =
                new PdfDocumentLevelActionsExecutor(PdfJavaScriptManager.JsApp);

            // set action executor for PdfDocumentLevelActionsExecutor to application action executor
            documentLevelActionsExecutor.ActionExecutor = PdfActionExecutorManager.ApplicationActionExecutor;

            // set action executor for PdfAnnotationTool to application action executor
            _annotationTool.ActionExecutor = PdfActionExecutorManager.ApplicationActionExecutor;

            // set action executor for BookmarkTreeView to application action executor
            documentBookmarks.ActionExecutor = PdfActionExecutorManager.ApplicationActionExecutor;
        }

        /// <summary>
        /// Initializes the visual tool actions.
        /// </summary>
        private void InitVisualToolActions()
        {
            visualToolsToolBar.AddAction(new SeparatorToolBarAction());

            VisualToolAction textSelectionAndFillFormsAction = new VisualToolAction(
                _defaultAnnotationTool,
                "Text Selection / Fill Forms",
                "Text Selection, Navigation, Fill Forms",
                DemosResourcesManager.GetResourceAsBitmap("TextSelectionFillForms.png"));
            textSelectionAndFillFormsAction.Activated += new EventHandler(TextSelectionAndFillFormsAction_Executed);
            visualToolsToolBar.AddAction(textSelectionAndFillFormsAction);
            textSelectionAndFillFormsAction.Activate();

            VisualToolAction textSelectionAction = new VisualToolAction(
                _textSelectionTool,
                "Text Selection",
                "Text Selection",
                DemosResourcesManager.GetResourceAsBitmap("TextExtraction.png"));
            textSelectionAction.Activated += new EventHandler(TextSelectionAction_Executed);
            visualToolsToolBar.AddAction(textSelectionAction);


            VisualToolAction annotationToolEditAnnotationsAction = new VisualToolAction(
                "Edit Annotations",
                "Edit Annotations",
                null,
                false);
            annotationToolEditAnnotationsAction.Clicked += new EventHandler(annotationToolEditAnnotationsAction_Clicked);
            VisualToolAction annotationToolEditFormFieldsAction = new VisualToolAction(
                "Edit Form Fields",
                "Edit Interctive Form Fields",
                null,
                false);
            annotationToolEditFormFieldsAction.Clicked += new EventHandler(annotationToolEditFormFieldsAction_Clicked);
            VisualToolAction annotationToolAction = new VisualToolAction(
                new WpfCompositeVisualTool(
#if !REMOVE_OFFICE_PLUGIN
                new Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction.WpfOfficeDocumentVisualEditorTextTool(),
#endif
                    _annotationTool),
                "Interactive Forms / Annotations",
                "Edit Interactive Forms and Annotations",
                DemosResourcesManager.GetResourceAsBitmap("EditInteractiveForms.png"),
                annotationToolEditAnnotationsAction,
                annotationToolEditFormFieldsAction);
            annotationToolAction.Activated += new EventHandler(AnnotationToolAction_Executed);
            visualToolsToolBar.AddAction(annotationToolAction);

#if !REMOVE_PDFVISUALEDITOR_PLUGIN
            VisualToolAction contentEditorToolAction = new VisualToolAction(
               _contentEditorToolComposition,
               "Content Editor",
               "Edit Page Content",
               DemosResourcesManager.GetResourceAsBitmap("ContentEditor.png"));
            contentEditorToolAction.Activated += new EventHandler(ContentEditorToolAction_Executed);
            visualToolsToolBar.AddAction(contentEditorToolAction);

            VisualToolAction removeContentToolAction = new VisualToolAction(
               _redactionTool,
               "Remove Content",
               "Remove Content & Black Out",
               DemosResourcesManager.GetResourceAsBitmap("RemoveContent.png"));
            removeContentToolAction.Activated += new EventHandler(RemoveContentToolAction_Executed);
            visualToolsToolBar.AddAction(removeContentToolAction);

            VisualToolAction cropSelectionToolAction = new VisualToolAction(
               _cropSelectionTool,
               "Crop Selection",
               "Crop Selection",
               DemosResourcesManager.GetResourceAsBitmap("CropSelection.png"));
            visualToolsToolBar.AddAction(cropSelectionToolAction);
#endif
            visualToolsToolBar.AddAction(new SeparatorToolBarAction());

            string annotationResourceNameFormatString = "{0}.png";

            VisualToolAction highlightTextToolAction = new VisualToolAction(
               _defaultAnnotationTool,
               "Highlight Text",
               "Markup text using Highlight text markup annotation",
               DemosResourcesManager.GetResourceAsBitmap(string.Format(annotationResourceNameFormatString, "Highlight")));
            visualToolsToolBar.AddAction(highlightTextToolAction);
            highlightTextToolAction.Activated += HighlightTextToolAction_Activated;
            highlightTextToolAction.Deactivated += TextMarkupToolAction_Deactivated;

            VisualToolAction strikeoutTextToolAction = new VisualToolAction(
               _defaultAnnotationTool,
               "Strikeout Text",
               "Markup text using Strikeout text markup annotation",
               DemosResourcesManager.GetResourceAsBitmap(string.Format(annotationResourceNameFormatString, "Strikeout")));
            visualToolsToolBar.AddAction(strikeoutTextToolAction);
            strikeoutTextToolAction.Activated += StrikeoutTextToolAction_Activated;
            strikeoutTextToolAction.Deactivated += TextMarkupToolAction_Deactivated;

            VisualToolAction underlineTextToolAction = new VisualToolAction(
               _defaultAnnotationTool,
               "Underline Text",
               "Markup text using Underline text markup annotation",
               DemosResourcesManager.GetResourceAsBitmap(string.Format(annotationResourceNameFormatString, "Underline")));
            visualToolsToolBar.AddAction(underlineTextToolAction);
            underlineTextToolAction.Activated += UnderlineTextToolAction_Activated;
            underlineTextToolAction.Deactivated += TextMarkupToolAction_Deactivated;

            VisualToolAction squigglyTextToolAction = new VisualToolAction(
              _defaultAnnotationTool,
              "Squiggly Underline Text",
              "Markup text using Squiggly underline text markup annotation",
              DemosResourcesManager.GetResourceAsBitmap(string.Format(annotationResourceNameFormatString, "Squiggly")));
            visualToolsToolBar.AddAction(squigglyTextToolAction);
            squigglyTextToolAction.Activated += SquigglyTextToolAction_Activated;
            squigglyTextToolAction.Deactivated += TextMarkupToolAction_Deactivated;

            VisualToolAction caretTextToolAction = new VisualToolAction(
             _defaultAnnotationTool,
             "Insert Text Caret Annotation",
             "Insert Text Caret Annotation under cursor position",
             DemosResourcesManager.GetResourceAsBitmap(string.Format(annotationResourceNameFormatString, "Caret")));
            visualToolsToolBar.AddAction(caretTextToolAction);
            caretTextToolAction.Activated += CaretTextToolAction_Activated;
            caretTextToolAction.Deactivated += TextMarkupToolAction_Deactivated;
        }

        #endregion


        #region UI state

        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            if (!_enableUpdateUI)
                return;

            // get the current status of application

            bool isPdfFileOpening = IsPdfFileOpening;
            bool isPdfFileLoaded = _document != null;
            bool isPdfFileReadOnlyMode = IsPdfFileReadOnlyMode;
            bool isPdfFileEmpty = true;
            bool isDocumentChanged = IsDocumentChanged;
            if (_contentEditorTool != null)
            {
                if (_contentEditorTool.FigureViewCollection.Count > 0)
                    isDocumentChanged = true;
            }
            bool isPdfFileSaving = IsPdfFileSaving;
            bool isEditorToolActivated = CurrentTool != null && CurrentTool is WpfPdfContentEditorTool;
            if (!isEditorToolActivated && CurrentTool is WpfCompositeVisualTool)
            {
                WpfCompositeVisualTool compositeTool = (WpfCompositeVisualTool)CurrentTool;
                foreach (WpfVisualTool tool in compositeTool)
                {
                    if (tool is WpfPdfContentEditorTool)
                    {
                        isEditorToolActivated = true;
                        break;
                    }
                }
            }

            bool isTextSearching = IsTextSearching;
            bool hasInteractiveForm = isPdfFileLoaded && _document.InteractiveForm != null;

            if (isPdfFileLoaded)
                isPdfFileEmpty = imageViewer1.Images.Count <= 0;


            // "File" menu

            fileMenuItem.IsEnabled = !isPdfFileOpening && !isPdfFileSaving && !isTextSearching;

            closeMenuItem.IsEnabled = isPdfFileLoaded;
            addPagesMenuItem.IsEnabled = isPdfFileLoaded;
            addOcrPagesMenuItem.IsEnabled = isPdfFileLoaded;
            addEmptyPageMenuItem.IsEnabled = isPdfFileLoaded;
            packMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileEmpty;
            securitySettingsMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileReadOnlyMode;
            saveMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileEmpty && !isPdfFileReadOnlyMode &&
                                            isDocumentChanged;
            saveAsMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileEmpty;
            saveToMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileEmpty;
            convertToTiffMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileEmpty;
            convertToSvgMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileEmpty;
            convertToDocxMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileEmpty;
            pageSettingsMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileEmpty;
            printMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileEmpty;


            // "View" menu                              
            customRendererSettingsMenuItem.IsEnabled = useCustomRendererMenuItem.IsChecked;

            // update "View => Image Display Mode" menu
            singlePageMenuItem.IsChecked = false;
            twoColumnsMenuItem.IsChecked = false;
            singleContinuousRowMenuItem.IsChecked = false;
            singleContinuousColumnMenuItem.IsChecked = false;
            twoContinuousRowsMenuItem.IsChecked = false;
            twoContinuousColumnsMenuItem.IsChecked = false;
            switch (imageViewer1.DisplayMode)
            {
                case ImageViewerDisplayMode.SinglePage:
                    singlePageMenuItem.IsChecked = true;
                    break;

                case ImageViewerDisplayMode.TwoColumns:
                    twoColumnsMenuItem.IsChecked = true;
                    break;

                case ImageViewerDisplayMode.SingleContinuousRow:
                    singleContinuousRowMenuItem.IsChecked = true;
                    break;

                case ImageViewerDisplayMode.SingleContinuousColumn:
                    singleContinuousColumnMenuItem.IsChecked = true;
                    break;

                case ImageViewerDisplayMode.TwoContinuousRows:
                    twoContinuousRowsMenuItem.IsChecked = true;
                    break;

                case ImageViewerDisplayMode.TwoContinuousColumns:
                    twoContinuousColumnsMenuItem.IsChecked = true;
                    break;
            }

            // "Document" menu

            documentMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileOpening && !isPdfFileSaving &&
                                                !isTextSearching;
            digitalSignaturesMenuItem.IsEnabled = !isPdfFileEmpty;
            bookmarksMenuItem.IsEnabled = !isPdfFileEmpty;
            thumbnailsMenuItem.IsEnabled = !isPdfFileEmpty;
            viewerPreferencesMenuItem.IsEnabled = isPdfFileLoaded;
            removeAttachmentsMenuItem.IsEnabled = isPdfFileLoaded && _document.Attachments != null;
            optionalContentMenuItem.IsEnabled = isPdfFileLoaded && _document.OptionalContentProperties != null;
            removeLayersOptionalContentMenuItem.IsEnabled = isPdfFileLoaded && _document.OptionalContentProperties != null;


            // "Page" menu
            pageMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileOpening && !isPdfFileEmpty &&
                                            !isPdfFileSaving && !isTextSearching;

            // "Text" menu
            UpdateTextMenuUI();


            // update the form title
            UpdateFormTitle();

            // update information about focused page
            UpdateFocusedImageInfo();


            // resolution 
            resolutionToolBar.IsEnabled = !isPdfFileOpening && !isPdfFileSaving && !isTextSearching;

            // tools tab control
            toolsTabControl.IsEnabled = isPdfFileLoaded && !isPdfFileEmpty && !isPdfFileOpening && !IsPdfFileSaving && !isTextSearching;

            // change the status of bookmark panel
            documentBookmarks.IsEnabled = !isPdfFileEmpty;


            // viewerToolStrip
            viewerToolBar.IsEnabled = !IsPdfFileOpening && !isPdfFileSaving;
            viewerToolBar.SaveButtonEnabled = isPdfFileLoaded && !isPdfFileEmpty && !isPdfFileReadOnlyMode &&
                                                isDocumentChanged && !isTextSearching;
            viewerToolBar.PrintButtonEnabled = isPdfFileLoaded && !isPdfFileEmpty;
        }

        /// <summary>
        /// Updates the user interface of "Text" menu.
        /// </summary>
        private void UpdateTextMenuUI()
        {
            // get the current status of application
            bool isPdfFileOpening = IsPdfFileOpening;
            bool isPdfFileLoaded = _document != null;
            bool isPdfFileEmpty = true;
            bool isPdfFileSaving = IsPdfFileSaving;
            bool isTextSelected = false;
            bool isTextSearching = IsTextSearching;

            if (_textSelectionTool != null && _textSelectionTool.ImageViewer != null)
            {
                isTextSelected = _textSelectionTool.HasSelectedText;
            }

            if (isPdfFileLoaded)
                isPdfFileEmpty = imageViewer1.Images.Count <= 0;

            textMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileOpening && !isPdfFileEmpty &&
                                            !isPdfFileSaving && !isTextSearching;
            highlightSelectedTextMenuItem.IsEnabled = isTextSelected;
            highlightSelectedTextAnnotateMenuItem.IsEnabled = isTextSelected;
            strikeoutSelectedTextAnnotateMenuItem.IsEnabled = isTextSelected;
            underlineSelectedTextAnnotateMenuItem.IsEnabled = isTextSelected;
            squigglyUnderlineSelectedTextAnnotateMenuItem.IsEnabled = isTextSelected;

            removeSelectedTextMenuItem.IsEnabled = isTextSelected;
            addSelectedTextToRedactionMarksMenuItem.IsEnabled = isTextSelected;
        }

        /// <summary>
        /// Updates the form title.
        /// </summary>
        private void UpdateFormTitle()
        {
            bool isPdfFileLoaded = _document != null;

            if (isPdfFileLoaded && _pdfFileName != null)
            {
                string title = Path.GetFileName(_pdfFileName);
                if (_document.HasDocumentInformation && _document.DocumentInformation.Title != "")
                    title = string.Format("{0} ({1})", _document.DocumentInformation.Title, title);
                if (_document.IsEncrypted)
                    title += " (SECURED)";
                if (_isDocumentChanged)
                    title += "*";
                Title = string.Format(_titleFormatString, "- " + title);
            }
            else
            {
                Title = string.Format(_titleFormatString, "");
            }
        }

        /// <summary>
        /// Updates information about the focused image.
        /// </summary>
        private void UpdateFocusedImageInfo()
        {
            if (imageViewer1.Image == null)
            {
                pageInfoLabel.Content = "";
                return;
            }
            Resolution resolution = imageViewer1.Image.Resolution;
            string resolutionString;
            if (imageViewer1.Image.IsBad)
            {
                pageInfoLabel.Content = "Bad image!";
            }
            else
            {
                if (resolution.Horizontal == resolution.Vertical)
                    resolutionString = resolution.Horizontal.ToString();
                else
                    resolutionString = string.Format("{0}x{1}", resolution.Horizontal, resolution.Vertical);
                pageInfoLabel.Content = string.Format("Resolution: {0} DPI; Size: {1}x{2} px", resolutionString, imageViewer1.Image.Width, imageViewer1.Image.Height);
                if (imageViewer1.Image.LoadingError)
                    pageInfoLabel.Content = string.Format("[Loading error: {0}] {1}", imageViewer1.Image.LoadingErrorString, pageInfoLabel.Content);
            }
        }

        /// <summary>
        /// Update UI safely.
        /// </summary>
        private void UpdateUIAsync()
        {
            if (Dispatcher.Thread == Thread.CurrentThread)
                UpdateUI();
            else
                Dispatcher.BeginInvoke(new UpdateUIDelegate(UpdateUI));
        }

        #endregion


        #region UI

        #region 'File' menu

        /// <summary>
        /// Creates new PDF document.
        /// </summary>
        private void newMenuItem_Click(object sender, RoutedEventArgs e)
        {
            IsPdfFileOpening = true;
            try
            {
                CreateNewPdfDocument(PdfFormat.Pdf_16, null);
            }
            finally
            {
                IsPdfFileOpening = false;
            }
        }

        /// <summary>
        /// Assemblies a PDF Portfolio.
        /// </summary>
        private void createPortfolioMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string documentFilename;
            IsPdfFileOpening = true;
            try
            {
                if (!CreateNewPdfDocument(PdfFormat.Pdf_17, "Save Portfolio As"))
                    return;
                documentFilename = _pdfFileName;
            }
            finally
            {
                IsPdfFileOpening = false;
            }
            System.Windows.Forms.FolderBrowserDialog openFolder = new System.Windows.Forms.FolderBrowserDialog();
            openFolder.ShowNewFolderButton = false;
            openFolder.SelectedPath = Path.GetDirectoryName(documentFilename);
            openFolder.Description = "Select root path from which files and folders will be imported to Portfolio.";
            if (openFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // add page to document
                PdfPage page = _document.Pages.Add(PaperSizeKind.A4);

                // draw text on first page
                using (PdfGraphics g = page.GetGraphics())
                {
                    TextBoxFigure textBox = new TextBoxFigure();
                    textBox.Font = _document.FontManager.GetStandardFont(PdfStandardFontType.TimesRoman);
                    textBox.FontSize = 30;
                    textBox.Location = new System.Drawing.PointF(0, 0);
                    textBox.Size = page.MediaBox.Size;
                    textBox.TextAlignment = PdfContentAlignment.Top | PdfContentAlignment.Left | PdfContentAlignment.Right;
                    textBox.TextBrush = new PdfBrush(System.Drawing.Color.Black);
                    textBox.Text = "This document is Portfolio\n(Attachment Collection)\nTo view Portfolio you should use PDF viewer compatible with PDF 1.7 ExtensionLevel 3.";
                    textBox.Draw(g);
                }

                // create addtachements
                _document.CreateAttachments(true);

                // set viewer settings
                _document.Attachments.View = AttachmentCollectionViewMode.TileMode;
                _document.Attachments.SplitterBar = new PdfAttachmentCollectionSplitterBar(_document);
                _document.Attachments.SplitterBar.Direction = AttachmentCollectionSplitterBarDirection.None;
                _document.DocumentViewMode = PdfDocumentViewMode.UseAttachments;

                // save changes to document stream
                _document.SaveChanges();

                // add created page to viewer
                imageViewer1.Images.Add(_pdfFileStream);

                // add files and folders to portfolio
                StatusBarActionController actionController =
                    new StatusBarActionController(mainStatusBar, statusLabel, progressBar);
                actionController.StartAction("Add files to portfolio");
                AddPathRecursively(_document.Attachments.RootFolder, openFolder.SelectedPath, false, PdfCompression.None, actionController);
                actionController.EndAction();

                // show attachments editor
                AttachmentsEditorWindow window = new AttachmentsEditorWindow(_document);
                window.Owner = this;
                window.ShowDialog();
            }
        }

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        private void openMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_openPdfFileDialog.ShowDialog().Value)
            {
                IsPdfFileOpening = true;
                OpenPdfDocument(_openPdfFileDialog.FileName);
                IsPdfFileOpening = false;
                if (_document != null)
                {
                    if (_document.Attachments != null && _document.Attachments.View != AttachmentCollectionViewMode.Hidden)
                    {
                        AttachmentsEditorWindow window = new AttachmentsEditorWindow(_document);
                        window.Owner = this;
                        window.ShowDialog();
                    }
                }
            }
        }

        /// <summary>
        /// Closes PDF document.
        /// </summary>
        private void closeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ClosePdfDocument();
        }


        /// <summary>
        /// Adds images, as pages, to PDF document.
        /// </summary>
        private void addPagesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            IsPdfFileOpening = true;
            if (_openImageFileDialog.ShowDialog().Value)
            {
                StartAction("Add pages", false);
                try
                {
                    for (int i = 0; i < _openImageFileDialog.FileNames.Length; i++)
                    {
                        try
                        {
                            imageViewer1.Images.Add(_openImageFileDialog.FileNames[i]);
                            _isDocumentChanged = true;
                        }
                        catch (Exception ex)
                        {
                            DemosTools.ShowErrorMessage(ex, _openImageFileDialog.FileNames[i]);
                        }
                    }
                    if (_document.Pages.Count == 0)
                        SaveChangesInPdfDocument(PdfDocumentUpdateMode.Incremental);
                }
                finally
                {
                    EndAction();
                }
            }
            IsPdfFileOpening = false;
        }

        /// <summary>
        /// Handles the Click event of addOcrPagesUsingImageOverTextModeMenuItem object.
        /// </summary>
        private void addOcrPagesUsingImageOverTextModeMenuItem_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_OCR_PLUGIN
            RecognizeTextAndAddOcrResultToPdfDocument(PdfPageCreationMode.ImageOverText);
#endif
        }

        /// <summary>
        /// Handles the Click event of addOcrPagesUsingTextOverImageModeMenuItem object.
        /// </summary>
        private void addOcrPagesUsingTextOverImageModeMenuItem_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_OCR_PLUGIN
            RecognizeTextAndAddOcrResultToPdfDocument(PdfPageCreationMode.TextOverImage);
#endif
        }

#if !REMOVE_OCR_PLUGIN
        /// <summary>
        /// Recognizes text in images and adds result OCR pages to a searchable PDF document.
        /// </summary>
        /// <param name="pdfPageCreationMode">The mode that defines how to create PDF page.</param>
        private void RecognizeTextAndAddOcrResultToPdfDocument(PdfPageCreationMode pdfPageCreationMode)
        {
            // create a dialog that allows to select images from files ot TWAIN scanner
            AddImagesWindow form = new AddImagesWindow();
            form.Owner = this;

            // show dialog
            if (form.ShowDialog() == true)
            {
                int existingPageCount = imageViewer1.Images.Count;

                if (_tesseract == null)
                    // create Tesseract OCR engine
                    _tesseract = new TesseractOcr(TesseractOcrDllDirectory);

                // searchable PDF generator
                SearchablePdfGenerator pdfGenerator = null;
                // if searchable PDF generator for selected PDF page creating mode does not exist
                if (!_pdfPageCreationModeToGenerator.TryGetValue(pdfPageCreationMode, out pdfGenerator))
                {
                    // create searchable PDF generator
                    pdfGenerator = new SearchablePdfGenerator(new OcrEngineManager(_tesseract));
                    // specify PDF page creating mode in PDF generator
                    pdfGenerator.PageCreationMode = pdfPageCreationMode;

                    // create Tesseract OCR settings
                    TesseractOcrSettings tesseractOcrSettings = new TesseractOcrSettings(OcrLanguage.English);
                    tesseractOcrSettings.RecognitionRegionType = RecognitionRegionType.RecognizePageWithPageSegmentationAndOrientationDetection;
                    if (pdfPageCreationMode == PdfPageCreationMode.TextOverImage)
                        tesseractOcrSettings.UseSymbolRegionsCorrection = true;
                    else
                        tesseractOcrSettings.UseSymbolRegionsCorrection = false;
                    // set Tesseract OCR settings in PDF generator
                    pdfGenerator.OcrEngineSettings = tesseractOcrSettings;

                    // save reference to created PDF generator
                    _pdfPageCreationModeToGenerator.Add(pdfPageCreationMode, pdfGenerator);
                }

                // use current PDF document format
                pdfGenerator.PdfFormat = _document.Format;

                // set images to process
                pdfGenerator.SourceImages = form.Images;

                // if source images are processed using image segmentation command
                if (form.SegmentationResults != null && form.SegmentationResults.Count > 0)
                {
                    // set image regions in PDF generator

                    pdfGenerator.SourceImagesRegions = new Dictionary<VintasoftImage, IEnumerable<ImageRegion>>();
                    foreach (VintasoftImage image in form.SegmentationResults.Keys)
                        pdfGenerator.SourceImagesRegions.Add(image, form.SegmentationResults[image]);
                }
                else
                {
                    pdfGenerator.SourceImagesRegions = null;
                }

                // process images (cleanup and recognize) and add results to imageViewer1.Images
                ProcessingCommandWindow<ImageCollection> window = new ProcessingCommandWindow<ImageCollection>(imageViewer1.Images, pdfGenerator);
                window.Owner = this;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                // process images and add results to imageViewer1.Images
                window.ShowDialog();

                // if source PDF document is empty
                if (existingPageCount == 0 && imageViewer1.Images.Count > 0)
                {
                    // save changes in PDF document
                    SaveChangesInPdfDocument(PdfDocumentUpdateMode.Incremental);
                }

                // set focused index to the first added image
                if (existingPageCount < imageViewer1.Images.Count)
                    imageViewer1.FocusedIndex = existingPageCount;
            }

            // dispose source images
            form.Images.ClearAndDisposeItems();
        }
#endif

        /// <summary>
        /// Adds an empty page to PDF document.
        /// </summary>
        private void addEmptyPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            IsPdfFileOpening = true;
            AddEmptyPageWindow addEmptyPageDialog = new AddEmptyPageWindow(_emptyPageSize, _emptyPageUnits);
            addEmptyPageDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addEmptyPageDialog.Owner = this;
            if (addEmptyPageDialog.ShowDialog().Value)
            {
                _emptyPageSize = addEmptyPageDialog.PageSizeInUserUnits;
                _emptyPageUnits = addEmptyPageDialog.Units;

                // create temp stream
                Stream stream = new MemoryStream();

                // create temp document
                PdfDocument document = new PdfDocument(stream, PdfFormat.Pdf_14);
                if (addEmptyPageDialog.PaperKind == PaperSizeKind.Custom)
                {
                    document.Pages.Add(_emptyPageSize);
                }
                else
                {
                    if (addEmptyPageDialog.Rotated)
                        document.Pages.Add(ImageSize.FromPaperKindRotated(addEmptyPageDialog.PaperKind));
                    else
                        document.Pages.Add(addEmptyPageDialog.PaperKind);
                }
                document.SaveChanges();
                // close document
                document.Dispose();

                // add document to collection
                imageViewer1.Images.Add(stream, true);

                _isDocumentChanged = true;

                if (_document.Pages.Count == 0)
                    SaveChangesInPdfDocument(PdfDocumentUpdateMode.Incremental);
            }
            IsPdfFileOpening = false;
        }

        /// <summary>
        /// Adds a PDF document to the current PDF document.
        /// </summary>
        private void addPdfDocumentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_openPdfFileDialog.ShowDialog() == true)
            {
                if (MessageBox.Show("Current document will be saved automatically. Continue?", "Add PDF document", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    IsPdfFileOpening = true;
                    try
                    {
                        PdfDocument sourceDocument = _document;

                        // open selected PDF document
                        using (PdfDocument tempDocument = new PdfDocument(_openPdfFileDialog.FileName))
                        {
                            // create PdfDocumentCopyCommand
                            PdfDocumentCopyCommand copyDocument = new PdfDocumentCopyCommand(sourceDocument);

                            // copy selected PDF document at the end of current PDF document
                            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(tempDocument, copyDocument);

                            // if PDF document is loaded in image viewer
                            if (imageViewer1.Images.Count > 0)
                                // save changes in current PDF document using PdfEncoder
                                SaveChangesInPdfDocument(PdfDocumentUpdateMode.Incremental);
                            // if PDF document is NOT loaded in image viewer
                            else
                                // save changes in current PDF document using PdfDocument
                                sourceDocument.SaveChanges();
                        }

                        // reload current PDF document
                        string filename = Filename;
                        ClosePdfDocument();
                        OpenPdfDocument(filename);
                    }
                    catch (Exception ex)
                    {
                        DemosTools.ShowErrorMessage(ex);
                    }
                    finally
                    {
                        IsPdfFileOpening = false;
                    }
                }
            }
        }

        /// <summary>
        /// Packs PDF document.
        /// </summary>
        private void packMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OnPdfDocumentSaving();
            bool isSaved = false;
            try
            {
                SelectPdfFormatWindow selectFormat = new SelectPdfFormatWindow(_document.Format, _document.EncryptionSystem);
                selectFormat.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                selectFormat.Owner = this;
                if (selectFormat.ShowDialog() == true)
                {
                    isSaved = SaveAndPackPdfDocument(selectFormat.Format, selectFormat.NewEncryptionSettings);
                }
            }
            finally
            {
                OnPdfDocumentSaved(!isSaved);
            }
        }

        /// <summary>
        /// Save changes in PDF document.
        /// </summary>
        private void saveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OnPdfDocumentSaving();
            try
            {
                SaveChangesInPdfDocument(PdfDocumentUpdateMode.Incremental);
            }
            finally
            {
                OnPdfDocumentSaved(false);
            }
        }

        /// <summary>
        /// Saves PDF document to new source and switches to the source.
        /// </summary>
        private void saveAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SavePdfDocumentAs();
        }

        /// <summary>
        /// Saves PDF document to new source but do not switches to the source.
        /// </summary>
        private void saveToMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OnPdfDocumentSaving();
            bool isCanceled = true;
            try
            {
                _saveFileDialog.Title = null;
                if (_saveFileDialog.ShowDialog().Value)
                {
                    isCanceled = false;
                    try
                    {
                        SavePdfDocument(_saveFileDialog.FileName, false);
                    }
                    catch (Exception ex)
                    {
                        DemosTools.ShowErrorMessage(ex);
                    }
                }
            }
            finally
            {
                OnPdfDocumentSaved(isCanceled);
            }
        }

        /// <summary>
        /// Converts PDF document to TIFF file.
        /// </summary>
        private void convertToTiffMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _convertToFileDialog.Filter = "TIFF Files|*.tif";

            if (_convertToFileDialog.ShowDialog().Value)
            {
                // create dialog that allows to set settings of TIFF encoder
                TiffEncoderSettingsWindow tiffSaveSettingsDialog = new TiffEncoderSettingsWindow();
                tiffSaveSettingsDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                tiffSaveSettingsDialog.Owner = this;

                // create TIFF encoder
                TiffEncoder tiffEncoder = new TiffEncoder(true);
                tiffSaveSettingsDialog.EncoderSettings = tiffEncoder.Settings;
                // set settings of TIFF encoder
                if (tiffSaveSettingsDialog.ShowDialog().Value)
                {
                    // render the figure collection on focused PDF page
                    _contentEditorTool.RenderFiguresOnPage();

                    // specify that image collection of image viewer should not switch to the saved file
                    tiffEncoder.SaveAndSwitchSource = false;

                    // start the 'Convert to SVG" action
                    StartAction("Convert to TIFF", true);

                    // start the asynchronous saving of PDF document to a TIFF file
                    imageViewer1.Images.SaveAsync(_convertToFileDialog.FileName, tiffEncoder);
                }
            }
        }

        /// <summary>
        /// Converts PDF document to the SVG files.
        /// </summary>
        private void convertToSvgMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _convertToFileDialog.Filter = "SVG Files|*.svg";

            if (_convertToFileDialog.ShowDialog().Value)
            {
                // create dialog that allows to set settings of SVG encoder
                SvgEncoderSettingsWindow svgSaveSettingsDialog = new SvgEncoderSettingsWindow();

                // create SVG encoder
                SvgEncoder svgEncoder = new SvgEncoder();
                svgSaveSettingsDialog.EncoderSettings = svgEncoder.Settings;
                // set settings of SVG encoder
                if (svgSaveSettingsDialog.ShowDialog().Value)
                {
                    // render the figure collection on focused PDF page
                    _contentEditorTool.RenderFiguresOnPage();

                    // specify that image collection of image viewer should not switch to the saved file
                    svgEncoder.SaveAndSwitchSource = false;

                    // start the 'Convert to SVG" action
                    StartAction("Convert to SVG", true);

                    // start the asynchronous converting of PDF document to SVG files
                    Thread savingThread = new Thread(new ParameterizedThreadStart(ConvertPdfDocumentToSvgFilesThread));
                    savingThread.IsBackground = true;
                    savingThread.Start(new object[] { svgEncoder, _convertToFileDialog.FileName });
                }
            }
        }

        /// <summary>
        /// Converts PDF file to a Docx file.
        /// </summary>
        private void convertToDocxMenuItem_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_OFFICE_PLUGIN
            if (IsDocumentChanged)
            {
                if (MessageBox.Show(
                    "The document is not saved\n" +
                    "Do you want to save PDF document right now?\n\n" +
                    "Click 'Yes' if you want to save PDF document right now.\n" +
                    "Click 'No' if you do not want save PDF document.",
                    "Save document",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    SavePdfDocumentAs();

                    // if document is not saved
                    if (IsDocumentChanged)
                        return;
                }
                else
                {
                    return;
                }
            }

            _convertToFileDialog.Filter = "DOCX Files|*.docx";
            if (!string.IsNullOrEmpty(Filename))
                _convertToFileDialog.FileName = Path.GetFileNameWithoutExtension(Filename);

            if (_convertToFileDialog.ShowDialog() == true)
            {
                // convert PDF document to DOCX file in background thread
                Thread savingThread = new Thread(new ParameterizedThreadStart(ConvertPdfDocumentToDocxFileThread));
                savingThread.Name = "Convert To DOCX";
                savingThread.IsBackground = true;
                savingThread.Start(_convertToFileDialog.FileName);
            }
#endif
        }

        /// <summary>
        /// Shows a page settings dialog.
        /// </summary>
        private void pageSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PageSettingsWindow pageSettingsForm =
                new PageSettingsWindow(
                    _thumbnailViewerPrintManager,
                    _thumbnailViewerPrintManager.PagePadding,
                    _thumbnailViewerPrintManager.ImagePadding);

            pageSettingsForm.Owner = this;
            if (pageSettingsForm.ShowDialog().Value)
            {
                _thumbnailViewerPrintManager.PagePadding = pageSettingsForm.PagePadding;
                _thumbnailViewerPrintManager.ImagePadding = pageSettingsForm.ImagePadding;
            }
        }

        /// <summary>
        /// Prints page(s) of PDF document.
        /// </summary>
        private void printMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PrintPdfDocument();
        }


        /// <summary>
        /// Exits the application.
        /// </summary>
        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ClosePdfDocument();
            Close();
        }


        /// <summary>
        /// Updates UI when "File" menu is opening.
        /// </summary>
        private void fileMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        #endregion


        #region 'Edit' menu

        /// <summary>
        /// "Edit" menu is opened.
        /// </summary>
        private void editMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            UpdateEditMenuItems();
        }

        /// <summary>
        /// "Edit" menu is closed.
        /// </summary>
        private void editMenuItem_SubmenuClosed(object sender, RoutedEventArgs e)
        {
            EnableEditMenuItems();
        }


        /// <summary>
        /// Cuts selected item into clipboard.
        /// </summary>
        private void cutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CutItemUIAction cutUIAction = PdfDemosTools.GetUIAction<CutItemUIAction>(CurrentTool);
            if (cutUIAction != null)
                cutUIAction.Execute();
        }

        /// <summary>
        /// Copies selected item into clipboard.
        /// </summary>
        private void copyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CopyItemUIAction copyUIAction = PdfDemosTools.GetUIAction<CopyItemUIAction>(CurrentTool);
                if (copyUIAction != null)
                    copyUIAction.Execute();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Pastes previously copied item from clipboard.
        /// </summary>
        private void pasteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PasteItemUIAction pasteUIAction = PdfDemosTools.GetUIAction<PasteItemUIAction>(CurrentTool);
            if (pasteUIAction != null)
                pasteUIAction.Execute();
        }

        /// <summary>
        /// Deletes selected item of current visual tool.
        /// </summary>
        private void deleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (thumbnailViewer1.IsFocused)
            {
                thumbnailViewer1.DoDelete();
            }
            else
            {
                DeleteItemUIAction deleteUIAction = PdfDemosTools.GetUIAction<DeleteItemUIAction>(CurrentTool);
                if (deleteUIAction != null)
                    deleteUIAction.Execute();
            }
        }

        /// <summary>
        /// Selects all items of current visual tool.
        /// </summary>
        private void selectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (thumbnailViewer1.IsFocused)
            {
                thumbnailViewer1.DoSelectAll();
            }
            else
            {
                SelectAllItemsUIAction selectAllUIAction = PdfDemosTools.GetUIAction<SelectAllItemsUIAction>(CurrentTool);
                if (selectAllUIAction != null)
                    selectAllUIAction.Execute();
            }
        }

        /// <summary>
        /// Deselects all items of current visual tool.
        /// </summary>
        private void deselectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!thumbnailViewer1.IsFocused)
            {
                DeselectAllItemsUIAction deselectAllUIAction = PdfDemosTools.GetUIAction<DeselectAllItemsUIAction>(CurrentTool);
                if (deselectAllUIAction != null)
                    deselectAllUIAction.Execute();
            }
        }

        #endregion


        #region 'View' menu

        /// <summary>
        /// Enables/disables the processing of touch screen events.
        /// </summary>
        private void touchScreenToolMenuItem_CheckChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (touchScreenZoomToolMenuItem.IsChecked)
                    // enable processing of touch screen events in zoom tool
                    _zoomTouchTool.ProcessTouchEvents = true;
                else
                    // disable processing of touch screen events in zoom tool
                    _zoomTouchTool.ProcessTouchEvents = false;
                _zoomTouchTool.IsEnabled = _zoomTouchTool.ProcessTouchEvents;

                if (touchScreenPanToolMenuItem.IsChecked)
                    // enable processing of touch screen events in pan tool
                    _panTouchTool.ProcessTouchEvents = true;
                else
                    // disable processing of touch screen events in pan tool
                    _panTouchTool.ProcessTouchEvents = false;
                _panTouchTool.IsEnabled = _panTouchTool.ProcessTouchEvents;
            }
            catch (NotSupportedException)
            {
                DemosTools.ShowErrorMessage("Touch screen events can be processed starting from .NET 4.");

                touchScreenZoomToolMenuItem.IsChecked = false;
                touchScreenPanToolMenuItem.IsChecked = false;
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);

                touchScreenZoomToolMenuItem.IsChecked = false;
                touchScreenPanToolMenuItem.IsChecked = false;
            }
        }


        /// <summary>
        /// Shows interaction area settings form for PDF annotation tool.
        /// </summary>
        private void interactionPointSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WpfInteractionAreaAppearanceManagerWindow window = new WpfInteractionAreaAppearanceManagerWindow();
            window.Owner = this;
            window.InteractionAreaSettings = InteractionAreaAppearanceManager;
            window.ShowDialog();
        }


        #region PDF actions execution

        /// <summary>
        /// Enables or disables PDF actions execution.
        /// </summary>
        private void enableExecuteActionsMenuItem_CheckedChanged(object sender, RoutedEventArgs e)
        {
            PdfActionExecutorManager.ApplicationActionExecutor.IsEnabled =
                enableExecuteActionsMenuItem.IsChecked;
        }

        /// <summary>
        /// Enables or disables JavaScript interpreter.
        /// </summary>
        private void enableJavaScriptExecutingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfJavaScriptManager.IsJavaScriptEnabled = enableJavaScriptExecutingMenuItem.IsChecked;
        }

        /// <summary>
        /// Shows the JavaScript debugger window.
        /// </summary>
        private void debuggerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_debuggerWindow == null)
                _debuggerWindow = new PdfJavaScriptDebuggerWindow(PdfJavaScriptManager.JavaScriptActionExecutor, imageViewer1);
            _debuggerWindow.Show();
        }

        #endregion


        #region Thumbnail viewer

        /// <summary>
        /// Shows the thumbnail viewer settings. 
        /// </summary>
        private void thumbnailViewerSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ThumbnailViewerSettingsWindow viewerSettingsDialog = new ThumbnailViewerSettingsWindow(thumbnailViewer1, (Style)Resources["ThumbnailItemStyle"]);
            viewerSettingsDialog.Owner = this;
            viewerSettingsDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            viewerSettingsDialog.ShowDialog();
        }

        /// <summary>
        /// Enables/disables usage of embedded thumbnails of PDF pages by thumbnail viewer.
        /// </summary>
        private void useEmbeddedThumbnailsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_document != null)
                _document.RenderingSettings.UseEmbeddedThumbnails = useEmbeddedThumbnailsMenuItem.IsChecked;
        }

        #endregion


        #region Image viewer

        /// <summary>
        /// Changes image display mode of image viewer.
        /// </summary>
        private void ImageDisplayMode_Click(object sender, RoutedEventArgs e)
        {
            MenuItem imageDisplayModeMenuItem = (MenuItem)sender;
            imageViewer1.DisplayMode = (ImageViewerDisplayMode)imageDisplayModeMenuItem.Tag;
            _defaultImageViewerDisplayMode = imageViewer1.DisplayMode;
            UpdateUI();
        }

        /// <summary>
        /// Changes image scale mode of image viewer.
        /// </summary>
        private void ImageScale_Click(object sender, RoutedEventArgs e)
        {
            _imageScaleSelectedMenuItem.IsChecked = false;
            _imageScaleSelectedMenuItem = (MenuItem)sender;

            // if menu item sets ImageSizeMode
            if (_imageScaleSelectedMenuItem.Tag is ImageSizeMode)
            {
                // set size mode
                imageViewer1.SizeMode = (ImageSizeMode)_imageScaleSelectedMenuItem.Tag;
                _imageScaleSelectedMenuItem.IsChecked = true;
            }
            // if menu item sets zoom
            else
            {
                // get zoom value
                int zoomValue = (int)_imageScaleSelectedMenuItem.Tag;
                // set ImageSizeMode as Zoom
                imageViewer1.SizeMode = ImageSizeMode.Zoom;
                // set zoom value
                imageViewer1.Zoom = zoomValue;
                _imageScaleSelectedMenuItem = scaleMenuItem;
                _imageScaleSelectedMenuItem.IsChecked = true;
            }
        }

        /// <summary>
        /// Enables/disables centering of image in image viewer.
        /// </summary>
        private void centerImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (centerImageMenuItem.IsChecked)
            {
                imageViewer1.FocusPointAnchor = AnchorType.None;
                imageViewer1.IsFocusPointFixed = true;
                imageViewer1.ScrollToCenter();
            }
            else
            {
                imageViewer1.FocusPointAnchor = AnchorType.Left | AnchorType.Top;
                imageViewer1.IsFocusPointFixed = true;
            }
        }

        /// <summary>
        /// Rotates images in both annotation viewer and thumbnail viewer by 90 degrees clockwise.
        /// </summary>
        private void rotateClockwiseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RotateViewClockwise();
        }

        /// <summary>
        /// Rotates images in both annotation viewer and thumbnail viewer by 90 degrees counterclockwise.
        /// </summary>
        private void rotateCounterclockwiseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RotateViewCounterClockwise();
        }

        /// <summary>
        /// Shows the image viewer settings.
        /// </summary>
        private void imageViewerSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImageViewerSettingsWindow viewerSettingsDialog = new ImageViewerSettingsWindow(imageViewer1);
            viewerSettingsDialog.Owner = this;
            viewerSettingsDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // save current image viewer display mode
            ImageViewerDisplayMode displayMode = imageViewer1.DisplayMode;

            viewerSettingsDialog.ShowDialog();

            // if display mode is changed
            if (displayMode != imageViewer1.DisplayMode)
                // update the default display mode
                _defaultImageViewerDisplayMode = imageViewer1.DisplayMode;

            UpdateUI();
        }

        /// <summary>
        /// Changes rendering settings of image viewer.
        /// </summary>
        private void viewerRenderingSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CompositeRenderingSettingsWindow viewerRenderingSettingsDialog = new CompositeRenderingSettingsWindow(imageViewer1.ImageRenderingSettings);
            viewerRenderingSettingsDialog.Owner = this;
            viewerRenderingSettingsDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            viewerRenderingSettingsDialog.ShowDialog();
            UpdateUI();
        }

        /// <summary>
        /// Shows the image magnifier settings.
        /// </summary>
        private void magnifierSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MagnifierToolAction magnifierToolAction = visualToolsToolBar.FindAction<MagnifierToolAction>();

            if (magnifierToolAction != null)
                magnifierToolAction.ShowVisualToolSettings();
        }

        /// <summary>
        /// Changes the rendering settings.
        /// </summary>
        private void renderingSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfRenderingSettingsWindow renderingSettingsDialog =
                new PdfRenderingSettingsWindow((PdfRenderingSettings)imageViewer1.ImageRenderingSettings.Clone());
            if (renderingSettingsDialog.ShowDialog().Value)
            {
                imageViewer1.ImageRenderingSettings = renderingSettingsDialog.RenderingSettings;
            }
        }

        #endregion


        /// <summary>
        /// Enables/disables the spell cheking of PDF interactive form fields.
        /// </summary>
        private void enableFormFieldsSpellCheckingMenuItem_CheckedChanged(object sender, RoutedEventArgs e)
        {
            InteractionAreaAppearanceManager.IsEnabledFormFieldSpellChecking = enableFormFieldsSpellCheckingMenuItem.IsChecked;
        }

        /// <summary>
        /// Enables/disables the spell cheking of PDF annotations.
        /// </summary>
        private void enableAnnotationsSpellCheckingMenuItem_CheckedChanged(object sender, RoutedEventArgs e)
        {
            InteractionAreaAppearanceManager.IsEnabledAnnotationsSpellChecking = enableAnnotationsSpellCheckingMenuItem.IsChecked;
        }


        /// <summary>
        /// Edits the color management settings.
        /// </summary>
        private void colorManagementMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ColorManagementSettingsWindow.EditColorManagement(imageViewer1);
        }


        #region Custom PDF content renderer

        /// <summary>
        /// Enables/disables the custom PDF content renderer.
        /// </summary>
        private void useCustomRendererMenuItem_CheckedChanged(object sender, RoutedEventArgs e)
        {
            PdfRenderingSettings settings;
            if (imageViewer1.ImageRenderingSettings is PdfRenderingSettings)
            {
                settings = (PdfRenderingSettings)imageViewer1.ImageRenderingSettings.Clone();
            }
            else
            {
                settings = new PdfRenderingSettings();
                if (imageViewer1.ImageRenderingSettings != null)
                {
                    settings.InterpolationMode = imageViewer1.ImageRenderingSettings.InterpolationMode;
                    settings.SmoothingMode = imageViewer1.ImageRenderingSettings.SmoothingMode;
                    settings.Resolution = imageViewer1.ImageRenderingSettings.Resolution;
                }
            }
            if (useCustomRendererMenuItem.IsChecked)
            {
                settings.ContentRenderer = _pdfCustomContentRenderer;
                imageViewer1.ImageRenderingSettings = settings;
            }
            else
            {
                settings.ContentRenderer = null;
                imageViewer1.ImageRenderingSettings = settings;
            }
            UpdateUI();
            ReloadImagesAsync();
        }

        /// <summary>
        /// Sets the settings of custom renderer.
        /// </summary>
        private void customRendererSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CustomContentRendererSettingsWindow window = new CustomContentRendererSettingsWindow(_pdfCustomContentRenderer);
            window.Owner = this;
            if (window.ShowDialog().Value)
                ReloadImagesAsync();
        }

        #endregion


        /// <summary>
        /// Starts a process of refreshing (actualization) of PostScript names of fonts
        /// in font programs controller.
        /// </summary>
        private void refreshPostScriptFontNamesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (FontProgramsTools.RefreshFontNamesOfProgramsController(this) == true)
            {
                if (imageViewer1.Images.Count > 0)
                    DemosTools.ShowInfoMessage("Reopen the document for using new font names.");
            }
        }

        #endregion


        #region 'Document' menu

        #region Information about PDF document

        /// <summary>
        /// Shows information about PDF document.
        /// </summary>
        private void documentInformationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_document != null)
            {
                PdfDemosTools.ShowDocumentInformation(this, _document, true, documentInfoPropertyGrid_PropertyValueChanged);
            }
        }

        /// <summary>
        /// Shows information about PDF document developer extensions.
        /// </summary>
        private void extensionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string message;
            if (_document.Extensions == null || _document.Extensions.Count == 0)
            {
                message = "Document does not have developer extensions.";
            }
            else
            {
                StringBuilder extensions = new StringBuilder();
                foreach (string name in _document.Extensions.Keys)
                    extensions.AppendLine(string.Format("{0}: {1}", name, _document.Extensions[name]));
                message = string.Format("Document has {0} developer extension(s):\n\n{1}", _document.Extensions.Count, extensions);
            }
            MessageBox.Show(message, "Developer extensions", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Updates UI when item in PDF document information is changed.
        /// </summary>
        private void documentInfoPropertyGrid_PropertyValueChanged(object sender, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            // update the UI
            UpdateUI();
        }

        #endregion


        #region PDF/A document verification

        /// <summary>
        /// Verifies the PDF document for compatibility with PDF/A-1b format.
        /// </summary>
        private void pdfA1bVerifierMenuItem_Click(object sender, RoutedEventArgs e)
        {
            VerifyPdfDocumentForCompatibilityWithPdfA(new PdfA1bVerifier());
        }

        /// <summary>
        /// Verifies the PDF document for compatibility with PDF/A-2b format.
        /// </summary>
        private void pdfA2bVerifierMenuItem_Click(object sender, RoutedEventArgs e)
        {
            VerifyPdfDocumentForCompatibilityWithPdfA(new PdfA2bVerifier());
        }

        /// <summary>
        /// Verifies the PDF document for compatibility with PDF/A-3b format.
        /// </summary>
        private void pdfA3bVerifierMenuItem_Click(object sender, RoutedEventArgs e)
        {
            VerifyPdfDocumentForCompatibilityWithPdfA(new PdfA3bVerifier());
        }

        /// <summary>
        /// Verifies the PDF document for compatibility with PDF/A-1a format. 
        /// </summary>
        private void pdfA1aVerifierMenuItem_Click(object sender, RoutedEventArgs e)
        {
            VerifyPdfDocumentForCompatibilityWithPdfA(new PdfA1aVerifier());
        }

        /// <summary>
        /// Verifies the PDF document for compatibility with PDF/A-2a format. 
        /// </summary>
        private void pdfA2aVerifierMenuItem_Click(object sender, RoutedEventArgs e)
        {
            VerifyPdfDocumentForCompatibilityWithPdfA(new PdfA2aVerifier());
        }

        /// <summary>
        /// Verifies the PDF document for compatibility with PDF/A-2u format. 
        /// </summary>
        private void pdfA2uVerifierMenuItem_Click(object sender, RoutedEventArgs e)
        {
            VerifyPdfDocumentForCompatibilityWithPdfA(new PdfA2uVerifier());
        }

        /// <summary>
        /// Verifies the PDF document for compatibility with PDF/A-3a format. 
        /// </summary>
        private void pdfA3aVerifierMenuItem_Click(object sender, RoutedEventArgs e)
        {
            VerifyPdfDocumentForCompatibilityWithPdfA(new PdfA3aVerifier());
        }

        /// <summary>
        /// Verifies the PDF document for compatibility with PDF/A-3u format. 
        /// </summary>
        private void pdfA3uVerifierMenuItem_Click(object sender, RoutedEventArgs e)
        {
            VerifyPdfDocumentForCompatibilityWithPdfA(new PdfA3uVerifier());
        }

        /// <summary>
        /// Verifies the PDF document for compatibility with PDF/A-4 format. 
        /// </summary>
        private void pdfA4VerifierMenuItem_Click(object sender, RoutedEventArgs e)
        {
            VerifyPdfDocumentForCompatibilityWithPdfA(new PdfA4Verifier());
        }

        /// <summary>
        /// Verifies the PDF document for compatibility with PDF/A-4f format. 
        /// </summary>
        private void pdfA4fVerifierMenuItem_Click(object sender, RoutedEventArgs e)
        {
            VerifyPdfDocumentForCompatibilityWithPdfA(new PdfA4fVerifier());
        }

        /// <summary>
        /// Verifies the PDF document for compatibility with PDF/A-4e format. 
        /// </summary>
        private void pdfA4eVerifierMenuItem_Click(object sender, RoutedEventArgs e)
        {
            VerifyPdfDocumentForCompatibilityWithPdfA(new PdfA4eVerifier());
        }

        #endregion


        #region PDF document conversion

        /// <summary>
        /// Converts the PDF document to the PDF/A-1b format.
        /// </summary>
        private void pdfA1bConverterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConvertPdfDocumentToPdfA(new PdfA1bConverter());
        }

        /// <summary>
        /// Converts the PDF document to the PDF/A-2b format.
        /// </summary>
        private void pdfA2bConverterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConvertPdfDocumentToPdfA(new PdfA2bConverter());
        }

        /// <summary>
        /// Converts the PDF document to the PDF/A-3b format.
        /// </summary>
        private void pdfA3bConverterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConvertPdfDocumentToPdfA(new PdfA3bConverter());
        }

        /// <summary>
        /// Converts the PDF document to the PDF/A-1a format.
        /// </summary>
        private void pdfA1aConverterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConvertPdfDocumentToPdfA(new PdfA1aConverter());
        }

        /// <summary>
        /// Converts the PDF document to the PDF/A-2a format.
        /// </summary>
        private void pdfA2aConverterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConvertPdfDocumentToPdfA(new PdfA2aConverter());
        }

        /// <summary>
        /// Converts the PDF document to the PDF/A-2u format.
        /// </summary>
        private void pdfA2uConverterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConvertPdfDocumentToPdfA(new PdfA2uConverter());
        }

        /// <summary>
        /// Converts the PDF document to the PDF/A-3a format.
        /// </summary>
        private void pdfA3aConverterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConvertPdfDocumentToPdfA(new PdfA3aConverter());
        }

        /// <summary>
        /// Converts the PDF document to the PDF/A-3u format.
        /// </summary>
        private void pdfA3uConverterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConvertPdfDocumentToPdfA(new PdfA3uConverter());
        }

        /// <summary>
        /// Converts the PDF document to the PDF/A-4 format.
        /// </summary>
        private void pdfA4ConverterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConvertPdfDocumentToPdfA(new PdfA4Converter());
        }

        /// <summary>
        /// Converts the PDF document to the PDF/A-4f format.
        /// </summary>
        private void pdfA4fConverterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConvertPdfDocumentToPdfA(new PdfA4fConverter());
        }

        /// <summary>
        /// Converts the PDF document to the PDF/A-4e format.
        /// </summary>
        private void pdfA4eConverterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConvertPdfDocumentToPdfA(new PdfA4eConverter());
        }

        /// <summary>
        /// Converts the PDF document to image-only PDF document.
        /// </summary>
        private void convertToImageonlyDocumentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _annotationTool.CancelBuilding();

            ProcessingCommandWindow<ImageCollection>.ExecuteProcessing(
                imageViewer1.Images,
                new PdfConvertToImageOnlyCommand(),
                true);
        }

        #endregion


        #region PDF document processing

        /// <summary>
        /// Determines that a content operator uses overprint control.
        /// </summary>
        private void overprintControlIsUsedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                ProcessingHelper.CreateTriggerActivationPredicate(
                    "Overprint control is used",
                    TriggerSeverity.Important,
                    new PdfDocumentProcessingTree(PdfTriggers.ContentStreamOperatorUsesOverprintControl)
                ),
                false);
        }

        /// <summary>
        /// Determines whether document has signature fields.
        /// </summary>
        private void DocumentHasSignatureFieldsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
               _document,
               PdfAnalyzers.DocumentHasSignatureFields,
               true);
        }

        /// <summary>
        /// Returns information about PDF document conformance
        /// from the metadata identification schema.
        /// </summary>
        private void detectTheConformanceUsingTheMetadataIdentificationSchemaMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                PdfAnalyzers.DocumentConformance,
                false);
        }

        /// <summary>
        /// Determines that PDF file XREF table is corrupt.
        /// </summary>
        private void theCrossreferenceTableIsCorruptMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                PdfAnalyzers.CrossReferenceTableIsCorrupt,
                false);
        }

        /// <summary>
        /// Returns count of indirect objects in PDF document.
        /// </summary>
        private void numberOfIndirectObjectsInAPDFFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                PdfAnalyzers.IndirectObjectCount,
                false);
        }

        /// <summary>
        /// Determines that the cross-reference information
        /// is stored in a cross-reference stream.
        /// </summary>
        private void theCrossreferenceInformationIsStoredInACrossreferenceStreamMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                PdfAnalyzers.PdfFileUsesCrossReferenceStreams,
                false);
        }

        /// <summary>
        /// Determines that PDF file uses the compressed object streams.
        /// </summary>
        private void documentUsesTheCompressedObjectStreamsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                PdfAnalyzers.PdfFileUsesCompressedObjectStreams,
                false);
        }

        /// <summary>
        /// Determines that PDF file occurs at offset 0 of the stream.
        /// </summary>
        private void pDFFileHeaderOccursAtByteOffset0OfTheFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                PdfAnalyzers.PdfFileHeaderStartsAtOffset0,
                false);
        }

        /// <summary>
        /// Determines that PDF file has linearization information.
        /// </summary>
        private void pDFFileHasTheLinearizationInfoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                PdfAnalyzers.PdfFileHasLinearizationInfo,
                false);
        }

        /// <summary>
        /// Removes unused resources from PDF Document.
        /// </summary>
        private void removeUnusedResourcesToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                 new PdfDocumentRemoveUnusedNamedResourcesCommand(),
                 true);
        }

        /// <summary>
        /// Decompress all data streams of document.
        /// </summary>
        private void decompressStreamsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfStreamCompressionConverter converter = new PdfStreamCompressionConverter(PdfCompression.None);
            converter.ProcessImageStreams = false;
            PdfDocumentConverter decompressor = new PdfDocumentConverter("Data resources decompressor",
                new PdfDocumentProcessingTree(converter));
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                 decompressor,
                 true);
        }

        /// <summary>
        /// Compress all data streams of document.
        /// </summary>
        private void compressDataStreamsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfStreamCompressionConverter converter = new PdfStreamCompressionConverter(PdfCompression.Zip);
            converter.CompressionSettings.ZipCompressionLevel = 9;
            converter.ProcessImageStreams = false;
            PdfDocumentConverter compressor = new PdfDocumentConverter("Data resources compressor",
                new PdfDocumentProcessingTree(converter));
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                 compressor,
                 true);
        }

        /// <summary>
        /// Converts TrueType fonts to the CFF fonts.
        /// </summary>
        private void convertTrueTypeFontsToCffFontsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfFontProcessingCommandExecutor fontsProcessing = new PdfFontProcessingCommandExecutor("Document fonts processing",
                new TrueTypeToCffFontProgramConverter());
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                 fontsProcessing,
                 true);
        }

        /// <summary>
        /// Removes transparency from document.
        /// </summary>
        private void removeTransparencyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProcessingCommand<PdfDocument> command = ProcessingHelper.CreateCompositeProcessing<PdfDocument>("Remove transparency from document",
                new PdfAnnotationProcessingCommandExecutor(PdfFixups.AnnotationUsesTransparencyFixup),
                new PdfPageProcessingCommandExecutor(new RemoveTransparencyFromContentStreamCommand().ConvertTarget<PdfPage>())
            );
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                command,
                true);
            DemosTools.ReloadImagesInViewer(imageViewer1);
        }

        /// <summary>
        /// Removes duplicate resources from document.
        /// </summary>
        private void removeDuplicateResourcesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            IProcessingCommand<PdfDocument> command = new PdfTreeRemoveDuplicateNodesCommand();
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                command,
                true);
        }

        /// <summary>
        /// Removes signature fields from PDF document.
        /// </summary>
        private void RemoveSignatureFieldsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                  _document,
                  new PdfDocumentRemoveSignatureFieldsCommand(),
                  true);
            DemosTools.ReloadImagesInViewer(imageViewer1);
        }

        /// <summary>
        /// Compresses PDF document image-resources.
        /// </summary>
        private void compressImageResourcesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                new PdfCompressImageResourcesCommand(),
                true);
        }

        /// <summary>
        /// Optimizes color depth of PDF documents images.
        /// </summary>
        private void optimizeImagesColorDepthMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                new PdfOptimizeImageColorDepthCommand(),
                true);
        }

        /// <summary>
        /// Optimizes PDF document content images.
        /// </summary>
        private void optimizeContentImagesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                new PdfOptimizeContentImageCommand(),
                true);
        }

        /// <summary>
        /// Removes elements from PDF document.
        /// </summary>
        private void discardDocumentElementsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                new PdfDocumentRemoveElementsCommand(),
                true);
        }

        /// <summary>
        /// Compresses PDF document.
        /// </summary>
        private void compressDocumentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfDocumentCompressorCommand command = new PdfDocumentCompressorCommand();
            command.Started += PdfDocumentCompressorCommand_Started;
            command.Finished += PdfDocumentCompressorCommand_Finished;

            // Images

            command.DetectBlackWhiteImageResources = true;
            command.DetectBitonalImageResources = true;
            command.DetectGrayscaleImageResources = true;
            command.DetectIndexed8ImageResources = false;
            command.ColorDepthDetectionMaxInaccuracy = 5;

            command.ColorImagesCompression = PdfCompression.Jpeg;
            command.ColorImagesCompressionResolution = new Resolution(150, 150);
            command.ColorImagesCompressionMinResolution = new Resolution(188, 188);

            command.GrayscaleImagesCompression = PdfCompression.Jpeg;
            command.GrayscaleImagesCompressionResolution = new Resolution(150, 150);
            command.GrayscaleImagesCompressionMinResolution = new Resolution(188, 188);

            command.BitonalImagesCompression = PdfCompression.Jbig2;
            command.BitonalImagesCompressionResolution = new Resolution(300, 300);
            command.BitonalImagesCompressionMinResolution = new Resolution(375, 375);

            command.IndexedImagesCompression = PdfCompression.Undefined;

            command.DownscaleInterpolationMode = ImageInterpolationMode.HighQualityBicubic;


            // Resources

            command.UseFlateInsteadLzwCompression = true;
            command.UseFlateInsteadNoneCompression = true;
            command.RecompressFlateCompression = false;
            command.FlateCompressionLevel = 9;


            // Clean Up

            command.SubsetFonts = true;
            command.RemoveInvalidBookmarks = true;
            command.RemoveInvalidLinks = true;
            command.RemoveUnusedPages = true;
            command.RemoveUnusedNames = true;
            command.RemoveUnusedNamedResources = true;
            command.RemoveDuplicateResources = true;


            // Discard

            command.RemoveAnnotations = false;
            command.RemoveBookmarks = false;
            command.RemoveEmbeddedThumbnails = false;
            command.RemoveDocumentInformation = false;
            command.RemoveEmbeddedFiles = false;
            command.RemoveMetadata = false;


            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(
                _document,
                command,
                true,
                true);
        }

        #endregion


        #region PDF document annotations

        /// <summary>
        /// Handles the Click event of importFromXFDFMenuItem object.
        /// </summary>
        private void importFromXFDFMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // create "Open file" dialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XFDF Files|*.xfdf";

            // show dialog
            if (openFileDialog.ShowDialog().Value)
            {
                try
                {
                    // open selected file
                    using (FileStream stream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        // create PDF annotation XFDF codec
                        PdfAnnotationXfdfCodec xfdfCodec = new PdfAnnotationXfdfCodec();
                        // import annotations from file to PDF document
                        xfdfCodec.Import(_document, stream);
                    }

                    // reload images in image viewer
                    DemosTools.ReloadImagesInViewer(imageViewer1);
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of exportToXFDFMenuItem object.
        /// </summary>
        private void exportToXFDFMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // create "Save file" dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XFDF Files|*.xfdf";

            // show dialog
            if (saveFileDialog.ShowDialog().Value)
            {
                try
                {
                    // create new file
                    using (FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite))
                    {
                        // create PDF annotation XFDF codec
                        PdfAnnotationXfdfCodec xfdfCodec = new PdfAnnotationXfdfCodec();
                        // export annotations from PDF document
                        xfdfCodec.Export(_document, stream);
                    }
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of removeAllAnnotationsMenuItem object.
        /// </summary>
        private void removeAllAnnotationsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProcessingDemosTools.ExecuteProcessing<ImageCollection>(
                imageViewer1.Images, new PdfPageRemoveAnnotationsCommand(PdfPageRemoveAnnotationsCommand.AllAnnotationTypes));
        }

        /// <summary>
        /// Handles the Click event of removeMarkupAnnotationsMenuItem object.
        /// </summary>
        private void removeMarkupAnnotationsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProcessingDemosTools.ExecuteProcessing<ImageCollection>(
                imageViewer1.Images, new PdfPageRemoveAnnotationsCommand(PdfPageRemoveAnnotationsCommand.MarkupAnnotationTypes));
        }

        #endregion


        #region PDF document security

        /// <summary>
        /// Shows the security properties of PDF document.
        /// </summary>
        private void securityPropertiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SecurityPropertiesWindow securityProperties = new SecurityPropertiesWindow(_document);
            securityProperties.Owner = this;
            securityProperties.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            securityProperties.ShowDialog();
        }

        /// <summary>
        /// Changes the security settings of PDF document.
        /// </summary>
        private void securitySettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SecuritySettingsWindow securitySettings = new SecuritySettingsWindow(_document.EncryptionSystem);
            securitySettings.Owner = this;
            if (securitySettings.ShowDialog().Value)
            {
                if (securitySettings.NewEncryptionSettings != _document.EncryptionSystem)
                {
                    if (_isDocumentChanged)
                        SaveChangesInPdfDocument(PdfDocumentUpdateMode.Incremental);
                    if (imageViewer1.Images.Count > 0)
                    {
                        EncryptionSystem newEncryptionSettings = securitySettings.NewEncryptionSettings;
                        PdfFormat format = _document.Format;
                        if (newEncryptionSettings != null)
                            // RC4
                            if (newEncryptionSettings.Algorithm == EncryptionAlgorithm.RC4)
                            {
                                // 40 bits
                                if (newEncryptionSettings.KeyLength == 40)
                                {
                                    // PDF 1.1 or greater version required                                   
                                    if (format.VersionNumber == 10)
                                        format = new PdfFormat("1.1", false, format.BinaryFormat);
                                }
                                // greater than 40 bits
                                else
                                {
                                    // PDF 1.4 or greater version required                                   
                                    if (format.VersionNumber < 14)
                                        format = new PdfFormat("1.4", false, format.BinaryFormat);
                                }
                            }
                            // AES
                            else if (newEncryptionSettings.Algorithm == EncryptionAlgorithm.AES)
                            {
                                // PDF 1.6 or greater version required                               
                                if (format.VersionNumber < 16)
                                    format = new PdfFormat("1.6", format.CompressedCrossReferenceTable, format.BinaryFormat);
                            }
                        SaveAndPackPdfDocument(format, newEncryptionSettings);
                    }
                }
            }
        }

        #endregion


        #region PDF document signing and signatures

        /// <summary>
        /// Shows digital signatures of PDF document.
        /// </summary>
        private void digitalSignaturesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_document.InteractiveForm == null)
                {
                    DemosTools.ShowErrorMessage("Document does not contain digital signatures.");
                    return;
                }

                DocumentSignaturesWindow form = new DocumentSignaturesWindow(_document);
                form.Owner = this;
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Creates signature field, adds signature field to PDF document and
        /// signs PDF document using new signature field.
        /// </summary>
        private void signDocumentUsingNewSignatureFieldMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // check pages of PDF document
            if (!PdfDemosTools.CheckAllPagesFromDocument(imageViewer1.Images, _document))
                return;


            // if PDF document version is less than 1.3
            if (_document.Format.VersionNumber < 13)
            {
                DemosTools.ShowErrorMessage("PDF document version 1.3 or greater is required.");
            }
            // if PDF document version is equal or more than 1.3
            else
            {
                if (MessageBox.Show("Use mouse for drawing the area (left click and drag mouse cursor) where signature must appear.", "Sign Document", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    // create visual tool that will create the signature field
                    SignatureMakerTool signatureMaker = new SignatureMakerTool(imageViewer1.VisualTool, _signatureAppearance, false, this);
                    // add handler to the SignatureAdded event
                    signatureMaker.SignatureAdded += new EventHandler(signatureMaker_SignatureAdded);
                    // activate visual tool in image viewer
                    imageViewer1.VisualTool = signatureMaker;
                }
            }
        }

        /// <summary>
        /// Signs PDF document using the existing empty signature field.
        /// </summary>
        private void signDocumentUsingExisingEmptySignatureFieldMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // check pages of PDF document
            if (!PdfDemosTools.CheckAllPagesFromDocument(imageViewer1.Images, _document))
                return;


            // if document does NOT have interactive form
            if (_document.InteractiveForm == null)
                return;

            // list with signature fields which do NOT have values
            List<PdfInteractiveFormSignatureField> emptySignatureFields = new List<PdfInteractiveFormSignatureField>();
            // get all signature fields of PDF document
            PdfInteractiveFormSignatureField[] signatureFields = _document.InteractiveForm.GetSignatureFields();
            // for each signature field
            foreach (PdfInteractiveFormSignatureField field in signatureFields)
            {
                // if signature field does NOT have value
                if (field.SignatureInfo == null)
                    // add signature field to the list
                    emptySignatureFields.Add(field);
            }

            // if document does NOT have empty signature fields
            if (emptySignatureFields.Count == 0)
            {
                DemosTools.ShowInfoMessage("Document does not have empty signature fields.");
            }
            // if interactive form have empty signature fields
            else
            {
                // select signtaure field from the list of fields
                PdfInteractiveFormSignatureField field = SelectSignatureField(emptySignatureFields);
                // if signature field is selected
                if (field != null)
                {
                    // if new signature field is created
                    if (CreateSignatureFieldWithAppearance.ShowDialog(field, _signatureAppearance))
                    {
                        SavePdfDocumentToSourceOrNewFile(true);
                    }
                }
            }
        }

        /// <summary>
        /// Adds an empty signature field to PDF document.
        /// </summary>
        private void addEmptySignatureFieldMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (FocusedPage == null)
            {
                ShowMessage_CurrentImageIsNotPdfPage();
                return;
            }

            if (MessageBox.Show("Use mouse for drawing the area (left click and drag mouse cursor) where signature must appear.", "Sign Document", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
            {
                // create visual tool that will create the signature field
                SignatureMakerTool signatureMaker = new SignatureMakerTool(imageViewer1.VisualTool, _signatureAppearance, true, this);
                // add handler to the SignatureAdded event
                signatureMaker.SignatureAdded += new EventHandler(signatureMaker_EmptySignatureAdded);
                // activate visual tool in image viewer
                imageViewer1.VisualTool = signatureMaker;
            }
        }

        /// <summary>
        /// Clears signature value of signature field of PDF document.
        /// </summary>
        private void clearSignatureMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // if document does NOT have interactive form
            if (_document.InteractiveForm == null)
                return;

            // list with signature fields which have values
            List<PdfInteractiveFormSignatureField> signedSignatureFields = new List<PdfInteractiveFormSignatureField>();
            // get all signature fields of PDF document
            PdfInteractiveFormSignatureField[] signatureFields = _document.InteractiveForm.GetSignatureFields();
            // for each signature field
            foreach (PdfInteractiveFormSignatureField field in signatureFields)
            {
                // if signature field has value
                if (field.SignatureInfo != null)
                    // add signature field to the list
                    signedSignatureFields.Add(field);
            }

            // if document does NOT have signed signature fields
            if (signedSignatureFields.Count == 0)
            {
                DemosTools.ShowInfoMessage("Document does NOT have signed signature fields.");
            }
            // if document has signed signature fields
            else
            {
                // select signtaure field from the list of fields
                PdfInteractiveFormSignatureField field = SelectSignatureField(signedSignatureFields);
                // if signature field is selected
                if (field != null)
                {
                    // remove signature value
                    field.SignatureInfo = null;
                    // remove signature appearance
                    field.Annotation.Appearances = null;
                    // reload image associated with field annotation page
                    ReloadImage(field.Annotation.Page);
                }
            }
        }

        /// <summary>
        /// Clears signature values of all signature fields of PDF document.
        /// </summary>
        private void clearAllSignaturesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // if document does NOT have interactive form
            if (_document.InteractiveForm == null)
                return;

            // array with fignature fields of PDF document
            PdfInteractiveFormSignatureField[] signatureFields = null;
            // get signature fields of PDF document
            signatureFields = _document.InteractiveForm.GetSignatureFields();

            // if document has signature fields
            if (signatureFields != null && signatureFields.Length > 0)
            {
                // if signature fields must be cleared
                if (MessageBox.Show("Do you want to clear all signatures of PDF document?", "Clear all signatrues", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // for each signature field
                    foreach (PdfInteractiveFormSignatureField field in signatureFields)
                    {
                        // clear the signature field
                        field.SignatureInfo = null;
                        // remove signature appearance
                        field.Annotation.Appearances = null;
                        // reload image associated with field annotation page
                        ReloadImage(field.Annotation.Page);
                    }
                }
            }
            // if document does NOT have signature fields
            else
            {
                DemosTools.ShowInfoMessage("Document does not have signature fields.");
            }
        }

        /// <summary>
        /// Removes signature field of PDF document.
        /// </summary>
        private void removeSignatureMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // if document does NOT have interactive form
            if (_document.InteractiveForm == null)
                return;

            // list with signature fields
            List<PdfInteractiveFormSignatureField> signatureFields = new List<PdfInteractiveFormSignatureField>();
            // add all signature fields of PDF document to the list
            signatureFields.AddRange(_document.InteractiveForm.GetSignatureFields());

            // if document does NOT have signature fields
            if (signatureFields.Count == 0)
            {
                DemosTools.ShowInfoMessage("Document does not have signature fields.");
            }
            // if document has signature fields
            else
            {
                // select signtaure field from the list of fields
                PdfInteractiveFormSignatureField field = SelectSignatureField(signatureFields);
                // if signature field is selected
                if (field != null)
                {
                    PdfPage page = field.Annotation.Page;
                    // delete signature field: remove field from interactive form and 
                    // remove widget annotation from page
                    _document.InteractiveForm.RemoveField(field);
                    // reload image associated with field annotation page
                    ReloadImage(page);
                }
            }
        }

        /// <summary>
        /// Removes all signature fields from PDF document.
        /// </summary>
        private void removeAllSignaturesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // if document does NOT have interactive form
            if (_document.InteractiveForm == null)
                return;

            // array with fignature fields of PDF document
            PdfInteractiveFormSignatureField[] signatureFields = null;
            // get signature fields of PDF document
            signatureFields = _document.InteractiveForm.GetSignatureFields();

            // if document does NOT have signature fields
            if (signatureFields == null || signatureFields.Length == 0)
            {
                DemosTools.ShowInfoMessage("PDF document does NOT have digital signatures.");
            }
            // if document has signature fields
            else
            {
                // if signatures must be removed
                if (MessageBox.Show("Do you want to remove all signatures from PDF document?", "Remove all signatures", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // for each signature field
                    foreach (PdfInteractiveFormSignatureField field in signatureFields)
                    {
                        PdfPage page = field.Annotation.Page;
                        // delete signatue field: remove fields from interactive form and 
                        // remove widget annotations from pages
                        _document.InteractiveForm.RemoveField(field);
                        // reload image associated with field annotation page
                        ReloadImage(page);
                    }

                    DemosTools.ShowInfoMessage(string.Format("{0} signatures are removed.", signatureFields.Length));
                    UpdateUI();
                }
            }
        }

        #endregion


        #region PDF bookmarks

        /// <summary>
        /// Adds bookmark to PDF document.
        /// </summary>
        private void bookmarks_AddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (documentBookmarks.Document == null)
                documentBookmarks.Document = _document;
            if (imageViewer1.FocusedIndex >= 0)
                documentBookmarks.AddBookmark(imageViewer1.FocusedIndex);
        }

        /// <summary>
        /// Edits bookmark of PDF document.
        /// </summary>
        private void bookmarks_EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (documentBookmarks.Document == null)
                documentBookmarks.Document = _document;
            documentBookmarks.EditSelectedBookmark();
        }

        /// <summary>
        /// Removes bookmark of PDF document.
        /// </summary>
        private void bookmarks_DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (documentBookmarks.Document == null)
                documentBookmarks.Document = _document;
            documentBookmarks.DeleteSelectedBookmark();
        }

        #endregion


        #region Thumbnails in PDF document

        /// <summary>
        /// Removes thumbnails of all pages of PDF document.
        /// </summary>
        private void thumbnails_RemoveAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            StartAction("Remove thumbnails", false);
            _document.Pages.RemoveThumbnails();
            EndAction();
        }

        /// <summary>
        /// Generates thumbnails for all pages of PDF document.
        /// </summary>
        private void thumbnails_GenerateAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            StartAction("Generate thumbnails", true);
            _document.Progress += pdfDocument_ThumbnailGenerationProgress;
            _document.Pages.CreateThumbnails(128, 128);
            _document.Progress -= pdfDocument_ThumbnailGenerationProgress;
            EndAction();
        }

        /// <summary>
        /// Generates thumbnails for all pages of PDF document.
        /// </summary>
        private void thumbnails_ViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            List<PdfImageResource> thumbs = new List<PdfImageResource>();
            List<string> names = new List<string>();
            for (int i = 0; i < _document.Pages.Count; i++)
            {
                if (_document.Pages[i].Thumbnail != null)
                {
                    thumbs.Add(_document.Pages[i].Thumbnail);
                    names.Add("Page " + (i + 1).ToString());
                }
            }
            if (thumbs.Count > 0)
            {
                PdfResourcesViewerWindow dlg = new PdfResourcesViewerWindow(_document, thumbs.ToArray());
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dlg.Owner = this;
                dlg.ShowDialog();
            }
            else
            {
                MessageBox.Show("This PDF document does not contain thumbnails.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion


        #region Common parameters of PDF document

        /// <summary>
        /// PDF document view mode is changed.
        /// </summary>
        private void documentViewModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _document.DocumentViewMode = (PdfDocumentViewMode)Enum.Parse(typeof(PdfDocumentViewMode), documentViewModeComboBox.SelectedItem.ToString());
        }

        /// <summary>
        /// PDF page layout is changed.
        /// </summary>
        private void viewerPageLayoutComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _document.ViewerPageLayout = (PdfDocumentPageLayoutMode)Enum.Parse(
                typeof(PdfDocumentPageLayoutMode), viewerPageLayoutComboBox.SelectedItem.ToString());

            // set the display mode of image viewer
            if (_document.ViewerPageLayout == PdfDocumentPageLayoutMode.Undefined)
                imageViewer1.DisplayMode = _defaultImageViewerDisplayMode;
            else
                imageViewer1.DisplayMode = PdfDemosTools.ConvertPageLayoutModeToImageViewerDisplayMode(_document.ViewerPageLayout);
        }

        /// <summary>
        /// Shows a form that allows to view and edit viewer preferences of PDF document.
        /// </summary>
        private void viewerPreferencesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_document.ViewerPreferences == null)
            {
                if (MessageBox.Show("Document does not have Viewer Preferences. Do you want to create Viewer Preferences properties?", "Create Viewer Preferences?",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _document.ViewerPreferences = new PdfDocumentViewerPreferences(_document);
                }
                else
                {
                    return;
                }
            }
            PropertyGridWindow viewerPreferencesForm = new PropertyGridWindow(_document.ViewerPreferences, "Viewer Preferences");
            viewerPreferencesForm.ShowDialog();
        }

        /// <summary>
        /// Shows a form that allows to view and edit open actions of PDF document.
        /// </summary>
        private void documentActionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfTriggersEditorWindow window = new PdfTriggersEditorWindow(_document);
            window.Owner = this;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            window.ShowDialog();
        }

        /// <summary>
        /// Shows a form that allows to edit the JavaScript of PDF document.
        /// </summary>
        private void documentLevelJavaStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_document.JavaScripts == null)
                _document.JavaScripts = new PdfJavaScriptActionDictionary(_document);

            PdfJavaScriptActionDictionaryEditorDialog dialog =
                new PdfJavaScriptActionDictionaryEditorDialog(_document.JavaScripts);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        #endregion


        #region Attachments & embedded files in PDF document

        /// <summary>
        /// Shows a form that allows to view and edit attachments (portfolio) of PDF document.
        /// </summary>
        private void attachmentsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_document.Attachments == null)
                if (MessageBox.Show("Document does not have attachments. Do you want to create attachments?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            AttachmentsEditorWindow window = new AttachmentsEditorWindow(_document);
            window.Owner = this;
            window.ShowDialog();
            UpdateUI();
        }

        /// <summary>
        /// Removes all attachments (portfolio) of PDF document.
        /// </summary>
        private void removeAttachmentsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to remove all attachments?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _document.RemoveAttachments(true);
                UpdateUI();
            }
        }

        /// <summary>
        /// Shows a form that allows to view and edit embedded files of PDF document.
        /// </summary>
        private void embeddedFilesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            EmbeddedFilesWindow dlg = new EmbeddedFilesWindow();
            dlg.Owner = this;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Document = _document;
            dlg.ShowDialog();
        }

        #endregion


        #region Resources in PDF document

        /// <summary>
        /// Shows a form that allows to view resources of PDF document.
        /// </summary>
        private void resourcesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfResourcesViewerWindow window = new PdfResourcesViewerWindow(_document);
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = this;
            window.ShowDialog();

            if (window.PropertyValueChanged)
                imageViewer1.ReloadImage();
        }

        /// <summary>
        /// Shows a form that allows to view image-resources of PDF document.
        /// </summary>
        private void documentImageResourcesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            StartAction("Collect resources", false);
            PdfImageResource[] images = _document.GetImages();
            EndAction();
            if (images.Length > 0)
            {
                PdfResourcesViewerWindow resourcesViewer = new PdfResourcesViewerWindow(_document);
                resourcesViewer.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                resourcesViewer.Owner = this;
                resourcesViewer.ShowFormResources = false;
                resourcesViewer.ShowDialog();
                if (resourcesViewer.PropertyValueChanged)
                    imageViewer1.ReloadImage();
            }
            else
            {
                MessageBox.Show("This PDF document does not contain image resources.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion


        #region Fonts in PDF document

        /// <summary>
        /// Shows a form that allows to view information about fonts of PDF document.
        /// </summary>
        private void fontsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // get all PDF documents loaded in image viewer
            List<PdfDocument> documents = GetPdfDocumentsAssociatedWithImageCollection(imageViewer1.Images);
            List<PdfFont> fonts = new List<PdfFont>();
            foreach (PdfDocument document in documents)
                fonts.AddRange(document.GetFonts());

            if (fonts.Count == 0)
            {
                MessageBox.Show("This document does not contain fonts.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                ViewDocumentFontsWindow viewDocumentFontsDialog = new ViewDocumentFontsWindow(fonts);
                viewDocumentFontsDialog.Owner = this;
                viewDocumentFontsDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                viewDocumentFontsDialog.ShowDialog();
            }
        }

        /// <summary>
        /// Embeds all fonts into PDF document.
        /// </summary>
        private void embedAllFontsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string message = "Do you want to embed all external fonts of the document?";
            if (MessageBox.Show(message, "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                try
                {
                    StartAction("Embed fonts", false);
                    // embed all external fonts for all PDF documents loaded in image viewer
                    foreach (PdfDocument document in GetPdfDocumentsAssociatedWithImageCollection(imageViewer1.Images))
                        document.FontManager.EmbedAllFonts();
                    EndAction();
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                    EndFailedAction();
                }
            }
        }

        /// <summary>
        /// Removes all unused characters from fonts of PDF document.
        /// </summary>
        private void subsetAllFontsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string message = "Do you want to subset all fonts (remove all unused symbols from the fonts) of the document?";
            if (MessageBox.Show(message, "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                try
                {
                    StartAction("Subset fonts", false);
                    ActionProgressWindow progressForm = new ActionProgressWindow(PackAllFonts, 2, "Subset all embedded fonts");
                    if (progressForm.RunAndShowDialog(this).Value)
                        EndAction();
                    else
                        EndCanceledAction();
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                    EndFailedAction();
                }
            }
        }

        #endregion


        #region Layers (optional content) of PDF document

        /// <summary>
        /// Shows a form that allows to view setting of layers (optional content) of PDF document.
        /// </summary>
        private void optionalContentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OptionalContentSettingsWindow window = new OptionalContentSettingsWindow(_document, imageViewer1);
            window.Owner = this;
            window.ShowDialog();
            imageViewer1.Image.Reload(false);
        }

        /// <summary>
        /// Shows form that allows to remove layers (optional conetent) of PDF document.
        /// </summary>
        private void removeLayersOptionalContentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RemoveOptionalContentWindow window = new RemoveOptionalContentWindow(_document);
            window.Owner = this;
            window.ShowDialog();

            if (window.DialogResult == true)
            {
                RemoveMarkedContentCommand command = new RemoveMarkedContentCommand();
                command.RemovingMarkedContent = window.SelectedOptionalGroups.ToArray();

                DocumentProcessingCommandWindow.ExecuteDocumentProcessing(_document, command, false);

                imageViewer1.Image.Reload(false);
            }
        }

        #endregion

        #endregion


        #region 'Page' menu

        #region Processing

        /// <summary>
        /// Changes scale of PDF page.
        /// </summary>
        private void changeScaleOfPDFPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExecuteProcessingCommandOnFocusedImage(new PdfPageScalingCommand(), true, true);
            annotationToolControl.AnnotationTool.FocusedAnnotationView = null;
        }

        /// <summary>
        /// Resizes canvas of PDF page.
        /// </summary>
        private void resizeCanvasOfPDFPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExecuteProcessingCommandOnFocusedImage(new PdfPageResizeCanvasCommand(), true, true);
            annotationToolControl.AnnotationTool.FocusedAnnotationView = null;
        }

        /// <summary>
        /// Rotates PDF page by the orthogonal angle.
        /// </summary>
        private void rotatePDFPageBy90DegressMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExecuteProcessingCommandOnFocusedImage(new PdfPageRotateOrthogonallyCommand(90), true, false);
            annotationToolControl.AnnotationTool.FocusedAnnotationView = null;
        }

        /// <summary>
        /// Clears the content of PDF page.
        /// </summary>
        private void clearPDFPageContentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExecuteProcessingCommandOnFocusedImage(new PdfPageClearContentCommand(), true, true);
        }

        /// <summary>
        /// Burns the PDF annotations on PDF page.
        /// </summary>
        private void burnThePDFAnnotationsOnPDFPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExecuteProcessingCommandOnFocusedImage(new PdfPageBurnAnnotationsCommand(), true, false);
        }

        /// <summary>
        /// Removes the PDF annotations on PDF page.
        /// </summary>
        private void removePdfAnnotationsFromPdfPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExecuteProcessingCommandOnFocusedImage(new PdfPageRemoveAnnotationsCommand(), true, false);
        }

        /// <summary>
        /// Blends colors on PDF page.
        /// </summary>
        private void colorBlendingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExecuteProcessingCommandOnFocusedImage(new PdfPageColorBlendingCommand(), true, false);
        }

        /// <summary>
        /// Inverts colors in PDF page.
        /// </summary>
        private void invertPDFPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfInvertCommand invertCommand = new PdfInvertCommand();
            ExecuteProcessingCommandOnFocusedImage(invertCommand, false, false);
        }

        /// <summary>
        /// Converts PDF document or PDF page to an image-only PDF document.
        /// </summary>
        private void convertToImageonlyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExecuteProcessingCommandOnFocusedImage(new PdfConvertToImageOnlyCommand(), true, true);
        }

        /// <summary>
        /// Simplify content of PDF page.
        /// </summary>
        private void simplifyPageContentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExecuteProcessingCommandOnFocusedImage(new PdfSimplifyContentCommand(), true, true);
        }

        /// <summary>
        /// Removes unused resources from PDF page.
        /// </summary>
        private void removeUnusedResourcesFromPDFPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExecuteProcessingCommandOnFocusedImage(new PdfPageRemoveUnusedNamedResourcesCommand(), true, false);
        }

        #endregion


        /// <summary>
        /// Shows properties of PDF page.
        /// </summary>
        private void pagePropertiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (FocusedPage == null)
            {
                ShowMessage_CurrentImageIsNotPdfPage();
                return;
            }
            PropertyGridWindow propertyGridWindow = new PropertyGridWindow(FocusedPage, "Page properties");
            propertyGridWindow.Owner = this;
            propertyGridWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            propertyGridWindow.PropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(propertyGrid_PropertyValueChanged);
            propertyGridWindow.ShowDialog();
            propertyGridWindow.PropertyGrid.PropertyValueChanged -= propertyGrid_PropertyValueChanged;
        }

        /// <summary>
        /// Shows a form that allows to view and edit actions of PDF page.
        /// </summary>
        private void pageActionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfTriggersEditorWindow window = new PdfTriggersEditorWindow(FocusedPage);
            window.Owner = this;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            window.ShowDialog();
        }

        /// <summary>
        /// Shows a form that allows to view resources of PDF page.
        /// </summary>
        private void pageResourcesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfResourcesViewerWindow resourcesViewer = new PdfResourcesViewerWindow(FocusedPage);
            resourcesViewer.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            resourcesViewer.Owner = this;
            resourcesViewer.ShowDialog();
            if (resourcesViewer.PropertyValueChanged)
                imageViewer1.ReloadImage();
        }

        /// <summary>
        /// Shows a form that allows to view image-resources of PDF page.
        /// </summary>
        private void pageImageResourcesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageViewer1.FocusedIndex >= 0)
            {
                if (FocusedPage != null)
                {
                    IList<PdfImageResource> images = FocusedPage.GetImages();
                    if (images.Count == 0)
                    {
                        MessageBox.Show("This page does not contain image resources.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        PdfResourcesViewerWindow resourcesViewer = new PdfResourcesViewerWindow(FocusedPage);
                        resourcesViewer.ShowFormResources = false;
                        resourcesViewer.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        resourcesViewer.Owner = this;
                        resourcesViewer.ShowDialog();
                        if (resourcesViewer.PropertyValueChanged)
                            imageViewer1.ReloadImage();
                    }
                }
                else
                {
                    ShowMessage_CurrentImageIsNotPdfPage();
                }
            }
        }

        /// <summary>
        /// Renders PDF page and saves rendered image to a file.
        /// </summary>
        private void saveAsImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (imageViewer1.FocusedIndex >= 0)
            {
                if (_saveImageFileDialog.ShowDialog().Value)
                {
                    PdfRenderingSettings settings = (PdfRenderingSettings)CompositeRenderingSettings.GetRenderingSettings<PdfRenderingSettings>(
                        imageViewer1.ImageRenderingSettings).Clone();
                    CompositeRenderingSettingsWindow renderingSettingsDialog = new CompositeRenderingSettingsWindow(settings);
                    renderingSettingsDialog.Owner = this;
                    renderingSettingsDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    if (renderingSettingsDialog.ShowDialog().Value)
                    {
                        StartAction("Rendering and save", false);
                        RenderingSettings currentRenderingSettings = imageViewer1.Image.RenderingSettings;
                        imageViewer1.Image.RenderingSettings = settings;
                        try
                        {
                            imageViewer1.Image.Save(_saveImageFileDialog.FileName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Image saving error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        finally
                        {
                            imageViewer1.Image.RenderingSettings = currentRenderingSettings;
                        }
                        EndAction();
                    }
                }
            }
        }


        /// <summary>
        /// Updates UI when PDF page properties are changed.
        /// </summary>
        private void propertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            string propertyName = e.ChangedItem.PropertyDescriptor.Name;
            if (propertyName == "Rotate" || propertyName == "UserUnitFactor" || propertyName.EndsWith("Box"))
            {
                imageViewer1.Image.Reload(true);
                _textSelectionTool.ClearSelection();
            }
        }

        #endregion


        #region 'Text' menu

        /// <summary>
        /// "Text" menu is opened.
        /// </summary>
        private void textMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            UpdateTextMenuUI();
        }

        /// <summary>
        /// Handles the Click event of textMarkupHighlightToolStripMenuItem object.
        /// </summary>
        private void textMarkupHighlightToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfTextMarkupTools.HighlightSelectedText(imageViewer1);
        }

        /// <summary>
        /// Handles the Click event of textMarkupStrikeOutToolStripMenuItem object.
        /// </summary>
        private void textMarkupStrikeOutToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfTextMarkupTools.StrikeoutSelectedText(imageViewer1);
        }

        /// <summary>
        /// Handles the Click event of textMarkupUnderlineToolStripMenuItem object.
        /// </summary>
        private void textMarkupUnderlineToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfTextMarkupTools.UnderlineSelectedText(imageViewer1);
        }

        /// <summary>
        /// Handles the Click event of textMarkupSquigglyUnderlineToolStripMenuItem object.
        /// </summary>
        private void textMarkupSquigglyUnderlineToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfTextMarkupTools.SquigglyUnderlineSelectedText(imageViewer1);
        }

        /// <summary>
        /// Shows a form that allows to find text in PDF document.
        /// </summary>
        private void findTextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            IsTextSearching = true;

            FindTextWindow findTextDialog = new FindTextWindow(_textSelectionTool);

            TabItem selectedTab = (TabItem)toolsTabControl.SelectedItem;
            toolsTabControl.SelectedItem = textExtractionTabItem;

            if (_textSelectionTool.HasSelectedText)
                findTextDialog.FindWhat = _textSelectionTool.SelectedText;

            findTextDialog.Owner = this;
            findTextDialog.ShowDialog();

            toolsTabControl.SelectedItem = selectedTab;

            IsTextSearching = false;
        }

        /// <summary>
        /// Highlights selected text.
        /// </summary>
        private void highlightSelectedTextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            using (PdfGraphics graphics = PdfGraphics.FromPage(FocusedPage))
            {
                System.Drawing.Color brushColor = System.Drawing.Color.FromArgb(255, 255, 0);
                PdfBrush brush = new PdfBrush(brushColor, GraphicsStateBlendMode.Multiply);
                TextRegion selectedRegion = _textSelectionTool.GetSelectedRegion(imageViewer1.Image);
                if (selectedRegion != null)
                {
                    TextRegionLine[] lines = selectedRegion.Lines;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        RegionF[] regions = lines[i].SelectionRegions;
                        for (int j = 0; j < regions.Length; j++)
                            graphics.FillPolygon(brush, regions[j].ToPolygon());
                    }
                }
            }
            imageViewer1.Image.Reload(true);
        }

        /// <summary>
        /// Removes selected text.
        /// </summary>
        private void removeSelectedTextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_textSelectionTool.HasSelectedText)
            {
                VintasoftImage[] imagesWithSelection = _textSelectionTool.GetSelectionImages();
                foreach (VintasoftImage image in imagesWithSelection)
                {
                    TextRegion selectedText = _textSelectionTool.GetSelectedRegion(image);
                    if (!selectedText.IsEmpty)
                    {
                        PdfPage page = PdfDocumentController.GetPageAssociatedWithImage(image);
                        page.RemoveText(selectedText);
                        image.Reload(true);
                    }
                }
                _textSelectionTool.ClearSelection();
                _textSelectionTool.FocusedTextSymbol = null;
            }
            else
            {
                MessageBox.Show("No selected text.");
            }
        }

        /// <summary>
        /// Creates redaction mark from selected text.
        /// </summary>
        private void addSelectedTextToRedactionMarksMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_textSelectionTool.HasSelectedText)
            {
                VintasoftImage[] imagesWithSelection = _textSelectionTool.GetSelectionImages();
                foreach (VintasoftImage image in imagesWithSelection)
                {
                    TextRegion selectedText = _textSelectionTool.GetSelectedRegion(image);
                    if (!selectedText.IsEmpty)
                    {
                        _removeContentTool.Add(image, new WpfTextRedactionMark(image, selectedText));
                    }
                }
                _textSelectionTool.ClearSelection();
                toolsTabControl.SelectedItem = removeContentTabItem;
                CurrentTool = _redactionTool;
            }
            else
            {
                MessageBox.Show("No selected text.");
            }
        }

        /// <summary>
        /// Obfuscates the text encoding of the current PDF page.
        /// </summary>
        private void currentPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string message = "Do you want to obfuscate the encoding of all text of the page?";
            if (MessageBox.Show(message, "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                ActionProgressWindow progressWindow =
                    new ActionProgressWindow(ObfuscateTextEncodingOfCurrentPage, 2, "Obfuscate text encoding of current page");
                if (progressWindow.RunAndShowDialog(this).Value)
                    ShowPackDialogAfterTextObfuscation();
            }
        }

        /// <summary>
        /// Obfuscates the text encoding of the selected PDF pages.
        /// </summary>
        private void selectedPagesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            VintasoftImage[] selectedImages = new VintasoftImage[thumbnailViewer1.SelectedThumbnails.Count];
            for (int i = 0; i < selectedImages.Length; i++)
                selectedImages[i] = thumbnailViewer1.SelectedThumbnails[i].Source;
            if (selectedImages.Length == 0)
            {
                MessageBox.Show("No selected pages.");
                return;
            }

            string message = "Do you want to obfuscate the encoding of all text of the selected pages?";
            if (MessageBox.Show(message, "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                PdfPage[] pages = new PdfPage[selectedImages.Length];
                for (int i = 0; i < selectedImages.Length; i++)
                    pages[i] = PdfDocumentController.GetPageAssociatedWithImage(selectedImages[i]);
                _pagesForObfuscation = pages;

                ActionProgressWindow progressWindow =
                    new ActionProgressWindow(ObfuscateTextEncodingOfSelectedPages, 2, "Obfuscate text encoding of selected pages");
                if (progressWindow.RunAndShowDialog(this).Value)
                    ShowPackDialogAfterTextObfuscation();

                _pagesForObfuscation = null;
            }
        }

        /// <summary>
        /// Obfuscates the text encoding of the PDF document.
        /// </summary>
        private void documentMenuItem1_Click(object sender, RoutedEventArgs e)
        {
            string message = "Do you want to obfuscate the encoding of all text of the document?";
            if (MessageBox.Show(message, "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                ActionProgressWindow progressWindow =
                    new ActionProgressWindow(ObfuscateTextEncodingOfDocument, 2, "Obfuscate text encoding of document");

                if (progressWindow.RunAndShowDialog(this).Value)
                    ShowPackDialogAfterTextObfuscation();
            }
        }

        /// <summary>
        /// Shows the form with properties of the text encoding obfuscator.
        /// </summary>
        private void settingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PropertyGridWindow form = new PropertyGridWindow(_textEncodingObfuscator, "Text encoding obfuscator settings", true);
            form.ShowDialog();
        }

        #endregion


        #region 'Forms' menu

        /// <summary>
        /// "Forms" menu is opened.
        /// </summary>
        private void formsMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            bool isPdfFileOpening = IsPdfFileOpening;
            bool isPdfFileLoaded = _document != null;
            bool isPdfFileEmpty = true;
            bool isPdfFileSaving = IsPdfFileSaving;
            bool hasInteractiveForm = isPdfFileLoaded && _document.InteractiveForm != null;

            if (isPdfFileLoaded)
                isPdfFileEmpty = imageViewer1.Images.Count <= 0;

            // "Forms" menu
            importDataMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileOpening && !isPdfFileEmpty &&
                                               !isPdfFileSaving;
            exportDataMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileOpening && !isPdfFileEmpty &&
                                               !isPdfFileSaving && hasInteractiveForm;
            resetFormMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileOpening && !isPdfFileEmpty &&
                                               !isPdfFileSaving && hasInteractiveForm;
            calculationOrderMenuItem.IsEnabled = isPdfFileLoaded && !isPdfFileOpening && !isPdfFileEmpty &&
                                               !isPdfFileSaving && hasInteractiveForm;
        }

        /// <summary>
        /// Imports data of PDF interactive form.
        /// </summary>
        private void importDataMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_document.InteractiveForm == null)
            {
                MessageBox.Show("Data cannot be imported because PDF document does not have interactive form.",
                    "Import Form Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XFDF Files|*.xfdf";

            if (openFileDialog.ShowDialog().Value)
            {
                try
                {
                    using (FileStream stream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        PdfInteractiveFormDataXfdfCodec xfdfCodec = new PdfInteractiveFormDataXfdfCodec();
                        xfdfCodec.FieldValueChanged += new EventHandler<PropertyChangedEventArgs<object>>(xfdfCodec_FieldValueChanged);
                        xfdfCodec.Import(_document.InteractiveForm, stream);
                    }

                    // reload images in image viewer
                    DemosTools.ReloadImagesInViewer(imageViewer1);
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }
        }

        /// <summary>
        /// Value of PDF interactive field is changed.
        /// </summary>
        private void xfdfCodec_FieldValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            PdfInteractiveFormField formField = sender as PdfInteractiveFormField;
            if (formField != null && formField.AppearanceDependsFromValue)
                formField.UpdateAppearance();
        }

        /// <summary>
        /// Exports data of PDF interactive form.
        /// </summary>
        private void exportDataMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XFDF Files|*.xfdf";

            if (saveFileDialog.ShowDialog().Value)
            {
                try
                {
                    using (FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite))
                    {
                        PdfInteractiveFormDataXfdfCodec xfdfCodec = new PdfInteractiveFormDataXfdfCodec();
                        xfdfCodec.Export(_document.InteractiveForm, stream);
                    }
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }
        }

        /// <summary>
        /// Resets interactive form of PDF document.
        /// </summary>
        private void resetFormMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // reset field values
                _document.InteractiveForm.ResetFieldValues(true);
                // reload images in image viewer
                DemosTools.ReloadImagesInViewer(imageViewer1);
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Shows a dialog that allows to change the calculation order of fields in interactive form.
        /// </summary>
        private void calculationOrderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PdfInteractiveFormFieldCalculationOrderEditorWindow dialog =
                    new PdfInteractiveFormFieldCalculationOrderEditorWindow();
                dialog.Owner = this;
                dialog.InteractiveForm = _document.InteractiveForm;
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        #endregion


        #region 'Help' menu

        /// <summary>
        /// Shows the "About" dialog.
        /// </summary>
        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder description = new StringBuilder();
            description.AppendLine("This project demonstrates the following SDK capabilities:");
            description.AppendLine();
            description.AppendLine("- Create and load PDF document.");
            description.AppendLine();
            description.AppendLine("- Display and print PDF document with or without color management.");
            description.AppendLine();
            description.AppendLine("- View and change general information about PDF document.");
            description.AppendLine();
            description.AppendLine("- View and change permission settings of PDF document.");
            description.AppendLine();
            description.AppendLine("- Change password of PDF document.");
            description.AppendLine();
            description.AppendLine("- Encrypt PDF document.");
            description.AppendLine();
            description.AppendLine("- Verify PDF document to conformance with PDF/A-1a, PDF/A-1b, PDF/A-2a, PDF/A-2b, PDF/A-3a or PDF/A-3b specification.");
            description.AppendLine();
            description.AppendLine("- Convert PDF document to PDF/A-1a, PDF/A-1b, PDF/A-2a, PDF/A-2b, PDF/A-3a or PDF/A-3b format.");
            description.AppendLine();
            description.AppendLine("- View, validate, add, remove digital signatures. Sign PDF document.");
            description.AppendLine();
            description.AppendLine("- View, add, remove, edit annotations.");
            description.AppendLine();
            description.AppendLine("- View, add, remove, edit, fill interactive form fields.");
            description.AppendLine();
            description.AppendLine("- Navigate PDF document.");
            description.AppendLine();
            description.AppendLine("- Execute JavaScript actions of PDF document.");
            description.AppendLine();
            description.AppendLine("- Add, reorder, remove pages of PDF document.");
            description.AppendLine();
            description.AppendLine("- Save PDF page to any supported image file format.");
            description.AppendLine();
            description.AppendLine("- Extract text and images from PDF page.");
            description.AppendLine();
            description.AppendLine("- Draw figures (text, image, rectangle, ellipse, lines, curves, polygons) on PDF page.");
            description.AppendLine();
            description.AppendLine("- Remove content of PDF document.");
            description.AppendLine();
            description.AppendLine("- Select text on PDF page.");
            description.AppendLine();
            description.AppendLine("- Obfuscate text in PDF document.");
            description.AppendLine();
            description.AppendLine("- Add, edit, reorder, remove bookmarks of PDF document.");
            description.AppendLine();
            description.AppendLine("- View and edit annotations of PDF document.");
            description.AppendLine();
            description.AppendLine("- View and edit the interactive fields of PDF document.");
            description.AppendLine();
            description.AppendLine("- Fill, export and import the interactive form of PDF document.");
            description.AppendLine();
            description.AppendLine("- View, generate and remove embedded thumbnails of PDF pages.");
            description.AppendLine();
            description.AppendLine("- View, add and remove embedded files of PDF document.");
            description.AppendLine();
            description.AppendLine("- View and compress image resources of PDF document.");
            description.AppendLine();
            description.AppendLine("- View information about fonts of PDF document.");
            description.AppendLine();
            description.AppendLine("- Save embedded files or image resources to a file.");
            description.AppendLine();
            description.AppendLine("- Pack PDF document.");
            description.AppendLine();
            description.AppendLine("- Save changes in PDF document.");
            description.AppendLine();
            description.AppendLine("- Convert PDF document to a TIFF file.");
            description.AppendLine();
            description.AppendLine();
            description.AppendLine("The project is available in C# and VB.NET for Visual Studio .NET.");

            WpfAboutBoxBaseWindow dlg = new WpfAboutBoxBaseWindow("vspdf-dotnet");
            dlg.Description = description.ToString();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        #endregion


        #region Toolbar

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        private void viewerToolBar_OpenFile(object sender, EventArgs e)
        {
            openMenuItem_Click(sender, null);
        }

        /// <summary>
        /// Save changes in PDF document.
        /// </summary>
        private void viewerToolBar_SaveFile(object sender, EventArgs e)
        {
            saveMenuItem_Click(sender, null);
        }

        /// <summary>
        /// Prints page(s) of PDF document.
        /// </summary>
        private void viewerToolBar_Print(object sender, EventArgs e)
        {
            printMenuItem_Click(sender, null);
        }

        #endregion


        #region Left side-panel

        /// <summary>
        /// Tools tab is selected.
        /// </summary>
        private void toolsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource != toolsTabControl)
                return;

            if (e.AddedItems != null && e.AddedItems.Count == 1 &&
                e.RemovedItems != null && e.RemovedItems.Count == 1)
            {
                if (toolsTabControl.SelectedItem == bookmarksTabItem && documentBookmarks.Document == null)
                    documentBookmarks.Document = _document;

                InteractionAreaAppearanceManager.VisualTool = _annotationTool;

                if (toolsTabControl.SelectedItem == annotationToolTabItem)
                {
                    if (_annotationTool.ImageViewer == null)
                    {
                        imageViewer1.VisualTool = new WpfCompositeVisualTool(
#if !REMOVE_ANNOTATION_PLUGIN
                        _commentTool,
#endif
#if !REMOVE_OFFICE_PLUGIN
                        new Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction.WpfOfficeDocumentVisualEditorTextTool(),
#endif
                        _annotationTool);
                    }
                    annotationToolControl.UpdateUI();
                }
                else if (toolsTabControl.SelectedItem == textExtractionTabItem)
                {
                    _textSelectionTool.Enabled = true;
                    imageViewer1.VisualTool = _textSelectionTool;
                }
                else if (toolsTabControl.SelectedItem == removeContentTabItem)
                {
                    InteractionAreaAppearanceManager.VisualTool = _removeContentTool;
                    imageViewer1.VisualTool = _redactionTool;
                }
                else if (toolsTabControl.SelectedItem == contentEditorTabItem)
                {
                    InteractionAreaAppearanceManager.VisualTool = _contentEditorTool;
                    imageViewer1.VisualTool = _contentEditorToolComposition;
                }
                else if (toolsTabControl.SelectedItem == commentsTabItem)
                {
                    imageViewer1.VisualTool = _defaultAnnotationTool;
                }

                if (imageViewer1.Image != null &&
                    PdfDocumentController.GetPageAssociatedWithImage(imageViewer1.Image) == null)
                {
                    string status = string.Empty;
                    if (toolsTabControl.SelectedItem != pagesTabItem &&
                        toolsTabControl.SelectedItem != bookmarksTabItem)
                        status = "Current image is not a PDF page. Save document and try again.";
                    SetStatusLabelTextSafe(status);
                }

                UpdateUI();
            }
        }

        #endregion

        #endregion


        #region 'Edit' menu

        /// <summary>
        /// Updates the "Edit" menu items.
        /// </summary>
        private void UpdateEditMenuItems()
        {
            if (thumbnailViewer1.IsFocused)
            {
                UpdateEditMenuItem(cutMenuItem, null, "Cut");
                UpdateEditMenuItem(copyMenuItem, null, "Copy");
                UpdateEditMenuItem(pasteMenuItem, null, "Paste");
                deleteMenuItem.IsEnabled = true;
                deleteMenuItem.Header = "Delete Page(s)";
                selectAllMenuItem.IsEnabled = true;
                selectAllMenuItem.Header = "Select All Pages";
                deselectAllMenuItem.IsEnabled = false;
                deselectAllMenuItem.Header = "Deselect All";
            }
            else
            {
                UpdateEditMenuItem(cutMenuItem, PdfDemosTools.GetUIAction<CutItemUIAction>(CurrentTool), "Cut");
                UpdateEditMenuItem(copyMenuItem, PdfDemosTools.GetUIAction<CopyItemUIAction>(CurrentTool), "Copy");
                UpdateEditMenuItem(pasteMenuItem, PdfDemosTools.GetUIAction<PasteItemUIAction>(CurrentTool), "Paste");
                UpdateEditMenuItem(deleteMenuItem, PdfDemosTools.GetUIAction<DeleteItemUIAction>(CurrentTool), "Delete");
                UpdateEditMenuItem(selectAllMenuItem, PdfDemosTools.GetUIAction<SelectAllItemsUIAction>(CurrentTool), "Select All");
                UpdateEditMenuItem(deselectAllMenuItem, PdfDemosTools.GetUIAction<DeselectAllItemsUIAction>(CurrentTool), "Deselect All");
            }
        }

        /// <summary>
        /// Enables the "Edit" menu items.
        /// </summary>
        private void EnableEditMenuItems()
        {
            cutMenuItem.IsEnabled = true;
            copyMenuItem.IsEnabled = true;
            pasteMenuItem.IsEnabled = true;
            deleteMenuItem.IsEnabled = true;
            selectAllMenuItem.IsEnabled = true;
            deselectAllMenuItem.IsEnabled = true;
        }

        /// <summary>
        /// Updates the "Edit" menu item.
        /// </summary>
        /// <param name="menuItem">The menu item.</param>
        /// <param name="action">The action.</param>
        /// <param name="defaultText">The default text of the menu item.</param>
        private void UpdateEditMenuItem(MenuItem menuItem, UIAction action, string defaultText)
        {
            if (action != null && action.IsEnabled)
            {
                menuItem.IsEnabled = true;
                menuItem.Header = action.Name;
            }
            else
            {
                menuItem.IsEnabled = false;
                menuItem.Header = defaultText;
            }
        }

        #endregion


        #region Image Viewer

        /// <summary>
        /// Image is loading in image viewer.
        /// </summary>
        private void imageViewer1_ImageLoading(object sender, ImageLoadingEventArgs e)
        {
            StartAction("Rendering", false);
        }

        /// <summary>
        /// Image is loaded in image viewer.
        /// </summary>
        private void imageViewer1_ImageLoaded(object sender, ImageLoadedEventArgs e)
        {
            EndAction();
            UpdateUI();

            // if font missing error is not shown
            if (!_isCJKFontMissingErrorMessageShown)
            {
                // if document is not empty
                if (_document != null)
                {
                    // get all runtime messages
                    PdfRuntimeMessage[] runtimeMessages = _document.GetAllRuntimeMessages();
                    // if document has runtime messages
                    if (runtimeMessages.Length != 0)
                    {
                        // for each message
                        foreach (PdfRuntimeMessage runtimeMessage in runtimeMessages)
                        {
                            // if message is an error
                            if (runtimeMessage is PdfRuntimeError)
                            {
                                // if message about missing CJK font
                                if (runtimeMessage.InnerException is CJKFontProgramNotFoundException)
                                {
                                    // show error message
                                    DemosTools.ShowErrorMessage(runtimeMessage.Message);
                                    // specify that error message is shown
                                    _isCJKFontMissingErrorMessageShown = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Image is not loaded in image viewer because of error.
        /// </summary>
        private void imageViewer1_ImageLoadingException(object sender, Vintasoft.Imaging.ExceptionEventArgs e)
        {
            EndAction();
            UpdateUI();
            MessageBox.Show(e.Exception.Message, "Image loading exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        /// <summary>
        /// Index of focused image in viewer is changing.
        /// </summary>
        private void imageViewer1_FocusedIndexChanging(object sender, PropertyChangedEventArgs<int> e)
        {
            AbortBuildFigure();
        }

        /// <summary>
        /// Index of focused image in viewer is changed.
        /// </summary>
        private void imageViewer1_FocusedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (imageViewer1.Images.Count > 0)
                UpdateUI();
        }


        /// <summary>
        /// Zoom of image viewer is changed.
        /// </summary>
        /// <remarks>
        /// Checks current ImageSizeMode
        /// and changes text in zoom combo box according to the current zoom.
        /// </remarks>
        private void imageViewer1_ZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            _imageScaleSelectedMenuItem.IsChecked = false;
            switch (imageViewer1.SizeMode)
            {
                case ImageSizeMode.BestFit:
                    _imageScaleSelectedMenuItem = bestFitMenuItem;
                    break;
                case ImageSizeMode.FitToHeight:
                    _imageScaleSelectedMenuItem = fitToHeightMenuItem;
                    break;
                case ImageSizeMode.FitToWidth:
                    _imageScaleSelectedMenuItem = fitToWidthMenuItem;
                    break;
                case ImageSizeMode.Normal:
                    _imageScaleSelectedMenuItem = normalImageMenuItem;
                    break;
                case ImageSizeMode.PixelToPixel:
                    _imageScaleSelectedMenuItem = pixelToPixelMenuItem;
                    break;
                case ImageSizeMode.Zoom:
                    _imageScaleSelectedMenuItem = scaleMenuItem;
                    break;
            }

            _imageScaleSelectedMenuItem.IsChecked = true;
        }

        #endregion


        #region Image collection

        /// <summary>
        /// Image collection of image viewer is changed.
        /// </summary>
        void Images_ImageCollectionChanged(object sender, ImageCollectionChangeEventArgs e)
        {
            IsDocumentChanged = true;
        }


        /// <summary>
        /// Image collection saving is in progress.
        /// </summary>
        void Images_ImageCollectionSavingProgress(object sender, ProgressEventArgs e)
        {
            ShowProgressInfo(e.Progress);
        }

        /// <summary>
        /// Performs when image collection is saved.
        /// </summary>
        void OnImageCollectionSaved(bool error)
        {
            if (_switchPdfFileStreamToNewStream)
            {
                _switchPdfFileStreamToNewStream = false;
                _pdfFileStream.Close();
                _pdfFileStream.Dispose();
                _pdfFileStream = _newFileStream;

                RemoveDocumentEventHandlers();
                PdfDocumentController.CloseDocument(_document);
                _document = PdfDocumentController.OpenDocument(_newFileStream);
                AddDocumentEventHandlers();
                _newFileStream = null;
            }
        }

        #endregion


        #region Thumbnail viewer

        /// <summary>
        /// Loading of thumbnails is in progress.
        /// </summary>
        private void thumbnailViewer1_ThumbnailLoadingProgress(object sender, ProgressEventArgs e)
        {
            bool progressVisible = e.Progress != 100;
            addThumbnailsProgressBar.Value = e.Progress;
            addThumbnailsProgressBar.Visibility = progressVisible ? Visibility.Visible : Visibility.Collapsed;
            addThumbnailsStatusLabel.Visibility = progressVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion


        #region Visual Tools

        #region Actions

        /// <summary>
        /// Handles the Deactivated event of TextMarkupToolAction object.
        /// </summary>
        private void TextMarkupToolAction_Deactivated(object sender, EventArgs e)
        {
            _textMarkupTool.Enabled = false;
            _defaultAnnotationTool.ActiveTool = null;
        }

        /// <summary>
        /// Handles the Activated event of HighlightTextToolAction object.
        /// </summary>
        private void HighlightTextToolAction_Activated(object sender, EventArgs e)
        {
            ActivateTextMarkupTool(WpfPdfTextMarkupToolMode.Highlight);
        }

        /// <summary>
        /// Handles the Activated event of UnderlineTextToolAction object.
        /// </summary>
        private void UnderlineTextToolAction_Activated(object sender, EventArgs e)
        {
            ActivateTextMarkupTool(WpfPdfTextMarkupToolMode.Underline);
        }

        /// <summary>
        /// Handles the Activated event of StrikeoutTextToolAction object.
        /// </summary>
        private void StrikeoutTextToolAction_Activated(object sender, EventArgs e)
        {
            ActivateTextMarkupTool(WpfPdfTextMarkupToolMode.Strikeout);
        }

        /// <summary>
        /// Handles the Activated event of SquigglyTextToolAction object.
        /// </summary>
        private void SquigglyTextToolAction_Activated(object sender, EventArgs e)
        {
            ActivateTextMarkupTool(WpfPdfTextMarkupToolMode.SquigglyUnderline);
        }

        /// <summary>
        /// Handles the Activated event of CaretTextToolAction object.
        /// </summary>
        private void CaretTextToolAction_Activated(object sender, EventArgs e)
        {
            ActivateTextMarkupTool(WpfPdfTextMarkupToolMode.Caret);
        }

        /// <summary>
        /// Activates the text markup tool.
        /// </summary>
        /// <param name="markupMode">The markup mode.</param>
        private void ActivateTextMarkupTool(WpfPdfTextMarkupToolMode markupMode)
        {
            _textSelectionTool.Enabled = true;
            bool restorePreviousVisualTool;
            if (markupMode == WpfPdfTextMarkupToolMode.Caret)
            {
                restorePreviousVisualTool = _textSelectionTool.FocusedTextSymbol != null;
            }
            else
            {
                restorePreviousVisualTool = _textSelectionTool.HasSelectedText;
            }
            _defaultAnnotationTool.ActiveTool = _textSelectionTool;
            _textMarkupTool.MarkupMode = markupMode;
            _textMarkupTool.Enabled = true;
            if (restorePreviousVisualTool)
                visualToolsToolBar.SetPreviousAction();
        }

        /// <summary>
        /// Handles the Executed event of the RemoveContentToolAction.
        /// </summary>
        private void RemoveContentToolAction_Executed(object sender, EventArgs e)
        {
            toolsTabControl.SelectedItem = removeContentTabItem;
        }

        /// <summary>
        /// Handles the Executed event of the ContentEditorToolAction.
        /// </summary>
        private void ContentEditorToolAction_Executed(object sender, EventArgs e)
        {
            toolsTabControl.SelectedItem = contentEditorTabItem;
        }

        /// <summary>
        /// Handles the Executed event of the TextSelectionAction.
        /// </summary>
        private void TextSelectionAction_Executed(object sender, EventArgs e)
        {
            toolsTabControl.SelectedItem = textExtractionTabItem;
        }

        /// <summary>
        /// Handles the Executed event of the AnnotationToolAction.
        /// </summary>
        private void AnnotationToolAction_Executed(object sender, EventArgs e)
        {
#if !REMOVE_PDFVISUALEDITOR_PLUGIN
            _annotationTool.InteractionMode = WpfPdfAnnotationInteractionMode.Edit;
#else
            _annotationTool.InteractionMode = WpfPdfAnnotationInteractionMode.View;
#endif
            toolsTabControl.SelectedItem = annotationToolTabItem;
        }


        /// <summary>
        /// Handles the Activated event of the annotationToolEditFormFieldsAction.
        /// </summary>
        private void annotationToolEditFormFieldsAction_Clicked(object sender, EventArgs e)
        {
            VisualToolAction action = (VisualToolAction)sender;

            if (!action.Parent.IsActivated)
                action.Parent.Activate();

            _annotationTool.InteractionMode = WpfPdfAnnotationInteractionMode.Edit;
            toolsTabControl.SelectedItem = annotationToolTabItem;
            annotationToolControl.ShowFormFieldsTab();
        }

        /// <summary>
        /// Handles the Activated event of the annotationToolEditAnnotationsAction.
        /// </summary>
        private void annotationToolEditAnnotationsAction_Clicked(object sender, EventArgs e)
        {
            VisualToolAction action = (VisualToolAction)sender;

            if (!action.Parent.IsActivated)
                action.Parent.Activate();

            _annotationTool.InteractionMode = WpfPdfAnnotationInteractionMode.Edit;
            toolsTabControl.SelectedItem = annotationToolTabItem;
            annotationToolControl.ShowAnnotationsTab();
        }

        /// <summary>
        /// Handles the Executed event of the TextSelectionAndFillFormsAction.
        /// </summary>
        private void TextSelectionAndFillFormsAction_Executed(object sender, EventArgs e)
        {
            if (_annotationTool.InteractionMode != WpfPdfAnnotationInteractionMode.View &&
                 _annotationTool.InteractionMode != WpfPdfAnnotationInteractionMode.Markup)
            {
                _annotationTool.InteractionMode = WpfPdfAnnotationInteractionMode.Markup;
                toolsTabControl.SelectedItem = pagesTabItem;
            }
            if (_annotationTool.InteractionMode != WpfPdfAnnotationInteractionMode.Edit)
                _textSelectionTool.Enabled = true;
        }

        #endregion


        #region Annotation Tool

        /// <summary>
        /// Handles the InteractionModeChanged event of the PDF AnnotationTool.
        /// </summary>
        private void AnnotationTool_InteractionModeChanged(object sender, PropertyChangedEventArgs<WpfPdfAnnotationInteractionMode> e)
        {
            if (imageViewer1.VisualTool == _defaultAnnotationTool)
            {
                if (e.NewValue == WpfPdfAnnotationInteractionMode.Edit)
                {
                    _textSelectionTool.ClearSelection();
                    _textSelectionTool.Enabled = false;
                }
                else
                {
                    _textSelectionTool.Enabled = true;
                }
            }

#if !REMOVE_ANNOTATION_PLUGIN
            if (commentsControl.IsCommentsOnViewerVisible)
                _commentTool.Enabled = e.NewValue != WpfPdfAnnotationInteractionMode.Edit;
#endif
        }


        /// <summary>
        /// Shows information about hovered annotation.
        /// </summary>
        private void AnnotationTool_HoveredAnnotationChanged(
            object sender,
            PdfAnnotationEventArgs e)
        {
            if (e.Annotation != null)
            {
                PdfAnnotation annotation = e.Annotation;
                if (_annotationTool.InteractionMode == WpfPdfAnnotationInteractionMode.View ||
                    _annotationTool.InteractionMode == WpfPdfAnnotationInteractionMode.Markup)
                    statusLabel.Content = PdfActionsTools.GetActivateActionDescription(annotation);
                else if (_annotationTool.InteractionMode == WpfPdfAnnotationInteractionMode.Edit)
                    statusLabel.Content = PdfDemosTools.GetAnnotationDescription(annotation);
            }
            else
                statusLabel.Content = "";
        }

        #endregion


        #region Text Selection Tool


        /// <summary>
        /// Text search is in progress.
        /// </summary>
        private void TextSelectionTool_TextSearchingProgress(
            object sender,
            TextSearchingProgressEventArgs e)
        {
            statusLabel.Content = string.Format("Search on page {0}...", e.ImageIndex + 1);
        }

        /// <summary>
        /// Text selection is changed.
        /// </summary>
        private void TextSelectionTool_SelectionChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        #endregion


        #region Text Markup Tool

        /// <summary>
        /// Handles the MarkupAnnotationCreated event of TextMarkupTool object.
        /// </summary>
        private void TextMarkupTool_MarkupAnnotationCreated(object sender, WpfPdfMarkupAnnotationEventArgs e)
        {
            e.MarkupAnnotation.Title = Environment.UserName;
            e.MarkupAnnotation.CreationDate = DateTime.Now;
            switch (e.MarkupAnnotation.AnnotationType)
            {
                case PdfAnnotationType.Caret:
                    e.MarkupAnnotation.Subject = "Insert Text";
                    break;
                case PdfAnnotationType.StrikeOut:
                    e.MarkupAnnotation.Subject = "Strikethrough Text";
                    break;
                case PdfAnnotationType.Highlight:
                    e.MarkupAnnotation.Subject = "Highlighted Text";
                    break;
                case PdfAnnotationType.Squiggly:
                    e.MarkupAnnotation.Subject = "Squiggly Underlined Text";
                    break;
                case PdfAnnotationType.Underline:
                    e.MarkupAnnotation.Subject = "Underlined text";
                    break;
                default:
                    e.MarkupAnnotation.Subject = e.MarkupAnnotation.AnnotationType.ToString();
                    break;
            }
        }

        /// <summary>
        /// Handles the MarkupAnnotationAdded event of TextMarkupTool object.
        /// </summary>
        private void TextMarkupTool_MarkupAnnotationAdded(object sender, WpfPdfMarkupAnnotationEventArgs e)
        {
#if !REMOVE_ANNOTATION_PLUGIN
            commentsTabItem.IsSelected = true;
            if (commentsControl.CommentTool == null || !commentsControl.CommentTool.Enabled)
                commentsControl.OpenComment(e.MarkupAnnotation, true);
#endif
        }

        #endregion


        #region Remove Content

        /// <summary>
        /// Redaction marks are applied to the current PDF document.
        /// </summary>
        void pdfRemoveContentControl_RedactionMarkApplied(object sender, EventArgs e)
        {
            if (pdfRemoveContentControl.ShowPackDialogAfterMarkApplied)
                ShowPackDialogAfterMarkIsApplied();
        }

        /// <summary>
        /// Shows the pack dialog after mark is applied.
        /// </summary>
        private void ShowPackDialogAfterMarkIsApplied()
        {
            StringBuilder messageString = new StringBuilder();
            messageString.AppendLine("Redacted content will be removed from PDF document only after packing of PDF document.");
            messageString.AppendLine("Do you want to pack PDF document right now?");
            messageString.AppendLine();
            messageString.AppendLine("Click 'Yes' if you want to pack PDF document right now.");
            messageString.AppendLine();
            messageString.AppendLine("Click 'No' if you will pack PDF document later (\"File -> Pack\" menu) and now you want to work with unpacked PDF document.");

            if (MessageBox.Show(messageString.ToString(), "Pack document?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                SaveAndPackPdfDocument();
        }

        #endregion


        #region PDF Editor Tool

        /// <summary>
        /// Handles the KeyUp event of Window object.
        /// </summary>
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            // if ESC pressed
            if (e.Key == Key.Escape)
            {
                AbortBuildFigure();
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        /// <summary>
        /// Aborts the building of PDF figure.
        /// </summary>
        private void AbortBuildFigure()
        {
#if !REMOVE_PDFVISUALEDITOR_PLUGIN
            _contentEditorTool.AbortBuildFigure();
#endif
        }

        #endregion


        #region Comment Tool

        /// <summary>
        /// Adds new comment to focused page.
        /// </summary>
        private void AddCommentButton_Click(object sender, RoutedEventArgs e)
        {
            if (FocusedPage != null)
            {
                CurrentTool = _defaultAnnotationTool;
                annotationToolControl.AnnotationsControl.AnnotationBuilderControl.AddAndBuildTextAnnotation("Comment");
            }
        }

        /// <summary>
        /// Close all comments on focused image.
        /// </summary>
        private void CloseAllCommentsButton_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_ANNOTATION_PLUGIN
            if (FocusedPage != null)
            {
                CommentCollection comments = _commentTool.CommentController.GetComments(imageViewer1.Image);
                if (comments != null)
                {
                    foreach (Comment comment in comments)
                        comment.IsOpen = false;
                }
            }
#endif
        }

        #endregion

        #endregion


        #region PDF document

        /// <summary>
        /// Creates new PDF document.
        /// </summary>
        private bool CreateNewPdfDocument(PdfFormat format, string saveDialogTitle)
        {
            ClosePdfDocument();

            SelectPdfFormatWindow selectPdfFormatDialog = new SelectPdfFormatWindow(format, null);
            selectPdfFormatDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            selectPdfFormatDialog.Owner = this;
            if (selectPdfFormatDialog.ShowDialog().Value &&
                _saveFileDialog.ShowDialog().Value)
            {
                _saveFileDialog.Title = saveDialogTitle;
                string filename = _saveFileDialog.FileName;
                try
                {
                    // try open in read-write mode
                    _pdfFileStream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
                }
                catch (IOException e)
                {
                    DemosTools.ShowErrorMessage(e);
                    return false;
                }
                _document = new PdfDocument(_pdfFileStream, selectPdfFormatDialog.Format, selectPdfFormatDialog.NewEncryptionSettings, false);

                AddDocumentEventHandlers();
                PdfDocumentController.RegisterDocument(_pdfFileStream, _document);

                documentViewModeComboBox.SelectedItem = _document.DocumentViewMode.ToString();
                viewerPageLayoutComboBox.SelectedItem = _document.ViewerPageLayout.ToString();

                Filename = filename;
                IsDocumentChanged = true;
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Opens an existing PDF document.
        /// </summary>
        private void OpenPdfDocument(string filename)
        {
            ClosePdfDocument();

            _enableUpdateUI = false;
            try
            {
                IsPdfFileReadOnlyMode = false;
                try
                {
                    // try open in read-write mode
                    _pdfFileStream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
                }
                catch (SystemException)
                {
                    IsPdfFileReadOnlyMode = true;
                    try
                    {
                        // try open in read mode
                        _pdfFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                    }
                    catch (SystemException e)
                    {
                        DemosTools.ShowErrorMessage(e);
                        return;
                    }
                }
                catch (Exception e)
                {
                    DemosTools.ShowErrorMessage(e);
                    return;
                }

                StartAction("Open document", false);
                try
                {
                    _document = PdfDocumentController.OpenDocument(_pdfFileStream);
                    if (_document.IsEncrypted && _document.AuthorizationResult == AuthorizationResult.IncorrectPassword)
                    {
                        ClosePdfDocument();
                        return;
                    }

                    if (_document.Pages.Count == 0)
                    {
                        DemosTools.ShowErrorMessage("PDF document does not contain pages.");
                        ClosePdfDocument();
                        return;
                    }
                }
                catch (Exception e)
                {
                    DemosTools.ShowErrorMessage(e);
                    ClosePdfDocument();
                    return;
                }
                finally
                {
                    EndAction();
                }

                Filename = filename;
                AddDocumentEventHandlers();
                if (imageViewer1.ImageRenderingSettings is PdfRenderingSettings)
                {
                    ((PdfRenderingSettings)imageViewer1.ImageRenderingSettings).UseEmbeddedThumbnails = useEmbeddedThumbnailsMenuItem.IsChecked;
                    _document.RenderingSettings = (PdfRenderingSettings)imageViewer1.ImageRenderingSettings.Clone();
                }
                _document.RenderingSettings.UseEmbeddedThumbnails = useEmbeddedThumbnailsMenuItem.IsChecked;

                documentViewModeComboBox.SelectedItem = _document.DocumentViewMode.ToString();
                viewerPageLayoutComboBox.SelectedItem = _document.ViewerPageLayout.ToString();

                // set the display mode of image viewer
                if (_document.ViewerPageLayout == PdfDocumentPageLayoutMode.Undefined)
                    imageViewer1.DisplayMode = _defaultImageViewerDisplayMode;
                else
                    imageViewer1.DisplayMode = PdfDemosTools.ConvertPageLayoutModeToImageViewerDisplayMode(_document.ViewerPageLayout);

                if (toolsTabControl.SelectedItem == bookmarksTabItem)
                    documentBookmarks.Document = _document;

                try
                {
                    _pdfFileStream.Position = 0;
                    imageViewer1.Images.Add(_pdfFileStream);
                }
                catch (Exception e)
                {
                    DemosTools.ShowErrorMessage(e);
                }

                imageViewer1.Images.ImageCollectionChanged += Images_ImageCollectionChanged;

                IsDocumentChanged = _document.IsChanged;
            }
            finally
            {
                _enableUpdateUI = true;
                UpdateUI();
            }
        }

        /// <summary>
        /// Closes PDF document.
        /// </summary>
        private void ClosePdfDocument()
        {
            _annotationTool.CancelBuilding();
            _annotationTool.Clipboard.Clear();

            if (_document != null)
            {
                RemoveDocumentEventHandlers();

                imageViewer1.Images.ImageCollectionChanged -= Images_ImageCollectionChanged;
                imageViewer1.Images.ClearAndDisposeItems();

                PdfDocumentController.CloseDocument(_document);
                _document = null;
                _pdfFileName = "";

                documentBookmarks.Document = null;

                statusLabel.Content = "";
                pageInfoLabel.Content = "";
            }
            if (_pdfFileStream != null)
            {
                _pdfFileStream.Close();
                _pdfFileStream.Dispose();
                _pdfFileStream = null;
            }
            IsDocumentChanged = false;
            Filename = null;

            _isCJKFontMissingErrorMessageShown = false;
        }


        /// <summary>
        /// Returns all PDF documents loaded in image collection.
        /// </summary>
        /// <param name="images">The collection of images.</param>
        private static List<PdfDocument> GetPdfDocumentsAssociatedWithImageCollection(ImageCollection images)
        {
            List<PdfDocument> documents = new List<PdfDocument>();
            foreach (VintasoftImage image in images)
            {
                PdfPage page = PdfDocumentController.GetPageAssociatedWithImage(image);
                PdfDocument document = page.Document;
                if (!documents.Contains(document))
                    documents.Add(document);
            }
            return documents;
        }


        #region PDF document events

        /// <summary>
        /// Adds the event handlers to current document.
        /// </summary>
        private void AddDocumentEventHandlers()
        {
            _document.DocumentChanged += Document_IsSavingRequiredChanged_Changed;
        }

        /// <summary>
        /// Removes the event handlers from current document.
        /// </summary>
        private void RemoveDocumentEventHandlers()
        {
            _document.DocumentChanged -= Document_IsSavingRequiredChanged_Changed;
        }

        /// <summary>
        /// The PdfDocument.IsSavingRequiredChanged property is changed.
        /// </summary>
        private void Document_IsSavingRequiredChanged_Changed(object sender, EventArgs e)
        {
            IsDocumentChanged = _document.IsChanged;
        }

        #endregion

        #endregion


        #region Verify PDF/A document

        /// <summary>
        /// Verifies PDF document for compatibility with PDF/A format.
        /// </summary>
        /// <param name="pdfAVerifier">PDF/A verifier.</param>
        private void VerifyPdfDocumentForCompatibilityWithPdfA(PdfAVerifier pdfAVerifier)
        {
            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(_document, pdfAVerifier);
        }

        #endregion


        #region Sign PDF document

        /// <summary>
        /// Signature field (signature field with value) is added to PDF document.
        /// </summary>
        private void signatureMaker_SignatureAdded(object sender, EventArgs e)
        {
            // update UI
            UpdateUI();
            // sign PDF document and save it to a file
            SavePdfDocumentToSourceOrNewFile(true);
            // reload current image
            imageViewer1.Image.Reload(true);
        }

        /// <summary>
        /// Empty signature field (signature field without value) is added to PDF document.
        /// </summary>
        private void signatureMaker_EmptySignatureAdded(object sender, EventArgs e)
        {
            // update UI
            UpdateUI();
            // reload current image
            imageViewer1.Image.Reload(true);
        }

        /// <summary>
        /// Selects the signature field from list of signature fields.
        /// </summary>
        /// <param name="signatureFields">The list of signature fields.</param>
        /// <returns>Selected signature field.</returns>
        private PdfInteractiveFormSignatureField SelectSignatureField(
            List<PdfInteractiveFormSignatureField> signatureFields)
        {
            // create dialog that allows to select signature field
            SelectSignatureFieldWindow selectSignatureField = new SelectSignatureFieldWindow(signatureFields.ToArray());
            selectSignatureField.Owner = this;
            // if signature field is selected
            if (selectSignatureField.ShowDialog().Value)
                // return selected field
                return selectSignatureField.SelectedField;

            return null;
        }

        /// <summary>
        /// Handles the Click event of addLtvMenuItem object.
        /// </summary>
        private void addLtvMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfDemosTools.AddLongTimeValidationInfo(_document);
        }

        #endregion


        #region Obfuscate text in PDF document

        /// <summary>
        /// Obfuscates the text encoding of current page.
        /// </summary>
        /// <param name="progressController">The progress controller.</param>
        private void ObfuscateTextEncodingOfCurrentPage(IActionProgressController progressController)
        {
            _textEncodingObfuscator.Obfuscate(progressController, this.FocusedPage);
        }

        /// <summary>
        /// Obfuscates the text encoding of selected pages.
        /// </summary>
        /// <param name="progressController">The progress controller.</param>
        private void ObfuscateTextEncodingOfSelectedPages(IActionProgressController progressController)
        {
            _textEncodingObfuscator.Obfuscate(progressController, _pagesForObfuscation);
        }

        /// <summary>
        /// Obfuscates the text encoding of document.
        /// </summary>
        /// <param name="progressController">The progress controller.</param>
        private void ObfuscateTextEncodingOfDocument(IActionProgressController progressController)
        {
            List<PdfDocument> loadedDocuments = GetPdfDocumentsAssociatedWithImageCollection(imageViewer1.Images);
            if (loadedDocuments.Count == 1)
            {
                _textEncodingObfuscator.Obfuscate(loadedDocuments[0], progressController);
            }
            else
            {
                progressController.Start("Obfuscating text of loaded documents", loadedDocuments.Count, this);
                foreach (PdfDocument document in loadedDocuments)
                {
                    progressController.Next(false);
                    _textEncodingObfuscator.Obfuscate(document);
                }
                progressController.Finish();
            }
        }

        /// <summary>
        /// Shows the dialog asking to pack PDF document after text obfuscation.
        /// </summary>
        private void ShowPackDialogAfterTextObfuscation()
        {
            StringBuilder messageString = new StringBuilder();
            messageString.AppendLine("Original fonts and content of pages will be removed from PDF document only after packing of PDF document.");
            messageString.AppendLine("Do you want to pack PDF document right now?");
            messageString.AppendLine();
            messageString.AppendLine("Click 'Yes' if you want to pack PDF document right now.");
            messageString.AppendLine();
            messageString.AppendLine("Click 'No' if you will pack PDF document later (\"File -> Pack\" menu) and now you want to work with unpacked PDF document.");

            if (MessageBox.Show(messageString.ToString(), "Pack document?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                SaveAndPackPdfDocument();
        }

        #endregion


        #region Compress PDF document

        /// <summary>
        /// PDF document compression is started.
        /// </summary>
        private void PdfDocumentCompressorCommand_Started(object sender, ProcessingEventArgs e)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.Invoke(new EventHandler<ProcessingEventArgs>(PdfDocumentCompressorCommand_Started), sender, e);
            }
            else
            {
                PdfDocument document = (PdfDocument)e.Target;
                string filename;
                if (document.SourceStream != null)
                    filename = ((FileStream)document.SourceStream).Name;
                else
                    filename = "document1.pdf";

                PdfDocumentCompressorCommand pdfDocumentCompressor = (PdfDocumentCompressorCommand)sender;
                if (pdfDocumentCompressor.DocumentPackOutputFilename == null)
                {
                    _saveFileDialog.FileName = string.Format("{0}_Compressed.pdf",
                        Path.GetFileNameWithoutExtension(filename));
                }
                else
                {
                    _saveFileDialog.FileName = pdfDocumentCompressor.DocumentPackOutputFilename;
                }

                if (_saveFileDialog.ShowDialog() == true)
                {
                    string newFilename = Path.GetFullPath(_saveFileDialog.FileName);
                    string outputFilename = null;
                    if (Path.GetFullPath(filename).ToUpperInvariant() != newFilename.ToUpperInvariant())
                        outputFilename = newFilename;

                    pdfDocumentCompressor.PackDocument = true;
                    pdfDocumentCompressor.DocumentPackFormat = document.Format;
                    pdfDocumentCompressor.DocumentPackOutputFilename = outputFilename;
                }
                else
                {
                    MessageBox.Show(
                        "This PDF document will not be packed(saved) after compression.",
                        "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    pdfDocumentCompressor.PackDocument = false;
                }
            }
        }

        /// <summary>
        /// PDF document compression is finished.
        /// </summary>
        private void PdfDocumentCompressorCommand_Finished(object sender, ProcessingEventArgs e)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.Invoke(new EventHandler<ProcessingEventArgs>(PdfDocumentCompressorCommand_Finished), sender, e);
            }
            else
            {
                PdfDocument document = (PdfDocument)e.Target;
                if (document.SourceStream != null)
                {
                    string filename = ((FileStream)document.SourceStream).Name;
                    if (filename.ToUpperInvariant() != Filename.ToUpperInvariant())
                        OpenPdfDocument(filename);
                }
            }
        }

        #endregion


        #region Pack PDF document

        /// <summary>
        /// Saves and packs PDF document.
        /// </summary>
        private bool SaveAndPackPdfDocument()
        {
            return SaveAndPackPdfDocument(_document.Format, _document.EncryptionSystem);
        }

        /// <summary>
        /// Saves and packs PDF document.
        /// </summary>
        private bool SaveAndPackPdfDocument(PdfFormat format, EncryptionSystem encryptionSystem)
        {
            _saveFileDialog.Title = null;
            if (_pdfFileName != null)
                _saveFileDialog.FileName = _pdfFileName;
            bool isSaved = false;
            if (_saveFileDialog.ShowDialog().Value)
            {
                SavePdfDocument(_saveFileDialog.FileName, true, PdfDocumentUpdateMode.CleanupAndPack, format, encryptionSystem);
                isSaved = true;
            }

            return isSaved;
        }

        /// <summary>
        /// Packs PDF document.
        /// </summary>
        private void PackPdfDocument(PdfFormat format, EncryptionSystem encryptionSystem)
        {
            StartAction("Cleanup", false);
            _document.RemoveUnusedPages();
            _document.RemoveUnusedNames();
            EndAction();

            _document.Progress += _pdfDocument_PackProgress;
            StartAction("Pack", true);
            long size = _document.StreamLength;
            try
            {
                _document.Pack(format, encryptionSystem);
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
            EndAction();
            statusLabel.Content = string.Format("{0} [{1}->{2} bytes]", statusLabel.Content, size, _document.StreamLength);
            _document.Progress -= _pdfDocument_PackProgress;
        }

        /// <summary>
        /// PDF document packing is in progress.
        /// </summary>
        private void _pdfDocument_PackProgress(object sender, ImageFileProgressEventArgs e)
        {
            ShowProgressInfo(e.Progress);
        }

        #endregion


        #region Save PDF document

        /// <summary>
        /// Saves document to the source PDF file or new PDF file.
        /// </summary>
        private void SavePdfDocumentToSourceOrNewFile(bool incrementalUpdate)
        {
            OnPdfDocumentSaving();
            bool isCanceled = true;
            try
            {
                if (_pdfFileName != null)
                {
                    _saveFileDialog.Title = null;
                    _saveFileDialog.FileName = _pdfFileName;
                }
                if (_saveFileDialog.ShowDialog().Value)
                {
                    isCanceled = false;
                    SavePdfDocument(
                        _saveFileDialog.FileName,
                        true,
                        incrementalUpdate ? PdfDocumentUpdateMode.Incremental : PdfDocumentUpdateMode.Auto,
                        _document.Format,
                        _document.EncryptionSystem);
                }
            }
            finally
            {
                OnPdfDocumentSaved(isCanceled);
            }
        }

        /// <summary>
        /// Saves changes in PDF document.
        /// </summary>
        private void SaveChangesInPdfDocument(PdfDocumentUpdateMode updateMode)
        {
            SaveChangesInPdfDocument(updateMode, _document.Format, _document.EncryptionSystem);
        }

        /// <summary>
        /// Saves changes in PDF document.
        /// </summary>
        private void SaveChangesInPdfDocument(PdfDocumentUpdateMode updateMode, PdfFormat format, EncryptionSystem encryptionSystem)
        {
            _contentEditorTool.RenderFiguresOnPage();
            if (IsDocumentChanged ||
                format != _document.Format ||
                encryptionSystem != _document.EncryptionSystem)
            {
                AbortBuildFigure();

                IsDocumentChanged = false;
                PdfEncoder encoder = CreateEncoder(format, encryptionSystem);
                encoder.SaveAndSwitchSource = true;
                encoder.Settings.UpdateMode = updateMode;
                StartAction("Save", true);
                try
                {
                    imageViewer1.Images.SaveSync(_pdfFileStream, encoder);
                    OnImageCollectionSaved(false);
                }
                catch (Exception e)
                {
                    OnImageCollectionSaved(true);
                    DemosTools.ShowErrorMessage("Saving error", e);
                }
                EndAction();
            }
        }

        /// <summary>
        /// Saves PDF document.
        /// </summary>
        private void SavePdfDocument(string filename, bool switchToSource)
        {
            SavePdfDocument(
                filename,
                switchToSource,
                PdfDocumentUpdateMode.Auto,
                _document.Format,
                _document.EncryptionSystem);
        }

        /// <summary>
        /// Saves PDF document.
        /// </summary>
        private void SavePdfDocument(
            string filename,
            bool switchToSource,
            PdfDocumentUpdateMode updateMode,
            PdfFormat format,
            EncryptionSystem encryptionSystem)
        {
            _contentEditorTool.RenderFiguresOnPage();
            _annotationTool.FocusedAnnotationView = null;

            if (filename.ToLowerInvariant() == _pdfFileName.ToLowerInvariant())
            {
                try
                {
                    SaveChangesInPdfDocument(updateMode, format, encryptionSystem);
                }
                catch (Exception e)
                {
                    DemosTools.ShowErrorMessage(e);
                }
                return;
            }

            PdfEncoder encoder = CreateEncoder(format, encryptionSystem);
            encoder.SaveAndSwitchSource = switchToSource;
            encoder.Settings.UpdateMode = updateMode;


            if (switchToSource)
            {
                AbortBuildFigure();
                try
                {
                    _newFileStream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
                }
                catch (IOException e)
                {
                    DemosTools.ShowErrorMessage(e);

                    return;
                }
                IsDocumentChanged = false;

                StartAction("Save As", true);
                _switchPdfFileStreamToNewStream = true;

                ImageCollection images = imageViewer1.Images;
                try
                {
                    images.ImageCollectionSavingProgress += new EventHandler<ProgressEventArgs>(Images_ImageCollectionSavingProgress);
                    encoder.SaveAndSwitchSource = true;
                    images.SaveSync(_newFileStream, encoder);

                    OnImageCollectionSaved(false);
                }
                catch (Exception e)
                {
                    OnImageCollectionSaved(true);
                    DemosTools.ShowErrorMessage("Saving error", e);
                }
                finally
                {
                    images.ImageCollectionSavingProgress -= new EventHandler<ProgressEventArgs>(Images_ImageCollectionSavingProgress);
                }
                Filename = filename;
                EndAction();
            }
            else
            {
                StartAction("Save To", true);
                using (Stream destStream = File.Create(filename))
                {
                    ImageCollection images = imageViewer1.Images;
                    try
                    {
                        images.ImageCollectionSavingProgress += new EventHandler<ProgressEventArgs>(Images_ImageCollectionSavingProgress);
                        encoder.SaveAndSwitchSource = false;
                        images.SaveSync(destStream, encoder);

                        OnImageCollectionSaved(false);
                    }
                    catch (Exception e)
                    {
                        OnImageCollectionSaved(true);
                        DemosTools.ShowErrorMessage("Saving error", e);
                    }
                    finally
                    {
                        images.ImageCollectionSavingProgress -= new EventHandler<ProgressEventArgs>(Images_ImageCollectionSavingProgress);
                    }
                }
                EndAction();
            }
        }

        /// <summary>
        /// Saves PDF document as new PDF document.
        /// </summary>
        private void SavePdfDocumentAs()
        {
            OnPdfDocumentSaving();
            bool isCanceled = true;
            try
            {
                _saveFileDialog.Title = null;
                if (_saveFileDialog.ShowDialog().Value)
                {
                    isCanceled = false;
                    SavePdfDocument(_saveFileDialog.FileName, true);
                }
            }
            finally
            {
                OnPdfDocumentSaved(isCanceled);
            }
        }

        /// <summary>
        /// PDF document is saving.
        /// </summary>
        private void OnPdfDocumentSaving()
        {
            IsPdfFileSaving = true;
            _enableUpdateUI = false;

            // execute PdfDocument.AdditionalActions.Saving action
            PdfJsDoc doc = _annotationTool.JsApp.GetDoc(_document);
            if (doc != null)
                _annotationTool.RaiseDocumentTriggerEvent(PdfJsEvent.CreateDocWillSaveEventObject(doc));
        }

        /// <summary>
        /// PDF document is saved.
        /// </summary>
        /// <param name="isCanceled">Indicates that saving of the PDF document is canceled.</param>
        private void OnPdfDocumentSaved(bool isCanceled)
        {
            if (!isCanceled)
            {
                // execute PdfDocument.AdditionalActions.Saved action
                PdfJsDoc doc = _annotationTool.JsApp.GetDoc(_document);
                if (doc != null)
                    _annotationTool.RaiseDocumentTriggerEvent(PdfJsEvent.CreateDocDidSaveEventObject(doc));
            }

            _enableUpdateUI = true;
            IsPdfFileSaving = false;

            if (!isCanceled)
                IsDocumentChanged = _document.IsChanged;
        }

        /// <summary>
        /// Creates new instance of PdfEncoder class.
        /// </summary>
        private PdfEncoder CreateEncoder(PdfFormat format, EncryptionSystem encryptionSystem)
        {
            _usePdfEncoderSettingsForAllImages = false;
            PdfEncoder encoder = new PdfEncoder();
            encoder.DocumentFormat = format;
            if (encryptionSystem != _document.EncryptionSystem)
                encoder.SetEncryptionSystem(encryptionSystem);
            encoder.Settings = _pdfEncoderSettings;
            encoder.ImageSaving += new EventHandler<ImageSavingEventArgs>(encoder_ImageSaving);
            return encoder;
        }

        /// <summary>
        /// Image (PDF page) is saving.
        /// </summary>
        private void encoder_ImageSaving(object sender, ImageSavingEventArgs e)
        {
            if (_usePdfEncoderSettingsForAllImages)
                return;
            if (PdfDocumentController.GetPageAssociatedWithImage(e.Image) != null)
                // this is PdfPage, not image!
                return;

            int index = imageViewer1.Images.IndexOf(e.Image);
            SetCompressionParamsWindow setCompressionParamsDialog = new SetCompressionParamsWindow(index, e.Image, _pdfEncoderSettings);
            setCompressionParamsDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            setCompressionParamsDialog.Owner = this;
            if (setCompressionParamsDialog.ShowDialog().Value)
                _usePdfEncoderSettingsForAllImages = setCompressionParamsDialog.UseCompressionForAllImages;
        }

        #endregion


        #region Convert PDF document to PDF/A document

        /// <summary>
        /// Converts PDF document to PDF/A.
        /// </summary>
        /// <param name="pdfAConverter">PDF/A converter.</param>
        private void ConvertPdfDocumentToPdfA(PdfDocumentConverter pdfAConverter)
        {
            pdfAConverter.Started += new EventHandler<ProcessingEventArgs>(PdfAConverter_Started);
            pdfAConverter.Finished += new EventHandler<ProcessingFinishedEventArgs>(PdfAConverter_Finished);

            DocumentProcessingCommandWindow.ExecuteDocumentProcessing(_document, pdfAConverter);
        }

        /// <summary>
        /// PDF document conversion is started.
        /// </summary>
        private void PdfAConverter_Started(object sender, ProcessingEventArgs e)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.Invoke(new EventHandler<ProcessingEventArgs>(PdfAConverter_Started), sender, e);
            }
            else
            {
                PdfDocument document = (PdfDocument)e.Target;
                string filename;
                if (document.SourceStream != null)
                    filename = ((FileStream)document.SourceStream).Name;
                else
                    filename = "document1.pdf";

                PdfAConverter pdfAConverter = (PdfAConverter)sender;
                if (pdfAConverter.OutputFilename == null)
                {
                    _saveFileDialog.FileName = string.Format("{0}_Converted.pdf",
                        Path.GetFileNameWithoutExtension(filename));
                }
                else
                {
                    _saveFileDialog.FileName = pdfAConverter.OutputFilename;
                }

                if (_saveFileDialog.ShowDialog() == true)
                {
                    string newFilename = Path.GetFullPath(_saveFileDialog.FileName);
                    string outputFilename = null;
                    if (Path.GetFullPath(filename).ToUpperInvariant() != newFilename.ToUpperInvariant())
                        outputFilename = newFilename;
                    pdfAConverter.OutputFilename = outputFilename;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// PDF document conversion is finished.
        /// </summary>
        private void PdfAConverter_Finished(object sender, ProcessingFinishedEventArgs e)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.Invoke(new EventHandler<ProcessingFinishedEventArgs>(PdfAConverter_Finished), sender, e);
            }
            else
            {
                PdfDocument document = (PdfDocument)e.Target;
                if (document.SourceStream != null)
                {
                    string filename = ((FileStream)document.SourceStream).Name;
                    if (filename.ToUpperInvariant() != Filename.ToUpperInvariant())
                        OpenPdfDocument(filename);
                }
            }
        }

        #endregion


        #region Convert PDF document to DOCX

#if !REMOVE_OFFICE_PLUGIN
        /// <summary>
        /// Converts PDF file to a DOCX file.
        /// </summary>
        /// <param name="obj">A string that contains path to the output DOCX file.</param>
        private void ConvertPdfDocumentToDocxFileThread(object obj)
        {
            string filePath = (string)obj;

            // start the 'Convert to DOCX" action
            StartAction("Convert to DOCX", true);

            // create converter
            using (Vintasoft.Imaging.Pdf.Office.PdfToDocxConverter converter = new Vintasoft.Imaging.Pdf.Office.PdfToDocxConverter())
            {
                // set converter settings
                converter.DecodingSettings = imageViewer1.ImageDecodingSettings;
                converter.RenderingSettings = imageViewer1.ImageRenderingSettings;
                converter.OutputFilename = filePath;

                // convert PDF document to DOCX file
                using (ProcessingState state = new ProcessingState(Images_ImageCollectionSavingProgress))
                    converter.Execute(_document, state);
            }

            EndAction();
        }
#endif

        #endregion


        #region Convert PDF document to SVG

        /// <summary>
        /// Converts PDF document to the SVG files.
        /// </summary>
        /// <param name="obj">An array that contains SVG encoder and file path.</param>
        private void ConvertPdfDocumentToSvgFilesThread(object obj)
        {
            object[] paramsArray = (object[])obj;
            EncoderBase svgEncoder = paramsArray[0] as EncoderBase;
            string filePath = paramsArray[1] as string;

            // create action progress controller to track saving progress
            ActionProgressController progressController = new ActionProgressController(ActionProgressHandlers.CreateActionProgressHandler(Images_ImageCollectionSavingProgress));

            // start progress controller
            progressController.Start("Converting to SVG", imageViewer1.Images.Count, this);

            try
            {
                if (imageViewer1.Images.Count == 1)
                {
                    // increase progress value
                    progressController.Next(false);

                    imageViewer1.Image.Save(_convertToFileDialog.FileName, svgEncoder);
                }
                else
                {
                    // create file name template with page number
                    string dirPath = Path.GetDirectoryName(filePath);
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string pageNameTemplate = Path.Combine(dirPath, fileName + "_PAGE_{0}.svg");

                    for (int i = 0; i < imageViewer1.Images.Count; i++)
                    {
                        // increase progress value
                        progressController.Next(false);

                        // create file path
                        filePath = string.Format(pageNameTemplate, i + 1);
                        // save image to SVG file
                        imageViewer1.Images[i].Save(filePath, svgEncoder);
                    }
                }
            }
            finally
            {
                svgEncoder.Dispose();
            }

            progressController.Finish();

            EndAction();
        }

        #endregion


        #region Hot keys

        /// <summary>
        /// Handles the CanExecute event of newCommandBinding object.
        /// </summary>
        private void newCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = newMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of openCommandBinding object.
        /// </summary>
        private void openCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = openMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of closeCommandBinding object.
        /// </summary>
        private void closeCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = closeMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of addPagesCommandBinding object.
        /// </summary>
        private void addPagesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = addPagesMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of saveAsCommandBinding object.
        /// </summary>
        private void saveAsCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = saveAsMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of printCommandBinding object.
        /// </summary>
        private void printCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = printMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of documentInformationCommandBinding object.
        /// </summary>
        private void documentInformationCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = documentMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of findTextCommandBinding object.
        /// </summary>
        private void findTextCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = textMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of cutCommandBinding object.
        /// </summary>
        private void cutCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            CutItemUIAction cutUIAction = PdfDemosTools.GetUIAction<CutItemUIAction>(CurrentTool);
            e.CanExecute = cutUIAction != null && cutUIAction.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of copyCommandBinding object.
        /// </summary>
        private void copyCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            CopyItemUIAction copyUIAction = PdfDemosTools.GetUIAction<CopyItemUIAction>(CurrentTool);
            e.CanExecute = copyUIAction != null && copyUIAction.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of pasteCommandBinding object.
        /// </summary>
        private void pasteCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            PasteItemUIAction pasteUIAction = PdfDemosTools.GetUIAction<PasteItemUIAction>(CurrentTool);
            e.CanExecute = pasteUIAction != null && pasteUIAction.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of deleteCommandBinding object.
        /// </summary>
        private void deleteCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DeleteItemUIAction deleteUIAction = PdfDemosTools.GetUIAction<DeleteItemUIAction>(CurrentTool);
            e.CanExecute = deleteUIAction != null && deleteUIAction.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of selectAllCommandBinding object.
        /// </summary>
        private void selectAllCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            SelectAllItemsUIAction selectAllUIAction = PdfDemosTools.GetUIAction<SelectAllItemsUIAction>(CurrentTool);
            e.CanExecute = selectAllUIAction != null && selectAllUIAction.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of rotateClockwiseCommandBinding object.
        /// </summary>
        private void rotateClockwiseCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = rotateClockwiseMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of rotateCounterclockwiseCommandBinding object.
        /// </summary>
        private void rotateCounterclockwiseCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = rotateCounterclockwiseMenuItem.IsEnabled;
        }

        #endregion


        #region Utils

        /// <summary>
        /// Prints page(s) of PDF document.
        /// </summary>
        private void PrintPdfDocument()
        {
            PrintDialog printDialog = _thumbnailViewerPrintManager.PrintDialog;
            printDialog.MinPage = 1;
            printDialog.MaxPage = (uint)_thumbnailViewerPrintManager.Images.Count;
            printDialog.UserPageRangeEnabled = true;

            // show print dialog and
            // start print if dialog results is OK
            if (printDialog.ShowDialog().Value)
            {
                PdfJsDoc doc = _annotationTool.JsApp.GetDoc(_document);

                // execute PdfDocument.AdditionalActions.Printing action
                if (doc != null)
                    _annotationTool.RaiseDocumentTriggerEvent(PdfJsEvent.CreateDocWillPrintEventObject(doc));

                try
                {
                    _thumbnailViewerPrintManager.Print(this.Title);
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }

                // execute PdfDocument.AdditionalActions.Printed action                
                if (doc != null)
                    _annotationTool.RaiseDocumentTriggerEvent(PdfJsEvent.CreateDocDidPrintEventObject(doc));
            }
        }


        /// <summary>
        /// Handles the StatusChanged event of the JavaScriptActionExecutor.
        /// </summary>
        private void JavaScriptActionExecutor_StatusChanged(
            object sender,
            PdfJavaScriptActionStatusChangedEventArgs e)
        {
            SetStatusLabelTextSafe(e.Status);
        }


        /// <summary>
        /// Shows a message box with error information.
        /// </summary>
        private void ShowMessage_CurrentImageIsNotPdfPage()
        {
            MessageBox.Show("Current image is not a PDF page. Save document and try again.", "Information",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Shows progress information.
        /// </summary>
        private void ShowProgressInfo(int progress)
        {
            if (Dispatcher.Thread == Thread.CurrentThread)
            {
                bool progressVisible = progress != 100;
                progressBar.Value = progress;
                progressBar.Visibility = progressVisible ? Visibility.Visible : Visibility.Collapsed;
                if (!progressVisible)
                    EndAction();
            }
            else
                Dispatcher.Invoke(new ShowProgressInfoDelegate(ShowProgressInfo), progress);
        }

        /// <summary>
        /// Sets text of status label (thread-safe).
        /// </summary>
        private void SetStatusLabelTextSafe(string text)
        {
            if (Dispatcher.Thread == Thread.CurrentThread)
            {
                statusLabel.Content = text;
            }
            else
                Dispatcher.Invoke(new SetStatusLabelTextDelegate(SetStatusLabelTextSafe), text);
        }

        /// <summary>
        /// Returns the document processing target.
        /// </summary>
        private PdfDocument GetDocumentProcessingTarget()
        {
            if (GetDocumentStructureIsChanged(imageViewer1.Images, _document))
            {
                DemosTools.ShowInfoMessage("PDF Document Processing", "Current document has not saved structural changes, please save or pack the document first.");
                return null;
            }
            return _document;
        }

        /// <summary>
        /// Returns the processing target.
        /// </summary>
        private PdfDocument GetProcessingTarget()
        {
            return _document;
        }


        /// <summary>
        /// Rotates images in both annotation viewer and thumbnail viewer by 90 degrees clockwise.
        /// </summary>
        private void RotateViewClockwise()
        {
            imageViewer1.RotateViewClockwise();
            thumbnailViewer1.RotateViewClockwise();
        }

        /// <summary>
        /// Rotates images in both annotation viewer and thumbnail viewer by 90 degrees counterclockwise.
        /// </summary>
        private void RotateViewCounterClockwise()
        {
            imageViewer1.RotateViewCounterClockwise();
            thumbnailViewer1.RotateViewCounterClockwise();
        }


        /// <summary>
        /// Reloads the image associated with PDF page.
        /// </summary>
        /// <param name="page">The PDF page.</param>
        private void ReloadImage(PdfPage page)
        {
            VintasoftImage image = PdfDemosTools.FindImageByPage(page, imageViewer1.Images);

            if (image != null)
            {
                if (image == imageViewer1.Image)
                    imageViewer1.ReloadImage();
                else
                    image.Reload(true);
            }
        }

        /// <summary>
        /// Reloads the images asynchronously.
        /// </summary>
        private void ReloadImagesAsync()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(ReloadImages);
        }

        /// <summary>
        /// Reloads the images.
        /// </summary>
        private void ReloadImages(object state)
        {
            VintasoftImage currentImage = imageViewer1.Image;
            try
            {
                if (currentImage != null)
                    currentImage.Reload(true);
            }
            catch
            {
            }
            foreach (VintasoftImage image in imageViewer1.Images)
            {
                if (currentImage != image)
                {
                    try
                    {
                        image.Reload(true);
                    }
                    catch
                    {
                    }
                }
            }
        }


        /// <summary>
        /// Packs all fonts of the loaded PDF documents in a background thread.
        /// </summary>
        /// <param name="progressController">Progress controller.</param>
        private void PackAllFonts(IActionProgressController progressController)
        {
            List<PdfDocument> loadedDocuments = GetPdfDocumentsAssociatedWithImageCollection(imageViewer1.Images);
            if (loadedDocuments.Count == 1)
            {
                // pack all fonts of PDF document loaded in image viewer
                loadedDocuments[0].FontManager.PackAllFonts(progressController);
            }
            else
            {
                progressController.Start("Packing fonts of loaded documents", loadedDocuments.Count, this);
                // pack all fonts of all PDF documents loaded in image viewer
                foreach (PdfDocument document in loadedDocuments)
                {
                    progressController.Next(false);
                    document.FontManager.PackAllFonts();
                }
                progressController.Finish();
            }
        }


        /// <summary>
        /// Handles the ImageSavingException event of the imageViewer control.
        /// </summary>
        private void Images_ImageSavingException(object sender, Vintasoft.Imaging.ExceptionEventArgs e)
        {
            DemosTools.ShowErrorMessage(e.Exception);
        }

        /// <summary>
        /// Adds the path (all files and sub folders) to specified folder.
        /// </summary>
        /// <param name="folder">The portfolio folder.</param>
        /// <param name="path">The path that should be added to the portfolio.</param>
        /// <param name="addPathAsFolder">Determines that portfolio must contain folder with the path filename.</param>
        /// <param name="compression">The compression that should be applied to files and folders.</param>
        /// <param name="actionController">The action controller.</param>
        /// <returns>Added folder.</returns>
        private static PdfAttachmentFolder AddPathRecursively(
            PdfAttachmentFolder folder,
            string path,
            bool addPathAsFolder,
            PdfCompression compression,
            StatusBarActionController actionController)
        {
            // folder to which path must be added
            PdfAttachmentFolder currentFolder;
            // if portfolio must contain folder with the path filename
            if (addPathAsFolder)
            {
                // add new folder to portfolio folder and use it as current folder
                currentFolder = folder.AddFolder(Path.GetFileName(path));
                currentFolder.CreationDate = DateTime.Now;
            }
            else
            {
                // use root folder as current folder
                currentFolder = folder;
                folder.ModificationDate = DateTime.Now;
            }

            // get files in the specified path
            string[] files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
            // if files are found
            if (files.Length > 0)
            {
                // for each file
                foreach (string filename in files)
                {
                    try
                    {
                        // if file is not hidden
                        if ((File.GetAttributes(filename) & FileAttributes.Hidden) == 0)
                        {
                            // add file
                            actionController.NextSubAction(Path.GetFileName(filename));
                            PdfEmbeddedFileSpecification file = currentFolder.AddFile(filename, compression);
                            file.EmbeddedFile.CreationDate = DateTime.Now;
                        }
                    }
                    catch (Exception ex)
                    {
                        DemosTools.ShowErrorMessage(string.Format("{0}: {1}", filename, ex.Message));
                    }
                }
                currentFolder.ModificationDate = DateTime.Now;
            }

            // get directories in the specified path
            string[] paths = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            // for each directory
            foreach (string subPath in paths)
            {
                try
                {
                    // if directory is no thidden
                    if ((File.GetAttributes(subPath) & FileAttributes.Hidden) != 0)
                    {
                        // add the directory (all files and sub folders) to current portfolio folder
                        AddPathRecursively(currentFolder, subPath, true, compression, actionController);
                    }
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(string.Format("{0}: {1}", currentFolder, ex.Message));
                }
            }

            return currentFolder;
        }


        /// <summary>
        /// PDF thumbnail generation is in progress.
        /// </summary>
        private void pdfDocument_ThumbnailGenerationProgress(object sender, ImageFileProgressEventArgs e)
        {
            ShowProgressInfo(e.Progress);
        }


        /// <summary>
        /// Determines when PDF document structure is changed.
        /// </summary>
        /// <param name="documentPages">The document pages.</param>
        /// <param name="document">The document.</param>
        private static bool GetDocumentStructureIsChanged(ImageCollection documentPages, PdfDocument document)
        {
            if (documentPages.Count != document.Pages.Count)
                return true;
            for (int i = 0; i < documentPages.Count; i++)
                if (PdfDocumentController.GetPageAssociatedWithImage(documentPages[i]) != document.Pages[i])
                    return true;
            return false;
        }


        /// <summary>
        /// Executes the specified processing command for the focused image.
        /// </summary>
        /// <param name="command">The processing command.</param>
        /// <param name="showDialog">Indicates that the processing dialog must be shown.</param>
        /// <param name="clearTextSelection">Indicates that selected text must be cleared.</param>
        private void ExecuteProcessingCommandOnFocusedImage(
            IProcessingCommand<VintasoftImage> command,
            bool showDialog,
            bool clearTextSelection)
        {
            if (clearTextSelection &&
                !string.IsNullOrEmpty(_textSelectionTool.SelectedText))
            {
                _textSelectionTool.ClearSelection();
                _textSelectionTool.FocusedTextSymbol = null;
            }

            ProcessingCommandWindow<VintasoftImage>.ExecuteProcessing(
                imageViewer1.Image, command, showDialog);
        }


        #region Action

        /// <summary>
        /// Saves the start time of the action.
        /// </summary>
        private void StartAction(string actionName, bool progress)
        {
            _actionStartTime = DateTime.Now;
            _actionName = actionName;

            if (progress)
                actionName += ":";
            else
                actionName += "...";

            SetStatusLabelTextSafe(actionName);
        }

        /// <summary>
        /// Does the tasks when action is finished.
        /// </summary>
        private void EndAction()
        {
            if (_actionName != "")
            {
                double ms = (DateTime.Now - _actionStartTime).TotalMilliseconds;
                string msString;
                if (ms < 1)
                    msString = "<1";
                else
                    msString = ms.ToString();
                SetStatusLabelTextSafe(string.Format("{0}: {1} ms", _actionName, msString));
                _actionName = "";
            }
        }

        /// <summary>
        /// Does the tasks when action is canceled.
        /// </summary>
        private void EndCanceledAction()
        {
            if (_actionName != "")
            {
                SetStatusLabelTextSafe(string.Format("{0}: canceled", _actionName));
                _actionName = "";
            }
        }

        /// <summary>
        /// Does the tasks when action is failed.
        /// </summary>
        private void EndFailedAction()
        {
            SetStatusLabelTextSafe("");
            _actionName = "";
        }

        #endregion

        #endregion

        #endregion

        #endregion



        #region Delegates

        delegate void UpdateUIDelegate();

        delegate void CloseDocumentDelegate();

        delegate void ShowProgressInfoDelegate(int progress);

        delegate void SetStatusLabelTextDelegate(string text);


        #endregion


    }
}
