using System.Windows;

using Vintasoft.Imaging.Pdf.Tree;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view and edit PDF JavaScript action.
    /// </summary>
    public partial class PdfJavaScriptActionEditorWindow : Window
    {

        /// <summary>
        /// The JavaScript action.
        /// </summary>
        PdfJavaScriptAction _action;



        /// <summary>
        /// Initializes a new instance of the <see cref="PdfJavaScriptActionEditorWindow"/> class.
        /// </summary>
        /// <param name="action">The JavaScript action.</param>
        public PdfJavaScriptActionEditorWindow(PdfJavaScriptAction action)
        {
            InitializeComponent();

            _action = action;

            javaScriptTextBox.Text = action.JavaScript;
        }



        /// <summary>
        /// "Ok" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            _action.JavaScript = javaScriptTextBox.Text;
            DialogResult = true;
        }

    }
}
