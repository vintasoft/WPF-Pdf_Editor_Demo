using System.ComponentModel;
using System.Drawing;

using Vintasoft.Imaging.Pdf.Drawing;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms.AppearanceGenerators;

using WpfDemosCommonCode.Pdf.Security;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Appearance generator for PDF signature field.
    /// </summary>
    public class SignatureAppearanceGenerator : PdfWidgetAnnotationAppearanceGenerator
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SignatureAppearanceGenerator"/> class.
        /// </summary>
        public SignatureAppearanceGenerator()
            : base()
        {
        }

        #endregion



        #region Properties

        SignatureAppearanceGraphicsFigure _signatureApprearance = null;
        /// <summary>
        /// Gets or sets the signature appearance graphics figure.
        /// </summary>
        [Browsable(false)]
        public SignatureAppearanceGraphicsFigure SignatureAppearance
        {
            get
            {
                return _signatureApprearance;
            }
            set
            {
                _signatureApprearance = value;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Sets the appearance of specified PDF annotation.
        /// </summary>
        /// <param name="annotation">The PDF annotation.</param>
        public override void SetAppearance(PdfAnnotation annotation)
        {
            base.SetAppearance(annotation);
            
            annotation.Document.InteractiveForm.NeedAppearances = false;

            PdfInteractiveFormField field = ((PdfWidgetAnnotation)annotation).Field;

            PdfInteractiveFormSignatureField signatureField = (PdfInteractiveFormSignatureField)field;

            if (_signatureApprearance.SignatureField != signatureField)
            {
                _signatureApprearance.SignatureField = signatureField;
                CreateSignatureAppearanceWindow.SetDefaultSignatureAppearance(SignatureAppearance);
            }
            _signatureApprearance.SetDefaultLocations();
            using (PdfGraphics g = signatureField.CreateAppearanceGraphics())
                _signatureApprearance.Draw(g);
        }

        /// <summary>
        /// Returns the default rectangle of PDF annotation.
        /// </summary>
        /// <returns></returns>
        public override RectangleF GetDefaultAnnotationRectangle()
        {
            return new RectangleF(0, 0, 200, 70);
        }

        /// <summary>
        /// Copies the state of the current object to the target object.
        /// </summary>
        /// <param name="dest"></param>
        protected override void CopyTo(PdfAnnotationAppearanceGenerator dest)
        {
            base.CopyTo(dest);

            SignatureAppearanceGenerator typedDets = (SignatureAppearanceGenerator)dest;
            if (_signatureApprearance != null)
            {
                typedDets._signatureApprearance = (SignatureAppearanceGraphicsFigure)_signatureApprearance.Clone();
            }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override PdfAnnotationAppearanceGenerator Clone()
        {
            SignatureAppearanceGenerator result = new SignatureAppearanceGenerator();
            CopyTo(result);
            return result;
        }

        #endregion

    }
}
