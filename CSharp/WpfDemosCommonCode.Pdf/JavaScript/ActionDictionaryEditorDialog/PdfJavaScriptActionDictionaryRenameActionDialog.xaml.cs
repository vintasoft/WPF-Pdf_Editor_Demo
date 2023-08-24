using System;
using System.Windows;

using Vintasoft.Imaging.Pdf.Tree;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to rename the JavaScript Action of PDF document.
    /// </summary>
    public partial class PdfJavaScriptActionDictionaryRenameActionDialog : Window
    {

        #region Fields

        /// <summary>
        /// An array of JavaScript action names.
        /// </summary>
        string[] _javaScriptActionNames = null;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of
        /// the <see cref="PdfJavaScriptActionDictionaryRenameActionDialog"/> class.
        /// </summary>
        /// <param name="title">The dialog title.</param>
        /// <param name="message">The dialog message.</param>
        /// <param name="javaScriptActionNames">An array
        /// that contains JavaScript action names of PDF document.</param>
        public PdfJavaScriptActionDictionaryRenameActionDialog(
            string title, string message, string[] javaScriptActionNames)
        {
            InitializeComponent();

            Title = title;
            messageLabel.Content = message;
            _javaScriptActionNames = javaScriptActionNames;

            textBox.Focus();
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets or sets the name of the action.
        /// </summary>
        /// <value>
        /// The name of the action.
        /// </value>
        public string ActionName
        {
            get
            {
                return textBox.Text;
            }
            set
            {
                textBox.Text = value;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// "OK" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            // get name of action
            string name = textBox.Text;
            name = name.Trim();

            // if name is empty
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Name can not be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // search name in dictionary
            foreach (string key in _javaScriptActionNames)
            {
                if (String.Equals(key, name, StringComparison.InvariantCulture))
                {
                    string errorMessage = string.Format("PDF document already contains ction with name \"{0}\".", name);
                    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            DialogResult = true;
        }

        /// <summary>
        /// "Cancel" button is clicked.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

    }
}
