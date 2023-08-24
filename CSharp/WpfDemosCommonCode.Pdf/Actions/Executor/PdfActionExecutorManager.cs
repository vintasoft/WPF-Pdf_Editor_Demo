using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Wpf.UI;
using Vintasoft.Imaging.Pdf.Wpf.UI.Annotations;
using Vintasoft.Imaging.Wpf.UI;

using WpfDemosCommonCode.Pdf.JavaScript;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Manager of Application Action Executors.
    /// </summary>
    public static class PdfActionExecutorManager
    {

        #region Properties

        static PdfActionCompositeExecutor _applicationActionExecutor = new PdfActionCompositeExecutor();
        /// <summary>
        /// Gets the application action executor.
        /// </summary>
        public static PdfActionCompositeExecutor ApplicationActionExecutor
        {
            get
            {
                return _applicationActionExecutor;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Initializes the application action executor,
        /// which does NOT have an associated image viewer.
        /// </summary>
        public static void Initialize()
        {
            _applicationActionExecutor.Items.Clear();
            AddCommonActionExecutors(null);
        }

        /// <summary>
        /// Initializes the application action executor,
        /// which has an associated image viewer.
        /// </summary>
        /// <param name="viewer">The viewer.</param>
        /// <param name="viewerNamedActions">Additional viewer handlers of named actions.</param>
        public static void Initialize(
            WpfImageViewer viewer,
            params PdfViewerNamedAction[] viewerNamedActions)
        {
            _applicationActionExecutor.Items.Clear();
            AddImageViewerActionExecutors(viewer, viewerNamedActions);
            AddCommonActionExecutors(viewer);
        }

        /// <summary>
        /// Initializes the application action executor,
        /// which has an associated image viewer with PDF annotation tool.
        /// </summary>
        /// <param name="viewer">The viewer.</param>
        /// <param name="annotationTool">The PDF annotation tool.</param>
        /// <param name="viewerNamedActions">Additional viewer handlers of named actions.</param>
        public static void Initialize(
            WpfImageViewer viewer,
            WpfPdfAnnotationTool annotationTool,
            params PdfViewerNamedAction[] viewerNamedActions)
        {
            _applicationActionExecutor.Items.Clear();
            AddAnnotationToolActionExecutors(annotationTool);
            AddImageViewerActionExecutors(viewer, viewerNamedActions);
            AddCommonActionExecutors(viewer);
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Adds the common action executors.
        /// </summary>
        /// <param name="viewer">Image viewer associated with the manager.</param>
        private static void AddCommonActionExecutors(WpfImageViewer viewer)
        {
            // PdfJavaScriptAction (a script to be compiled and executed by the JavaScript interpreter)
            _applicationActionExecutor.Items.Add(PdfJavaScriptManager.JavaScriptActionExecutor);

            // PdfUriAction (typically a file that is the destination of a hypertext link)
            _applicationActionExecutor.Items.Add(new PdfUriActionExecutor());

            // PdfLaunchAction (launches an application or opens or prints a document)
            _applicationActionExecutor.Items.Add(new PdfLaunchActionExecutor());

            // PdfSubmitFormAction (transmits the names and values of selected 
            // interactive form fields to a specified URL)
            _applicationActionExecutor.Items.Add(new PdfSubmitActionExecutor(viewer));

            // Default action executor (PdfResetFormAction, PdfAnnotationHideAction)
            _applicationActionExecutor.Items.Add(new PdfActionExecutor());
        }

        /// <summary>
        /// Adds the image viewer action executors.
        /// </summary>
        /// <param name="viewer">Image viewer associated with the manager.</param>
        /// <param name="viewerNamedActions">Additional viewer handlers for named actions.</param>
        private static void AddImageViewerActionExecutors(
            WpfImageViewer viewer,
            PdfViewerNamedAction[] viewerNamedActions)
        {
            // PdfGotoAction (navigation on Document)
            _applicationActionExecutor.Items.Add(new WpfPdfGotoActionExecutor(viewer));

            // PdfNamedAction (navigation on ImageViewer)
            _applicationActionExecutor.Items.Add(new PdfViewerNamedActionExecutor(viewer, viewerNamedActions));
        }

        /// <summary>
        /// Adds the annotation tool action executors.
        /// </summary>
        /// <param name="annotationTool">The annotation tool.</param>
        private static void AddAnnotationToolActionExecutors(WpfPdfAnnotationTool annotationTool)
        {
            // PdfResetFormAction (reset interactive form fields)
            _applicationActionExecutor.Items.Add(new WpfPdfAnnotationToolResetFormActionExecutor(annotationTool));

            // PdfAnnotationHideAction (hide/show annotation)
            _applicationActionExecutor.Items.Add(new WpfPdfAnnotationToolAnnotationHideActionExecutor(annotationTool));
        }

        #endregion

        #endregion

    }
}
