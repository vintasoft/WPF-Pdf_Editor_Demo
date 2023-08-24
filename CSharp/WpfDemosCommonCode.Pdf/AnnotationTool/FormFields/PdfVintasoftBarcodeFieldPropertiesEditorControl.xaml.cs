using System;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;
using Vintasoft.Imaging.Wpf;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit properties of
    /// the <see cref="PdfInteractiveFormVintasoftBarcodeField"/>.
    /// </summary>
    public partial class PdfVintasoftBarcodeFieldPropertiesEditorControl : UserControl, IPdfInteractiveFormPropertiesEditor
    {

        #region Constants

        const float MODULE_WIDTH_FACTOR = 0.001f;

        #endregion



        #region Fields

        /// <summary>
        /// Determines that the coefficient, of error correction, is updating.
        /// </summary>
        bool _isUpdateErrorCorrectionCoefficient = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of
        /// the <see cref="PdfVintasoftBarcodeFieldPropertiesEditorControl"/> class.
        /// </summary>
        public PdfVintasoftBarcodeFieldPropertiesEditorControl()
        {
            InitializeComponent();

            foreach (VintasoftBarcodeSymbologyType type in Enum.GetValues(typeof(VintasoftBarcodeSymbologyType)))
            {
                if (type == VintasoftBarcodeSymbologyType.None ||
                    type == VintasoftBarcodeSymbologyType.UnknownLinear)
                    continue;

                barcodeSymbologyComboBox.Items.Add(type);
            }

            foreach (BarcodeDataPreparationMode mode in Enum.GetValues(typeof(BarcodeDataPreparationMode)))
                dataPreparationStepsComboBox.Items.Add(mode);

            moduleWidthNumericUpDown.Minimum = 0;
            moduleWidthNumericUpDown.Maximum = int.MaxValue;

            moduleWidthLabel.Content = string.Format("{0} (x{1} inch)", moduleWidthLabel.Content, MODULE_WIDTH_FACTOR);
        }

        #endregion



        #region Properties

        PdfInteractiveFormVintasoftBarcodeField _field = null;
        /// <summary>
        /// Gets or sets the <see cref="PdfInteractiveFormBarcodeField"/>.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public PdfInteractiveFormVintasoftBarcodeField Field
        {
            get
            {
                return _field;
            }
            set
            {
                _field = value;

                mainTabControl.IsEnabled = _field != null;

                UpdateFieldInfo();
            }
        }

        /// <summary>
        /// Gets or sets the PDF interactive form field.
        /// </summary>
        PdfInteractiveFormField IPdfInteractiveFormPropertiesEditor.Field
        {
            get
            {
                return Field;
            }
            set
            {
                Field = value as PdfInteractiveFormVintasoftBarcodeField;
            }
        }

        #endregion



        #region Methods


        /// <summary>
        /// Updates information about the field.
        /// </summary>
        public void UpdateFieldInfo()
        {
            if (_field == null)
                return;

            if (mainTabControl.SelectedItem == valueTabPage)
            {
                bool updateErrorCorrectionCoefficientComboBox =
                    barcodeSymbologyComboBox.SelectedItem == null ||
                    (VintasoftBarcodeSymbologyType)barcodeSymbologyComboBox.SelectedItem == _field.VintasoftBarcodeSymbology;

                barcodeSymbologyComboBox.SelectedItem = _field.VintasoftBarcodeSymbology;

                if (updateErrorCorrectionCoefficientComboBox)
                    UpdateErrorCorrectionCoefficientComboBox();

                if (_field.Value == null)
                    valueTextBox.Text = string.Empty;
                else
                    valueTextBox.Text = _field.Value.TextValue;

                if (_field.DefaultValue == null)
                    defaultValueTextBox.Text = string.Empty;
                else
                    defaultValueTextBox.Text = _field.DefaultValue.TextValue;

                dataPreparationStepsComboBox.SelectedItem = _field.DataPreparationSteps;
                if (_field.ModuleWidth == 0)
                    moduleWidthNumericUpDown.Value = 0;
                else
                    moduleWidthNumericUpDown.Value = Convert.ToInt32(_field.ModuleWidth / MODULE_WIDTH_FACTOR);

                foregroundColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(_field.ForegroundColor);

                fitBarcodeToAppearanceRectCheckBox.IsChecked = _field.FitBarcodeToAppearanceRect;
                moduleWidthNumericUpDown.IsEnabled = !_field.FitBarcodeToAppearanceRect;

                paddingPanelControl.PaddingValue = _field.Padding;
            }
            else if (mainTabControl.SelectedItem == calculateTabPage)
            {
                if (_field.AdditionalActions == null)
                    _field.AdditionalActions = new PdfInteractiveFormFieldAdditionalActions(_field.Document);
                if (calculateActionEditorControl.Action == null)
                    calculateActionEditorControl.Document = _field.Document;

                calculateActionEditorControl.Action = _field.AdditionalActions.Calculate;
            }
        }

        /// <summary>
        /// Index in main tab control is changed.
        /// </summary>
        private void mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count == 1 && (e.AddedItems[0] is TabItem) &&
                e.RemovedItems != null && e.RemovedItems.Count == 1 && (e.RemovedItems[0] is TabItem))
                UpdateFieldInfo();
        }

        /// <summary>
        /// Calculate action is changed.
        /// </summary>
        private void calculateActionEditorControl_ActionChanged(object sender, EventArgs e)
        {
            PdfJavaScriptAction jsAction = calculateActionEditorControl.Action as PdfJavaScriptAction;
            if (calculateActionEditorControl.Action != null && jsAction == null)
            {
                string message = "Activate action of form field is not derived from PdfJavaScriptAction.";
                calculateActionEditorControl.Action = null;
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
                _field.AdditionalActions.Calculate = jsAction;
        }

        /// <summary>
        /// BarcodeSymbology of form field is changed.
        /// </summary>
        private void barcodeSymbologyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VintasoftBarcodeSymbologyType vintasoftBarcodeSymbology =
                (VintasoftBarcodeSymbologyType)barcodeSymbologyComboBox.SelectedItem;

            if (_field.VintasoftBarcodeSymbology != vintasoftBarcodeSymbology)
            {
                _field.VintasoftBarcodeSymbology = vintasoftBarcodeSymbology;
                OnPropertyValueChanged();
                UpdateErrorCorrectionCoefficientComboBox();
            }
        }

        /// <summary>
        /// Updates the error correction coefficient ComboBox.
        /// </summary>
        private void UpdateErrorCorrectionCoefficientComboBox()
        {
            if (_isUpdateErrorCorrectionCoefficient)
                return;

            _isUpdateErrorCorrectionCoefficient = true;
            VintasoftBarcodeSymbologyType barcodeSymbologyType = VintasoftBarcodeSymbologyType.None;
            if (barcodeSymbologyComboBox.SelectedItem != null)
                barcodeSymbologyType = (VintasoftBarcodeSymbologyType)barcodeSymbologyComboBox.SelectedItem;
            errorCorrectionCoefficientComboBox.Items.Clear();

            if (barcodeSymbologyType == VintasoftBarcodeSymbologyType.PDF417)
            {
                for (int i = 0; i < 9; i++)
                    errorCorrectionCoefficientComboBox.Items.Add(string.Format("Level{0}", i));
            }
            else if (barcodeSymbologyType == VintasoftBarcodeSymbologyType.QR)
            {
                errorCorrectionCoefficientComboBox.Items.Add("L");
                errorCorrectionCoefficientComboBox.Items.Add("M");
                errorCorrectionCoefficientComboBox.Items.Add("Q");
                errorCorrectionCoefficientComboBox.Items.Add("H");
            }

            if (errorCorrectionCoefficientComboBox.Items.Count > 0)
            {
                if (_field.ErrorCorrectionCoefficient < errorCorrectionCoefficientComboBox.Items.Count &&
                    _field.ErrorCorrectionCoefficient >= 0)
                    errorCorrectionCoefficientComboBox.SelectedIndex = _field.ErrorCorrectionCoefficient;
                errorCorrectionCoefficientComboBox.IsEnabled = true;
            }
            else
                errorCorrectionCoefficientComboBox.IsEnabled = false;
            _isUpdateErrorCorrectionCoefficient = false;
        }

        /// <summary>
        /// ErrorCorrectionCoefficient of form field is changed.
        /// </summary>
        private void errorCorrectionCoefficientComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (_field.ErrorCorrectionCoefficient != errorCorrectionCoefficientComboBox.SelectedIndex)
            {
                if (_isUpdateErrorCorrectionCoefficient)
                    return;
                _isUpdateErrorCorrectionCoefficient = true;
                _field.ErrorCorrectionCoefficient = errorCorrectionCoefficientComboBox.SelectedIndex;
                OnPropertyValueChanged();
                _isUpdateErrorCorrectionCoefficient = false;
            }
        }

        /// <summary>
        /// Value of form field is changed.
        /// </summary>
        private void valueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string newValue = valueTextBox.Text;
            if (_field.Value == null)
                _field.Value = new PdfInteractiveFormTextFieldStringValue(_field.Document, newValue);
            else
            {
                if (_field.Value.TextValue == newValue)
                    return;

                _field.Value.TextValue = newValue;
            }

            OnPropertyValueChanged();
        }

        /// <summary>
        /// DefaultValue of form field is changed.
        /// </summary>
        private void defaultValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string newValue = defaultValueTextBox.Text;
            if (_field.DefaultValue == null)
                _field.DefaultValue = new PdfInteractiveFormTextFieldStringValue(_field.Document, newValue);
            else
            {
                if (_field.DefaultValue.TextValue == newValue)
                    return;

                _field.DefaultValue.TextValue = newValue;
            }

            OnPropertyValueChanged();
        }

        /// <summary>
        /// DataPreparationSteps of form field is changed.
        /// </summary>
        private void dataPreparationStepsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BarcodeDataPreparationMode dataPreparationSteps =
                (BarcodeDataPreparationMode)dataPreparationStepsComboBox.SelectedItem;

            if (_field.DataPreparationSteps != dataPreparationSteps)
            {
                _field.DataPreparationSteps = (BarcodeDataPreparationMode)dataPreparationStepsComboBox.SelectedItem;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// ModuleWidth of form field is changed.
        /// </summary>
        private void moduleWidthNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            float moduleWidth = (float)moduleWidthNumericUpDown.Value * MODULE_WIDTH_FACTOR;
            float delta = Math.Abs(_field.ModuleWidth - moduleWidth);
            if (_field != null && delta >= 0.0001f)
            {
                _field.ModuleWidth = moduleWidth;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// Foreground color of form field is changed.
        /// </summary>
        private void foregroundColorPanelControl_ColorChanged(object sender, EventArgs e)
        {
            System.Drawing.Color foregroundColor =
                WpfObjectConverter.CreateDrawingColor(foregroundColorPanelControl.Color);
            if (_field.ForegroundColor != foregroundColor)
            {
                _field.ForegroundColor = foregroundColor;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// FitBarcodeToAppearanceRect of form field is changed.
        /// </summary>
        private void fitBarcodeToAppearanceRectCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            bool fitBarcodeToAppearanceRect = false;
            if (fitBarcodeToAppearanceRectCheckBox.IsChecked.Value == true)
                fitBarcodeToAppearanceRect = true;

            if (_field.FitBarcodeToAppearanceRect != fitBarcodeToAppearanceRect)
            {
                _field.FitBarcodeToAppearanceRect = fitBarcodeToAppearanceRect;
                moduleWidthNumericUpDown.IsEnabled = !_field.FitBarcodeToAppearanceRect;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// Padding of form field is changed.
        /// </summary>
        private void paddingPanelControl_PaddingValueChanged(object sender, EventArgs e)
        {
            _field.Padding = paddingPanelControl.PaddingValue;
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



        #region Events

        /// <summary>
        /// Occurs when value of property is changed.
        /// </summary>
        public event EventHandler PropertyValueChanged;

        #endregion

    }
}
