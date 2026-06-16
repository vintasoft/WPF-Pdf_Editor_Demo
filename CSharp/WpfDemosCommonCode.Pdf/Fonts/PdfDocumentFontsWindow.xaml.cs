using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Drawing;
using Vintasoft.Imaging.Fonts;
#if !REMOVE_OCR_PLUGIN
using Vintasoft.Imaging.Ocr;
using Vintasoft.Imaging.Ocr.Results;
using Vintasoft.Imaging.Ocr.Tesseract;
#endif
using Vintasoft.Imaging.Pdf.Content.TextExtraction;
using Vintasoft.Imaging.Pdf.Tree.Fonts;

using WpfCommonCode.Imaging;

namespace WpfCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view fonts of PDF document.
    /// </summary>
    public partial class PdfDocumentFontsWindow : Window
    {

        #region Fields

        IList<PdfFont> _fonts;

        #endregion



        #region Constructors

        public PdfDocumentFontsWindow(IList<PdfFont> fonts)
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

            pdfFontViewerControl.ShowUnicodeInfo = true;
        }

        #endregion



        #region Properties

        bool _isPdfFontUnicodeTableChanged = false;
        /// <summary>
        /// Gets a value indicating whether the Unicode table of one or several PDF fonts is changed.
        /// </summary>
        public bool IsPdfFontUnicodeTableChanged
        {
            get { return _isPdfFontUnicodeTableChanged; }
        }

        #endregion



        #region Methods

        #region PUBLIC

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

        #endregion


        #region PRIVATE

        #region Create Unicode table for PDF font

        /// <summary>
        /// The "Create Unicode Table" button is clicked.
        /// </summary>
        private void createUnicodeTable_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_OCR_PLUGIN
            // get the selected PDF font
            PdfFont pdfFont = pdfFontViewerControl.PdfFont;
            // if font is not selected
            if (pdfFont == null)
                return;

            // get the PDF text symbols from PDF font
            PdfTextSymbol[] pdfTextSymbols = PdfTextSymbol.GetFontSymbols(pdfFont);
            // if font contains more than 256 symbols
            if (pdfTextSymbols.Length > 256)
            {
                MessageBox.Show("This demo project does not allow to process PDF font if font contains more than 256 symbols. Please change the code if necessary.");
                return;
            }

            try
            {
                // get a path to folder that contains TesseractOCR DLL
                string tesseractOcrFolder = GetTesseractOcrFolder();
                // create Tesseract OCR engine
                using (TesseractOcr tesseractOcr = new TesseractOcr(tesseractOcrFolder))
                {
                    // the selected OCR languages
                    OcrLanguage[] selectedOcrLanguages = new OcrLanguage[] { OcrLanguage.English };
                    // create dialog that allows to select languages for OCR
                    OcrLanguagesWindow dlg = new OcrLanguagesWindow(selectedOcrLanguages, tesseractOcr.SupportedLanguages);
                    dlg.Owner = this;
                    // if selected languages must be changed
                    if (dlg.ShowDialog() == true)
                    {
                        // update selected languages
                        selectedOcrLanguages = dlg.SelectedLanguages;
                    }
                    else
                    {
                        return;
                    }

                    // initialize OCR engine
                    OcrEngineSettings ocrEngineSettings = new OcrEngineSettings(selectedOcrLanguages);
                    ocrEngineSettings.RecognitionRegionType = RecognitionRegionType.RecognizeSingleBlock;
                    tesseractOcr.Init(ocrEngineSettings);

                    // dictionary: PDF text symbol => a rectangle on image, where symbol drawn
                    Dictionary<PdfTextSymbol, Rectangle> pdfTextSymbolToImageRectMap = new Dictionary<PdfTextSymbol, Rectangle>();
                    // create image with glyphs of PDF text symbols
                    using (VintasoftImage imageWithTextSymbols = CreateImageWithGlyphsOfPdfTextSymbols(pdfTextSymbols, ref pdfTextSymbolToImageRectMap))
                    {
#if DEBUG_FONT_ENCODING
                        // for debug purposes save the image with glyphs of PDF text symbols
                        imageWithTextSymbols.Save(string.Format("Font-{0}-symbols.png", pdfFont.FontName));
#endif

                        // recognizes text in image
                        OcrPage ocrResult = tesseractOcr.Recognize(imageWithTextSymbols);

                        // clear the Unicode overrides for PDF font
                        pdfFontViewerControl.ClearUnicodeOverrides();

                        // get the dictionary from PDF text symbol to the Unicode symbol
                        Dictionary<PdfTextSymbol, char> pdfTextSymbolToCharMap = GetPdfTextSymbolToCharMap(pdfFont, ocrResult, pdfTextSymbolToImageRectMap);
                        // for each PDF text symbol
                        foreach (PdfTextSymbol pdfTextSymbol in pdfTextSymbolToCharMap.Keys)
                        {
                            // get Unicode symbol for PDF text symbol
                            char unicodeSymbol = pdfTextSymbolToCharMap[pdfTextSymbol];
                            // if OCR recognized new Unicode symbol for PDF text symbol
                            if (pdfTextSymbol.Symbol != unicodeSymbol)
                            {
                                // save information about new Unicode symbol for PDF text symbol
                                pdfFontViewerControl.OverrideUnicodeSymbol(pdfTextSymbol, unicodeSymbol);
                            }
                        }
                    }
                }

                // enable the "Save Unicode table" button
                saveUnicodeTableButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
#endif
        }

