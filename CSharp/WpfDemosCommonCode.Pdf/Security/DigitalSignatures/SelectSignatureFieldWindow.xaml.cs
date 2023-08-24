using System.Windows;

using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;


namespace WpfDemosCommonCode.Pdf.Security
{
    /// <summary>
    /// Form that allows to select signature field from list.
    /// </summary>
    public partial class SelectSignatureFieldWindow : Window
    {

        #region Fields

        /// <summary>
        /// Array of signature fields.
        /// </summary>
        PdfInteractiveFormSignatureField[] _fields;

        #endregion



        #region Constructors
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectSignatureFieldWindow"/> class.
        /// </summary>
        public SelectSignatureFieldWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectSignatureFieldWindow"/> class.
        /// </summary>
        /// <param name="fields">The signature fields.</param>
        public SelectSignatureFieldWindow(PdfInteractiveFormSignatureField[] fields)
            : this()
        {
            _fields = fields;
            for (int i = 0; i < _fields.Length; i++)
                signaturesListBox.Items.Add(GetSignatureFieldFriendlyName(_fields[i]));
            signaturesListBox.SelectedIndex = 0;
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets the selected signature field.
        /// </summary>
        public PdfInteractiveFormSignatureField SelectedField
        {
            get
            {
                return _fields[signaturesListBox.SelectedIndex];
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of the "OK" button.
        /// </summary>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of the "Cancel" button.
        /// </summary>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Gets the friendly name of the signature field.
        /// </summary>
        /// <param name="field">The field.</param>
        private string GetSignatureFieldFriendlyName(PdfInteractiveFormSignatureField field)
        {
            string name = field.FullyQualifiedName;

            // Signer name
            if (field.SignatureInfo != null && field.SignatureInfo.SignerName != null)
                name = string.Format("{0}: {1}", name, field.SignatureInfo.SignerName);

            // PDF Page
            if (field.Annotation.Page != null)
                return string.Format("Page {0}: {1}",
                    field.Document.Pages.IndexOf(field.Annotation.Page) + 1, name);

            return name;
        }

        #endregion

    }
}
