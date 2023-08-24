using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Drawing;
using Vintasoft.Imaging.Pdf.Tree.Fonts;
using Vintasoft.Imaging.Pdf.Wpf.UI;
using Vintasoft.Imaging.Wpf;

namespace WpfDemosCommonCode.Pdf.Security
{
    /// <summary>
    /// A window that allows to edit the redaction mark appearance.
    /// </summary>
    public partial class RedactionMarkAppearanceEditor : Window
    {

        #region Fields

        /// <summary>
        /// The PDF document.
        /// </summary>
        PdfDocument _document;

        /// <summary>
        /// The redaction mark appearance.
        /// </summary>
        RedactionMarkAppearanceGraphicsFigure _redactionMarkAppearance;

        #endregion



        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="RedactionMarkAppearanceEditor"/> class from being created.
        /// </summary>
        public RedactionMarkAppearanceEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionMarkAppearanceEditor"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="redactionMarkAppearance">The redaction mark appearance.</param>
        public RedactionMarkAppearanceEditor(PdfDocument document, RedactionMarkAppearanceGraphicsFigure redactionMarkAppearance)
            : this()
        {
            _document = document;
            _redactionMarkAppearance = redactionMarkAppearance;
            if (_redactionMarkAppearance.Font == null ||
                _redactionMarkAppearance.Font.Document != document)
                _redactionMarkAppearance.Font = _document.FontManager.GetStandardFont(PdfStandardFontType.TimesRoman);

            alignmentComboBox.Items.Add(PdfContentAlignment.Center);
            alignmentComboBox.Items.Add(PdfContentAlignment.Top);
            alignmentComboBox.Items.Add(PdfContentAlignment.Bottom);
            alignmentComboBox.Items.Add(PdfContentAlignment.Left);
            alignmentComboBox.Items.Add(PdfContentAlignment.Right);
            alignmentComboBox.Items.Add(PdfContentAlignment.Top | PdfContentAlignment.Left);
            alignmentComboBox.Items.Add(PdfContentAlignment.Top | PdfContentAlignment.Right);
            alignmentComboBox.Items.Add(PdfContentAlignment.Top | PdfContentAlignment.Left | PdfContentAlignment.Right);
            alignmentComboBox.Items.Add(PdfContentAlignment.Bottom | PdfContentAlignment.Left);
            alignmentComboBox.Items.Add(PdfContentAlignment.Bottom | PdfContentAlignment.Right);
            alignmentComboBox.Items.Add(PdfContentAlignment.Bottom | PdfContentAlignment.Right | PdfContentAlignment.Left);
            alignmentComboBox.Items.Add(PdfContentAlignment.Left | PdfContentAlignment.Top);
            alignmentComboBox.Items.Add(PdfContentAlignment.Left | PdfContentAlignment.Bottom);
            alignmentComboBox.Items.Add(PdfContentAlignment.Left | PdfContentAlignment.Bottom | PdfContentAlignment.Top);
            alignmentComboBox.Items.Add(PdfContentAlignment.Right | PdfContentAlignment.Top);
            alignmentComboBox.Items.Add(PdfContentAlignment.Right | PdfContentAlignment.Bottom);
            alignmentComboBox.Items.Add(PdfContentAlignment.Right | PdfContentAlignment.Bottom | PdfContentAlignment.Top);
            alignmentComboBox.SelectedItem = _redactionMarkAppearance.TextAlignment;

            fillColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(_redactionMarkAppearance.FillColor);
            isFillColorEnabledCheckBox.IsChecked = !_redactionMarkAppearance.FillColor.IsEmpty;

            borderColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(_redactionMarkAppearance.BorderColor);
            borderWidthNumericUpDown.Value = (int)_redactionMarkAppearance.BorderWidth;
            isBorderPropertiesEnabledCheckBox.IsChecked = !_redactionMarkAppearance.BorderColor.IsEmpty;

            autoFontSizeCheckBox.IsChecked = _redactionMarkAppearance.AutoFontSize;

            overlayTextBox.Text = _redactionMarkAppearance.Text;
            fontSizeNumericUpDown.Value = (int)_redactionMarkAppearance.FontSize;

            fontColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(_redactionMarkAppearance.TextColor);

            UpdateUI();
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the user interface.
        /// </summary>
        private void UpdateUI()
        {
            fillColorPanelControl.IsEnabled = isFillColorEnabledCheckBox.IsChecked.Value;
            borderPropertiesGroupBox.IsEnabled = isBorderPropertiesEnabledCheckBox.IsChecked.Value;
        }

        /// <summary>
        /// Windows is loaded.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // create temp PDF document
            SizeF size = new SizeF((float)redactionMarkEditor.ActualWidth, (float)redactionMarkEditor.ActualHeight);
            size.Width *= 72f / 96f;
            size.Height *= 72f / 96f;
            MemoryStream ms = new MemoryStream();
            using (PdfDocument tempDocument = new PdfDocument())
            {
                tempDocument.Pages.Add(size);
                tempDocument.Save(ms);
            }
            redactionMarkEditor.Images.Add(ms, true);
            _redactionMarkAppearance.SetRegion(new RegionF(new RectangleF(PointF.Empty, size)));

            // use PdfEditorTool for editing the signature appearance
            WpfPdfContentEditorTool editorTool = new WpfPdfContentEditorTool();
            WpfGraphicsFigureView view = WpfGraphicsFigureViewFactory.CreateView(_redactionMarkAppearance);
            view.Transformer = null;
            view.Builder = null;
            editorTool.FigureViewCollection.Add(view);
            redactionMarkEditor.VisualTool = editorTool;
        }


