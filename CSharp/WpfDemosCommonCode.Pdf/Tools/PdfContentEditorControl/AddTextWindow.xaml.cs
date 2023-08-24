using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Content.TextExtraction;
using Vintasoft.Imaging.Pdf.Tree.Fonts;
using Vintasoft.Imaging.Wpf;


namespace WpfDemosCommonCode
{
    /// <summary>
    /// A window that allows to enter the text, which must be added to a PDF document.
    /// </summary>
    public partial class AddTextWindow : Window
    {

        #region Fields

        List<PdfFont> _fonts;
        PdfDocument _document;
        PdfDocument _tempDocument = null;

        #endregion



        #region Constructor

        public AddTextWindow(PdfDocument document, IList<PdfFont> fonts, string initialText)
        {
            InitializeComponent();

            _document = document;
            _fonts = new List<PdfFont>();
            _fonts.AddRange(fonts);
            if (_fonts.Count > 0)
            {
                for (int i = 0; i < _fonts.Count; i++)
                    fontComboBox.Items.Add(string.Format("[N{1}] {0}", _fonts[i].FontName, _fonts[i].ObjectNumber));
                fontComboBox.SelectedIndex = 0;
            }
            fontSizeComboBox.Text = "12";
            textBox.Text = initialText;
            okButton.Focus();
        }

        #endregion



        #region Properties

        public string TextToAdd
        {
            get
            {
                return textBox.Text;
            }
        }

        public PdfFont TextFont
        {
            get
            {
                return _fonts[fontComboBox.SelectedIndex];
            }
        }

        public float TextSize
        {
            get
            {
                return float.Parse(fontSizeComboBox.Text, CultureInfo.InvariantCulture);
            }
        }

        public System.Drawing.Color TextColor
        {
            get
            {
                return WpfObjectConverter.CreateDrawingColor(colorPanelControl.Color);                
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of CancelButton object.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_tempDocument != null)
                _tempDocument.Dispose();
            DialogResult = false;
        }

        /// <summary>
        /// Handles the Click event of OkButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (fontComboBox.SelectedItem == null)
            {
                DemosTools.ShowWarningMessage("Text font is not specified.");
                return;
            }
            if (_tempDocument != null)
                _tempDocument.Dispose();
            DialogResult = true;
        }

        /// <summary>
        /// Handles the TextChanged event of TextBox object.
        /// </summary>
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            okButton.IsEnabled = textBox.Text.Length > 0;
        }

        /// <summary>
        /// Handles the SelectionChanged event of FontComboBox object.
        /// </summary>
        private void fontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pdfFontViewerControl.PdfFont = _fonts[fontComboBox.SelectedIndex];
        }

        /// <summary>
        /// Handles the MouseUp event of PdfFontViewerControl object.
        /// </summary>
        private void pdfFontViewerControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point location = e.GetPosition(pdfFontViewerControl);
            location.Offset(0, pdfFontViewerControl.VerticalOffset);
            PdfTextSymbol symbol = pdfFontViewerControl.GetTextSymbol(location);
            if (symbol != null)
                textBox.AppendText(symbol.Symbol.ToString());
        }

        /// <summary>
        /// Handles the Click event of AddFontButton object.
        /// </summary>
        private void addFontButton_Click(object sender, RoutedEventArgs e)
        {
            CreateFontWindow form = new CreateFontWindow(_document);
            form.Owner = this;
            if (form.ShowDialog().Value)
            {
                PdfFont font = form.SelectedFont;
                _fonts.Add(font);
                fontComboBox.Items.Add(string.Format("[N{1}] {0}", font.FontName, font.ObjectNumber));
                fontComboBox.SelectedIndex = fontComboBox.Items.Count - 1;
            }
        }

        #endregion

    }
}
