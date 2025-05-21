using System;
using System.Collections.Generic;
using System.Windows;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Security;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;
using Vintasoft.Imaging.UI;
using Vintasoft.Imaging.UIActions;
using Vintasoft.Imaging.Wpf.UI.VisualTools;

using WpfDemosCommonCode.Imaging;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Contains common static functions for PDF demos.
    /// </summary>
    public class PdfDemosTools
    {

        #region Properties

        static bool _needGenerateInteractiveFormsAppearance = false;
        /// <summary>
        /// Gets or sets a value indicating whether
        /// the interactive form fields appearances must be generated.
        /// </summary>
        public static bool NeedGenerateInteractiveFormFieldsAppearance
        {
            get
            {
                return _needGenerateInteractiveFormsAppearance;
            }
            set
            {
                if (_needGenerateInteractiveFormsAppearance != value)
                {
                    _needGenerateInteractiveFormsAppearance = value;
                    if (value)
                        PdfDocumentController.DocumentOpened += new EventHandler<PdfDocumentEventArgs>(GenerateInteractiveFormFieldsAppearance);
                    else
                        PdfDocumentController.DocumentOpened -= GenerateInteractiveFormFieldsAppearance;
                }
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Adds the long-term validation (LTV) information to the PDF document.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        public static void AddLongTimeValidationInfo(PdfDocument document)
        {
            if (document.IsChanged)
            {
                DemosTools.ShowInfoMessage("PDF document is changed. First please sign and save document.");
                return;
            }
            try
            {
                int count = Vintasoft.Imaging.Pdf.Tree.DigitalSignatures.PdfDocumentLtv.AddLtvInfo(document);
                if (count == 0)
                    DemosTools.ShowInfoMessage("LTV information is not required for this document.");
                else
                    DemosTools.ShowInfoMessage("LTV information is added.");
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
                return;
            }
        }

        /// <summary>
        /// Returns a value indicating whether all specified images are contained in specified PDF document.
        /// </summary>
        /// <param name="images">An image collection.</param>
        /// <param name="document">PDF document.</param>
        /// <returns>
        /// A value indicating whether all specified images are contained in specified PDF document.</returns>
        public static bool CheckAllPagesFromDocument(ImageCollection images, PdfDocument document)
        {
            bool result = true;
            foreach (VintasoftImage image in images)
            {
                PdfPage page = PdfDocumentController.GetPageAssociatedWithImage(image);
                if (page == null)
                {
                    result = false;
                    break;
                }
                if (page.Document != document)
                {
                    result = false;
                    break;
                }
            }
            if (!result)
            {
                DemosTools.ShowWarningMessage("One or several pages are not saved in PDF document. Save document and try again.");
            }
            return result;
        }

        /// <summary>
        /// Determines whether image is valid.
        /// </summary>
        /// <param name="image">The image.</param>
        public static bool IsValidImage(VintasoftImage image)
        {
            if (image == null)
                return false;

            if (image.IsDisposed)
                return false;

            if (image.IsBad)
                return false;

            PdfPage page = PdfDocumentController.GetPageAssociatedWithImage(image);
            if (page != null && page.Document.AuthorizationResult == AuthorizationResult.IncorrectPassword)
                return false;

            return true;
        }

        /// <summary>
        /// Finds the image by page.
        /// </summary>
        /// <param name="page">Source PDF page.</param>
        /// <param name="images">Image collection, where image must be searched.</param>
        /// <returns>Image if image is found; otherwise, <b>null</b>.</returns>
        public static VintasoftImage FindImageByPage(PdfPage page, ImageCollection images)
        {
            if (page != null)
            {
                for (int i = 0; i < images.Count; i++)
                {
                    if (PdfDocumentController.GetPageAssociatedWithImage(images[i]) == page)
                        return images[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the UI action of current visual tool.
        /// </summary>
        public static T GetUIAction<T>(WpfVisualTool visualTool)
            where T : UIAction
        {
            IList<UIAction> actions = null;
            if (TryGetCurrentToolActions(visualTool, out actions))
            {
                foreach (UIAction action in actions)
                {
                    if (action is T)
                        return (T)action;
                }
            }
            return default(T);
        }

        /// <summary>
        /// Shows the document information in a Property Grid.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        /// <param name="suggestToCreateDocumentInformationDictionary">
        /// Indicates that applicatiom must suggest to create the document information dictionary
        /// if PDF document does not have the document information dictionary.
        /// </param>
        /// <param name="propertyValueChangedEventHandler">
        /// The PropertyValueChanged event handler of the PropertyGrid.
        /// </param>
        public static void ShowDocumentInformation(
            Window ownerWindow,
            PdfDocument document,
            bool suggestCreating,
            System.Windows.Forms.PropertyValueChangedEventHandler propertyValueChangedEventHandler)
        {
            if (!document.HasDocumentInformation)
            {
                if (suggestCreating)
                {
                    if (MessageBox.Show("PDF document does not have the Information Dictionary. Do you want to create the Information Dictionary?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return;
                }
                else
                {
                    DemosTools.ShowInfoMessage("PDF document does not have the Information Dictionary.");
                    return;
                }
            }

            try
            {
                PropertyGridWindow form = new PropertyGridWindow(document.DocumentInformation, "Document Information");
                form.Owner = ownerWindow;
                form.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (propertyValueChangedEventHandler != null)
                    form.PropertyGrid.PropertyValueChanged += propertyValueChangedEventHandler;
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Converts PDF document page layout mode to the image viewer display mode.
        /// </summary>
        /// <param name="pageLayoutMode">PDF document page layout mode.</param>
        /// <returns>Image viewer display mode.</returns>
        public static ImageViewerDisplayMode ConvertPageLayoutModeToImageViewerDisplayMode(PdfDocumentPageLayoutMode pageLayoutMode)
        {
            switch (pageLayoutMode)
            {
                case (PdfDocumentPageLayoutMode.OneColumn):
                    return ImageViewerDisplayMode.SingleContinuousColumn;
                case (PdfDocumentPageLayoutMode.SinglePage):
                    return ImageViewerDisplayMode.SinglePage;
                case (PdfDocumentPageLayoutMode.TwoColumnLeft):
                    return ImageViewerDisplayMode.TwoContinuousColumns;
                case (PdfDocumentPageLayoutMode.TwoColumnRight):
                    return ImageViewerDisplayMode.TwoContinuousColumns;
                case (PdfDocumentPageLayoutMode.TwoPageLeft):
                    return ImageViewerDisplayMode.TwoColumns;
                case (PdfDocumentPageLayoutMode.TwoPageRight):
                    return ImageViewerDisplayMode.TwoColumns;
                default:
                    return ImageViewerDisplayMode.SinglePage;
            }
        }

        /// <summary>
        /// Returns the name of the annotation.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        public static string GetAnnotationName(PdfAnnotation annotation)
        {
            string name = annotation.Name;

            // if annotation is widget annotation
            if (annotation is PdfWidgetAnnotation)
            {
                PdfWidgetAnnotation widgetAnnotation = (PdfWidgetAnnotation)annotation;
                // get interactive form field
                PdfInteractiveFormField field = widgetAnnotation.Field;

                // get name of annotation
                name = field.PartialName;
                // if name is empty
                if (string.IsNullOrEmpty(name))
                {
                    // if field is switchable button
                    if (field is PdfInteractiveFormSwitchableButtonField)
                        // get value of button
                        name = ((PdfInteractiveFormSwitchableButtonField)field).ButtonValue;
                }
            }

            return name;
        }

        /// <summary>
        /// Gets the annotation description.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <returns>The annotation description.</returns>
        public static string GetAnnotationDescription(PdfAnnotation annotation)
        {
            PdfWidgetAnnotation widgetAnnotation = annotation as PdfWidgetAnnotation;
            if (widgetAnnotation != null)
            {
                return string.Format("Form field: {0} ({1})",
                    widgetAnnotation.Field.FullyQualifiedName,
                    widgetAnnotation.Field.GetType().Name);
            }
            return string.Format("{0} annotation", annotation.AnnotationType);
        }

        /// <summary>
        /// Gets the parent window of specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The parent window, or null.</returns>
        public static Window GetParentWindow(DependencyObject element)
        {
            DependencyObject parent = LogicalTreeHelper.GetParent(element);
            if (parent == null)
                return null;

            if (parent is Window)
                return (Window)parent;

            return GetParentWindow(parent);
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Generates the interactive form fields appearance.
        /// </summary>
        private static void GenerateInteractiveFormFieldsAppearance(
            object sender,
            PdfDocumentEventArgs e)
        {
            e.Document.AutoUpdateInteractiveFormAppearances = NeedGenerateInteractiveFormFieldsAppearance;
        }

        /// <summary>
        /// Returns UI actions of the current tool, if they exist.
        /// </summary>
        /// <param name="actions">The list of actions supported by the current visual tool.</param>
        private static bool TryGetCurrentToolActions(WpfVisualTool visualTool, out IList<UIAction> actions)
        {
            actions = null;
            ISupportUIActions currentToolWithUIActions = visualTool as ISupportUIActions;
            if (currentToolWithUIActions != null)
                actions = currentToolWithUIActions.GetSupportedUIActions();

            return actions != null;
        }

        #endregion

        #endregion

    }
}
