using System.Windows;

using Vintasoft.Imaging.Pdf.Tree;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view and edit PDF URI action.
    /// </summary>
    public partial class PdfUriActionEditorWindow : Window
    {

        /// <summary>
        /// The PDF URI action.
        /// </summary>
        PdfUriAction _action;



        /// <summary>
        /// Initializes a new instance of the <see cref="PdfUriActionEditorWindow"/> class.
        /// </summary>
        /// <param name="action">The PDF URI action.</param>
        public PdfUriActionEditorWindow(PdfUriAction action)
        {
            InitializeComponent();

            _action = action;

            uriTextBox.Text = _action.URI;
            okButton.IsEnabled = !string.IsNullOrEmpty(uriTextBox.Text);
        }



        /// <summary>
        /// "Ok" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            _action.URI = uriTextBox.Text;
            DialogResult = true;
        }

        /// <summary>
        /// The text of text box is changed.
        /// </summary>
        private void uriTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            okButton.IsEnabled = !string.IsNullOrEmpty(uriTextBox.Text);
        }

    }
}
