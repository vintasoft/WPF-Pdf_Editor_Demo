using System.Windows;

using Vintasoft.Imaging.Pdf.Tree;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view and edit PDF launch action.
    /// </summary>
    public partial class PdfLaunchActionEditorWindow : Window
    {

        /// <summary>
        /// The PDF launch action.
        /// </summary>
        PdfLaunchAction _action;



        /// <summary>
        /// Initializes a new instance of the <see cref="PdfLaunchActionEditorWindow"/> class.
        /// </summary>
        /// <param name="action">The PDF launch action.</param>
        public PdfLaunchActionEditorWindow(PdfLaunchAction action)
        {
            InitializeComponent();

            _action = action;

            commandLineTextBox.Text = _action.WinCommandLine;
            okButton.IsEnabled = !string.IsNullOrEmpty(commandLineTextBox.Text);
        }



        /// <summary>
        /// "Ok" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            _action.WinCommandLine = commandLineTextBox.Text;
            DialogResult = true;
        }

        /// <summary>
        /// The text of text box is changed.
        /// </summary>
        private void commandLineTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            okButton.IsEnabled = !string.IsNullOrEmpty(commandLineTextBox.Text);
        }

    }
}
