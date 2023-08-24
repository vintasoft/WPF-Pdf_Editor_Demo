using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree.DigitalSignatures;
using Vintasoft.Imaging.Pdf.Wpf.UI;

using WpfDemosCommonCode.CustomControls;
using WpfDemosCommonCode.Imaging;
using WpfDemosCommonCode.Imaging.Codecs;

namespace WpfDemosCommonCode.Pdf.Security
{
    /// <summary>
    /// Form that allows to create signature field appearance.
    /// </summary>
    public partial class CreateSignatureAppearanceWindow : Window
    {

        #region Fields

        /// <summary>
        /// The grahic figure that defines the signature appearance.
        /// </summary>
        SignatureAppearanceGraphicsFigure _signatureAppearance;

        /// <summary>
        /// The open image dialog.
        /// </summary>
        OpenFileDialog _openImageDialog = new OpenFileDialog();

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSignatureAppearanceForm"/> class.
        /// </summary>
        public CreateSignatureAppearanceWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSignatureAppearanceWindow"/> class.
        /// </summary>
        /// <param name="signatureAppearance">The signature appearance.</param>
        public CreateSignatureAppearanceWindow(SignatureAppearanceGraphicsFigure signatureAppearance)
            : this()
        {
            _signatureAppearance = signatureAppearance;

            CodecsFileFilters.SetFilters(_openImageDialog);

            // create temp PDF document
            MemoryStream ms = new MemoryStream();
            using (PdfDocument tempDocument = new PdfDocument())
            {
                tempDocument.Pages.Add(_signatureAppearance.SignatureField.Annotation.Rectangle.Size);
                tempDocument.Save(ms);
            }
            signatureAppearanceEditor.Images.Add(ms, true);

#if !REMOVE_PDFVISUALEDITOR_PLUGIN
            // use PdfEditorTool for editing the signature appearance
            WpfPdfContentEditorTool editorTool = new WpfPdfContentEditorTool();
            editorTool.RenderFiguresWhenImageIndexChanging = false;
            editorTool.FigureViewCollection.Add(WpfGraphicsFigureViewFactory.CreateView(_signatureAppearance));
            signatureAppearanceEditor.VisualTool = editorTool;
#endif
            // update states of checkboxes
            backgroundNoneRadioButton.IsChecked = false;
            backgroundImageRadioButton.IsChecked = false;
            backgroundColorRadioButton.IsChecked = false;
            if (signatureAppearance.BackgroundImage != null)
                backgroundImageRadioButton.IsChecked = true;
            else if (signatureAppearance.BackgroundColor != Colors.Transparent)
                backgroundColorRadioButton.IsChecked = true;
            else
                backgroundNoneRadioButton.IsChecked = true;

            if (signatureAppearance.SignatureImage != null)
                backgroundImageRadioButton.IsChecked = true;
            else if (signatureAppearance.ShowSignerName)
                signatureNameRadioButton.IsChecked = true;
            else
                singnatureNoneRadioButton.IsChecked = true;

            // update signature text
            UpdateSignatureText(
                _signatureAppearance,
                nameCheckBox.IsChecked.Value,
                reasonCheckBox.IsChecked.Value,
                locationCheckBox.IsChecked.Value,
                contactInfoCheckBox.IsChecked.Value,
                dateCheckBox.IsChecked.Value);
        }

        #endregion



        #region Properties

        #endregion



        #region Methods

        /// <summary>
        /// Sets the default signature appearance.
        /// </summary>
        /// <param name="signatureAppearance">The signature appearance figure.</param>
        public static void SetDefaultSignatureAppearance(
            SignatureAppearanceGraphicsFigure signatureAppearance)
        {
            signatureAppearance.SetDefaultLocations();
            UpdateSignatureText(signatureAppearance, true, true, true, true, true);
            signatureAppearance.DrawOnSignatureField();
        }

        /// <summary>
        /// Raizes event <see cref="E:System.Windows.Forms.Form.Closed" />.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // delete temp PDF document  
            signatureAppearanceEditor.Images.ClearAndDisposeItems();
        }

        /// <summary>
        /// Handles the Click event of the "OK" button.
        /// </summary>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            _signatureAppearance.DrawOnSignatureField();
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
        /// Handles the Click event of the "backgroundImageRadioButton" button.
        /// </summary>
        private void backgroundImageRadioButton_Click(object sender, RoutedEventArgs e)
        {
            VintasoftImage image = null;
            if (SelectImage(out image))
                _signatureAppearance.BackgroundImage = image;
        }

