using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.Fonts;
using Vintasoft.Imaging.Pdf.Wpf.UI.Annotations;
using Vintasoft.Imaging.Wpf.UI;
using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;
using WpfDemosCommonCode.Office;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A panel that allows to add and build new PDF annotations on a PDF page.
    /// </summary>
    public partial class PdfAnnotationBuilderControl : UserControl
    {

        #region Constants

        /// <summary>
        /// The color for selected menu item.
        /// </summary>
        readonly Brush SELECTED_MENU_ITEM_BRUSH;

        /// <summary>
        /// The color for unselected menu item.
        /// </summary>
        readonly Brush UNSELECTED_MENU_ITEM_BRUSH;

        /// <summary>
        /// The text font default size.
        /// </summary>
        const float TEXT_FONT_DEFAULT_SIZE = 12;

        #endregion



        #region Fields

        /// <summary>
        /// Dictionary: the framework element => annotation type.
        /// </summary>
        Dictionary<FrameworkElement, AnnotationType> _frameworkElementToAnnotationType =
            new Dictionary<FrameworkElement, AnnotationType>();

        /// <summary>
        /// The background color of annotations.
        /// </summary>
        System.Drawing.Color _annotationFillColor = System.Drawing.Color.White;

        /// <summary>
        /// The mouse observer of visual tool.
        /// </summary>
        VisualToolMouseObserver _visualToolMouseObserver = new VisualToolMouseObserver();

        /// <summary>
        ///  The prevous icon name of file attachment annotation.
        /// </summary>
        string _previousFileAttachmentIconName = string.Empty;

        /// <summary>
        /// The annotation button, which is currently selected in the control.
        /// </summary>
        FrameworkElement _selectedAnnotationButton;

        /// <summary>
        /// The type of the last built annotation.
        /// </summary>
        AnnotationType _lastBuiltAnnotationType = AnnotationType.Unknown;

        /// <summary>
        /// Indicates that the focused index of image viewer is changing.
        /// </summary>
        bool _isFocusedIndexChanging = false;

        /// <summary>
        /// Indicates that the interaction mode is changing.
        /// </summary>
        bool _isInteractionModeChanging = false;

        /// <summary>
        /// Indicates that the annotation building must be continued after changing focus in viewer.
        /// </summary>
        bool _needContinueBuildAnnotationsAfterFocusedIndexChanged = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAnnotationBuilderControl"/> class.
        /// </summary>
        public PdfAnnotationBuilderControl()
        {
            SELECTED_MENU_ITEM_BRUSH = new SolidColorBrush(Color.FromRgb(50, 150, 255));
            UNSELECTED_MENU_ITEM_BRUSH = Brushes.Transparent;

            InitializeComponent();

            InitializeAnnotationButtons();
        }

        #endregion



        #region Properties

        WpfPdfAnnotationTool _annotationTool = null;
        /// <summary>
        /// Gets or sets the PDF annotation tool.
        /// </summary>
        [Browsable(false)]
        [DefaultValue((object)null)]
        public WpfPdfAnnotationTool AnnotationTool
        {
            get
            {
                return _annotationTool;
            }
            set
            {
                if (value != null && value.ImageViewer != null)
                    throw new InvalidOperationException("Annotation tool should be deactivated.");

                // if annotation tool is not empty
                if (_annotationTool != null)
                    UnscribeFromPdfAnnotationToolEvents(_annotationTool);

                // set new annotation tool
                _annotationTool = value;
                _visualToolMouseObserver.VisualTool = value;

                // update user interface
                UpdateUI();

                // if annotation tool is not empty
                if (_annotationTool != null)
                    SubscribeToPdfAnnotationToolEvents(_annotationTool);
            }
        }

        bool _needBuildAnnotationsContinuously = false;
        /// <summary>
        /// Gets a value indicating whether the annotations must be built continuously.
        /// </summary>
        public bool NeedBuildAnnotationsContinuously
        {
            get
            {
                return _needBuildAnnotationsContinuously;
            }
            set
            {
                _needBuildAnnotationsContinuously = value;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Updates the user interface of this control.
        /// </summary>
        public void UpdateUI()
        {
            mainMenu.IsEnabled =
                _annotationTool != null &&
                _annotationTool.ImageViewer != null &&
                _annotationTool.ImageViewer.Image != null &&
                PdfDocumentController.GetPageAssociatedWithImage(_annotationTool.ImageViewer.Image) != null;
        }

        #endregion


        #region PRIVATE

        #region Image viewer

        /// <summary>
        /// Subscribes to the image viewer events.
        /// </summary>
        /// <param name="imageViewer">The image viewer.</param>
        private void SubscribeToImageViewerEvents(WpfImageViewer imageViewer)
        {
            imageViewer.FocusedIndexChanged += new Vintasoft.Imaging.PropertyChangedEventHandler<int>(imageViewer_FocusedIndexChanged);
            imageViewer.VisualToolChanging += new Vintasoft.Imaging.PropertyChangingEventHandler<Vintasoft.Imaging.Wpf.UI.VisualTools.WpfVisualTool>(imageViewer_VisualToolChanging);
        }

        /// <summary>
        /// Unsubscribes from the image viewer events.
        /// </summary>
        /// <param name="imageViewer">The image viewer.</param>
        private void UnscribeFromImageViewerEvents(WpfImageViewer imageViewer)
        {
            imageViewer.FocusedIndexChanged -= new Vintasoft.Imaging.PropertyChangedEventHandler<int>(imageViewer_FocusedIndexChanged);
            imageViewer.VisualToolChanging -= new Vintasoft.Imaging.PropertyChangingEventHandler<Vintasoft.Imaging.Wpf.UI.VisualTools.WpfVisualTool>(imageViewer_VisualToolChanging);
        }

        /// <summary>
        /// Index, of focused image in viewer, is changing.
        /// </summary>
        private void imageViewer_FocusedIndexChanging(object sender, Vintasoft.Imaging.PropertyChangedEventArgs<int> e)
        {
            _isFocusedIndexChanging = true;

            if (AnnotationTool != null && NeedBuildAnnotationsContinuously)
            {
                // if focused annotation view is not empty
                if (AnnotationTool.FocusedAnnotationView != null &&
                    IsNotPdfWidgetAnnotation(AnnotationTool.FocusedAnnotationView))
                {
                    // if focused annotation view is building
                    if (AnnotationTool.FocusedAnnotationView.IsBuilding)
                    {
                        _needContinueBuildAnnotationsAfterFocusedIndexChanged = true;

                        AnnotationTool.CancelBuilding();
                    }
                }
            }
        }

        /// <summary>
        /// Index, of focused image in viewer, is changed.
        /// </summary>
        private void imageViewer_FocusedIndexChanged(
            object sender,
            Vintasoft.Imaging.PropertyChangedEventArgs<int> e)
        {
            _isFocusedIndexChanging = false;

            UpdateUI();

            if (_needContinueBuildAnnotationsAfterFocusedIndexChanged &&
                AnnotationTool.ImageViewer.FocusedIndex != -1)
            {
                AddAndBuildAnnotation(_lastBuiltAnnotationType);
            }

            _needContinueBuildAnnotationsAfterFocusedIndexChanged = false;
        }

        /// <summary>
        /// Visual tool of image viewer is changing.
        /// </summary>
        private void imageViewer_VisualToolChanging(
            object sender,
            Vintasoft.Imaging.PropertyChangingEventArgs<Vintasoft.Imaging.Wpf.UI.VisualTools.WpfVisualTool> e)
        {
            if (AnnotationTool != null)
                AnnotationTool.CancelBuilding();
        }

        #endregion


        #region PDF annotation tool

        /// <summary>
        /// Subscribes to the PDF annotation tool events.
        /// </summary>
        /// <param name="annotationTool">The PDF annotation tool.</param>
        private void SubscribeToPdfAnnotationToolEvents(WpfPdfAnnotationTool annotationTool)
        {
            annotationTool.Activating += new EventHandler(pdfAnnotationTool_Activating);
            annotationTool.Activated += new EventHandler(pdfAnnotationTool_Activated);
            annotationTool.Deactivating += new EventHandler(pdfAnnotationTool_Deactivating);
            annotationTool.Deactivated += new EventHandler(pdfAnnotationTool_Deactivated);
            annotationTool.BuildingFinished += new EventHandler<WpfPdfAnnotationViewEventArgs>(pdfAnnotationTool_BuildingFinished);
            annotationTool.BuildingCanceled += new EventHandler<WpfPdfAnnotationViewEventArgs>(pdfAnnotationTool_BuildingCanceled);
        }

        /// <summary>
        /// Unsubscribes from the PDF annotation tool events.
        /// </summary>
        /// <param name="annotationTool">The PDF annotation tool.</param>
        private void UnscribeFromPdfAnnotationToolEvents(WpfPdfAnnotationTool annotationTool)
        {
            annotationTool.Activating -= new EventHandler(pdfAnnotationTool_Activating);
            annotationTool.Activated -= new EventHandler(pdfAnnotationTool_Activated);
            annotationTool.Deactivating -= new EventHandler(pdfAnnotationTool_Deactivating);
            annotationTool.Deactivated -= new EventHandler(pdfAnnotationTool_Deactivated);
            annotationTool.BuildingFinished -= new EventHandler<WpfPdfAnnotationViewEventArgs>(pdfAnnotationTool_BuildingFinished);
            annotationTool.BuildingCanceled -= new EventHandler<WpfPdfAnnotationViewEventArgs>(pdfAnnotationTool_BuildingCanceled);
        }

        /// <summary>
        /// PDF annotation tool is activating.
        /// </summary>
        private void pdfAnnotationTool_Activating(object sender, EventArgs e)
        {
            AnnotationTool.ImageViewer.FocusedIndexChanging += new Vintasoft.Imaging.PropertyChangedEventHandler<int>(imageViewer_FocusedIndexChanging);
        }

        /// <summary>
        /// PDF annotation tool is activated.
        /// </summary>
        private void pdfAnnotationTool_Activated(object sender, EventArgs e)
        {
            SubscribeToImageViewerEvents(AnnotationTool.ImageViewer);

            UpdateUI();
        }

        /// <summary>
        /// PDF annotation tool is deactivating.
        /// </summary>
        private void pdfAnnotationTool_Deactivating(object sender, EventArgs e)
        {
            AnnotationTool.ImageViewer.FocusedIndexChanging -= imageViewer_FocusedIndexChanging;
        }

        /// <summary>
        /// PDF annotation tool is deactivated.
        /// </summary>
        private void pdfAnnotationTool_Deactivated(object sender, EventArgs e)
        {
            UnscribeFromImageViewerEvents(AnnotationTool.ImageViewer);

            mainMenu.IsEnabled = false;
        }

        /// <summary>
        /// The annotation building is canceled.
        /// </summary>
        private void pdfAnnotationTool_BuildingCanceled(object sender, WpfPdfAnnotationViewEventArgs e)
        {
            if (!_isFocusedIndexChanging &&
                !_isInteractionModeChanging)
                EndBuilding();
        }

        /// <summary>
        /// The annotation building is finished.
        /// </summary>
        private void pdfAnnotationTool_BuildingFinished(object sender, WpfPdfAnnotationViewEventArgs e)
        {
            // if annotation tool does not contain annotation
            if (!_annotationTool.AnnotationCollection.Contains(e.AnnotationView.Annotation))
                return;

            // if annotation view is PDF link annotation view
            if (e.AnnotationView is WpfPdfLinkAnnotationView)
            {
                // create new action
                PdfAction activateAction = CreatePdfActionWindow.CreateAction(e.AnnotationView.Annotation.Document, Window.GetWindow(this));
                // if action is not empty
                if (activateAction != null)
                {
                    if (PdfActionsEditorTool.EditAction(activateAction, AnnotationTool.ImageViewer.Images, Window.GetWindow(this)))
                        e.AnnotationView.Annotation.ActivateAction = activateAction;
                }
                ((WpfPdfLinkAnnotationView)e.AnnotationView).IsHighlighted = false;
            }
            // if annotation view is PDF file attachment annotation view
            else if (e.AnnotationView is WpfPdfFileAttachmentAnnotationView)
            {
                // get the annotation
                PdfFileAttachmentAnnotation fileAttachmentAnnotation = ((PdfFileAttachmentAnnotation)e.AnnotationView.Annotation);
                SetEmbeddedFile(fileAttachmentAnnotation.Document, fileAttachmentAnnotation);
            }
#if !REMOVE_OFFICE_PLUGIN
            // if annotation view is PDF office document annotation view
            else if (e.AnnotationView is WpfPdfOfficeDocumentAnnotationView)
            {
                // if last buil annotation is Cart
                if (_lastBuiltAnnotationType == AnnotationType.Chart)
                {
                    // show chart data editor dialog
                    e.AnnotationView.InteractionController = e.AnnotationView.Transformer;
                    Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction.WpfOfficeDocumentVisualEditor visualEditor =
                        WpfCompositeInteractionController.FindInteractionController<Vintasoft.Imaging.Office.OpenXml.Wpf.UI.VisualTools.UserInteraction.WpfOfficeDocumentVisualEditor>(e.AnnotationView.InteractionController);
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
            }
#endif

            if (IsNotPdfWidgetAnnotation(e.AnnotationView))
            {
                if (NeedBuildAnnotationsContinuously)
                {
                    AddAndBuildAnnotation(_lastBuiltAnnotationType);
                }
                else
                {
                    EndBuilding();
                }
            }
        }

        #endregion


        #region Annotation buttons

        /// <summary>
        /// Initializes the annotation buttons.
        /// </summary>
        private void InitializeAnnotationButtons()
        {
            InitializeAnnotationButton(lineButton,
                "Line", AnnotationType.Line);

            InitializeAnnotationButton(lineWithArrowMenuItem,
                "LineWithArrow", AnnotationType.LineWithArrow);

            InitializeAnnotationButton(lineWithArrowsMenuItem,
                "LineWithArrows", AnnotationType.LineWithArrows);

            InitializeAnnotationButton(inkButton,
                "Ink", AnnotationType.Ink);

            InitializeAnnotationButton(rectangleButton,
                "Rectangle", AnnotationType.Rectangle);

            InitializeAnnotationButton(filledRectangleMenuItem,
                "FilledRectangle", AnnotationType.FilledRectangle);

            InitializeAnnotationButton(cloudRectangleMenuItem,
                "CloudRectangle", AnnotationType.CloudRectangle);

            InitializeAnnotationButton(cloudFilledRectangleMenuItem,
                "CloudFilledRectangle", AnnotationType.CloudFilledRectangle);

            InitializeAnnotationButton(ellipseButton,
                "Ellipse", AnnotationType.Ellipse);

            InitializeAnnotationButton(filledEllipseMenuItem,
                "FilledEllipse", AnnotationType.FilledEllipse);

            InitializeAnnotationButton(cloudEllipseMenuItem,
                "CloudEllipse", AnnotationType.CloudEllipse);

            InitializeAnnotationButton(cloudFilledEllipseMenuItem,
                "CloudFilledEllipse", AnnotationType.CloudFilledEllipse);

            InitializeAnnotationButton(polylineButton,
                "Polyline", AnnotationType.Polyline);

            InitializeAnnotationButton(polylineWithArrowMenuItem,
                "PolylineWithArrow", AnnotationType.PolylineWithArrow);

            InitializeAnnotationButton(polylineWithArrowsMenuItem,
                "PolylineWithArrows", AnnotationType.PolylineWithArrows);

            InitializeAnnotationButton(freehandPolylineMenuItem,
                "FreehandPolyline", AnnotationType.FreehandPolyline);

            InitializeAnnotationButton(polygonButton,
                "Polygon", AnnotationType.Polygon);

            InitializeAnnotationButton(filledPolygonMenuItem,
                "FilledPolygon", AnnotationType.FilledPolygon);

            InitializeAnnotationButton(cloudPolygonMenuItem,
                "CloudPolygon", AnnotationType.CloudPolygon);

            InitializeAnnotationButton(cloudFilledPolygonMenuItem,
                "CloudFilledPolygon", AnnotationType.CloudFilledPolygon);

            InitializeAnnotationButton(freehandPolygonMenuItem,
                "FreehandPolygon", AnnotationType.FreehandPolygon);

            InitializeAnnotationButton(linkButton,
                "Link", AnnotationType.Link);

            InitializeAnnotationButton(labelButton,
                "Label", AnnotationType.Label);

            InitializeAnnotationButton(textButton,
                "Text", AnnotationType.Text);

            InitializeAnnotationButton(cloudTextMenuItem,
                "CloudText", AnnotationType.CloudText);

            InitializeAnnotationButton(freeTextButton,
                "FreeText", AnnotationType.FreeText);

            InitializeAnnotationButton(cloudFreeTextMenuItem,
                "CloudFreeText", AnnotationType.CloudFreeText);

            InitializeAnnotationButton(fileAttachmentButton,
                "FileAttachment", AnnotationType.FileAttachment);

            InitializeAnnotationButton(graphFileAttachmentMenuItem,
                "GraphFileAttachment", AnnotationType.GraphFileAttachment);

            InitializeAnnotationButton(pushPinFileAttachmentMenuItem,
                "PushPinFileAttachment", AnnotationType.PushPinFileAttachment);

            InitializeAnnotationButton(paperclipFileAttachmentMenuItem,
                "PaperclipFileAttachment", AnnotationType.PaperclipFileAttachment);

            InitializeAnnotationButton(tagFileAttachmentMenuItem,
                "TagFileAttachment", AnnotationType.TagFileAttachment);

            InitializeAnnotationButton(textCommentButton,
                "Text_Comment", AnnotationType.Text_Comment);

            InitializeAnnotationButton(textCheckMenuItem,
                "Text_Check", AnnotationType.Text_Check);

            InitializeAnnotationButton(textCheckmarkMenuItem,
                "Text_Checkmark", AnnotationType.Text_Checkmark);

            InitializeAnnotationButton(textCircleMenuItem,
                "Text_Circle", AnnotationType.Text_Circle);

            InitializeAnnotationButton(textRectangleMenuItem,
                "Text_Rectangle", AnnotationType.Text_Rectangle);

            InitializeAnnotationButton(textCrossMenuItem,
                "Text_Cross", AnnotationType.Text_Cross);

            InitializeAnnotationButton(textCrossHairsMenuItem,
                "Text_CrossHairs", AnnotationType.Text_CrossHairs);

            InitializeAnnotationButton(textHelpMenuItem,
                "Text_Help", AnnotationType.Text_Help);

            InitializeAnnotationButton(textInsertMenuItem,
                "Text_Insert", AnnotationType.Text_Insert);

            InitializeAnnotationButton(textKeyMenuItem,
                "Text_Key", AnnotationType.Text_Key);

            InitializeAnnotationButton(textNewParagraphMenuItem,
                "Text_NewParagraph", AnnotationType.Text_NewParagraph);

            InitializeAnnotationButton(textNoteMenuItem,
                "Text_Note", AnnotationType.Text_Note);

            InitializeAnnotationButton(textParagraphMenuItem,
                "Text_Paragraph", AnnotationType.Text_Paragraph);

            InitializeAnnotationButton(textRightArrowMenuItem,
                "Text_RightArrow", AnnotationType.Text_RightArrow);

            InitializeAnnotationButton(textRightPointerMenuItem,
                "Text_RightPointer", AnnotationType.Text_RightPointer);

            InitializeAnnotationButton(textStarMenuItem,
                "Text_Star", AnnotationType.Text_Star);

            InitializeAnnotationButton(textUpArrowMenuItem,
                "Text_UpArrow", AnnotationType.Text_UpArrow);

            InitializeAnnotationButton(textUpLeftArrowMenuItem,
                "Text_UpLeftArrow", AnnotationType.Text_UpLeftArrow);

            InitializeAnnotationButton(formattedTextButton,
                "EmptyDocument", AnnotationType.EmptyOfficeDocument);

            InitializeAnnotationButton(officeDocumentMenuItem,
                "OfficeDocument", AnnotationType.OfficeDocument);

            InitializeAnnotationButton(chartButton,
                "Chart", AnnotationType.Chart);
        }

        /// <summary>
        /// Initializes the annotation button.
        /// </summary>
        /// <param name="annotationButton">A button, which must be clicked for starting of annotation building.</param>
        /// <param name="annotationImageResourceName">Name of the resource, which stores image for annotation button.</param>
        /// <param name="annotationType">The annotation type, which must be built when button is clicked.</param>
        private void InitializeAnnotationButton(
            FrameworkElement annotationButton,
            string annotationImageResourceName,
            AnnotationType annotationType)
        {
            if (Vintasoft.Imaging.ImagingEnvironment.IsInDesignMode)
                return;

            string resourceNameFormatString = "WpfDemosCommonCode.Pdf.AnnotationTool.Annotations.Resources.{0}.png";
            using (Vintasoft.Imaging.VintasoftImage image =
                DemosResourcesManager.GetResourceAsImage(string.Format(resourceNameFormatString, annotationImageResourceName)))
            {
                BitmapSource bitmap = Vintasoft.Imaging.Wpf.VintasoftImageConverter.ToBitmapSource(image);
                annotationButton.Tag = bitmap;
            }

            _frameworkElementToAnnotationType[annotationButton] = annotationType;

            SetAnnotationButtonCheckedState(annotationButton, false);
        }


        /// <summary>
        /// "Build annotation" button is clicked.
        /// </summary>
        private void addAndBuildAnnotationButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement item = (FrameworkElement)sender;

            // get new building annotation type
            AnnotationType annotationType = _frameworkElementToAnnotationType[item];

            Button button = sender as Button;
            // if buiding must be stopped
            if (_lastBuiltAnnotationType == annotationType ||
               (button != null &&
                button.BorderBrush == SELECTED_MENU_ITEM_BRUSH))
            {
                item = null;
                annotationType = AnnotationType.Unknown;
            }

            // cancel current building
            _annotationTool.CancelBuilding();

            // select the button of building annotation
            SetSelectedAnnotationButton(item);
            // add and build annotation
            AddAndBuildAnnotation(annotationType);
        }

        /// <summary>
        /// Sets the selected annotation button.
        /// </summary>
        /// <param name="annotationButton">The annotation button, which must be selected.</param>
        private void SetSelectedAnnotationButton(FrameworkElement annotationButton)
        {
            // uncheck current button
            SetAnnotationButtonCheckedState(_selectedAnnotationButton, false);

            // check specified button
            SetAnnotationButtonCheckedState(annotationButton, true);

            _selectedAnnotationButton = annotationButton;
        }

        /// <summary>
        /// Sets the checked state of annotation button.
        /// </summary>
        /// <param name="annotationButton">The annotation button.</param>
        /// <param name="isAnnotationButtonChecked">Indicates that annotation button is checked.</param>
        private void SetAnnotationButtonCheckedState(FrameworkElement annotationButton, bool isAnnotationButtonChecked)
        {
            if (annotationButton == null)
                return;

            Brush borderBrush = UNSELECTED_MENU_ITEM_BRUSH;

            if (isAnnotationButtonChecked)
                borderBrush = SELECTED_MENU_ITEM_BRUSH;

            if (annotationButton is Button)
            {
                Button button = (Button)annotationButton;

                button.BorderBrush = borderBrush;
            }
            else if (annotationButton is MenuItem)
            {
                MenuItem menuItem = (MenuItem)annotationButton;

                if (menuItem.Header is StackPanel)
                {
                    StackPanel stackPanel = (StackPanel)menuItem.Header;

                    foreach (UIElement uiElement in stackPanel.Children)
                    {
                        if (uiElement is Button)
                        {
                            SetAnnotationButtonCheckedState((Button)uiElement, isAnnotationButtonChecked);
                            break;
                        }
                    }
                }
                else
                {
                    menuItem.BorderBrush = borderBrush;
                }

                if (menuItem.Parent is MenuItem)
                {
                    SetAnnotationButtonCheckedState((MenuItem)menuItem.Parent, isAnnotationButtonChecked);
                }
            }
        }

        #endregion


        #region AddAndBuild...

        /// <summary>
        /// Adds and builds an annotation.
        /// </summary>
        /// <param name="annotationType">The type of annotation, which must be built.</param>
        private void AddAndBuildAnnotation(AnnotationType annotationType)
        {
            try
            {
                _lastBuiltAnnotationType = annotationType;

                switch (annotationType)
                {
                    case AnnotationType.Line:
                        AddAndBuildLineAnnotation(
                            PdfAnnotationLineEndingStyle.None,
                            PdfAnnotationLineEndingStyle.None);
                        break;

                    case AnnotationType.LineWithArrow:
                        AddAndBuildLineAnnotation(
                            PdfAnnotationLineEndingStyle.OpenArrow,
                            PdfAnnotationLineEndingStyle.None);
                        break;

                    case AnnotationType.LineWithArrows:
                        AddAndBuildLineAnnotation(
                            PdfAnnotationLineEndingStyle.OpenArrow,
                            PdfAnnotationLineEndingStyle.OpenArrow);
                        break;

                    case AnnotationType.Ink:
                        AddAndBuildInkAnnotation();
                        break;

                    case AnnotationType.Rectangle:
                        AddAndBuildSquareAnnotation(
                            PdfAnnotationBorderEffectType.Solid,
                            System.Drawing.Color.Empty, 5);
                        break;

                    case AnnotationType.FilledRectangle:
                        AddAndBuildSquareAnnotation(
                            PdfAnnotationBorderEffectType.Solid,
                            _annotationFillColor, 5);
                        break;

                    case AnnotationType.CloudRectangle:
                        AddAndBuildSquareAnnotation(
                            PdfAnnotationBorderEffectType.Cloudy,
                            System.Drawing.Color.Empty, 1);
                        break;

                    case AnnotationType.CloudFilledRectangle:
                        AddAndBuildSquareAnnotation(
                            PdfAnnotationBorderEffectType.Cloudy,
                            _annotationFillColor, 1);
                        break;

                    case AnnotationType.Ellipse:
                        AddAndBuildCircleAnnotation(
                            PdfAnnotationBorderEffectType.Solid,
                            System.Drawing.Color.Empty, 5);
                        break;

                    case AnnotationType.FilledEllipse:
                        AddAndBuildCircleAnnotation(
                            PdfAnnotationBorderEffectType.Solid,
                            _annotationFillColor, 5);
                        break;

                    case AnnotationType.CloudEllipse:
                        AddAndBuildCircleAnnotation(
                            PdfAnnotationBorderEffectType.Cloudy,
                            System.Drawing.Color.Empty, 1);
                        break;

                    case AnnotationType.CloudFilledEllipse:
                        AddAndBuildCircleAnnotation(
                            PdfAnnotationBorderEffectType.Cloudy,
                            _annotationFillColor, 1);
                        break;

                    case AnnotationType.Polyline:
                        AddAndBuildPolylineAnnotation(
                            PdfAnnotationLineEndingStyle.None,
                             PdfAnnotationLineEndingStyle.None);
                        break;

                    case AnnotationType.PolylineWithArrow:
                        AddAndBuildPolylineAnnotation(
                            PdfAnnotationLineEndingStyle.OpenArrow,
                             PdfAnnotationLineEndingStyle.None);
                        break;

                    case AnnotationType.PolylineWithArrows:
                        AddAndBuildPolylineAnnotation(
                            PdfAnnotationLineEndingStyle.OpenArrow,
                             PdfAnnotationLineEndingStyle.OpenArrow);
                        break;

                    case AnnotationType.FreehandPolyline:
                        AddAndBuildFreehandPolylineAnnotation();
                        break;

                    case AnnotationType.Polygon:
                        AddAndBuildPolygonAnnotation(
                            PdfAnnotationBorderEffectType.Solid,
                            System.Drawing.Color.Empty, 5);
                        break;

                    case AnnotationType.FilledPolygon:
                        AddAndBuildPolygonAnnotation(
                            PdfAnnotationBorderEffectType.Solid,
                            _annotationFillColor, 5);
                        break;

                    case AnnotationType.CloudPolygon:
                        AddAndBuildPolygonAnnotation(
                            PdfAnnotationBorderEffectType.Cloudy,
                            System.Drawing.Color.Empty, 1);
                        break;

                    case AnnotationType.CloudFilledPolygon:
                        AddAndBuildPolygonAnnotation(
                            PdfAnnotationBorderEffectType.Cloudy,
                            _annotationFillColor, 1);
                        break;

                    case AnnotationType.FreehandPolygon:
                        AddAndBuildFreehandPolygonAnnotation(_annotationFillColor);
                        break;

                    case AnnotationType.Link:
                        AddAndBuildLinkAnnotation();
                        break;

                    case AnnotationType.Label:
                        AddAndBuildLabelAnnotation();
                        break;

                    case AnnotationType.Text:
                        AddAndBuildTextAnnotation(PdfAnnotationBorderEffectType.Solid);
                        break;

                    case AnnotationType.CloudText:
                        AddAndBuildTextAnnotation(PdfAnnotationBorderEffectType.Cloudy);
                        break;

                    case AnnotationType.FreeText:
                        AddAndBuildFreeTextAnnotation(PdfAnnotationBorderEffectType.Solid);
                        break;

                    case AnnotationType.CloudFreeText:
                        AddAndBuildFreeTextAnnotation(PdfAnnotationBorderEffectType.Cloudy);
                        break;

                    case AnnotationType.FileAttachment:
                        AddAndBuildFileAttachmentAnnotation(_previousFileAttachmentIconName);
                        break;

                    case AnnotationType.GraphFileAttachment:
                        AddAndBuildFileAttachmentAnnotation("Graph");
                        break;

                    case AnnotationType.PushPinFileAttachment:
                        AddAndBuildFileAttachmentAnnotation("PushPin");
                        break;

                    case AnnotationType.PaperclipFileAttachment:
                        AddAndBuildFileAttachmentAnnotation("Paperclip");
                        break;

                    case AnnotationType.TagFileAttachment:
                        AddAndBuildFileAttachmentAnnotation("Tag");
                        break;

                    case AnnotationType.Text_Comment:
                        AddAndBuildTextAnnotation("Comment");
                        break;

                    case AnnotationType.Text_Check:
                        AddAndBuildTextAnnotation("Check");
                        break;

                    case AnnotationType.Text_Checkmark:
                        AddAndBuildTextAnnotation("Checkmark");
                        break;

                    case AnnotationType.Text_Circle:
                        AddAndBuildTextAnnotation("Circle");
                        break;

                    case AnnotationType.Text_Rectangle:
                        AddAndBuildTextAnnotation("Rectangle");
                        break;

                    case AnnotationType.Text_Cross:
                        AddAndBuildTextAnnotation("Cross");
                        break;

                    case AnnotationType.Text_CrossHairs:
                        AddAndBuildTextAnnotation("CrossHairs");
                        break;

                    case AnnotationType.Text_Help:
                        AddAndBuildTextAnnotation("Help");
                        break;

                    case AnnotationType.Text_Insert:
                        AddAndBuildTextAnnotation("Insert");
                        break;

                    case AnnotationType.Text_Key:
                        AddAndBuildTextAnnotation("Key");
                        break;

                    case AnnotationType.Text_NewParagraph:
                        AddAndBuildTextAnnotation("NewParagraph");
                        break;

                    case AnnotationType.Text_Note:
                        AddAndBuildTextAnnotation("Note");
                        break;

                    case AnnotationType.Text_Paragraph:
                        AddAndBuildTextAnnotation("Paragraph");
                        break;

                    case AnnotationType.Text_RightArrow:
                        AddAndBuildTextAnnotation("RightArrow");
                        break;

                    case AnnotationType.Text_RightPointer:
                        AddAndBuildTextAnnotation("RightPointer");
                        break;

                    case AnnotationType.Text_Star:
                        AddAndBuildTextAnnotation("Star");
                        break;

                    case AnnotationType.Text_UpArrow:
                        AddAndBuildTextAnnotation("UpArrow");
                        break;

                    case AnnotationType.Text_UpLeftArrow:
                        AddAndBuildTextAnnotation("UpLeftArrow");
                        break;

                    case AnnotationType.EmptyOfficeDocument:
                        AddAndBuildOfficeDocumentAnnotation(DemosResourcesManager.GetResourceAsStream("EmptyDocument.docx"));
                        break;

                    case AnnotationType.OfficeDocument:
#if !REMOVE_OFFICE_PLUGIN
                        Stream officeDocumentStream = OfficeDemosTools.SelectOfficeDocument();
                        if (officeDocumentStream != null)
                            AddAndBuildOfficeDocumentAnnotation(officeDocumentStream);
#endif
                        break;

                    case AnnotationType.Chart:
                        AddAndBuildChartAnnotation();
                        break;
                }
            }
            catch (Exception e)
            {
                DemosTools.ShowErrorMessage(e.Message);
            }
        }

        /// <summary>
        /// Adds and builds an annotation.
        /// </summary>
        /// <param name="annotationView">The PDF annotation view.</param>
        /// <exception cref="InvalidOperationException">Thrown if annotation tool is empty.</exception>
        private void AddAndBuildAnnotation(WpfPdfAnnotationView annotationView)
        {
            // if annotation tool is empty
            if (_annotationTool == null)
                throw new InvalidOperationException();

            annotationView.Annotation.Modified = DateTime.Now;
            annotationView.Annotation.Title = Environment.UserName;

            SetInteractionMode(annotationView.Annotation);

            // if focused annotation view is not empty
            if (_annotationTool.FocusedAnnotationView != null)
            {
                // if focused annotation view is building
                if (_annotationTool.FocusedAnnotationView.IsBuilding)
                {
                    // if focused annotation view building is started
                    if (_annotationTool.FocusedAnnotationView.IsBuildingStarted)
                    {
                        // finish interaction
                        _annotationTool.FinishInteraction();
                    }
                    // if focused annotation view building is not started
                    else
                    {
                        // cancel building annotation
                        _annotationTool.CancelBuilding();
                    }

                    // if focused annotation view is ink annotation view
                    if (_annotationTool.FocusedAnnotationView is WpfPdfInkAnnotationView)
                        return;
                }
            }

            // if the unique name for the annotation must be created
            if (_annotationTool.InteractiveFormEditorManager.UseUniqueAnnotationName)
            {
                // get the unique name
                annotationView.Annotation.Name = _annotationTool.InteractiveFormEditorManager.GetUniqueAnnotationName(
                    _annotationTool.AnnotationCollection, annotationView.Annotation);
            }

            // build the annotation
            _annotationTool.AddAndBuildAnnotation(annotationView);
        }

        /// <summary>
        /// Adds and builds an annotation.
        /// </summary>
        /// <param name="annotation">The PDF annotation.</param>
        /// <param name="appearanceGenerator">The PDF annotation appearance generator.</param>
        /// <returns>PDF annotation view.</returns>
        /// <exception cref="InvalidOperationException">Thrown if annotation tool is empty.</exception>
        private WpfPdfAnnotationView AddAndBuildAnnotation(PdfAnnotation annotation)
        {
            // if annotation tool is empty
            if (_annotationTool == null)
                throw new InvalidOperationException();

            annotation.Modified = DateTime.Now;
            annotation.Title = Environment.UserName;

            SetInteractionMode(annotation);

            // if focused annotation view is not empty
            if (_annotationTool.FocusedAnnotationView != null)
            {
                // if focused annotation view is building
                if (_annotationTool.FocusedAnnotationView.IsBuilding)
                {
                    // if focused annotation view building is started
                    if (_annotationTool.FocusedAnnotationView.IsBuildingStarted)
                    {
                        // finish interaction
                        _annotationTool.FinishInteraction();
                    }
                    // if focused annotation view building is not started
                    else
                    {
                        // cancel building of annotation
                        _annotationTool.CancelBuilding();
                    }
                }
            }

            // if the unique name for the annotation must be created
            if (_annotationTool.InteractiveFormEditorManager.UseUniqueAnnotationName)
            {
                // get the unique name
                annotation.Name = _annotationTool.InteractiveFormEditorManager.GetUniqueAnnotationName(
                    _annotationTool.AnnotationCollection, annotation);
            }

            try
            {
                // build the annotation
                return _annotationTool.AddAndBuildAnnotation(annotation);
            }
            catch (Exception e)
            {
                DemosTools.ShowErrorMessage(e.Message);
                return null;
            }
        }


        /// <summary>
        /// Adds and builds a line annotation.
        /// </summary>
        /// <param name="startPointLineEndingStyle">The line ending style of start point.</param>
        /// <param name="endPointLineEndingStyle">The line ending style of end point.</param>
        private void AddAndBuildLineAnnotation(
            PdfAnnotationLineEndingStyle startPointLineEndingStyle,
            PdfAnnotationLineEndingStyle endPointLineEndingStyle)
        {
            // create new line annotation
            PdfLineAnnotation line = new PdfLineAnnotation(AnnotationTool.FocusedPage);
            // set the annotation rectangle
            line.Rectangle = GetNewAnnotationRectangle();
            // set the annotation color
            line.Color = System.Drawing.Color.Black;
            // set the annotation border width
            line.BorderWidth = 3;
            if (startPointLineEndingStyle != PdfAnnotationLineEndingStyle.None)
            {
                // set start point line ending style
                line.StartPointLineEndingStyle = startPointLineEndingStyle;
            }
            if (endPointLineEndingStyle != PdfAnnotationLineEndingStyle.None)
            {
                // set end point line ending style
                line.EndPointLineEndingStyle = endPointLineEndingStyle;
            }

            // build the line annotation
            AddAndBuildAnnotation(line);
        }

        /// <summary>
        /// Adds and builds a polyline annotation.
        /// </summary>
        /// <param name="startPointLineEndingStyle">The line ending style of start point.</param>
        /// <param name="endPointLineEndingStyle">The line ending style of end point.</param>
        private void AddAndBuildPolylineAnnotation(
            PdfAnnotationLineEndingStyle startPointLineEndingStyle,
            PdfAnnotationLineEndingStyle endPointLineEndingStyle)
        {
            // create new polyline annotation
            PdfPolylineAnnotation polyline = new PdfPolylineAnnotation(AnnotationTool.FocusedPage);
            // set the annotation rectangle
            polyline.Rectangle = GetNewAnnotationRectangle();
            // set the annotation color
            polyline.Color = System.Drawing.Color.Black;
            // set the annotation border width
            polyline.BorderWidth = 3;
            if (startPointLineEndingStyle != PdfAnnotationLineEndingStyle.None)
            {
                // set start point line ending style
                polyline.StartPointLineEndingStyle = startPointLineEndingStyle;
            }
            if (endPointLineEndingStyle != PdfAnnotationLineEndingStyle.None)
            {
                // set end point line ending style
                polyline.EndPointLineEndingStyle = endPointLineEndingStyle;
            }

            // build the polyline annotation
            AddAndBuildAnnotation(polyline);
        }

        /// <summary>
        /// Adds and builds a freehand polyline annotation.
        /// </summary>
        private void AddAndBuildFreehandPolylineAnnotation()
        {
            // create new polyline annotation
            PdfPolylineAnnotation polyline = new PdfPolylineAnnotation(AnnotationTool.FocusedPage);
            // set the annotation rectangle
            polyline.Rectangle = GetNewAnnotationRectangle();
            // set the annotation color
            polyline.Color = System.Drawing.Color.Red;
            // set the annotation border width
            polyline.BorderWidth = 3;
            // create new polyline annotation view
            WpfPdfPolygonalAnnotationView view = new WpfPdfPolylineAnnotationView(polyline, AnnotationTool.FocusedJsDoc);

            // create new freehand builder
            WpfPointBasedObjectFreehandBuilder builder = new WpfPointBasedObjectFreehandBuilder(view, 2, 0.1f);
            // 0 - MouseUp, 1 - MouseClick, 2 - MouseDoubleClick
            builder.FinishBuildingMouseClickCount = 2;
            // set the freehand builder to the polyline annotation view
            view.Builder = builder;

            // build the polyline annotation
            AddAndBuildAnnotation(view);
        }

        /// <summary>
        /// Adds and builds an Ink annotation.
        /// </summary>
        private void AddAndBuildInkAnnotation()
        {
            // create new ink annotation
            PdfInkAnnotation ink = new PdfInkAnnotation(AnnotationTool.FocusedPage);
            // set the annotation rectangle
            ink.Rectangle = GetNewAnnotationRectangle();
            // set the annotation color
            ink.Color = System.Drawing.Color.Red;
            // set the annotation border width
            ink.BorderWidth = 2;
            // create new ink annotation view
            WpfPdfInkAnnotationView inkView = new WpfPdfInkAnnotationView(ink, AnnotationTool.FocusedJsDoc);

            // get a minimum line border width
            float accuracy = ink.BorderWidth;
            // create new annotation builder
            WpfPdfInkAnnotationBuilder builder = new WpfPdfInkAnnotationBuilder(inkView, accuracy);
            // set the annotation builder to the ink annotation view
            inkView.Builder = builder;

            // build the ink annotation
            AddAndBuildAnnotation(inkView);
        }

        /// <summary>
        /// Adds and builds a office document annotation.
        /// </summary>
        /// <param name="documentStream">The document stream.</param>
        public PdfOfficeDocumentAnnotation AddAndBuildOfficeDocumentAnnotation(Stream documentStream)
        {
            // create new office doeument annotation
            PdfOfficeDocumentAnnotation officeAnnotation = new PdfOfficeDocumentAnnotation(AnnotationTool.FocusedPage);
            officeAnnotation.CreationDate = DateTime.Now;

            // set office document stream
            officeAnnotation.SetDocumentStream(documentStream);
            documentStream.Dispose();

            // set the annotation rectangle
            officeAnnotation.Rectangle = GetNewAnnotationRectangle();

            // build the comment annotation
            AddAndBuildAnnotation(officeAnnotation);
            return officeAnnotation;
        }

        /// <summary>
        /// Adds and builds a chart annotation.
        /// </summary>
        public PdfOfficeDocumentAnnotation AddAndBuildChartAnnotation()
        {
            // select chart
            Stream documentStream = OfficeDemosTools.SelectChartResource();
            if (documentStream != null)
            {
                // create new office doeument annotation
                PdfOfficeDocumentAnnotation officeAnnotation = new PdfOfficeDocumentAnnotation(AnnotationTool.FocusedPage);
                officeAnnotation.CreationDate = DateTime.Now;

                // set office document stream
                officeAnnotation.SetDocumentStream(documentStream);
                documentStream.Dispose();

                // enable use relative size instread specified size of graphics object
                officeAnnotation.UseGraphicObjectRelativeSize = true;

                // set the annotation rectangle
                officeAnnotation.Rectangle = GetNewAnnotationRectangle();

                // build the comment annotation
                AddAndBuildAnnotation(officeAnnotation);
                return officeAnnotation;
            }
            return null;
        }

        /// <summary>
        /// Adds and builds a freehand polygon annotation.
        /// </summary>
        /// <param name="interiorColor">The background color of polygon annotation.</param>
        private void AddAndBuildFreehandPolygonAnnotation(
            System.Drawing.Color interiorColor)
        {
            // create new polygon annotation
            PdfPolygonAnnotation polygon = new PdfPolygonAnnotation(AnnotationTool.FocusedPage);
            // set the annotation rectangle
            polygon.Rectangle = GetNewAnnotationRectangle();
            // set the annotation color
            polygon.Color = System.Drawing.Color.Red;
            // set the annotation background color
            polygon.InteriorColor = interiorColor;
            // set the annotation border width
            polygon.BorderWidth = 3;
            // create new polygon annotation view
            WpfPdfPolygonalAnnotationView view = new WpfPdfPolygonAnnotationView(polygon, AnnotationTool.FocusedJsDoc);

            // create and set new freehand builder
            view.Builder = new WpfPointBasedObjectFreehandBuilder(view, 2, 0.1f);

            // build the polygon annotation
            AddAndBuildAnnotation(view);
        }

        /// <summary>
        /// Adds and builds a polygon annotation.
        /// </summary>
        /// <param name="borderEffect">The border effect of polygon annotation.</param>
        /// <param name="interiorColor">The background color of polygon annotation.</param>
        /// <param name="borderWidth">The border width of circle annotation.</param>
        private void AddAndBuildPolygonAnnotation(
            PdfAnnotationBorderEffectType borderEffect,
            System.Drawing.Color interiorColor,
            int borderWidth)
        {
            // create new polygon annotation
            PdfPolygonAnnotation polygon = new PdfPolygonAnnotation(AnnotationTool.FocusedPage);
            // set the annotation rectangle
            polygon.Rectangle = GetNewAnnotationRectangle();
            // set the annotation color
            polygon.Color = System.Drawing.Color.Black;
            // set the annotation background color
            polygon.InteriorColor = interiorColor;
            // set the annotation border width
            polygon.BorderWidth = borderWidth;
            // set the annotation border effect
            polygon.BorderEffect = borderEffect;
            polygon.BorderEffectIntensity = 2f;

            // build the polygon annotation
            AddAndBuildAnnotation(polygon);
        }

        /// <summary>
        /// Adds and builds a circle annotation.
        /// </summary>
        /// <param name="borderEffect">The border effect of circle annotation.</param>
        /// <param name="interiorColor">The background color of circle annotation.</param>
        /// <param name="borderWidth">The border width of circle annotation.</param>
        private void AddAndBuildCircleAnnotation(
            PdfAnnotationBorderEffectType borderEffect,
            System.Drawing.Color interiorColor,
            int borderWidth)
        {
            // create new circle annotation
            PdfCircleAnnotation circle = new PdfCircleAnnotation(AnnotationTool.FocusedPage);
            // set the annotation rectangle
            circle.Rectangle = GetNewAnnotationRectangle();
            // set the annotation color
            circle.Color = System.Drawing.Color.Black;
            // set the annotation border effect
            circle.BorderEffect = borderEffect;
            circle.BorderEffectIntensity = 2f;
            // set the annotation background color
            circle.InteriorColor = interiorColor;
            // set the annotation border width
            circle.BorderWidth = borderWidth;

            // build the polygon annotation
            AddAndBuildAnnotation(circle);
        }

        /// <summary>
        /// Adds and builds a square annotation.
        /// </summary>
        /// <param name="borderEffect">The border effect of square annotation.</param>
        /// <param name="interiorColor">The background color of square annotation.</param>
        /// <param name="borderWidth">The border width of circle annotation.</param>
        private void AddAndBuildSquareAnnotation(
            PdfAnnotationBorderEffectType borderEffect,
            System.Drawing.Color interiorColor,
            int borderWidth)
        {
            // create new square annotation
            PdfSquareAnnotation square = new PdfSquareAnnotation(AnnotationTool.FocusedPage);
            // set the annotation rectangle
            square.Rectangle = GetNewAnnotationRectangle();
            // set the annotation color
            square.Color = System.Drawing.Color.Black;
            // set the annotation background color
            square.InteriorColor = interiorColor;
            // set the annotation border width
            square.BorderWidth = borderWidth;
            // set the annotation border effect
            square.BorderEffect = borderEffect;
            square.BorderEffectIntensity = 2f;

            // build the square annotation
            AddAndBuildAnnotation(square);

            // set default size of building annotation
            WpfPointBasedObjectLineBuilder builder = AnnotationTool.FocusedAnnotationView.Builder as WpfPointBasedObjectLineBuilder;
            if (builder != null)
                builder.DefaultSize = new Size(200, 100);
        }

        /// <summary>
        /// Adds and builds a file attachment annotation.
        /// </summary>
        /// <param name="iconName">The icon of file attachment annotation.</param>
        private void AddAndBuildFileAttachmentAnnotation(string iconName)
        {
            // create new file attachment annotation
            PdfFileAttachmentAnnotation fileAttachment = new PdfFileAttachmentAnnotation(AnnotationTool.FocusedPage);
            // set the annotation color
            fileAttachment.Color = System.Drawing.Color.Yellow;
            // set the annotation rectangle
            fileAttachment.Rectangle = GetNewAnnotationRectangle();
            // if icon name is not empty
            if (!string.IsNullOrEmpty(iconName))
            {
                // set the icon name
                fileAttachment.IconName = iconName;
                _previousFileAttachmentIconName = iconName;
            }

            // build the file attachment annotation
            AddAndBuildAnnotation(fileAttachment);
        }

        /// <summary>
        /// Adds and builds a text annotation.
        /// </summary>
        /// <param name="stickName">The stick name.</param>
        public PdfTextAnnotation AddAndBuildTextAnnotation(string stickName)
        {
            // create new file attachment annotation
            PdfTextAnnotation commentAnnotation = new PdfTextAnnotation(AnnotationTool.FocusedPage);
            commentAnnotation.CreationDate = DateTime.Now;
            commentAnnotation.StickName = stickName;
            // set the annotation color
            if (stickName == "Checkmark")
                commentAnnotation.Color = System.Drawing.Color.FromArgb(64, 64, 64);
            else
                commentAnnotation.Color = System.Drawing.Color.Yellow;
            // set the annotation rectangle
            commentAnnotation.Rectangle = GetNewAnnotationRectangle();
            // build the comment annotation
            AddAndBuildAnnotation(commentAnnotation);
            return commentAnnotation;
        }

        /// <summary>
        /// Adds and builds a link annotation.
        /// </summary>
        private void AddAndBuildLinkAnnotation()
        {
            // create new link annotation
            PdfLinkAnnotation link = new PdfLinkAnnotation(AnnotationTool.FocusedPage);
            // set the annotation rectangle
            link.Rectangle = GetNewAnnotationRectangle();
            // create new link annotation view
            WpfPdfAnnotationView view = AddAndBuildAnnotation(link);
            // if annotation tool interaction mode is edit
            if (AnnotationTool.InteractionMode != WpfPdfAnnotationInteractionMode.Edit)
            {
                // highlight link annotation
                ((WpfPdfLinkAnnotationView)view).IsHighlighted = true;
            }
        }

        /// <summary>
        /// Adds and builds a text annotation.
        /// </summary>
        /// <param name="borderEffect">The border effect of text annotation.</param>
        private void AddAndBuildTextAnnotation(
            PdfAnnotationBorderEffectType borderEffect)
        {
            // create new free text annotation
            PdfFreeTextAnnotation text = new PdfFreeTextAnnotation(AnnotationTool.FocusedPage);
            // set the annotation rectangle
            text.Rectangle = PdfAnnotationsTools.GetNewAnnotationRectangle(AnnotationTool, _visualToolMouseObserver,
                TEXT_FONT_DEFAULT_SIZE * 6f, TEXT_FONT_DEFAULT_SIZE * 1.5f);
            // set the annotation color
            text.Color = System.Drawing.Color.White;
            // set the font of the annotation
            PdfFont font = text.Document.FontManager.GetStandardFont(PdfStandardFontType.TimesRoman);
            text.SetTextDefaultAppearance(font, TEXT_FONT_DEFAULT_SIZE, System.Drawing.Color.Black);
            // set the annotation border width
            text.BorderWidth = 1;
            // set the annotation border effect
            text.BorderEffect = borderEffect;
            text.BorderEffectIntensity = 2f;

            // build the free text annotation
            AddAndBuildAnnotation(text);
        }

        /// <summary>
        /// Adds and builds a label annotation.
        /// </summary>
        private void AddAndBuildLabelAnnotation()
        {
            // create new free text annotation
            PdfFreeTextAnnotation label = new PdfFreeTextAnnotation(AnnotationTool.FocusedPage);
            // set the annotation rectangle
            label.Rectangle = PdfAnnotationsTools.GetNewAnnotationRectangle(AnnotationTool, _visualToolMouseObserver,
                TEXT_FONT_DEFAULT_SIZE * 6f, TEXT_FONT_DEFAULT_SIZE * 1.5f);
            // set the content of the annotation
            label.Contents = "Label";
            // set the font of the annotation
            PdfFont font = label.Document.FontManager.GetStandardFont(PdfStandardFontType.TimesRoman);
            label.SetTextDefaultAppearance(font, TEXT_FONT_DEFAULT_SIZE, System.Drawing.Color.Black);
            // set the annotation border width
            label.BorderWidth = 0;
            label.IsLocked = true;

            // build the free text annotation
            AddAndBuildAnnotation(label);
        }

        /// <summary>
        /// Adds and builds a free text annotation.
        /// </summary>
        /// <param name="borderEffect">The border effect of free text annotation.</param>
        private void AddAndBuildFreeTextAnnotation(
            PdfAnnotationBorderEffectType borderEffect)
        {
            // create new free text annotation
            PdfFreeTextAnnotation freeText = new PdfFreeTextAnnotation(AnnotationTool.FocusedPage);
            freeText.LineEndingStyle = PdfAnnotationLineEndingStyle.OpenArrow;
            // set the annotation rectangle
            freeText.Rectangle = GetNewAnnotationRectangle();
            // set the annotation color
            freeText.Color = System.Drawing.Color.White;
            // set the font of the annotation
            PdfFont font = freeText.Document.FontManager.GetStandardFont(PdfStandardFontType.TimesRoman);
            freeText.SetTextDefaultAppearance(font, TEXT_FONT_DEFAULT_SIZE, System.Drawing.Color.Black);
            // set the annotation border width
            freeText.BorderWidth = 1;
            // set the annotation border effect
            freeText.BorderEffect = borderEffect;
            freeText.BorderEffectIntensity = 2f;
            // set the annotation rectangle
            freeText.Rectangle = new System.Drawing.RectangleF(0, 0, 100, 100);
            // set the annotation text padding
            freeText.TextPadding = new Vintasoft.Imaging.PaddingF(0, 100 - TEXT_FONT_DEFAULT_SIZE * 1.5f, 0, 0);
            // set the annotation callout line
            freeText.CalloutLinePoints = new System.Drawing.PointF[3] {
                new System.Drawing.PointF(50, 100),
                new System.Drawing.PointF(50, 50),
                new System.Drawing.PointF(50, TEXT_FONT_DEFAULT_SIZE * 1.5f) };

            // build the free text annotation
            AddAndBuildAnnotation(freeText);
        }

        #endregion


        /// <summary>
        /// Ends the annotation building.
        /// </summary>
        private void EndBuilding()
        {
            _lastBuiltAnnotationType = AnnotationType.Unknown;
            SetSelectedAnnotationButton(null);
        }

        /// <summary>
        /// Determines whether the specified view is not widget annotation.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns>
        /// <b>true</b> if the annotation view is not widget annotation; otherwise, <b>false</b>.
        /// </returns>
        private bool IsNotPdfWidgetAnnotation(WpfPdfAnnotationView view)
        {
            if (view is WpfPdfWidgetAnnotationView)
                return false;

            return true;
        }

        /// <summary>
        /// Sets embedded file to the PDF file attachment annotation.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        /// <param name="fileAttachmentAnnotation">The PDF file attachment annotation.</param>
        private void SetEmbeddedFile(PdfDocument document, PdfFileAttachmentAnnotation fileAttachmentAnnotation)
        {
            // create embedded file specification window
            EmbeddedFileSpecificationWindow embeddedFileSpecificationWindow = new EmbeddedFileSpecificationWindow();
            embeddedFileSpecificationWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            embeddedFileSpecificationWindow.Owner = Window.GetWindow(this);
            // get embedded file specification
            PdfEmbeddedFileSpecification fileSpecification = fileAttachmentAnnotation.FileSpecification;
            // if specification is not empty
            if (fileSpecification == null)
            {
                // create new embedded file specification
                fileSpecification = new PdfEmbeddedFileSpecification(document);
            }
            // set embedded file specification to the window
            embeddedFileSpecificationWindow.EmbeddedFileSpecification = fileSpecification;
            // if dialog result is true
            if (embeddedFileSpecificationWindow.ShowDialog() == true)
            {
                // set embedded file specification to the file attachment annotation
                fileAttachmentAnnotation.FileSpecification = fileSpecification;
            }
        }

        /// <summary>
        /// Returns the rectangle for new annotation specified using
        /// the appearance generator key (builder button).
        /// </summary>
        /// <param name="appearanceGeneratorKey">The appearance generator key.</param>
        private System.Drawing.RectangleF GetNewAnnotationRectangle()
        {
            return PdfAnnotationsTools.GetNewAnnotationRectangle(AnnotationTool, _visualToolMouseObserver, 20, 20);
        }

        /// <summary>
        /// Sets the interaction mode of PDF annotation tool.
        /// </summary>
        /// <param name="annotation">The PDF annotation.</param>
        private void SetInteractionMode(PdfAnnotation annotation)
        {
            _isInteractionModeChanging = true;

            switch (_annotationTool.InteractionMode)
            {
                case WpfPdfAnnotationInteractionMode.None:
                case WpfPdfAnnotationInteractionMode.View:
                    if (annotation is PdfMarkupAnnotation)
                        _annotationTool.InteractionMode = WpfPdfAnnotationInteractionMode.Markup;
                    else
                        _annotationTool.InteractionMode = WpfPdfAnnotationInteractionMode.Edit;
                    break;
                case WpfPdfAnnotationInteractionMode.Markup:
                    if (!(annotation is PdfMarkupAnnotation))
                        _annotationTool.InteractionMode = WpfPdfAnnotationInteractionMode.Edit;
                    break;
            }

            _isInteractionModeChanging = false;
        }

        #endregion

        #endregion

    }
}
