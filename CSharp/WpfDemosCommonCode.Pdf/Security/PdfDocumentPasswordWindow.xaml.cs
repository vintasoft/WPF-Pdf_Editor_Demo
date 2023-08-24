using System.Windows;
using System.Windows.Input;

#if !REMOVE_PDF_PLUGIN
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Security;
#endif


namespace WpfDemosCommonCode.Pdf.Security
{
    /// <summary>
    /// Form that allows to enter password of PDF document.
    /// </summary>
    public partial class PdfDocumentPasswordWindow : Window
    {

        #region Constructor

        /// <summary>
        /// Prevents a default instance of the <see cref="PdfDocumentPasswordWindow"/> class from being created.
        /// </summary>
        private PdfDocumentPasswordWindow()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDocumentPasswordWindow"/> class.
        /// </summary>
        /// <param name="authenticateType">Type of the authentication.</param>
        private PdfDocumentPasswordWindow(int authenticateType)
        {
            InitializeComponent();

            passwordBox.Focus();
            authenticateTypeComboBox.SelectedIndex = authenticateType;
        }

        #endregion



        #region Properties

        string _filename;
        /// <summary>
        /// Gets or sets the filename of PDF document.
        /// </summary>
        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
                if (_filename != null)
                    Title = string.Format("Password - {0}", System.IO.Path.GetFileName(_filename));
                else
                    Title = "Password";
            }
        }

        /// <summary>
        /// Gets the password of PDF document.
        /// </summary>
        public string Password
        {
            get
            {
                return passwordBox.Password;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// "OK" button is pressed.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// "Cancel" button is pressed.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

#if !REMOVE_PDF_PLUGIN
        /// <summary>
        /// Authenticates the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="filename">The filename.</param>
        public static bool Authenticate(PdfDocument document, string filename)
        {
            if (document.IsEncrypted &&
                document.AuthorizationResult == AuthorizationResult.IncorrectPassword)
            {
                int authenticateType = 0;
                while (true)
                {
                    PdfDocumentPasswordWindow enterPasswordDialog = new PdfDocumentPasswordWindow(authenticateType);
                    enterPasswordDialog.Filename = filename;
                    if (enterPasswordDialog.ShowDialog() == true)
                    {
                        AuthorizationResult result = AuthorizationResult.IncorrectPassword;
                        string authenticateTypeText = "";
                        switch (enterPasswordDialog.authenticateTypeComboBox.SelectedIndex)
                        {
                            case 0:
                                authenticateTypeText = "user";
                                result = document.AuthenticateAsUser(enterPasswordDialog.Password);
                                break;
                            case 1:
                                authenticateTypeText = "owner";
                                result = document.AuthenticateAsOwner(enterPasswordDialog.Password);
                                break;
                            case 2:
                                authenticateTypeText = "user or owner";
                                result = document.Authenticate(enterPasswordDialog.Password);
                                break;
                        }

                        authenticateType = enterPasswordDialog.authenticateTypeComboBox.SelectedIndex;
                        if (result == AuthorizationResult.IncorrectPassword)
                        {
                            MessageBox.Show(
                                string.Format("The {0} password is incorrect.", authenticateTypeText),
                                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            if (authenticateType == 2)
                                MessageBox.Show(
                                    string.Format("Authorization result: {0}", document.AuthorizationResult),
                                    "Authorization Result", MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
#endif
        /// <summary>
        /// Handles the KeyUp event of the passwordBox control.
        /// </summary>
        private void passwordBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                okButton_Click(sender, null);
        }

        #endregion

    }
}
