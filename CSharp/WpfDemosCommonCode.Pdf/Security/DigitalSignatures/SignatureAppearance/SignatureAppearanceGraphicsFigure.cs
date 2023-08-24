using System;
using Vintasoft.Imaging.Pdf.Drawing;
using Vintasoft.Imaging.Pdf.Drawing.GraphicsFigures;
using Vintasoft.Imaging.Pdf.Tree.Fonts;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;
using Vintasoft.Imaging;
using System.Drawing;

namespace WpfDemosCommonCode.Pdf.Security
{
    /// <summary>
    /// Represents a PDF graphic figure that defines the appearance of the signature field.
    /// </summary>
    /// <remarks>
    /// Figure defines appearance of the signature field and can be edited using mouse.
    /// </remarks>
    public class SignatureAppearanceGraphicsFigure : GraphicsFigureGroup
    {

        #region Fields

        /// <summary>
        /// The figure of background layer.
        /// </summary>
        VintasoftImageFigure _backgroundFigure;

        /// <summary>
        /// The signer name figure.
        /// </summary>
        TextBoxFigure _signerNameFigure;

        /// <summary>
        /// The signature image figure.
        /// </summary>
        VintasoftImageFigure _signatureImageFigure;

        /// <summary>
        /// The text figure.
        /// </summary>
        TextBoxFigure _textFigure;

        #endregion



        #region Consturctors

