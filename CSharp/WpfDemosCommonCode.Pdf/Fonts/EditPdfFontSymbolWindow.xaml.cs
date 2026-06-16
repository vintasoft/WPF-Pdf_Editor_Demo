using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

using Vintasoft.Imaging.Drawing.Gdi;
using Vintasoft.Imaging.Pdf.Content.TextExtraction;
using Vintasoft.Imaging.Wpf;

namespace WpfCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to edit Unicode symbol of PDF text symbol.
    /// </summary>
    public partial class EditPdfFontSymbolWindow : Window
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EditPdfFontSymbolWindow"/> class.
        /// </summary>
        /// <param name="pdfTextSymbol">The PDF text symbol.</param>
        /// <param name="unicodeSymbol">The unicode symbol.</param>
        public EditPdfFontSymbolWindow(PdfTextSymbol pdfTextSymbol, char? unicodeSymbol)
        {
            InitializeComponent();

            if (pdfTextSymbol == null)
                throw new ArgumentNullException();

            _pdfTextSymbol = pdfTextSymbol;
            _unicodeSymbol = unicodeSymbol;

            if (unicodeSymbol == null)
                unicodeSymbolTextBox.Text = pdfTextSymbol.Symbols;
            else
                unicodeSymbolTextBox.Text = string.Format("{0}", unicodeSymbol);

            try
            {
                // draw glyph on canvas

                GdiGraphicsPath glyphGraphicsPath = (GdiGraphicsPath)_pdfTextSymbol.GetAsGraphicsPath();

                Path glyphPath = new Path();
                glyphPath.Stroke = Brushes.Black;
                glyphPath.StrokeThickness = 1;
                glyphPath.Fill = Brushes.Black;

                PathGeometry glyphPathGeometry = WpfObjectConverter.CreateWindowsPathGeometry(glyphGraphicsPath.Source);
                Rect glyphBoundingBox = glyphPathGeometry.Bounds;
                Matrix m = new Matrix();
                m.Translate(
                    glyphBoundingBox.X - glyphBoundingBox.X / 1000 + (glyphCanvas.Width - glyphBoundingBox.Width / 1000 * glyphCanvas.Width) / 2,
                    glyphBoundingBox.Y - glyphBoundingBox.Y / 1000 + (glyphCanvas.Height - glyphBoundingBox.Height / 1000 * glyphCanvas.Height) / 2 - 70);
                m.ScalePrepend(glyphCanvas.Width / 1000, glyphCanvas.Height / 1000);
                glyphPathGeometry.Transform = new MatrixTransform(m);

                glyphPath.Data = glyphPathGeometry;

                glyphCanvas.Children.Add(glyphPath);
            }
            catch
            {
            }
        }

        #endregion



        #region Properties


        PdfTextSymbol _pdfTextSymbol;
        /// <summary>
        /// Gets the PDF text symbol.
        /// </summary>
        public PdfTextSymbol PdfTextSymbol
        {
            get
            {
                return _pdfTextSymbol;
            }
        }

        char? _unicodeSymbol;
        /// <summary>
        /// Gets the Unicode symbol of PDF text symbol.
        /// </summary>
        public char? UnicodeSymbol
        {
            get
            {
                return _unicodeSymbol;
            }
        }

        #endregion


        #region Methods

        /// <summary>
        /// Handles the Click event of okButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(unicodeSymbolTextBox.Text))
            {
                MessageBox.Show("Unicode symbol cannot be empty.");
                return;
            }

            _unicodeSymbol = unicodeSymbolTextBox.Text[0];

            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of cancelButton object.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

    }
}