        /// <summary>
        /// Selects the image from file.
        /// </summary>
        /// <param name="image">Selected image.</param>
        /// <returns>
        /// <b>true</b> - image is selected;
        /// <b>false</b> - image is NOT selected.
        /// </returns>
        private bool SelectImage(out VintasoftImage image)
        {
            image = null;
            if (_openImageDialog.ShowDialog().Value)
            {
                // select image from file
                image = SelectImageWindow.SelectImageFromFile(_openImageDialog.FileName);

                // if the selected image is not null return true
                return image != null;
            }
            return false;
        }

        /// <summary>
        /// Handles the Click event of the "Background->Color" radio button.
        /// </summary>
        private void backgroundColorRadioButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerDialog colorDialog = new ColorPickerDialog();
            colorDialog.StartingColor = _signatureAppearance.BackgroundColor;
            if (colorDialog.ShowDialog().Value)
            {
                _signatureAppearance.BackgroundImage = null;
                _signatureAppearance.BackgroundColor = colorDialog.SelectedColor;
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the "Background->None" radio button.
        /// </summary>
        private void backgroundNoneRadioButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (IsInitialized && backgroundNoneRadioButton.IsChecked.Value)
            {
                _signatureAppearance.BackgroundImage = null;
                _signatureAppearance.BackgroundColor = Colors.Transparent;
            }
        }

        /// <summary>
        /// Handles the Click event of the "Signature->Imported image" radio button.
        /// </summary>
        private void singatureImageRadioButton_Click(object sender, RoutedEventArgs e)
        {
            _signatureAppearance.ShowSignerName = false;
            VintasoftImage image = null;
            if (SelectImage(out image))
            {
                _signatureAppearance.BackgroundColor = Colors.Transparent;
                _signatureAppearance.SignatureImage = image;
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the "Signature->None" radio button.
        /// </summary>
        private void singnatureNoneRadioButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (IsInitialized && singnatureNoneRadioButton.IsChecked.Value)
            {
                _signatureAppearance.ShowSignerName = false;
                _signatureAppearance.SignatureImage = null;
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the "Background->Name" radio button.
        /// </summary>
        private void signatureNameRadioButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (IsInitialized && signatureNameRadioButton.IsChecked.Value)
            {
                _signatureAppearance.ShowSignerName = true;
                _signatureAppearance.SignatureImage = null;
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the text boxes.
        /// </summary>
        private void textCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
                return;

            UpdateSignatureText(
                _signatureAppearance,
                nameCheckBox.IsChecked.Value,
                reasonCheckBox.IsChecked.Value,
                locationCheckBox.IsChecked.Value,
                contactInfoCheckBox.IsChecked.Value,
                dateCheckBox.IsChecked.Value);
        }

        /// <summary>
        /// Updates the signature text.
        /// </summary>
        /// <param name="signatureAppearance">The signature appearance.</param>
        /// <param name="addName">Indicates whether the signer name must be added
        /// to the signature text.</param>
        /// <param name="addReason">Indicates whether the signature reason must be added
        /// to the signature text.</param>
        /// <param name="addLocation">Indicates whether the signature location must be added
        /// to the signature text.</param>
        /// <param name="addContactInfo">Indicates whether the signer contact information must be added
        /// to the signature text.</param>
        /// <param name="addDate">Indicates whether the signature date must be added
        /// to the signature text.</param>
        private static void UpdateSignatureText(
            SignatureAppearanceGraphicsFigure signatureAppearance,
            bool addName,
            bool addReason,
            bool addLocation,
            bool addContactInfo,
            bool addDate)
        {
            StringBuilder text = new StringBuilder();
            PdfSignatureInformation signatureInfo = signatureAppearance.SignatureField.SignatureInfo;
            if (signatureInfo == null)
            {
                text.AppendLine("Empty Signature Field");
            }
            else
            {
                if (addName && signatureInfo.SignerName != null)
                    text.AppendLine(string.Format("Digitally signed by {0}", signatureInfo.SignerName));
                if (addReason && signatureInfo.Reason != null)
                    text.AppendLine(string.Format("Reason: {0}", signatureInfo.Reason));
                if (addLocation && signatureInfo.Location != null)
                    text.AppendLine(string.Format("Location: {0}", signatureInfo.Location));
                if (addContactInfo && signatureInfo.ContactInfo != null)
                    text.AppendLine(string.Format("Contact: {0}", signatureInfo.ContactInfo));
                if (addDate && signatureInfo.SigningTime != DateTime.MinValue)
                    text.AppendLine(string.Format("Signing Date:\n{0}", signatureInfo.SigningTime.ToString("f", CultureInfo.InvariantCulture)));
            }

            if (text.Length > 0)
            {
                text.Remove(text.Length - 1, 1);
                signatureAppearance.Text = text.ToString();
            }
            else
            {
                signatureAppearance.Text = null;
            }
        }

        #endregion

    }
}
