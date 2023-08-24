using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Win32;
using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Wpf.UI.Annotations;
using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit PDF annotations.
    /// </summary>
    public partial class PdfAnnotationsControl : UserControl
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAnnotationsControl"/> class.
        /// </summary>
        public PdfAnnotationsControl()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        WpfPdfAnnotationTool _annotationTool = null;
        /// <summary>
        /// Gets or sets the PDF annotation tool.
        /// </summary>
        public WpfPdfAnnotationTool AnnotationTool
        {
            get
            {
                return _annotationTool;
            }
            set
            {
                // if annotation tool is not empty
                if (_annotationTool != null)
                    UnsubscribeFromAnnotationTool(_annotationTool);

                // set new annotation tool
                _annotationTool = value;
                AnnotationBuilderControl.AnnotationTool = value;
                pdfAnnotationToolAnnotationsControl.AnnotationTool = value;

                // if annotation tool is not empty
                if (_annotationTool != null)
                    SubscribeToAnnotationTool(_annotationTool);

                // update user interface
                UpdateUI();
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Updates the User Interface.
        /// </summary>
        public void UpdateUI()
        {
            mainPanel.IsEnabled = _annotationTool != null && _annotationTool.ImageViewer != null;

            AnnotationBuilderControl.UpdateUI();
        }

        /// <summary>
        /// Updates the annotation.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        public void UpdateAnnotation(PdfAnnotation annotation)
        {
            pdfAnnotationToolAnnotationsControl.UpdateAnnotation(annotation);
        }

        /// <summary>
        /// Refresh the annotation list.
        /// </summary>
        public void RefreshAnnotationList()
        {
            pdfAnnotationToolAnnotationsControl.RefreshAnnotationList();
            if (AnnotationTool != null)
            {
                WpfPdfAnnotationView focusedAnnotationView = AnnotationTool.FocusedAnnotationView;
                if (focusedAnnotationView != null)
                {
                    PdfAnnotation focusedAnnotation = focusedAnnotationView.Annotation;
                    pdfAnnotationToolAnnotationsControl.SelectedAnnotation = focusedAnnotation;
                }
            }
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Subscribes to the annotation tool events.
        /// </summary>
        /// <param name="annotationTool">The annotation tool.</param>
        private void SubscribeToAnnotationTool(WpfPdfAnnotationTool annotationTool)
        {
            annotationTool.Activated += new EventHandler(annotationTool_Activated);
            annotationTool.Deactivated += new EventHandler(annotationTool_Deactivated);
            annotationTool.MouseDoubleClick += new MouseButtonEventHandler(annotationTool_MouseDoubleClick);
            annotationTool.ActiveInteractionControllerChanged += new PropertyChangedEventHandler<IWpfInteractionController>(AnnotationTool_ActiveInteractionControllerChanged);
            annotationTool.BuildingStarted += new EventHandler<WpfPdfAnnotationViewEventArgs>(annotationTool_BuildingStarted);
            annotationTool.BuildingFinished += new EventHandler<WpfPdfAnnotationViewEventArgs>(annotationTool_BuildingFinished);
            annotationTool.BuildingCanceled += new EventHandler<WpfPdfAnnotationViewEventArgs>(annotationTool_BuildingCanceled);
        }

        /// <summary>
        /// Unsubscribes from the annotation tool events.
        /// </summary>
        /// <param name="annotationTool">The annotation tool.</param>
        private void UnsubscribeFromAnnotationTool(WpfPdfAnnotationTool annotationTool)
        {
            annotationTool.Activated -= annotationTool_Activated;
            annotationTool.Deactivated -= annotationTool_Deactivated;
            annotationTool.MouseDoubleClick -= annotationTool_MouseDoubleClick;
            annotationTool.ActiveInteractionControllerChanged -= new PropertyChangedEventHandler<IWpfInteractionController>(AnnotationTool_ActiveInteractionControllerChanged);
            annotationTool.BuildingStarted -= annotationTool_BuildingStarted;
            annotationTool.BuildingFinished -= annotationTool_BuildingFinished;
            annotationTool.BuildingCanceled -= annotationTool_BuildingCanceled;
        }

        /// <summary>
        /// The annotaiton building is finished.
        /// </summary>
        private void annotationTool_BuildingFinished(object sender, WpfPdfAnnotationViewEventArgs e)
        {
            if (!AnnotationTool.IsFocusedAnnotationBuilding)
                pdfAnnotationToolAnnotationsControl.IsEnabled = true;
        }

        /// <summary>
        /// The annotation building is started.
        /// </summary>
        private void annotationTool_BuildingStarted(object sender, WpfPdfAnnotationViewEventArgs e)
        {
            pdfAnnotationToolAnnotationsControl.IsEnabled = false;
        }

        /// <summary>
        /// The annotation building is canceled.
        /// </summary>
        private void annotationTool_BuildingCanceled(object sender, WpfPdfAnnotationViewEventArgs e)
        {
            pdfAnnotationToolAnnotationsControl.IsEnabled = true;
        }

        /// <summary>
        /// Handles the ActiveInteractionControllerChanged event of the PdfAnnotationTool.
        /// </summary>
        private void AnnotationTool_ActiveInteractionControllerChanged(object sender, PropertyChangedEventArgs<IWpfInteractionController> e)
        {
            if (e.OldValue != null)
                e.OldValue.InteractionFinished -= new EventHandler<WpfInteractionEventArgs>(ActiveInteractionController_InteractionFinished);
            if (e.NewValue != null)
                e.NewValue.InteractionFinished += new EventHandler<WpfInteractionEventArgs>(ActiveInteractionController_InteractionFinished);
        }

        /// <summary>
        /// Handles the InteractionFinished event of the ActiveInteractionController.
        /// </summary>
        private void ActiveInteractionController_InteractionFinished(object sender, WpfInteractionEventArgs e)
        {
            if (_annotationTool.FocusedAnnotationView != null)
                UpdateAnnotation(_annotationTool.FocusedAnnotationView.Annotation);
            if (_annotationTool.InteractionMode == WpfPdfAnnotationInteractionMode.Edit)
                foreach (WpfPdfAnnotationView view in _annotationTool.SelectedAnnotationViewCollection)
                    UpdateAnnotation(view.Annotation);
        }

        /// <summary>
        /// Annotation tool is activated.
        /// </summary>
        private void annotationTool_Activated(object sender, EventArgs e)
        {
            mainPanel.IsEnabled = true;
        }

        /// <summary>
        /// Annotation tool is deactivated.
        /// </summary>
        private void annotationTool_Deactivated(object sender, EventArgs e)
        {
            mainPanel.IsEnabled = false;
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of the Annotation tool.
        /// </summary>
        private void annotationTool_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // if left mouse button is down
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // if tool uses View or Markup mode
                if (AnnotationTool.InteractionMode == WpfPdfAnnotationInteractionMode.View ||
                    AnnotationTool.InteractionMode == WpfPdfAnnotationInteractionMode.Markup)
                {
                    Point point = e.GetPosition(AnnotationTool);
                    // get focused annotation view
                    WpfPdfAnnotationView focusedView = AnnotationTool.FindAnnotationView(point);
                    // if focused annotation view is File Attachment Annotation
                    if (focusedView is WpfPdfFileAttachmentAnnotationView)
                    {
                        // get the annotation
                        PdfFileAttachmentAnnotation annotation = (PdfFileAttachmentAnnotation)focusedView.Annotation;

                        // if file specification is not empty
                        // and embedded file is not empty
                        if (annotation.FileSpecification != null && annotation.FileSpecification.EmbeddedFile != null)
                        {
                            OpenOrSaveFileAttachment(annotation);
                            e.Handled = true;
                        }
                        // if file specification is empty
                        // or embedded file is empty
                        else
                        {
                            SetEmbeddedFile(annotation);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets embedded file to the PDF file attachment annotation.
        /// </summary>
        /// <param name="fileAttachmentAnnotation">The PDF file attachment annotation.</param>
        private void SetEmbeddedFile(PdfFileAttachmentAnnotation annotation)
        {
            // create embedded file specification window
            EmbeddedFileSpecificationWindow embeddedFileSpecificationWindow = new EmbeddedFileSpecificationWindow();
            embeddedFileSpecificationWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            embeddedFileSpecificationWindow.Owner = Window.GetWindow(this);
            // get file specification
            PdfEmbeddedFileSpecification fileSpecification = annotation.FileSpecification;
            // if file specification is not empty
            if (fileSpecification == null)
            {
                // create new file specification
                fileSpecification = new PdfEmbeddedFileSpecification(annotation.Document);
            }
            // set file specification to the form
            embeddedFileSpecificationWindow.EmbeddedFileSpecification = fileSpecification;

            // if dialog result is true
            if (embeddedFileSpecificationWindow.ShowDialog() == true)
            {
                // set file specification to the file attachment annotation
                annotation.FileSpecification = fileSpecification;
            }
        }

        /// <summary>
        /// Opens or saves the embedded file from PDF file attachment annotation.
        /// </summary>
        /// <param name="annotation">The PDF file attachment annotation.</param>
        private static void OpenOrSaveFileAttachment(PdfFileAttachmentAnnotation annotation)
        {
            try
            {
                // get embedded file name
                string filename = Path.GetFileName(annotation.FileSpecification.Filename);

                // get description
                string description = annotation.Contents;
                if (string.IsNullOrEmpty(description))
                    description = annotation.FileSpecification.Description;
                if (string.IsNullOrEmpty(description))
                    description = Path.GetExtension(filename);

                // get message for message box
                string message =
                    "Open file '{0}' ({1}) using the default application, or save file to a disk?\n" +
                    "Press 'Yes' to open file using the default application.\n" +
                    "Press 'No' to save file to a disk.\n" +
                    "Press 'Cancel' to cancel this action.";
                // show message box
                MessageBoxResult result = MessageBox.Show(string.Format(message, filename, description),
                    "Embedded File", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                // if dialog result is YES
                if (result == MessageBoxResult.Yes)
                {
                    // get temp file name
                    string tempFilename = Path.GetTempFileName();
                    tempFilename = Path.Combine(Path.GetDirectoryName(tempFilename),
                        Path.GetFileNameWithoutExtension(tempFilename) + Path.GetExtension(filename));
                    // save embedded file to the temp file
                    File.WriteAllBytes(tempFilename, annotation.FileSpecification.EmbeddedFile.GetBytes());

                    // open temp file
                    ProcessStartInfo processInfo = new ProcessStartInfo(tempFilename);
                    processInfo.UseShellExecute = true;
                    Process.Start(processInfo);
                }
                // if dialog result is NO
                else if (result == MessageBoxResult.No)
                {
                    // create save file dialog
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    // set filename
                    saveDialog.FileName = filename;
                    // set default extension
                    saveDialog.DefaultExt = Path.GetExtension(filename);
                    // if dialog result is true
                    if (saveDialog.ShowDialog() == true)
                    {
                        // save embedded file to the new file
                        File.WriteAllBytes(saveDialog.FileName, annotation.FileSpecification.EmbeddedFile.GetBytes());
                    }
                }
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Annotation is double clicked.
        /// </summary>
        private void pdfAnnotationToolAnnotationsControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // if annotation is focused
            if (pdfAnnotationToolAnnotationsControl.SelectedAnnotation != null)
            {
                PdfAnnotation annotation = pdfAnnotationToolAnnotationsControl.SelectedAnnotation;
                WpfPdfAnnotationView view = AnnotationTool.GetAnnotationViewAssociatedWith(annotation);
                PdfAnnotationPropertiesWindow annotationProperties;
                // create annotation properties dialog
                if (view != null)
                    annotationProperties = new PdfAnnotationPropertiesWindow(view);
                else
                    annotationProperties = new PdfAnnotationPropertiesWindow(annotation);

                annotationProperties.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                annotationProperties.Owner = Window.GetWindow(this);
                annotationProperties.ShowDialog();
                pdfAnnotationToolAnnotationsControl.UpdateAnnotation(annotation);
            }
        }

        #endregion

        #endregion

    }
}
