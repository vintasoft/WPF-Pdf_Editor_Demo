using System.Drawing;

using Vintasoft.Imaging.Pdf.Drawing;
using Vintasoft.Imaging.Pdf.Drawing.GraphicsFigures;
using Vintasoft.Imaging.Pdf.Tree.Fonts;


namespace WpfDemosCommonCode.Pdf.Security
{
    /// <summary>
    /// Represents a PDF graphic figure that defines the appearance of the redaction mark.
    /// </summary>
    /// <remarks>
    /// Figure defines appearance of the redaction mark.
    /// </remarks>
    public class RedactionMarkAppearanceGraphicsFigure : GraphicsFigureGroup
    {

        #region Fields

        /// <summary>
        /// The background.
        /// </summary>
        PolygonFigure _background;

        /// <summary>
        /// The text figure.
        /// </summary>
        TextBoxFigure _textFigure;

        #endregion



        #region Consturctors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionMarkAppearanceGraphicsFigure"/> class.
        /// </summary>
        public RedactionMarkAppearanceGraphicsFigure()
        {
            _background = new PolygonFigure();
            Add(_background);

            _textFigure = new TextBoxFigure();
            _textFigure.TextAlignment = PdfContentAlignment.Center;
            Add(_textFigure);

            FillColor = Color.Black;
            TextColor = Color.Red;
            AutoFontSize = false;
            FontSize = 14;
            TextAlignment = PdfContentAlignment.Center;
        }

        #endregion



        #region Properties

        PdfFont _font;
        /// <summary>
        /// Gets or sets the text font.
        /// </summary>
        public PdfFont Font
        {
            get
            {
                return _font;
            }
            set
            {
                _font = value;
                _textFigure.Font = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the size of the font.
        /// </summary>
        public float FontSize
        {
            get
            {
                return _textFigure.FontSize;
            }
            set
            {
                _textFigure.FontSize = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the appearance can calculate font size automatically.
        /// </summary>
        public bool AutoFontSize
        {
            get
            {
                return _textFigure.AutoFontSize;
            }
            set
            {
                _textFigure.AutoFontSize = value;
                OnChanged();
            }
        }


        /// <summary>
        /// Gets or sets the redacted area fill color.
        /// </summary>
        public Color FillColor
        {
            get
            {
                if (_background.Brush == null)
                    return Color.Empty;
                return _background.Brush.Color;
            }
            set
            {
                if (value == Color.Empty || value == Color.Transparent)
                    _background.Brush = null;
                else
                    _background.Brush = new PdfBrush(value);
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the redacted area border color.
        /// </summary>
        public Color BorderColor
        {
            get
            {
                if (_background.Pen == null)
                    return Color.Empty;
                return _background.Pen.Color;
            }
            set
            {
                if (value == Color.Empty || value == Color.Transparent)
                    _background.Pen = null;
                else
                    _background.Pen = new PdfPen(value, BorderWidth);
                OnChanged();
            }
        }

        float _borderWidth = 0;
        /// <summary>
        /// Gets or sets the redacted area border width.
        /// </summary>
        public float BorderWidth
        {
            get
            {
                return _borderWidth;
            }
            set
            {
                _borderWidth = value;
                if (_background.Pen != null)
                    _background.Pen.Width = value;
                OnChanged();
            }
        }


        /// <summary>
        /// Gets or sets the overlay text color.
        /// </summary>
        public Color TextColor
        {
            get
            {
                if (_textFigure.TextBrush == null)
                    return Color.Empty;
                return _textFigure.TextBrush.Color;
            }
            set
            {
                if (value == Color.Empty)
                    _textFigure.TextBrush = null;
                else
                    _textFigure.TextBrush = new PdfBrush(value);
                OnChanged();
            }
        }


        /// <summary>
        /// Gets or sets the overlay text.
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
        /// Gets or sets an overlay text alignment.
        /// </summary>
        public PdfContentAlignment TextAlignment
        {
            get
            {
                return _textFigure.TextAlignment;
            }
            set
            {
                _textFigure.TextAlignment = value;
                OnChanged();
            }
        }

        #endregion       

    }
}
