using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree.DigitalSignatures;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;

using WpfDemosCommonCode.Imaging;

namespace WpfDemosCommonCode.Pdf.Security
{
    /// <summary>
    /// Form that allows to create new signature field or use existing signature field AND
    /// perform signing of PDF document using the signature field.
    /// </summary>
    public partial class CreateSignatureFieldWindow : Window
    {

        #region Fields

        /// <summary>
        /// Timestamp Server URL.
        /// </summary>
        static string TimestampServerUrl = "http://timestamp.comodoca.com/";

        /// <summary>
        /// User name of Timestamp Server.
        /// </summary>
        static string TimestampServerUserName = null;

        /// <summary>
        /// Password of Timestamp Server.
        /// </summary>
        static string TimestampServerPassword = null;

        /// <summary>
        /// The PDF document.
        /// </summary>
        PdfDocument _document;

        /// <summary>
        /// Annotation rectangle of new signature field.
        /// </summary>
        RectangleF _annotationRect;

        /// <summary>
        /// The X509 certificate with private key.
        /// </summary>
        X509Certificate2 _certificate;

        /// <summary>
        /// The signature appearance used for editing the signature appearance
        /// of specified signature field.
        /// </summary>
        SignatureAppearanceGraphicsFigure _signatureAppearance;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSignatureFieldWindow"/> class.
        /// </summary>
        public CreateSignatureFieldWindow()
        {
            InitializeComponent();

            timestampHashAlgorithmComboBox.SelectedIndex = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSignatureFieldWindow"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="annotationRect">The annotation rectangle of new signature field.</param>
        /// <param name="signatureAppearance">The signature appearance.</param>
        public CreateSignatureFieldWindow(
            PdfDocument document,
            RectangleF annotationRect,
            SignatureAppearanceGraphicsFigure signatureAppearance)
            : this()
        {
            _annotationRect = annotationRect;
            _document = document;
            _signatureAppearance = signatureAppearance;

            signatureNameTextBox.Text = GetNewSignatureName();

            signatureTypeComboBox.Items.Add("PKCS#1 (adbe.x509.rsa_sha1)");
            signatureTypeComboBox.Items.Add("PKCS#7 (adbe.pkcs7.detached)");
            signatureTypeComboBox.Items.Add("PKCS#7 (ETSI.CAdES.detached)");
            signatureTypeComboBox.SelectedIndex = 1;

            UpdateUI();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSignatureFieldWindow"/> class.
        /// </summary>
        /// <param name="signatureField">The signature field.</param>
        /// <param name="signatureAppearance">The signature appearance.</param>
        public CreateSignatureFieldWindow(
            PdfInteractiveFormSignatureField signatureField,
            SignatureAppearanceGraphicsFigure signatureAppearance)
            : this(signatureField.Document,
                   signatureField.Annotation.Rectangle,
                   signatureAppearance)
        {
            _signatureField = signatureField;
            signatureNameTextBox.Text = _signatureField.PartialName;
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
            // if certificate is NOT specified
            if (_certificate == null)
            {
                // if empty signature field must NOT be created
                if (MessageBox.Show("Certificate is not selected, create empty signature field?", "Create empty signature", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    // exit
                    return;
            }

            // if signature field is NOT specified
            if (_signatureField == null)
            {
                // if signature field is NOT created
                if (!CreateSignatureField())
                    // exit
                    return;
            }

            // init the signature field
            if (InitSignatureField())
            {
                // if annotation appearance of signature field is NOT set
                if (_signatureField.Annotation.Appearances == null)
                {
                    // associate the signature field with signature appearance
                    _signatureAppearance.SignatureField = _signatureField;
                    // create a form for creating or editing the appearance of the signature field
                    CreateSignatureAppearanceWindow createSignatureAppearance = new CreateSignatureAppearanceWindow(_signatureAppearance);
                    createSignatureAppearance.Owner = this;
                    // create or edit the appearance of the signature field
                    if (!createSignatureAppearance.ShowDialog().Value)
                        return;
                }

                DialogResult = true;
            }
        }

        /// <summary>
        /// Creates the signature field.
        /// </summary>
        /// <returns>
        /// <b>true</b> - signature field is created successfully;
        /// <b>false</b> - signature field is not created;
        /// </returns>
        private bool CreateSignatureField()
        {
            // if signature name is NOT valid
            if (!CheckSignatureName(signatureNameTextBox.Text))
            {
                DemosTools.ShowWarningMessage(string.Format("Document has annotation with name '{0}'.", signatureNameTextBox.Text));
                return false;
            }

            // create new signature field
            _signatureField = new PdfInteractiveFormSignatureField(_document, signatureNameTextBox.Text, _annotationRect);

            return true;
        }

        /// <summary>
        /// Inits the signature field.
        /// </summary>
        private bool InitSignatureField()
        {
            // set the signature field name
            _signatureField.PartialName = signatureNameTextBox.Text;

            // if certificate is selected 
            if (_certificate != null)
            {
                // get signature info from the signature field
                PdfSignatureInformation signatureInfo = _signatureField.SignatureInfo;
                // if signate info is NOT found
                if (signatureInfo == null)
                {
                    try
                    {
                        // create the signature info
                        signatureInfo = CreateSignatureInformation();
                    }
                    catch (Exception ex)
                    {
                        DemosTools.ShowErrorMessage(ex);
                    }
                    if (signatureInfo == null)
                        return false;

                    // set the signature information of signature field
                    _signatureField.SignatureInfo = signatureInfo;
                }


                // update the signature information
                if (contactInfoTextBox.Text != "")
                    signatureInfo.ContactInfo = contactInfoTextBox.Text;
                if (locationTextBox.Text != "")
                    signatureInfo.Location = locationTextBox.Text;
                if (reasonTextBox.Text != "")
                    signatureInfo.Reason = reasonTextBox.Text;
                if (signerNameTextBox.Text != "")
                    signatureInfo.SignerName = signerNameTextBox.Text;
                signatureInfo.SigningTime = DateTime.Now;

                // add application name
                signatureInfo.BuildProperties = new PdfSignatureBuildProperties(_document);
                signatureInfo.BuildProperties.Application = new PdfSignatureBuildData(_document);
                signatureInfo.BuildProperties.Application.Name = "VintaSoft Imaging .NET SDK - https://www.vintasoft.com";
            }

            return true;
        }

        /// <summary>
        /// Creates the signature information.
        /// </summary>
        private PdfSignatureInformation CreateSignatureInformation()
        {
            bool addCertificateChain = certificateChainCheckBox.IsChecked.Value;

            PdfPkcsSignature signature;
            if (signatureTypeComboBox.SelectedIndex == 0)
            {
                // create new PKCS#1 signature
                signature = PdfPkcsSignature.CreatePkcs1Signature(_certificate, addCertificateChain);
            }
            else
            {
                // create new PKCS#7 signature
                try
                {
                    PdfPkcsSignatureCreationParams creationParams = new PdfPkcsSignatureCreationParams(_certificate, addCertificateChain);
                    
                    // set ParentWindowHandle
                    creationParams.ParentWindowHandle = (new WindowInteropHelper(Application.Current.MainWindow)).Handle;

                    // if timestamp must be embedded into signature
                    if (addTimestampCheckBox.IsChecked.Value)
                    {
                        TimestampAuthorityWebClient timestampRequest = new TimestampAuthorityWebClient(TimestampServerUrl, TimestampServerUserName, TimestampServerPassword);
                        timestampRequest.HashAlgorithmName = timestampHashAlgorithmComboBox.Text;
                        creationParams.TimestampAuthorityClient = timestampRequest;
                    }

                    if (signatureTypeComboBox.SelectedIndex == 1)
                        creationParams.SignatureFormat = PdfPkcsSignatureFormat.CMS;
                    else if (signatureTypeComboBox.SelectedIndex == 2)
                        creationParams.SignatureFormat = PdfPkcsSignatureFormat.CAdES;
                    else
                        throw new NotImplementedException();

                    signature = PdfPkcsSignature.CreatePkcs7Signature(_document.Format, creationParams);
                }
                catch (System.Security.Cryptography.CryptographicException ex)
                {
                    DemosTools.ShowErrorMessage("Certificate error", ex.Message);
                    return null;
                }
            }

            // create signature information
            return new PdfSignatureInformation(_document, signature);
        }

        /// <summary>
        /// Handles the Click event of the selectCertificateButton control.
        /// </summary>
        private void selectCertificateButton_Click(object sender, RoutedEventArgs e)
        {
            SelectCertificate();
        }

        /// <summary>
        /// Handles the Click event of the "Signature Appearance..." button.
        /// </summary>
        private void signatureAppearanceButton_Click(object sender, RoutedEventArgs e)
        {
            // if signature field is NOT specified
            if (_signatureField == null)
            {
                // if signature field is NOT created
                if (!CreateSignatureField())
                    // exit
                    return;
            }

            // init the signature field
            if (InitSignatureField())
            {
                // associate the signature field with signature appearance
                _signatureAppearance.SignatureField = _signatureField;
                // create a form for creating or editing the appearance of the signature field
                CreateSignatureAppearanceWindow createSignatureAppearance = new CreateSignatureAppearanceWindow(_signatureAppearance);
                createSignatureAppearance.Owner = this;
                // create or edit the appearance of the signature field
                createSignatureAppearance.ShowDialog();
            }
        }

        /// <summary>
        /// Updates the User Interface.
        /// </summary>
        private void UpdateUI()
        {
            bool isCertificateSelected = _certificate != null;
            signerNameTextBox.IsEnabled = isCertificateSelected;
            locationTextBox.IsEnabled = isCertificateSelected;
            contactInfoTextBox.IsEnabled = isCertificateSelected;
            reasonTextBox.IsEnabled = isCertificateSelected;
            signatureTypeComboBox.IsEnabled = isCertificateSelected;
            certificateChainCheckBox.IsEnabled = isCertificateSelected;

            bool canEmbedTimestamp = signatureTypeComboBox.SelectedIndex == 1 || signatureTypeComboBox.SelectedIndex == 2;
            addTimestampCheckBox.IsEnabled = isCertificateSelected && canEmbedTimestamp;
            timestampServerSettingsButton.IsEnabled = isCertificateSelected && canEmbedTimestamp;
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
        /// <returns><b>true</b> if name is valid; otherwise <b>false</b></returns>
        private bool CheckSignatureName(string name)
        {
            if (_document.InteractiveForm != null)
                return _document.InteractiveForm.FindField(name) == null;
            return true;
        }

        /// <summary>
        /// Selects the certificate for a signature field.
        /// </summary>
        private void SelectCertificate()
        {
            // open certificate store for personal certificates
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            // show form that allows to select certificate
            SelectCertificateWindow window = new SelectCertificateWindow(store.Certificates);
            window.Owner = this;
            if (window.ShowDialog().Value)
            {
                if (!window.SelectedCertificate.HasPrivateKey)
                {
                    DemosTools.ShowErrorMessage("Certificate does not have Private Key.");
                }
                else
                {
                    _certificate = window.SelectedCertificate;
                    certificateTextBox.Text = ConvertCertificateToString(_certificate);
                    signerNameTextBox.Text = _certificate.GetNameInfo(X509NameType.SimpleName, false);
                    locationTextBox.Text = CultureInfo.CurrentCulture.EnglishName;
                    if (_signatureField != null)
                        _signatureField.Annotation.Appearances = null;
                }
            }

            UpdateUI();
        }

        /// <summary>
        /// Converts the certificate to a string.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        private string ConvertCertificateToString(X509Certificate2 certificate)
        {
            string email = certificate.GetNameInfo(X509NameType.EmailName, false);
            if (email != "")
                return string.Format("{0} <{1}>", certificate.GetNameInfo(X509NameType.SimpleName, false), email);
            else
                return certificate.GetNameInfo(X509NameType.SimpleName, false);
        }

        /// <summary>
        /// Handles the SelectionChanged event of the certificateTextBox control.
        /// </summary>
        private void certificateTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (IsInitialized && _certificate == null)
                SelectCertificate();
        }

        /// <summary>
        /// Clears the signature information.
        /// </summary>
        private void ClearSignatureInfo(object sender, SelectionChangedEventArgs e)
        {
            if (_signatureField != null)
                _signatureField.SignatureInfo = null;
            UpdateUI();
        }

        /// <summary>
        /// Handles the Checked event of CertificateChainCheckBox object.
        /// </summary>
        private void certificateChainCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ClearSignatureInfo(sender, null);
        }

        /// <summary>
        /// Set settings of timestamp server.
        /// </summary>
        private void timestampServerSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ServerSettingsWindow serverSettings = new ServerSettingsWindow();
            serverSettings.Owner = this;
            serverSettings.Title = "Timestamp Server Settings";
            serverSettings.ServerUrl = TimestampServerUrl;
            serverSettings.ServerUserName = TimestampServerUserName;
            serverSettings.ServerPassword = TimestampServerPassword;
            if (serverSettings.ShowDialog().Value)
            {
                TimestampServerUrl = serverSettings.ServerUrl;
                TimestampServerUserName = serverSettings.ServerUserName;
                TimestampServerPassword = serverSettings.ServerPassword;
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of AddTimestampCheckBox object.
        /// </summary>
        private void addTimestampCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            timestampProperitesGroupBox.IsEnabled = addTimestampCheckBox.IsChecked.Value == true;
        }

        #endregion

    }
}