#if !REMOVE_OCR_PLUGIN
        /// <summary>
        /// Creates image with glyphs of PDF text symbols.
        /// </summary>
        /// <param name="pdfTextSymbols">PDF text symbols.</param>
        /// <param name="pdfTextSymbolToImageRectMap">Dictionary: PDF text symbol => a rectangle on image, where symbol drawn.</param>
        /// <returns>An image with font symbols.</returns>
        private VintasoftImage CreateImageWithGlyphsOfPdfTextSymbols(PdfTextSymbol[] pdfTextSymbols, ref Dictionary<PdfTextSymbol, Rectangle> pdfTextSymbolToImageRectMap)
        {
            // a string that contains characters, which are available in PDF font
            string availableChars = "";

            // for each PDF text symbol
            foreach (PdfTextSymbol fontSymbol in pdfTextSymbols)
            {
                // add character to a string with available characters
                availableChars += fontSymbol.Symbol;
            }

            // the size of cell, where symbol is drawn
            const int CELL_SIZE = 200;

            // calculate the image width
            int imageWidth = CELL_SIZE * 8;
            // calculate the image height
            int imageHeight = CELL_SIZE * (availableChars.Length / 8);
            if ((availableChars.Length % 8) != 0)
                imageHeight += CELL_SIZE;
            imageHeight += CELL_SIZE;

            // create image that will contain glyphs of PDF text symbols
            VintasoftImage imageWithTextSymbols = new VintasoftImage(imageWidth, imageHeight, PixelFormat.Bgr24);

            // create graphics object for image
            using (DrawingEngine drawingEngine = imageWithTextSymbols.CreateDrawingEngine())
            {
                // fill the image
                drawingEngine.FillRectangle(Color.FromArgb(255, Color.White), new RectangleF(0, 0, imageWidth, imageHeight));

                using (IDrawingBrush brush = drawingEngine.DrawingFactory.CreateSolidBrush(Color.FromArgb(255, Color.Black)))
                {
                    using (IDrawingPen pen = drawingEngine.DrawingFactory.CreatePen(brush, 1))
                    {
                        // for each PDF text symbol
                        for (int i = 0; i < pdfTextSymbols.Length; i++)
                        {
                            PdfTextSymbol fontSymbol = pdfTextSymbols[i];

                            // get the outline of PDF text symbol
                            FontSymbolOutline fontSymbolOutline = fontSymbol.GetOutline();
                            // get graphics path that represents outline of PDF text symbol
                            IGraphicsPath graphicsPath = fontSymbolOutline.GetGraphicsPath();
                            // if graphics path exists
                            if (graphicsPath != null)
                            {
                                // calculate the position of graphics path on image

                                int x = (i % 8) * CELL_SIZE + CELL_SIZE;
                                int y = (i / 8) * CELL_SIZE + CELL_SIZE;

                                // save the transform for drawing engine
                                drawingEngine.SaveTransform();

                                // apply necessary transform to the drawing engine
                                AffineMatrix transform = new AffineMatrix();
                                transform.Translate(x, y);
                                transform.ScalePrepend(50f / 1000, 50f / 1000);
                                drawingEngine.MultiplyTransformPrepend(transform);

                                // fill the graphics path on image
                                drawingEngine.FillPath(brush, graphicsPath);

                                // restore the transform for drawing engine
                                drawingEngine.RestoreTransform();

                                // save information about position of PDF text symbol on image
                                pdfTextSymbolToImageRectMap.Add(pdfTextSymbols[i], new Rectangle(x, y, CELL_SIZE, CELL_SIZE));
                            }
                        }
                    }
                }
            }

            return imageWithTextSymbols;
        }

        /// <summary>
        /// Returns the dictionary from PDF text symbol to the Unicode symbol.
        /// </summary>
        /// <param name="pdfFont">PDF font.</param>
        /// <param name="ocrResult">OCR result.</param>
        /// <param name="pdfTextSymbolToImageRectMap">The dictionary from PDF text symbol to the image rectangle.</param>
        /// <returns>The dictionary from PDF text symbol to the Unicode symbol.</returns>
        private Dictionary<PdfTextSymbol, char> GetPdfTextSymbolToCharMap(
            PdfFont pdfFont,
            OcrPage ocrResult,
            Dictionary<PdfTextSymbol, Rectangle> pdfTextSymbolToImageRectMap)
        {
            // Dictionary: PDF text symbol => Unicode char
            Dictionary<PdfTextSymbol, char> pdfTextSymbolToCharMap = new Dictionary<PdfTextSymbol, char>();
            // Dictionary: PDF text symbol => char bounding box
            Dictionary<PdfTextSymbol, Rectangle> pdfTextSymbolToCharBoundingBoxMap = new Dictionary<PdfTextSymbol, Rectangle>();

            // for each region in OCR result
            foreach (OcrTextRegion ocrRegion in ocrResult.Regions)
            {
                // for each paragraph in region' paragraphs
                foreach (OcrParagraph ocrParagraph in ocrRegion.Paragraphs)
                {
                    // for each text line in text lines of paragraph
                    foreach (OcrTextLine ocrTextLine in ocrParagraph.TextLines)
                    {
                        // for each word in words of text line
                        foreach (OcrWord ocrWord in ocrTextLine.Words)
                        {
                            // for each OCR symbol in word's symbols
                            foreach (OcrSymbol ocrSymbol in ocrWord.Symbols)
                            {
                                // get the bounding box of OCR symbol
                                Rectangle ocrSymbolBoundingBox = ocrSymbol.GetBoundingBox();
                                // the maximum area for intersection rectangle between the bounding box of OCR symbol and bounding box of PDF text symbol
                                int intersectionRectAreaMax = 0;
                                // for each PDF text symbol in dictionary with bounding boxes
                                foreach (PdfTextSymbol pdfTextSymbol in pdfTextSymbolToImageRectMap.Keys)
                                {
                                    // get the intersection rectangle between the bounding box of OCR symbol and bounding box of PDF text symbol
                                    Rectangle intersectionRect = Rectangle.Intersect(ocrSymbolBoundingBox, pdfTextSymbolToImageRectMap[pdfTextSymbol]);
                                    // get the area of intersection rectangle
                                    int intersectionRectArea = intersectionRect.Width * intersectionRect.Height;
                                    // if the area of intersection rectangle is greater than the maximum area for intersection rectangle
                                    if (intersectionRectArea > intersectionRectAreaMax)
                                    {
                                        // if OCR symbol contains only 1 text char
                                        if (ocrSymbol.Text.Length == 1)
                                        {
                                            // if we do not have information about Unicode char for PDF text symbol
                                            if (!pdfTextSymbolToCharMap.ContainsKey(pdfTextSymbol))
                                            {
                                                // save information about Unicode char for PDF text symbol
                                                pdfTextSymbolToCharMap.Add(pdfTextSymbol, ocrSymbol.Text[0]);
                                                // save information about char bounding box for PDF text symbol
                                                pdfTextSymbolToCharBoundingBoxMap.Add(pdfTextSymbol, ocrSymbolBoundingBox);

                                                // update the maximum area for intersection rectangle
                                                intersectionRectAreaMax = intersectionRectArea;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

#if DEBUG_FONT_ENCODING
            // create image that contains glyph and Unicode symbol for each recognized PDF text symbol

            string imageWithTextSymbolsName = string.Format("Font-{0}-symbols.png", pdfFont.FontName);
            using (VintasoftImage image = new VintasoftImage(imageWithTextSymbolsName))
            {
                using (DrawingEngine drawingEngine = image.CreateDrawingEngine())
                {
                    foreach (PdfTextSymbol pdfTextSymbol1 in pdfTextSymbolToCharMap.Keys)
                    {
                        drawingEngine.DrawString(
                            string.Format("{0}", pdfTextSymbolToCharMap[pdfTextSymbol1]),
                            drawingEngine.DrawingFactory.CreateSystemFont("Arial", 16, false, false),
                            drawingEngine.DrawingFactory.CreateSolidBrush(Color.Red),
                            new PointF(pdfTextSymbolToImageRectMap[pdfTextSymbol1].X, pdfTextSymbolToImageRectMap[pdfTextSymbol1].Y));

                        drawingEngine.DrawRectangle(drawingEngine.DrawingFactory.CreatePen(Color.Green), pdfTextSymbolToImageRectMap[pdfTextSymbol1]);

                        drawingEngine.DrawRectangle(drawingEngine.DrawingFactory.CreatePen(Color.Blue), pdfTextSymbolToCharBoundingBoxMap[pdfTextSymbol1]);
                    }
                }
                image.Save(string.Format("Font-{0}-symbols-OCR.png", pdfFont.FontName));
            }
#endif

            return pdfTextSymbolToCharMap;
        }

        /// <summary>
        /// Returns path to a folder that contains TesseractOCR DLL.
        /// </summary>
        /// <returns>Path to a folder that contains TesseractOCR DLL if DLL is found; otherwise, <b>null</b>.</returns>
        private string GetTesseractOcrFolder()
        {
            // TesseractOCR DLL filename
            string dllFilename;
            // if is 64-bit system then
            if (IntPtr.Size == 8)
                dllFilename = "Tesseract5.Vintasoft.x64.dll";
            else
                dllFilename = "Tesseract5.Vintasoft.x86.dll";

            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);

            // an array of folder, where TesseractOCR DLL should be searched
            string[] directories = new string[]
            {
                @"..\TesseractOCR\",
                @"..\..\TesseractOCR\",
                @"..\..\..\..\Bin\TesseractOCR\",
                @"..\..\..\..\..\Bin\TesseractOCR\",
                @"..\..\..\..\..\..\..\Bin\TesseractOCR\"
            };

            // for each folder
            foreach (string dir in directories)
            {
                string dllDirectory = Path.Combine(currentDirectory, dir);
                // if folder containt TesseractOCR DLL
                if (File.Exists(Path.Combine(dllDirectory, dllFilename)))
                {
                    return dllDirectory;
                }
            }

            return null;
        }

#endif

        #endregion


        /// <summary>
        /// Handles the SelectionChanged event of fontComboBox object.
        /// </summary>
        private void fontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pdfFontViewerControl.PdfFont = _fonts[fontComboBox.SelectedIndex];

            // disable the "Save Unicode table" button
            saveUnicodeTableButton.IsEnabled = false;
        }

        /// <summary>
        /// Handles the ValueChanged event of cellSizeNumericUpDown object.
        /// </summary>
        private void cellSizeNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            pdfFontViewerControl.CellSize = new System.Windows.Size(cellSizeNumericUpDown.Value, cellSizeNumericUpDown.Value);
        }

        /// <summary>
        /// Handles the MouseMove event of pdfFontViewerControl object.
        /// </summary>
        private void pdfFontViewerControl_MouseMove(object sender, MouseEventArgs e)
        {
            // get PDF font symbol
            System.Windows.Point location = e.GetPosition(pdfFontViewerControl);
            location.Offset(0, pdfFontViewerControl.VerticalOffset);
            PdfTextSymbol pdfTextSymbol = pdfFontViewerControl.GetTextSymbol(location);

            // if symbol is not found
            if (pdfTextSymbol == null)
            {
                statusLabel.Content = "";
            }
            else
            {
                // if the symbol has character sequence
                if (pdfTextSymbol.HasCharacterSequence)
                {
                    statusLabel.Content =
                        string.Format("Symbols: '{0}'; Content code: {1}; Width: {2}", pdfTextSymbol.Symbols, pdfTextSymbol.ContentSymbolCode, pdfTextSymbol.Width);
                }
                else
                {
                    statusLabel.Content =
                        string.Format(
                            "Symbol: '{0}'; Unicode: {1}; Content code: {2}; Width: {3}",
                            pdfTextSymbol.Symbol,
                            pdfTextSymbol.SymbolCode,
                            pdfTextSymbol.ContentSymbolCode,
                            pdfTextSymbol.Width);
                }
            }
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of pdfFontViewerControl object.
        /// </summary>
        private void pdfFontViewerControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // get PDF font symbol
            System.Windows.Point location = e.GetPosition(pdfFontViewerControl);
            location.Offset(0, pdfFontViewerControl.VerticalOffset);
            PdfTextSymbol pdfTextSymbol = pdfFontViewerControl.GetTextSymbol(location);

            // if PDF font is found
            if (pdfTextSymbol != null)
            {
                // copy selected symbol to clipboard
                Clipboard.SetText(pdfTextSymbol.Symbols);

                // get cached value of Unicode symbol for PDF text symbol from PDF font viewer control
                char? unicodeSymbol = pdfFontViewerControl.GetUnicodeOverride(pdfTextSymbol);
                // if Unicode symbol is not found
                if (unicodeSymbol == null)
                {
                    // get Unicode symbol from PDF text symbol
                    unicodeSymbol = pdfTextSymbol.Symbol;
                }

                // create a dialog that allows to edit PDF text symbol
                EditPdfFontSymbolWindow dlg = new EditPdfFontSymbolWindow(pdfTextSymbol, unicodeSymbol);
                dlg.Owner = this;
                // show the dialog
                if (dlg.ShowDialog() == true)
                {
                    // if new Unicode symbol is specified for PDF text symbol
                    if (pdfTextSymbol.Symbol != dlg.UnicodeSymbol)
                    {
                        if (dlg.UnicodeSymbol.HasValue)
                        {
                            // save information about new Unicode symbol for PDF text symbol
                            pdfFontViewerControl.OverrideUnicodeSymbol(pdfTextSymbol, dlg.UnicodeSymbol.Value);

                            // enable the "Save Unicode table" button
                            saveUnicodeTableButton.IsEnabled = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of saveUnicodeTable object.
        /// </summary>
        private void saveUnicodeTable_Click(object sender, RoutedEventArgs e)
        {
            // get PDF font
            PdfFont pdfFont = pdfFontViewerControl.PdfFont;

            // get the dictionary from content code to the Unicode symbol for PDF font
            Dictionary<uint, char> contentCodeToUnicodeSymbol = pdfFontViewerControl.GetContentCodeToUnicodeSymbols();
            // set the Unicode mapping for PDF font
            pdfFont.SetUnicodeMapping(contentCodeToUnicodeSymbol);

            // reload information about PDF font in PDF font viewe control
            pdfFontViewerControl.ReloadPdfFontInfo();

            // specify that Unicode table of PDF font is changed
            _isPdfFontUnicodeTableChanged = true;
        }

        #endregion

        #endregion

    }
}
