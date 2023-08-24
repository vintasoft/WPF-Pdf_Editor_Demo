using System.Drawing;
using System.Windows;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;


namespace WpfDemosCommonCode.Pdf.Security
{
    /// <summary>
    /// Form that allows to create new empty signature field.
    /// </summary>
    public partial class CreateEmptySignatureFieldWindow : Window
    {

        #region Fields

        /// <summary>
        /// The PDF document.
        /// </summary>
        PdfDocument _document;

        /// <summary>
        /// Annotation rectangle of new signature field.
        /// </summary>
        RectangleF _annotationRect;

        /// <summary>
        /// The signature appearance.
        /// </summary>
        SignatureAppearanceGraphicsFigure _signatureAppearance;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateEmptySignatureFieldWindow"/> class.
        /// </summary>
        public CreateEmptySignatureFieldWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateEmptySignatureFieldWindow"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="annotationRect">The annotation rectangle of new signature field.</param>       
        public CreateEmptySignatureFieldWindow(
            PdfDocument document,
            RectangleF annotationRect,
            SignatureAppearanceGraphicsFigure signatureAppearance)
            : this()
        {
            _annotationRect = annotationRect;
            _document = document;
            _signatureAppearance = signatureAppearance;
            signatureNameTextBox.Text = GetNewSignatureName();
        }

        #endregion



        #region Properties

        PdfInteractiveFormSignatureField _signatureField;
        /// <summary>
        /// Gets the new signature field.
        /// </summary>
        public PdfInteractiveFormSignatureField SignatureField
        {
            get
            {
                return _signatureField;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Creates annotation appearance of signature field.
        /// </summary>
        private void signatureAppearanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_signatureField == null)
                _signatureField = new PdfInteractiveFormSignatureField(_document, signatureNameTextBox.Text, _annotationRect);

            _signatureAppearance.SignatureField = _signatureField;
            CreateSignatureAppearanceWindow createSignatureAppearance = new CreateSignatureAppearanceWindow(_signatureAppearance);
            createSignatureAppearance.Owner = this;
            createSignatureAppearance.ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of the "Cancel" button.
        /// </summary>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Handles the Click event of the "OK" button.
        /// </summary>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            // if signature field name is valid
            if (CheckSignatureName(signatureNameTextBox.Text))
            {
                // create new signature field
                if (_signatureField == null)
                    _signatureField = new PdfInteractiveFormSignatureField(_document, signatureNameTextBox.Text, _annotationRect);

                // if appearance is not set
                if (_signatureField.Annotation.Appearances == null)
                {
                    // set default signature appearance
                    _signatureAppearance.SignatureField = _signatureField;
                    CreateSignatureAppearanceWindow.SetDefaultSignatureAppearance(_signatureAppearance);
                }

                DialogResult = true;
            }
            else
            {
                DemosTools.ShowWarningMessage(string.Format("Document has annotation with name '{0}', change signature name.", signatureNameTextBox.Text));
            }
        }

        /// <summary>
        /// Gets the new name of the signature.
        /// </summary>
        private string GetNewSignatureName()
        {
            int i = 1;
            string name;
            do
            {
                name = string.Format("Signature{0}", i++);
            }
            while (!CheckSignatureName(name));
            return name;
        }

        /// <summary>
        /// Checks the name of the signature.
        /// </summary>
        /// <param name="name">The new signature name.</param>
        /// <returns><b>true</b> if name is valid; otherwise <b>false</b>.</returns>
        private bool CheckSignatureName(string name)
        {
            if (_document.InteractiveForm != null)
                return _document.InteractiveForm.FindField(name) == null;

            return true;
        }

        #endregion

    }
}
