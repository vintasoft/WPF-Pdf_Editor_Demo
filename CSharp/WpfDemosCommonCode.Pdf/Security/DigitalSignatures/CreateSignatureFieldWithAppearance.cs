using System.Drawing;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;

namespace WpfDemosCommonCode.Pdf.Security
{
    /// <summary>
    /// A class that allows to create new signature field or use existing signature field AND
    /// perform signing of PDF document using the signature field.
    /// </summary>
    public class CreateSignatureFieldWithAppearance
    {

        #region Fields

        /// <summary>
        /// The signature appearance graphics figure that is used for editing of the signature appearance of specified signature field.
        /// </summary>
        SignatureAppearanceGraphicsFigure _signatureAppearance;

        #endregion



        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="CreateSignatureFieldWithAppearance"/> class from being created.
        /// </summary>
        private CreateSignatureFieldWithAppearance()
        {
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Shows the <see cref="CreateSignatureFieldWindow"/> as a modal dialog.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        /// <param name="annotationRect">The annotation rectangle of new signature field.</param>
        /// <param name="signatureAppearance">The signature appearance.</param>
        /// <returns>
        /// <b>True</b> if dialog was shown and activity was accepted; otherwise, <b>false</b>.
        /// </returns>
        public static bool ShowDialog(
            PdfDocument document,
            RectangleF annotationRect,
            SignatureAppearanceGraphicsFigure signatureAppearance)
        {
            PdfInteractiveFormSignatureField signatureField = null;
            return ShowDialog(document, annotationRect, signatureAppearance, out signatureField);
        }

        /// <summary>
        /// Shows the <see cref="CreateSignatureFieldWindow"/> as a modal dialog.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        /// <param name="annotationRect">The annotation rectangle of new signature field.</param>
        /// <param name="signatureAppearance">The signature appearance.</param>
        /// <param name="signatureField">The PDF signature field.</param>
        /// <returns>
        /// <b>True</b> if dialog was shown and activity was accepted; otherwise, <b>false</b>.
        /// </returns>
        public static bool ShowDialog(
            PdfDocument document,
            RectangleF annotationRect,
            SignatureAppearanceGraphicsFigure signatureAppearance,
            out PdfInteractiveFormSignatureField signatureField)
        {
            CreateSignatureFieldWindow window = new CreateSignatureFieldWindow(document, annotationRect);
            CreateSignatureFieldWithAppearance appearance = new CreateSignatureFieldWithAppearance();
            bool result = appearance.ShowDialog(window, signatureAppearance);
            signatureField = window.SignatureField;
            return result;

        }

        /// <summary>
        /// Shows the <see cref="CreateSignatureFieldWindow"/> as a modal dialog.
        /// </summary>
        /// <param name="signatureField">The signature field.</param>
        /// <param name="signatureAppearance">The signature appearance.</param>
        /// <returns>
        /// <b>True</b> if dialog was shown and activity was accepted; otherwise, <b>false</b>.
        /// </returns>
        public static bool ShowDialog(
            PdfInteractiveFormSignatureField signatureField,
            SignatureAppearanceGraphicsFigure signatureAppearance)
        {
            CreateSignatureFieldWindow window = new CreateSignatureFieldWindow(signatureField);
            CreateSignatureFieldWithAppearance appearance = new CreateSignatureFieldWithAppearance();
            return appearance.ShowDialog(window, signatureAppearance);
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Shows the specified window as a modal dialog.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="signatureAppearance">The signature appearance.</param>
        /// <returns>
        /// <b>True</b> if dialog was shown and activity was accepted; otherwise, <b>false</b>.
        /// </returns>
        private bool ShowDialog(CreateSignatureFieldWindow window, SignatureAppearanceGraphicsFigure signatureAppearance)
        {
            _signatureAppearance = signatureAppearance;

            window.CanChangeSignatureAppearance = true;
            window.CreateSignatureAppearance += Form_CreateSignatureAppearance;

            return window.ShowDialog() == true;
        }

        /// <summary>
        /// Creates the signature apperance.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CreateSignatureAppearanceEventArgs"/> instance containing the event data.</param>
        private void Form_CreateSignatureAppearance(object sender, CreateSignatureAppearanceEventArgs e)
        {
            // associate the signature field with signature appearance
            _signatureAppearance.SignatureField = e.SignatureField;
            // create a dialog that allows to create or edit the appearance of the signature field
            CreateSignatureAppearanceWindow createSignatureAppearance = new CreateSignatureAppearanceWindow(_signatureAppearance);
            // show the dialog and create the appearance of the signature field
            if (createSignatureAppearance.ShowDialog() == true)
                e.Cancel = false;
            else
                e.Cancel = true;

        }

        #endregion

        #endregion

    }
}
