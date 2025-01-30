using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Text;
using Vintasoft.Imaging.Utils;
using Vintasoft.Imaging.Wpf.UI;
using Vintasoft.Imaging.Wpf;

using Vintasoft.Imaging.Wpf.UI.VisualTools;

namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A control that allows to select text on document page and extract text from document page.
    /// </summary>
    public partial class WpfTextSelectionControl : UserControl
    {

        #region Fields

        /// <summary>
        /// The context menu of visual tool.
        /// </summary>
        ContextMenu _textSelectionContextMenu = null;

        /// <summary>
        /// The copy menu item.
        /// </summary>
        MenuItem _copyMenuItem = null;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfTextSelectionControl"/> class.
        /// </summary>
        public WpfTextSelectionControl()
        {
            InitializeComponent();

            selectionModeFullLinesRadioButton.Tag = TextSelectionMode.UseFullLines;
            selectionModeRectangleRadioButton.Tag = TextSelectionMode.Rectangle;

            formattingModeParagraphsRadioButton.Tag = new TextRegionParagraphFormatter();
            formattingModeLinesRadioButton.Tag = new TextRegionLinesFormatter();
            formattingModeMonospaceRadioButton.Tag = new TextRegionMonospaceFormatter();

            _textSelectionContextMenu = (ContextMenu)Resources["TextSelectionContextMenu"];
            _copyMenuItem = (MenuItem)_textSelectionContextMenu.Items[0];
        }

        #endregion



        #region Properties

        WpfTextSelectionTool _textSelectionTool = null;
        /// <summary>
        /// Gets or sets the text selection tool.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public WpfTextSelectionTool TextSelectionTool
        {
            get
            {
                return _textSelectionTool;
            }
            set
            {
                // if tool is NOT changed
                if (_textSelectionTool == value)
                    return;

                // if old tool exists
                if (_textSelectionTool != null)
                    // unsubscribe from the tool events
                    UnsubscribeFromVisualToolEvents(_textSelectionTool);

                // save reference to the new tool
                _textSelectionTool = value;

                // if new tool exists
                if (_textSelectionTool != null)
                {
                    // set default text formatter
                    _textSelectionTool.TextFormatter = (TextRegionFormatter)formattingModeParagraphsRadioButton.Tag;

                    // update the selection mode radio buttons
                    UpdateSelectionModeRadioButtons(_textSelectionTool.SelectionMode);

                    // update the formatting mode radio buttons
                    UpdateFormattingModeRadioButtons(_textSelectionTool.TextFormatter);

                    // subscribe to the tool events
                    SubscribeToVisualToolEvents(_textSelectionTool);
                }

                UpdateUI();
            }
        }

        System.Drawing.Color _higlhightColor = System.Drawing.Color.FromArgb(255, 255, 0);
        /// <summary>
        /// Gets or sets the color that is used for highlighting the text.
        /// </summary>
        public System.Drawing.Color HighlightColor
        {
            get
            {
                return _higlhightColor;
            }
            set
            {
                _higlhightColor = value;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the user interface of this control.
        /// </summary>
        private void UpdateUI()
        {
            mainPanel.IsEnabled =
                _textSelectionTool != null &&
                _textSelectionTool.ImageViewer != null &&
                _textSelectionTool.ImageViewer.Image != null &&
                _textSelectionTool.FocusedImageHasTextRegion;

            if (mainPanel.IsEnabled)
            {
                bool textSelected = _textSelectionTool.HasSelectedText;

                saveAsTextButton.IsEnabled = textSelected;
                caretWidthNumericUpDown.Value = _textSelectionTool.TextCaretWidth;
                caretBlinkingIntervalNumericUpDown.Value = _textSelectionTool.TextCaretBlinkingInterval;
                mouseSelectionCheckBox.IsChecked = _textSelectionTool.IsMouseSelectionEnabled;
                keyboardSelectionCheckBox.IsChecked = _textSelectionTool.IsKeyboardSelectionEnabled;
            }
        }

        /// <summary>
        /// WpfTextSelectionTool is activated.
        /// </summary>
        private void selectionTool_Activated(object sender, EventArgs e)
        {
            WpfVisualTool tool = (WpfVisualTool)sender;
            SubscribeToImageViewerEvents(tool.ImageViewer);
            UpdateUI();
        }

        /// <summary>
        /// WpfTextSelectionTool is deactivated.
        /// </summary>
        private void selectionTool_Deactivated(object sender, EventArgs e)
        {
            WpfVisualTool tool = (WpfVisualTool)sender;
            UnsubscribeFromImageViewerEvents(tool.ImageViewer);
            mainPanel.IsEnabled = false;
        }

        /// <summary>
        /// Text selection in WpfTextSelectionTool is changed.
        /// </summary>
        private void selectionTool_SelectionChanged(object sender, EventArgs e)
        {
            if (IsEnabled)
            {
                // if selected region is not empty
                if (_textSelectionTool.SelectedRegion != null)
                    textExtractionTextBox.Text = _textSelectionTool.SelectedRegion.TextContent;
                else
                    textExtractionTextBox.Text = "";

                UpdateUI();
            }
        }

        /// <summary>
        /// Mouse is down.
        /// </summary>
        private void selectionTool_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEnabled && mainPanel.IsEnabled)
            {
                if (e.ChangedButton == MouseButton.Right)
                {
                    WpfImageViewer imageViewer = _textSelectionTool.ImageViewer;
                    if (imageViewer != null)
                    {
                        // show context menu
                        _textSelectionContextMenu.IsOpen = true;
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Selection mode is changed.
        /// </summary>
        private void selectionTool_SelectionModeChanged(
            object sender,
            PropertyChangedEventArgs<TextSelectionMode> e)
        {
            // update the selection mode radio buttons
            UpdateSelectionModeRadioButtons(e.NewValue);
        }

        /// <summary>
        /// Text formatter is changed.
        /// </summary>
        private void selectionTool_TextFormatterChanged(
            object sender,
            PropertyChangedEventArgs<TextRegionFormatter> e)
        {
            // update the formatting mode radio buttons
            UpdateFormattingModeRadioButtons(e.NewValue);
        }

        /// <summary>
        /// Focused text symbol is changed.
        /// </summary>
        private void SelectionTool_FocusedTextSymbolChanged(object sender, PropertyChangedEventArgs<TextRegionSymbol> e)
        {
            if (IsEnabled)
                ShowSymbolInfo(e.NewValue);
        }     

        /// <summary>
        /// Focused image in image viewer is changed.
        /// </summary>
        private void ImageViewer_FocusedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Selection mode is changed.
        /// </summary>
        private void selectionModeRectangleRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                RadioButton radioButton = (RadioButton)sender;
                TextSelectionMode selectionMode = (TextSelectionMode)radioButton.Tag;
                if (_textSelectionTool.SelectionMode != selectionMode)
                    _textSelectionTool.SelectionMode = selectionMode;
            }
        }

        /// <summary>
        /// Formatting mode is changed.
        /// </summary>
        private void formattingModeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (_textSelectionTool == null)
                return;
            RadioButton radioButton = (RadioButton)sender;
            _textSelectionTool.TextFormatter = (TextRegionFormatter)radioButton.Tag;
        }

        /// <summary>
        /// Context menu strip is opening.
        /// </summary>
        private void TextSelectionContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            // update the user interface
            _copyMenuItem.IsEnabled = _textSelectionTool.HasSelectedText;
        }

        /// <summary>
        /// The "Copy" button is clicked.
        /// </summary>
        private void copyToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Copy();
        }

        /// <summary>
        /// Copies all selected text to the clipboard.
        /// </summary>
        private void Copy()
        {
            string text = string.Empty;

            // if selected region exists
            if (_textSelectionTool.HasSelectedText)
                text = _textSelectionTool.SelectedText;

            // copy text to clipboard
            Clipboard.SetText(text, TextDataFormat.UnicodeText);
        }

        /// <summary>
        /// The "Select All" button is clicked.
        /// </summary>
        private void selectAllToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectAll();
        }

        /// <summary>
        /// Selects all text on current document page.
        /// </summary>
        private void SelectAll()
        {
            _textSelectionTool.SelectAll();
        }

        /// <summary>
        /// Subscribes to the events of WpfTextSelectionTool.
        /// </summary>
        /// <param name="selectionTool">The selection tool.</param>
        private void SubscribeToVisualToolEvents(WpfTextSelectionTool selectionTool)
        {
            selectionTool.SelectionChanged += new EventHandler(selectionTool_SelectionChanged);
            selectionTool.Activated += new EventHandler(selectionTool_Activated);
            selectionTool.Deactivated += new EventHandler(selectionTool_Deactivated);
            selectionTool.MouseDown += new MouseButtonEventHandler(selectionTool_MouseDown);
            selectionTool.SelectionModeChanged += new PropertyChangedEventHandler<TextSelectionMode>(selectionTool_SelectionModeChanged);
            selectionTool.TextFormatterChanged += new PropertyChangedEventHandler<TextRegionFormatter>(selectionTool_TextFormatterChanged);
            selectionTool.FocusedTextSymbolChanged += new PropertyChangedEventHandler<TextRegionSymbol>(SelectionTool_FocusedTextSymbolChanged);

            if (selectionTool.ImageViewer != null)
                SubscribeToImageViewerEvents(selectionTool.ImageViewer);
        }

    
        /// <summary>
        /// Unsubscribes from the events of WpfTextSelectionTool.
        /// </summary>
        /// <param name="selectionTool">The selection tool.</param>
        private void UnsubscribeFromVisualToolEvents(WpfTextSelectionTool selectionTool)
        {
            selectionTool.SelectionChanged -= selectionTool_SelectionChanged;
            selectionTool.Activated -= selectionTool_Activated;
            selectionTool.Deactivated -= selectionTool_Deactivated;
            selectionTool.MouseDown -= selectionTool_MouseDown;
            selectionTool.SelectionModeChanged -= selectionTool_SelectionModeChanged;
            selectionTool.TextFormatterChanged -= selectionTool_TextFormatterChanged;
            selectionTool.FocusedTextSymbolChanged -= SelectionTool_FocusedTextSymbolChanged;

            if (selectionTool.ImageViewer != null)
                UnsubscribeFromImageViewerEvents(selectionTool.ImageViewer);
        }

        /// <summary>
        /// Subscribes to the events of ImageViewer.
        /// </summary>
        /// <param name="viewer">The viewer.</param>
        private void SubscribeToImageViewerEvents(WpfImageViewer viewer)
        {
            viewer.FocusedIndexChanged += new PropertyChangedEventHandler<int>(ImageViewer_FocusedIndexChanged);
        }

        /// <summary>
        /// Unsubscribes from the events of ImageViewer.
        /// </summary>
        /// <param name="viewer">The viewer.</param>
        private void UnsubscribeFromImageViewerEvents(WpfImageViewer viewer)
        {
            viewer.FocusedIndexChanged -= ImageViewer_FocusedIndexChanged;
        }

        /// <summary>
        /// Updates the selection mode radio buttons.
        /// </summary>
        /// <param name="selectionMode">The selection mode.</param>
        private void UpdateSelectionModeRadioButtons(TextSelectionMode selectionMode)
        {
            if (selectionMode == TextSelectionMode.Rectangle)
                selectionModeRectangleRadioButton.IsChecked = true;
            else if (selectionMode == TextSelectionMode.UseFullLines)
                selectionModeFullLinesRadioButton.IsChecked = true;
        }

        /// <summary>
        /// Updates the formatting mode radio buttons.
        /// </summary>
        /// <param name="formatter">The formatter.</param>
        private void UpdateFormattingModeRadioButtons(TextRegionFormatter formatter)
        {
            if (formatter == formattingModeParagraphsRadioButton.Tag)
                formattingModeParagraphsRadioButton.IsChecked = true;
            else if (formatter == formattingModeLinesRadioButton.Tag)
                formattingModeLinesRadioButton.IsChecked = true;
            else if (formatter == formattingModeMonospaceRadioButton.Tag)
                formattingModeMonospaceRadioButton.IsChecked = true;
        }

        /// <summary>
        /// Shows the symbol information.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        private void ShowSymbolInfo(TextRegionSymbol symbol)
        {
            System.Drawing.Color symbolColor = System.Drawing.Color.Transparent;
            // if text region symbol exists
            if (symbol != null && TextSelectionTool.ImageViewer != null)
            {
                // update info about symbol               
                TextRegion textRegion = _textSelectionTool.FocusedTextRegion;
                VintasoftImage image = TextSelectionTool.ImageViewer.Image;
                AffineMatrix textSpaceToImageTransform = textRegion.GetTransformFromTextToImageSpace(image.Resolution);
                PointF centerPoint = PointFAffineTransform.TransformPoint(textSpaceToImageTransform, symbol.Location);
                imageLocationLabel.Content = centerPoint.ToString();
                pageLocationLabel.Content = string.Format("{0} units", symbol.Location); 
                symbolLabel.Content = symbol.TextSymbol.Symbol.ToString();
                symbolCodeLabel.Content = symbol.TextSymbol.SymbolCode.ToString();
                contentCodeLabel.Content = symbol.TextSymbol.ContentSymbolCode.ToString();
                RegionF region = symbol.SelectionRegion;
                symbolRectLabel.Content = string.Format("({0}; {1}) - ({2}; {3})",
                    region.LeftTop.X,
                    region.LeftTop.Y,
                    region.RightBottom.X,
                    region.RightBottom.Y);

                fontLabel.Content = FontDemosTools.GetFontInfo(symbol.TextSymbol.Font);

                // text space => printer's point space (1/72 inch)
                AffineMatrix textSpaceToPointSpaceTransform = AffineMatrix.Multiply(
                    // text space => DIP space
                    textRegion.GetTransformFromTextToImageSpace(new Resolution(96)),
                    // DIP space => printer's point space (1/72 inch)
                    UnitOfMeasureConverter.GetTransformFromDip(UnitOfMeasure.Points));
                PointF fontSizeInPt = PointFAffineTransform.TransformVector(textSpaceToPointSpaceTransform, new PointF(symbol.FontSize, symbol.FontSize));
                fontSizeLabel.Content = string.Format("{0} pt", fontSizeInPt.X);

                symbolColor = symbol.Color;

                renderingModeLabel.Content = symbol.RenderingMode.ToString();
            }
            // if PDF text region symbol does NOT exist
            else
            {
                // clear info about symbol
                imageLocationLabel.Content = "";
                pageLocationLabel.Content = "";
                contentCodeLabel.Content = "";
                renderingModeLabel.Content = "";
                symbolLabel.Content = "";
                symbolCodeLabel.Content = "";
                symbolRectLabel.Content = "";
                fontLabel.Content = "";
                fontSizeLabel.Content = "";
            }
            symbolColorPanel.Color = WpfObjectConverter.CreateWindowsColor(symbolColor);
        }          

        /// <summary>
        /// The "Save as text" button is clicked.
        /// </summary>
        private void saveAsTextButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files.|*.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                string fileName = saveFileDialog.FileName;
                string text = _textSelectionTool.SelectedText;
                System.IO.File.WriteAllText(fileName, text);
            }
        }

        /// <summary>
        /// Handles the ValueChanged event of the caretWidthNumericUpDown control.
        /// </summary>
        private void CaretWidthNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (_textSelectionTool != null)
                _textSelectionTool.TextCaretWidth = (int)caretWidthNumericUpDown.Value;
        }

        /// <summary>
        /// Handles the ValueChanged event of the caretBlinkingIntervalNumericUpDown control.
        /// </summary>
        private void CaretBlinkingIntervalNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (_textSelectionTool != null)
                _textSelectionTool.TextCaretBlinkingInterval = (int)caretBlinkingIntervalNumericUpDown.Value;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the mouseSelectionCheckBox control.
        /// </summary>
        private void MouseSelectionCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_textSelectionTool != null)
                _textSelectionTool.IsMouseSelectionEnabled = mouseSelectionCheckBox.IsChecked.Value == true;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the keyboardSelectionCheckBox control.
        /// </summary>
        private void KeyboardSelectionCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_textSelectionTool != null)
                _textSelectionTool.IsKeyboardSelectionEnabled = keyboardSelectionCheckBox.IsChecked.Value == true;
        }

        #endregion

    }
}
