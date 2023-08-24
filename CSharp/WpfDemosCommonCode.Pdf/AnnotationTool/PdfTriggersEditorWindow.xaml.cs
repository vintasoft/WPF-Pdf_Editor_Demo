using System;
using System.Windows;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A form that allows to view and edit triggers of PDF document or PDF page form field.
    /// </summary>
    public partial class PdfTriggersEditorWindow : Window
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfTriggersEditorWindow"/> class.
        /// </summary>
        public PdfTriggersEditorWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfTriggersEditorWindow"/> class.
        /// </summary>
        /// <param name="treeNode">The PDF tree node.</param>
        public PdfTriggersEditorWindow(PdfTreeNodeBase treeNode)
            : this()
        {
            if (treeNode == null)
                throw new ArgumentNullException();

            pdfTriggersEditorControl.TreeNode = treeNode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfTriggersEditorWindow"/> class.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        public PdfTriggersEditorWindow(PdfDocument document)
            : this(document.Catalog)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfTriggersEditorWindow"/> class.
        /// </summary>
        /// <param name="page">The PDF page.</param>
        public PdfTriggersEditorWindow(PdfPage page)
            : this((PdfTreeNodeBase)page)
        {
            if (page.AdditionalActions == null)
                page.AdditionalActions = new PdfPageAdditionalActions(page.Document);
        }



        /// <summary>
        /// "OK" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

    }
}
