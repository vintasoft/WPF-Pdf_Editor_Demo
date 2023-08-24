using System.Windows;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Security;


namespace WpfDemosCommonCode.Pdf.Security
{
    /// <summary>
    /// A window that shows the security properties of PDF document.
    /// </summary>
    public partial class SecurityPropertiesWindow : Window
    {

        #region Fields

        /// <summary>
        /// A message that specifies that user access permission is not allowed.
        /// </summary>
        const string _notAllowed = "Not Allowed";

        #endregion



        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityPropertiesWindow"/> class.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        public SecurityPropertiesWindow(PdfDocument document)
        {
            InitializeComponent();

            // if document is encrypted
            if (document.IsEncrypted)
            {
                // init the encryption system

                encryptionLabel.Content = document.EncryptionSystem.ToString();

                compatibilityModeLabel.Content = document.EncryptionSystem.CompatibilityMode.ToString();

                authorizationResultLabel.Content = document.AuthorizationResult.ToString();
                authorizationResultLabel.ToolTip = authorizationResultLabel.Content;

                if (document.EncryptionSystem.ContainsUserPassword)
                    userPasswordLabel.Content = "Yes";
                if (document.EncryptionSystem.ContainsOwnerPassword)
                    ownerPasswordLabel.Content = "Yes";

                // get the user access permissions
                UserAccessPermissions permissions = document.EncryptionSystem.UserPermissions;

                // init the user access permissions

                if ((permissions & UserAccessPermissions.AssembleDocument) == 0)
                    assembleDocumentLabel.Content = _notAllowed;

                if ((permissions & UserAccessPermissions.ExtractTextAndGraphics) == 0)
                    extractTextAndGraphicsLabel.Content = _notAllowed;

                if ((permissions & UserAccessPermissions.ExtractTextAndGraphicsForAccessibility) == 0)
                    extractTextAndGraphicsForAccessibilityLabel.Content = _notAllowed;

                if ((permissions & UserAccessPermissions.FillInteractiveFormFields) == 0)
                    fillInteractiveFormFieldsLabel.Content = _notAllowed;

                if ((permissions & UserAccessPermissions.ModifyAnnotations) == 0)
                    modifyAnnotationLabel.Content = _notAllowed;

                if ((permissions & UserAccessPermissions.ModifyContents) == 0)
                    modifyContentsLabel.Content = _notAllowed;

                if ((permissions & UserAccessPermissions.PrintDocumentInLowResolution) == 0)
                {
                    printingLabel.Content = _notAllowed;
                }
                else
                {
                    if ((permissions & UserAccessPermissions.PrintDocumentInHighResolution) == 0)
                        printingLabel.Content = string.Format("{0} (Low Resolution)", printingLabel.Content);
                    else
                        printingLabel.Content = string.Format("{0} (High Resolution)", printingLabel.Content);
                }
            }
            else
            {
                encryptionLabel.Content = "No Encryption";

                compatibilityModeLabel.Content = "";
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// "OK" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        #endregion

    }
}
