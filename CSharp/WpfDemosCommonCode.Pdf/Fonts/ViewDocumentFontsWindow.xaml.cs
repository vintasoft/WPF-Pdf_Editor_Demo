using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Content.TextExtraction;
using Vintasoft.Imaging.Pdf.Tree.Fonts;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view fonts of PDF document.
    /// </summary>
    public partial class ViewDocumentFontsWindow : Window
    {

        #region Fields

        IList<PdfFont> _fonts;

        #endregion



        #region Constructor

        public ViewDocumentFontsWindow(IList<PdfFont> fonts)
        {
            InitializeComponent();

            _fonts = fonts;
            for (int i = 0; i < _fonts.Count; i++)
            {
                PdfFont font = _fonts[i];
                string fontProgramType;
                if (font.StandardFontType != PdfStandardFontType.NotStandard)
                    fontProgramType = "Standard";
                else
                {
                    if (font.IsFullyDefined)
                        fontProgramType = "Embedded";
                    else
                        fontProgramType = "External";
                }
                string fontInfo = string.Format("[{1}] {0} ({2}, {3})", font.FontName, font.ObjectNumber, font.FontType, fontProgramType);
                fontComboBox.Items.Add(fontInfo);
            }
            fontComboBox.SelectedIndex = 0;
        }

        #endregion



        #region Methods

        public static string GetFontInformation(PdfFont font)
        {
            StringBuilder fontProgramType = new StringBuilder(font.FontType.ToString());
            if (font.StandardFontType != PdfStandardFontType.NotStandard)
            {
                fontProgramType.Append(", Standard");
            }
            else
            {
                if (font.IsFullyDefined)
                    fontProgramType.Append(", Embedded");
                else
                    fontProgramType.Append(", External");
                if (font.IsVertical)
                    fontProgramType.Append(", Vertical");
            }
            return string.Format("[{1}] {0} ({2})", font.FontName, font.ObjectNumber, fontProgramType);
        }

        /// <summary>
        /// Handles the SelectionChanged event of FontComboBox object.
        /// </summary>
        private void fontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pdfFontViewerControl.PdfFont = _fonts[fontComboBox.SelectedIndex];
        }

        /// <summary>
        /// Handles the ValueChanged event of CellSizeNumericUpDown object.
        /// </summary>
        private void cellSizeNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            pdfFontViewerControl.CellSize = new Size(cellSizeNumericUpDown.Value, cellSizeNumericUpDown.Value);
        }

        /// <summary>
        /// Handles the MouseMove event of PdfFontViewerControl object.
        /// </summary>
        private void pdfFontViewerControl_MouseMove(object sender, MouseEventArgs e)
        {
            Point location = e.GetPosition(pdfFontViewerControl);
            location.Offset(0, pdfFontViewerControl.VerticalOffset);
            PdfTextSymbol symbol = pdfFontViewerControl.GetTextSymbol(location);
            
            // if symbol is not found
            if (symbol == null)
            {
                statusLabel.Content = "";
            }
            else
            {
                // if the symbol has character sequence
                if (symbol.HasCharacterSequence)
                {
                    statusLabel.Content =
                        string.Format("Symbols: '{0}'; Content code: {1}; Width: {2}", symbol.Symbols, symbol.ContentSymbolCode, symbol.Width);
                }
                else
                {
                    statusLabel.Content =
                        string.Format(
                            "Symbol: '{0}'; Unicode: {1}; Content code: {2}; Width: {3}",
                            symbol.Symbol,
                            symbol.SymbolCode,
                            symbol.ContentSymbolCode,
                            symbol.Width);
                }
            }
        }

        #endregion

    }
}
