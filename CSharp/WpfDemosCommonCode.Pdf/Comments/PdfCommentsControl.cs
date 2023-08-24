#if !REMOVE_ANNOTATION_PLUGIN
using Vintasoft.Imaging.Annotation.UI.Comments;
using Vintasoft.Imaging.Annotation.Wpf.UI.VisualTools;
#endif

using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Wpf.UI.Annotations;
using Vintasoft.Imaging.Wpf;

using WpfDemosCommonCode.Annotation;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Provides a control that displays collection of PDF comments (PdfAnnotationComment).
    /// </summary>
    public class PdfCommentsControl : CommentsControl
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfCommentsControl"/> class.
        /// </summary>
        public PdfCommentsControl()
            : base()
        {
        }

        #endregion



        #region Properties

#if !REMOVE_ANNOTATION_PLUGIN
        WpfPdfAnnotationTool _annotationTool;
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
                if (_annotationTool != null)
                {
                    _annotationTool.BuildingStarted -= AnnotationTool_BuildingStarted;
                    _annotationTool.BuildingFinished -= AnnotationTool_BuildingFinished;
                    _annotationTool.PreviewMouseDown -= AnnotationTool_PreviewMouseDown;
                    _annotationTool.MouseDoubleClick -= AnnotationTool_MouseDoubleClick;
                }

                _annotationTool = value;

                if (_annotationTool != null)
                {
                    _annotationTool.BuildingStarted += AnnotationTool_BuildingStarted;
                    _annotationTool.BuildingFinished += AnnotationTool_BuildingFinished;
                    _annotationTool.PreviewMouseDown += AnnotationTool_PreviewMouseDown;
                    _annotationTool.MouseDoubleClick += AnnotationTool_MouseDoubleClick;
                }
            }
        }

        /// <summary>
        /// Gets or sets the comment visual tool.
        /// </summary>
        public override WpfCommentVisualTool CommentTool
        {
            get
            {
                return base.CommentTool;
            }
            set
            {
                if (CommentTool != null)
                {
                    CommentTool.SelectedCommentChanging -= CommentTool_SelectedCommentChanging;
                }

                base.CommentTool = value;

                if (CommentTool != null)
                {
                    CommentTool.SelectCommentControlOnMouseClick = false;
                    CommentTool.SelectCommentControlOnMouseDoubleClick = false;

                    CommentTool.SelectedCommentChanging += CommentTool_SelectedCommentChanging;
                }
            }
        }
#endif

        #endregion



        #region Methods

#if !REMOVE_ANNOTATION_PLUGIN

        /// <summary>
        /// Opens the comment for specified annotation.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <param name="needSetFocus">Determines that comment must receive focus.</param>
        public void OpenComment(PdfAnnotation annotation, bool needSetFocus)
        {
            ICommentControl control = FindCommentControlBySource(annotation);
            if (control != null)
            {
                control.Comment.IsOpen = true;
                control.IsCommentSelected = true;
                if (needSetFocus)
                {
                    if (CommentTool != null)
                    {
                        CommentControl controlOnViewer = (CommentControl)CommentTool.FindRootCommentControl(control.Comment);
                        if (controlOnViewer != null)
                            controlOnViewer.NeedSetFocusWhenLoaded = true;
                    }
                    else
                    {
                        ((CommentControl)control).NeedSetFocusWhenLoaded = true;
                    }
                }
            }
        }


        /// <summary>
        /// Annotation building is started.
        /// </summary>
        private void AnnotationTool_BuildingStarted(object sender, WpfPdfAnnotationViewEventArgs e)
        {
            if (IsCommentsOnViewerVisible && AnnotationTool != null &&
                (AnnotationTool.InteractionMode == WpfPdfAnnotationInteractionMode.Markup ||
                 AnnotationTool.InteractionMode == WpfPdfAnnotationInteractionMode.View))
                CommentTool.Enabled = false;
        }

        /// <summary>
        /// Annotation building is finished.
        /// </summary>
        private void AnnotationTool_BuildingFinished(object sender, WpfPdfAnnotationViewEventArgs e)
        {
            if (IsCommentsOnViewerVisible && AnnotationTool != null &&
                (AnnotationTool.InteractionMode == WpfPdfAnnotationInteractionMode.Markup ||
                 AnnotationTool.InteractionMode == WpfPdfAnnotationInteractionMode.View))
                CommentTool.Enabled = true;
            if (e.AnnotationView.Annotation is PdfTextAnnotation)
                OpenComment(e.AnnotationView.Annotation, true);
        }

        /// <summary>
        /// Mouse is clicked in annotation viewer.
        /// </summary>
        private void AnnotationTool_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // if comment control exists
            if (CommentTool != null)
            {
                // if action button of annotation tool is clicked
                if (WpfObjectConverter.CreateVintasoftMouseButtons(e) == AnnotationTool.ActionButton)
                {
                    ICommentControl selectedComment = null;

                    // find annnotation view
                    WpfPdfAnnotationView view = AnnotationTool.FindAnnotationView(e.GetPosition(AnnotationTool));
                    // if annotation view is found
                    if (view != null)
                    {
                        // find the comment control by annotation
                        ICommentControl control = FindCommentControlBySource(view.Annotation);
                        // if comment control is found
                        if (control != null)
                            // find root comment control
                            selectedComment = CommentTool.FindRootCommentControl(control.Comment);
                    }

                    // set the selected comment
                    CommentTool.SelectedComment = selectedComment;
                }
            }
        }

        /// <summary>
        /// Mouse is double clicked in annotation viewer.
        /// </summary>
        private void AnnotationTool_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // if comment control exists
            if (CommentTool != null)
            {
                // if action button of annotation tool is clicked
                if (WpfObjectConverter.CreateVintasoftMouseButtons(e) == AnnotationTool.ActionButton)
                {
                    // find annnotation view
                    WpfPdfAnnotationView view = AnnotationTool.FindAnnotationView(e.GetPosition(AnnotationTool));
                    // if annotation view is found
                    if (view != null)
                    {
                        PdfMarkupAnnotation source = view.Annotation as PdfFreeTextAnnotation;
#if !REMOVE_OFFICE_PLUGIN
                        if (source == null)
                            source = view.Annotation as PdfOfficeDocumentAnnotation;
#endif
                        // if annotation is NOT PdfFreeTextAnnotation/PdfOfficeDocumentAnnotation OR annotation has popup annotation
                        if (source == null || source.PopupAnnotation != null)
                        {
                            // if annotation view is NOT building
                            if (!view.IsBuilding)
                                // open the annotation comment control
                                OpenComment(view.Annotation, false);
                        }
                    }
                }
            }
        }

       
        /// <summary>
        /// Selected comment is changing.
        /// </summary>
        private void CommentTool_SelectedCommentChanging(object sender, Vintasoft.Imaging.PropertyChangingEventArgs<ICommentControl> e)
        {
            if (e.NewValue != null)
            {
                PdfFreeTextAnnotation source = e.NewValue.Comment.Source as PdfFreeTextAnnotation;
                if (source != null && source.PopupAnnotation == null)
                    e.Cancel = true;
            }
        }
#endif

        #endregion

    }
}
