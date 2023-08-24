using System.Windows;

using Vintasoft.Imaging.Pdf.Tree;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view and edit PDF named action.
    /// </summary>
    public partial class PdfNamedActionEditorWindow : Window
    {

        /// <summary>
        /// The PDF named action.
        /// </summary>
        PdfNamedAction _action;



        /// <summary>
        /// Initializes a new instance of the <see cref="PdfNamedActionEditorWindow"/> class.
        /// </summary>
        /// <param name="action">The PDF named action.</param>
        public PdfNamedActionEditorWindow(PdfNamedAction action)
        {
            InitializeComponent();

            _action = action;

            nameComboBox.Text = _action.ActionName;
            okButton.IsEnabled = !string.IsNullOrEmpty(nameComboBox.Text);
        }



        /// <summary>
        /// "Ok" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            _action.ActionName = nameComboBox.Text;
            DialogResult = true;
        }

        /// <summary>
        /// The text of text box is changed.
        /// </summary>
        private void nameComboBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            okButton.IsEnabled = !string.IsNullOrEmpty(nameComboBox.Text);
        }

    }
}
