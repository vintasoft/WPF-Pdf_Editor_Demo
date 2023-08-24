using System.Windows;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view and edit action of PDF document.
    /// </summary>
    public partial class PdfActionEditorWindow : Window
    {

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="PdfActionEditorWindow"/> class from being created.
        /// </summary>
        private PdfActionEditorWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfActionEditorWindow"/> class.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        public PdfActionEditorWindow(PdfDocument document)
            : this()
        {
            pdfActionEditorControl.Document = document;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfActionEditorWindow"/> class.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        /// <param name="action">The PDF action.</param>
        public PdfActionEditorWindow(PdfDocument document, PdfAction action)
            : this(document)
        {
            pdfActionEditorControl.Action = action;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfActionEditorWindow"/> class.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        /// <param name="action">The PDF action.</param>
        /// <param name="imageCollection">The image collection,
        /// which is associated with PDF document.</param>
        public PdfActionEditorWindow(
            PdfDocument document,
            PdfAction action,
            ImageCollection imageCollection)
            : this(document, action)
        {
            pdfActionEditorControl.ImageCollection = imageCollection;
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets or sets the PDF action.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public PdfAction Action
        {
            get
            {
                return pdfActionEditorControl.Action;
            }
            set
            {
                pdfActionEditorControl.Action = value;
            }
        }

        /// <summary>
        /// Gets or sets the image collection, which is associated with PDF document.
        /// </summary>
        public ImageCollection ImageCollection
        {
            get
            {
                return pdfActionEditorControl.ImageCollection;
            }
            set
            {
                pdfActionEditorControl.ImageCollection = value;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of OkButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        #endregion

    }
}