        /// <summary>
        /// Initializes a new instance of the <see cref="SignatureAppearanceGraphicsFigure"/> class.
        /// </summary>
        public SignatureAppearanceGraphicsFigure()
        {
            // background
            _backgroundFigure = new VintasoftImageFigure();
            _backgroundFigure.Image =
                DemosResourcesManager.GetResourceAsImage("VintasoftLogo_112x65.png");
            Add(_backgroundFigure);

            // text (right)
            _textFigure = new TextBoxFigure();
            _textFigure.AutoFontSize = true;
            _textFigure.TextAlignment =
                PdfContentAlignment.Left | PdfContentAlignment.Top | PdfContentAlignment.Bottom;
            Add(_textFigure);

            // signature image (left)
            _signatureImageFigure = new VintasoftImageFigure();
            Add(_signatureImageFigure);

            // signer name (left)
            _signerNameFigure = new TextBoxFigure();
            _signerNameFigure.AutoFontSize = true;
            _signerNameFigure.TextAlignment = PdfContentAlignment.Center;
            Add(_signerNameFigure);
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets or sets the text font.
        /// </summary>
        public PdfFont Font
        {
            get
            {
                return _textFigure.Font;
            }
            set
            {
                _signerNameFigure.Font = value;
                _textFigure.Font = value;
            }
        }

        PdfInteractiveFormSignatureField _signatureField = null;
        /// <summary>
        /// Gets or sets the signature field.
        /// </summary>
        public PdfInteractiveFormSignatureField SignatureField
        {
            get
            {
                return _signatureField;
            }
            set
            {
                if (_signatureField != value)
                {
                    _signatureField = value;
                    Font = _signatureField.Document.FontManager.GetStandardFont(PdfStandardFontType.TimesRoman);
                    SetDefaultLocations();
                    OnChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the background image.
        /// </summary>
        public VintasoftImage BackgroundImage
        {
            get
            {
                return _backgroundFigure.Image;
            }
            set
            {
                _backgroundFigure.Image = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public System.Windows.Media.Color BackgroundColor
        {
            get
            {
                return _backgroundFigure.BackgroundColor;
            }
            set
            {
                _backgroundFigure.BackgroundColor = value;
                OnChanged();
            }
        }

        bool _showSignerName = true;
        /// <summary>
        /// Gets or sets a value indicating whether the signer name must be shown.
        /// </summary>
        /// <value>
        /// <b>true</b> - the signer name must be shown;
        /// <b>false</b> - the signer name must NOT be shown.
        /// </value>
        public bool ShowSignerName
        {
            get
            {
                return _showSignerName;
            }
            set
            {
                _showSignerName = value;
                UpdateSignerName();
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text
        {
            get
            {
                return _textFigure.Text;
            }
            set
            {
                _textFigure.Text = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the signature image.
        /// </summary>
        public VintasoftImage SignatureImage
        {
            get
            {
                return _signatureImageFigure.Image;
            }
            set
            {
                _signatureImageFigure.Image = value;
                OnChanged();
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the name of the signer from current signatrue info.
        /// </summary>
        public void UpdateSignerName()
        {
            if (_showSignerName && _signatureField.SignatureInfo != null)
                _signerNameFigure.Text = _signatureField.SignatureInfo.SignerName;
            else
                _signerNameFigure.Text = null;
        }

        /// <summary>
        /// Sets default location of signature appearance elements.
        /// </summary>
        public void SetDefaultLocations()
        {
            // if the signature field is specified
            if (_signatureField != null)
            {
                SizeF size = _signatureField.Annotation.Rectangle.Size;

                RectangleF signerLogoRect, textRect;
                // if figure contains signer name or signature image
                if (_showSignerName || _signatureImageFigure.Image != null)
                {
                    signerLogoRect = new RectangleF(0, 0, size.Width / 2, size.Height);
                    textRect = new RectangleF(size.Width / 2, 0, size.Width / 2, size.Height);
                }
                else
                {
                    signerLogoRect = new RectangleF(0, 0, size.Width, size.Height);
                    textRect = signerLogoRect;
                }

                // padding
                float padding = Math.Min(size.Width, size.Height) / 20;
                signerLogoRect.Inflate(-padding, -padding);
                textRect.Inflate(-padding, -padding);

                // set location and size of figures
                _backgroundFigure.Location = PointF.Empty;
                _backgroundFigure.Size = size;
                _signerNameFigure.Location = signerLogoRect.Location;
                _signerNameFigure.Size = signerLogoRect.Size;
                _signatureImageFigure.Location = signerLogoRect.Location;
                _signatureImageFigure.Size = signerLogoRect.Size;
                _textFigure.Location = textRect.Location;
                _textFigure.Size = textRect.Size;
            }
        }

        /// <summary>
        /// Draws the appearance on signature field.
        /// </summary>
        public void DrawOnSignatureField()
        {
            using (PdfGraphics g = SignatureField.CreateAppearanceGraphics())
            {
                g.SaveGraphicsState();
                Draw(g);
                g.RestoreGraphicsState();
            }
        }

        /// <summary>
        /// Copies the state of the current object to the target object.
        /// </summary>
        /// <param name="target">Object to copy the state of the current object to.</param>
        protected override void CopyTo(GraphicsFigure target)
        {
            base.CopyTo(target);

            SignatureAppearanceGraphicsFigure typedTarget = (SignatureAppearanceGraphicsFigure)target;
            typedTarget.Clear();
            typedTarget._signatureField = _signatureField;
            typedTarget._showSignerName = _showSignerName;
            typedTarget._backgroundFigure = (VintasoftImageFigure)_backgroundFigure.Clone();
            typedTarget._textFigure = (TextBoxFigure)_textFigure.Clone();
            typedTarget._signatureImageFigure = (VintasoftImageFigure)_signatureImageFigure.Clone();
            typedTarget._signerNameFigure = (TextBoxFigure)_signerNameFigure.Clone();
            typedTarget.Add(typedTarget._backgroundFigure);
            typedTarget.Add(typedTarget._textFigure);
            typedTarget.Add(typedTarget._signatureImageFigure);
            typedTarget.Add(typedTarget._signerNameFigure);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override object Clone()
        {
            SignatureAppearanceGraphicsFigure result = new SignatureAppearanceGraphicsFigure();
            CopyTo(result);
            return result;
        }

        #endregion

    }
}