        /// <summary>
        /// The fill color is enabled/disabled.
        /// </summary>
        private void isFillColorEnabledCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (isFillColorEnabledCheckBox.IsChecked.Value)
                _redactionMarkAppearance.FillColor = WpfObjectConverter.CreateDrawingColor(fillColorPanelControl.Color);
            else
                _redactionMarkAppearance.FillColor = System.Drawing.Color.Empty;
            UpdateUI();
        }

        /// <summary>
        /// The fill color is changed.
        /// </summary>
        private void fillColorPanelControl_ColorChanged(object sender, EventArgs e)
        {
            _redactionMarkAppearance.FillColor = WpfObjectConverter.CreateDrawingColor(fillColorPanelControl.Color);
        }


        /// <summary>
        /// The border is enabled/disabled.
        /// </summary>
        private void isBorderPropertiesEnabledCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (isBorderPropertiesEnabledCheckBox.IsChecked.Value)
            {
                if (borderColorPanelControl.Color != WpfObjectConverter.CreateWindowsColor(Color.Empty))
                {
                    _redactionMarkAppearance.BorderColor = WpfObjectConverter.CreateDrawingColor(borderColorPanelControl.Color);
                }
                else
                {
                    _redactionMarkAppearance.BorderColor = Color.Black;
                    borderColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(Color.Black);
                }

                _redactionMarkAppearance.BorderWidth = (float)borderWidthNumericUpDown.Value;

            }
            else
            {
                _redactionMarkAppearance.BorderColor = Color.Empty;
                _redactionMarkAppearance.BorderWidth = 0;
                _redactionMarkAppearance.BorderWidth = 0;
            }

            UpdateUI();
        }

        /// <summary>
        /// The border color is changed.
        /// </summary>
        private void borderColorPanelControl_ColorChanged(object sender, EventArgs e)
        {
            _redactionMarkAppearance.BorderColor = WpfObjectConverter.CreateDrawingColor(borderColorPanelControl.Color);
        }

        /// <summary>
        /// The border width is changed.
        /// </summary>
        private void borderWidthNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _redactionMarkAppearance.BorderWidth = (float)borderWidthNumericUpDown.Value;
        }


        /// <summary>
        /// The overlay text is changed.
        /// </summary>
        private void overlayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.IsInitialized)
                return;

            _redactionMarkAppearance.Text = overlayTextBox.Text;
        }

        /// <summary>
        /// The overlay text alignment is changed.
        /// </summary>
        private void alignmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsInitialized)
                return;

            _redactionMarkAppearance.TextAlignment = (PdfContentAlignment)alignmentComboBox.SelectedItem;
        }

        /// <summary>
        /// The "Font..." button is clicked.
        /// </summary>
        private void fontButton_Click(object sender, RoutedEventArgs e)
        {
            CreateFontWindow fontWindow = new CreateFontWindow(_document);
            if (fontWindow.ShowDialog().Value)
                _redactionMarkAppearance.Font = fontWindow.SelectedFont;
        }

        /// <summary>
        /// Font size is changed.
        /// </summary>
        private void fontSizeNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (!this.IsInitialized)
                return;

            _redactionMarkAppearance.FontSize = (float)fontSizeNumericUpDown.Value;
        }

        /// <summary>
        /// The auto font size is enabled/disabled.
        /// </summary>
        private void autoFontSizeCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            _redactionMarkAppearance.AutoFontSize = autoFontSizeCheckBox.IsChecked.Value;
            fontSizeNumericUpDown.IsEnabled = !autoFontSizeCheckBox.IsChecked.Value;
        }

        /// <summary>
        /// The text color is changed.
        /// </summary>
        private void fontColorPanelControl_ColorChanged(object sender, EventArgs e)
        {
            _redactionMarkAppearance.TextColor = WpfObjectConverter.CreateDrawingColor(fontColorPanelControl.Color);
        }


        /// <summary>
        /// "Ok" button is clicked.
        /// </summary>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}
