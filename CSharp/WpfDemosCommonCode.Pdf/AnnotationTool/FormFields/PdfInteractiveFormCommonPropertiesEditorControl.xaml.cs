using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;
using Vintasoft.Imaging.Wpf;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit common properties of the <see cref="PdfInteractiveFormField"/>.
    /// </summary>
    public partial class PdfInteractiveFormCommonPropertiesEditorControl : UserControl, IPdfInteractiveFormPropertiesEditor
    {

        #region Constants

        /// <summary>
        /// A string value that determines that annotation font size must be calculated automatically.
        /// </summary>
        const string FONT_SIZE_AUTO = "Auto";

        /// <summary>
        /// A string value that determines that font size is not specified.
        /// </summary>
        const string FONT_SIZE_NOT_SPECIFIED = "Not Specified";

        #endregion



        #region Fields

        /// <summary>
        /// The annotation of field.
        /// </summary>
        PdfWidgetAnnotation _annotation = null;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfInteractiveFormCommonPropertiesEditorControl"/> class.
        /// </summary>
        public PdfInteractiveFormCommonPropertiesEditorControl()
        {
            InitializeComponent();

            float fontSizeMinValue = 4f;
            float fontSizeMaxValue = 20f;
            float fontSizeStep = 2f;

            fontSizeComboBox.Items.Add(FONT_SIZE_AUTO);
            autoFontSizeMinValueComboBox.Items.Add(FONT_SIZE_NOT_SPECIFIED);
            autoFontSizeMaxValueComboBox.Items.Add(FONT_SIZE_NOT_SPECIFIED);

            for (float fontSize = fontSizeMinValue; fontSize <= fontSizeMaxValue; fontSize += fontSizeStep)
            {
                string fontSizeStr = fontSize.ToString(CultureInfo.InvariantCulture);
                fontSizeComboBox.Items.Add(fontSizeStr);

                autoFontSizeMinValueComboBox.Items.Add(fontSizeStr);
                autoFontSizeMaxValueComboBox.Items.Add(fontSizeStr);
            }
        }

        #endregion



        #region Properties

        PdfInteractiveFormField _field = null;
        /// <summary>
        /// Gets or sets the field.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public PdfInteractiveFormField Field
        {
            get
            {
                return _field;
            }
            set
            {
                if (_field != value)
                {
                    _field = value;

                    if (_field == null)
                    {
                        borderStyleControl.Annotation = null;
                    }
                    else
                    {
                        _annotation = _field.Annotation;
                        if (_annotation != null)
                        {
                            if (_annotation.AppearanceCharacteristics == null)
                            {
                                _annotation.AppearanceCharacteristics =
                                    new PdfAnnotationAppearanceCharacteristics(_annotation.Document);
                            }
                        }
                        borderStyleControl.Annotation = _annotation;
                    }

                    UpdateFieldInfo();
                    UpdateUI();
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum value of font size.
        /// </summary>
        private float FontSizeMinValue
        {
            get
            {
                return _field.AutoFontSizeMinValue;
            }
            set
            {
                _field.AutoFontSizeMinValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum value of font size.
        /// </summary>
        private float FontSizeMaxValue
        {
            get
            {
                PdfInteractiveFormTextField textField = _field as PdfInteractiveFormTextField;
                if (textField != null && textField.IsMultiline)
                    return textField.MultilineAutoFontSizeMaxValue;
                else
                    return _field.AutoFontSizeMaxValue;
            }
            set
            {
                PdfInteractiveFormTextField textField = _field as PdfInteractiveFormTextField;
                if (textField != null && textField.IsMultiline)
                    textField.MultilineAutoFontSizeMaxValue = value;
                else
                    _field.AutoFontSizeMaxValue = value;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Updates the field information.
        /// </summary>
        public void UpdateFieldInfo()
        {
            borderStyleControl.UpdateBorderInfo();

            if (_field == null)
            {
                nameTextBox.Text = string.Empty;
                pdfFontPanelControl.PdfFont = null;
            }
            else
            {
                nameTextBox.Text = _field.PartialName;

                SetFontSizeComboBoxValue(fontSizeComboBox, _field.FontSize);
                SetFontSizeComboBoxValue(autoFontSizeMinValueComboBox, FontSizeMinValue);
                SetFontSizeComboBoxValue(autoFontSizeMaxValueComboBox, FontSizeMaxValue);

                pdfFontPanelControl.PdfFont = _field.Font;
                if (pdfFontPanelControl.PdfDocument == null)
                    pdfFontPanelControl.PdfDocument = _field.Document;
                fontColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(_field.TextColor);

                if (_field is PdfInteractiveFormVintasoftBarcodeField)
                {
                    PdfInteractiveFormVintasoftBarcodeField field = (PdfInteractiveFormVintasoftBarcodeField)_field;
                    backgroundColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(field.BackgroundColor);
                }
                else if (_annotation != null)
                    backgroundColorPanelControl.Color =
                        WpfObjectConverter.CreateWindowsColor(_annotation.AppearanceCharacteristics.BackgroundColor);

                requiredCheckBox.IsChecked = _field.IsRequired;
                readOnlyCheckBox.IsChecked = _field.IsReadOnly;
                noExportCheckBox.IsChecked = _field.IsNoExport;
            }
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Updates the User Interface.
        /// </summary>
        private void UpdateUI()
        {
            borderGroupBox.IsEnabled = _annotation != null;
            backgroundPanel.IsEnabled = _annotation != null;
            fontGroupBox.IsEnabled = true;

            if (_field is PdfInteractiveFormSignatureField)
            {
                fontGroupBox.IsEnabled = false;
            }
            else if (_field is PdfInteractiveFormBarcodeField)
            {
                fontGroupBox.IsEnabled = false;

                if (_field is PdfInteractiveFormVintasoftBarcodeField)
                {
                    borderGroupBox.IsEnabled = true;
                    backgroundPanel.IsEnabled = true;
                }
                else
                {
                    borderGroupBox.IsEnabled = false;
                    backgroundPanel.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// Field name is changed.
        /// </summary>
        private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(nameTextBox.Text))
                return;

            _field.PartialName = nameTextBox.Text;
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Field font name is changed.
        /// </summary>
        private void pdfFontPanelControl_PdfFontChanged(object sender, EventArgs e)
        {
            _field.Font = pdfFontPanelControl.PdfFont;
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Font size is changed.
        /// </summary>
        private void fontSizeComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_field != null)
            {
                float value;
                if (GetFontSizeComboBoxValue(fontSizeComboBox, out value))
                {
                    bool isAutoSize = value == 0;
                    if (isAutoSize && !FONT_SIZE_AUTO.Equals(fontSizeComboBox.Text))
                        fontSizeComboBox.Text = FONT_SIZE_AUTO;

                    if (_field.FontSize != value)
                    {
                        _field.FontSize = value;
                        OnPropertyValueChanged();
                    }

                    autoFontSizeMinValueComboBox.IsEnabled = isAutoSize;
                    autoFontSizeMaxValueComboBox.IsEnabled = isAutoSize;
                }
            }
        }

        /// <summary>
        /// Font minimum size is changed.
        /// </summary>
        private void autoFontSizeMinValueComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_field != null)
            {
                float value;
                if (GetFontSizeComboBoxValue(autoFontSizeMinValueComboBox, out value))
                {
                    if (value == 0 && !FONT_SIZE_NOT_SPECIFIED.Equals(autoFontSizeMinValueComboBox.Text))
                        autoFontSizeMinValueComboBox.Text = FONT_SIZE_NOT_SPECIFIED;

                    if (FontSizeMinValue != value)
                    {
                        FontSizeMinValue = value;
                        OnPropertyValueChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Font maximum size is changed.
        /// </summary>
        private void autoFontSizeMaxValueComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_field != null)
            {
                float value;
                if (GetFontSizeComboBoxValue((ComboBox)autoFontSizeMaxValueComboBox, out value))
                {
                    if (value == 0 && !FONT_SIZE_NOT_SPECIFIED.Equals(autoFontSizeMaxValueComboBox.Text))
                        autoFontSizeMaxValueComboBox.Text = FONT_SIZE_NOT_SPECIFIED;

                    if (FontSizeMaxValue != value)
                    {
                        FontSizeMaxValue = value;
                        OnPropertyValueChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Sets the font size.
        /// </summary>
        /// <param name="comboBox">The combo box.</param>
        /// <param name="value">The value.</param>
        private void SetFontSizeComboBoxValue(ComboBox comboBox, float value)
        {
            int index = 0;

            if (value != 0)
            {
                index = -1;
                for (int i = 1; i < comboBox.Items.Count; i++)
                {
                    if (Convert.ToSingle(comboBox.Items[i]) == value)
                    {
                        index = i;
                        break;
                    }
                }
            }

            if (index >= 0)
                comboBox.SelectedIndex = index;
            else
                comboBox.Text = value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns the font size.
        /// </summary>
        /// <param name="comboBox">The combo box.</param>
        /// <param name="value">The value.</param>
        private bool GetFontSizeComboBoxValue(ComboBox comboBox, out float value)
        {
            string text = comboBox.Text;

            if (FONT_SIZE_AUTO.Equals(text, StringComparison.InvariantCultureIgnoreCase) ||
                FONT_SIZE_NOT_SPECIFIED.Equals(text, StringComparison.InvariantCultureIgnoreCase))
                text = "0";

            text = text.Replace(",", ".");
            return float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// Font color is changed.
        /// </summary>
        private void fontColorPanelControl_ColorChanged(object sender, EventArgs e)
        {
            _field.TextColor = WpfObjectConverter.CreateDrawingColor(fontColorPanelControl.Color);
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Background color is changed.
        /// </summary>
        private void backgroundColorPanelControl_ColorChanged(object sender, EventArgs e)
        {
            if (_field is PdfInteractiveFormVintasoftBarcodeField)
            {
                PdfInteractiveFormVintasoftBarcodeField field = (PdfInteractiveFormVintasoftBarcodeField)_field;
                field.BackgroundColor = WpfObjectConverter.CreateDrawingColor(backgroundColorPanelControl.Color);
            }
            else
                _annotation.AppearanceCharacteristics.BackgroundColor =
                    WpfObjectConverter.CreateDrawingColor(backgroundColorPanelControl.Color);
            OnPropertyValueChanged();
        }

        /// <summary>
        /// IsRequired field is changed.
        /// </summary>
        private void requiredCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool isRequired = requiredCheckBox.IsChecked.Value == true;
            if (_field.IsRequired != isRequired)
            {
                _field.IsRequired = isRequired;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// IsReadOnly field is changed.
        /// </summary>
        private void readOnlyCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool isReadOnly = readOnlyCheckBox.IsChecked.Value == true;
            if (_field.IsReadOnly != isReadOnly)
            {
                _field.IsReadOnly = isReadOnly;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// IsNoExport field is changed.
        /// </summary>
        private void noExportCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool isNoExport = noExportCheckBox.IsChecked.Value == true;
            if (_field.IsNoExport != isNoExport)
            {
                _field.IsNoExport = isNoExport;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// The border style is changed.
        /// </summary>
        private void borderStyleControl_PropertyValueChanged(object sender, EventArgs e)
        {
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Raises the PropertyValueChanged event.
        /// </summary>
        private void OnPropertyValueChanged()
        {
            if (PropertyValueChanged != null)
                PropertyValueChanged(this, EventArgs.Empty);
        }

        #endregion

        #endregion



        #region Events

        /// <summary>
        /// Occurs when the property value is changed.
        /// </summary>
        public event EventHandler PropertyValueChanged;

        #endregion

    }
}
