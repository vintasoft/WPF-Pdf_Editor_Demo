using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit properties of the <see cref="PdfInteractiveFormTextField"/>.
    /// </summary>
    public partial class PdfTextFieldPropertiesEditorControl : UserControl, IPdfInteractiveFormPropertiesEditor
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfTextFieldPropertiesEditorControl"/> class.
        /// </summary>
        public PdfTextFieldPropertiesEditorControl()
        {
            InitializeComponent();

            foreach (TextQuaddingType item in Enum.GetValues(typeof(TextQuaddingType)))
                textQuaddingComboBox.Items.Add(item);
        }

        #endregion



        #region Properties

        PdfInteractiveFormTextField _field = null;
        /// <summary>
        /// Gets or sets the <see cref="PdfInteractiveFormTextField"/>.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public PdfInteractiveFormTextField Field
        {
            get
            {
                return _field;
            }
            set
            {
                _field = value;

                mainTabControl.IsEnabled = _field != null;

                if (_field == null)
                {
                    calculatePdfActionEditorControl.Document = null;
                    validatePdfActionEditorControl.Document = null;
                    formatPdfActionEditorControl.Document = null;
                    keystrokePdfActionEditorControl.Document = null;
                }
                else
                {
                    calculatePdfActionEditorControl.Document = _field.Document;
                    validatePdfActionEditorControl.Document = _field.Document;
                    formatPdfActionEditorControl.Document = _field.Document;
                    keystrokePdfActionEditorControl.Document = _field.Document;
                }

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
                Field = value as PdfInteractiveFormTextField;
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
            string value = string.Empty;
            string defaultValue = string.Empty;

            if (_field != null)
            {
                value = AddControlSymbols(_field.TextValue);
                defaultValue = AddControlSymbols(_field.TextDefaultValue);
                textQuaddingComboBox.SelectedItem = _field.TextQuadding;
                multilineCheckBox.IsChecked = _field.IsMultiline;
                passwordCheckBox.IsChecked = _field.IsPassword;
                spellCheckCheckBox.IsChecked = !_field.IsDoNotSpellCheck;

                UpdateActionEditorControl();
            }

            valueTextBox.Text = value;
            defaultValueTextBox.Text = defaultValue;
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Adds the '\r' symbol to a string.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <returns>String with '\r' symbol.</returns>
        private static string AddControlSymbols(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            string result = str.Replace("\r", "");
            result = result.Replace("\n", "\r\n");
            return result;
        }

        /// <summary>
        /// Removes the '\r' symbol from a string.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <returns>String without '\r' symbol.</returns>
        private static string RemoveControlSybmols(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            string result = str.Replace("\r", "");
            return result;
        }

        /// <summary>
        /// "Multiline" flag is changed.
        /// </summary>
        private void multilineCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool isChecked = false;
            if (multilineCheckBox.IsChecked.Value == true)
                isChecked = true;

            if (_field.IsMultiline != isChecked)
            {
                if (!isChecked)
                {
                    if (!string.IsNullOrEmpty(valueTextBox.Text))
                        valueTextBox.Text = valueTextBox.GetLineText(0).TrimEnd('\r', '\n');

                    if (!string.IsNullOrEmpty(defaultValueTextBox.Text))
                        defaultValueTextBox.Text = defaultValueTextBox.GetLineText(0).TrimEnd('\r', '\n');
                }

                _field.IsMultiline = isChecked;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// "Password" flag is changed.
        /// </summary>
        private void passwordCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool isPassword = passwordCheckBox.IsChecked.Value == true;

            if (_field.IsPassword != isPassword)
            {
                _field.IsPassword = isPassword;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// "Spell Check" flag is changed.
        /// </summary>
        private void spellCheckCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool isNotSpellChecked = spellCheckCheckBox.IsChecked.Value == false;

            if (_field.IsDoNotSpellCheck != isNotSpellChecked)
            {
                _field.IsDoNotSpellCheck = isNotSpellChecked;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// Field value is changed.
        /// </summary>
        private void valueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string value = RemoveControlSybmols(valueTextBox.Text);
            if (value != _field.TextValue)
            {
                _field.TextValue = value;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// Default value of field is changed.
        /// </summary>
        private void defaultValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string value = RemoveControlSybmols(defaultValueTextBox.Text);
            if (value != _field.TextDefaultValue)
            {
                _field.TextDefaultValue = value;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// Key is down in value or defalut value textbox.
        /// </summary>
        private void valueTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!_field.IsMultiline && e.Key == Key.Enter)
                e.Handled = true;
        }

        /// <summary>
        /// Text alignment is changed.
        /// </summary>
        private void textQuaddingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextQuaddingType textQuadding = (TextQuaddingType)textQuaddingComboBox.SelectedItem;
            if (_field.TextQuadding != textQuadding)
            {
                _field.TextQuadding = textQuadding;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// Tab page is changed.
        /// </summary>
        private void mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainTabControl.SelectedItem != valueTabPage)
                UpdateActionEditorControl();
        }

        /// <summary>
        /// Updates the action editor control.
        /// </summary>
        private void UpdateActionEditorControl()
        {
            if (_field.AdditionalActions == null)
                _field.AdditionalActions = new PdfInteractiveFormFieldAdditionalActions(_field.Document);

            if (mainTabControl.SelectedItem == calculateActionTabPage)
                calculatePdfActionEditorControl.Action = _field.AdditionalActions.Calculate;
            else if (mainTabControl.SelectedItem == validateActionTabPage)
                validatePdfActionEditorControl.Action = _field.AdditionalActions.Validate;
            else if (mainTabControl.SelectedItem == formatActionTabPage)
                formatPdfActionEditorControl.Action = _field.AdditionalActions.Format;
            else if (mainTabControl.SelectedItem == keystrokeActionTabPage)
                keystrokePdfActionEditorControl.Action = _field.AdditionalActions.Keystroke;
        }

        /// <summary>
        /// Calculate action of field is changed.
        /// </summary>
        private void calculatePdfActionEditorControl_ActionChanged(object sender, EventArgs e)
        {
            PdfJavaScriptAction jsAction = calculatePdfActionEditorControl.Action as PdfJavaScriptAction;
            if (calculatePdfActionEditorControl.Action != null && jsAction == null)
            {
                string message = "Calculate action of form field is not derived from PdfJavaScriptAction.";
                calculatePdfActionEditorControl.Action = null;
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
                _field.AdditionalActions.Calculate = jsAction;
        }

        /// <summary>
        /// Validate action of field is changed.
        /// </summary>
        private void validatePdfActionEditorControl_ActionChanged(object sender, EventArgs e)
        {
            PdfJavaScriptAction jsAction = validatePdfActionEditorControl.Action as PdfJavaScriptAction;
            if (validatePdfActionEditorControl.Action != null && jsAction == null)
            {
                string message = "Validate action of form field is not derived from PdfJavaScriptAction.";
                validatePdfActionEditorControl.Action = null;
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
                _field.AdditionalActions.Validate = jsAction;
        }

        /// <summary>
        /// The Format action of field is changed.
        /// </summary>
        private void formatPdfActionEditorControl_ActionChanged(object sender, EventArgs e)
        {
            PdfJavaScriptAction jsAction = formatPdfActionEditorControl.Action as PdfJavaScriptAction;
            if (formatPdfActionEditorControl.Action != null && jsAction == null)
            {
                string message = "The Format action of form field is not derived from PdfJavaScriptAction.";
                formatPdfActionEditorControl.Action = null;
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
                _field.AdditionalActions.Format = jsAction;
        }

        /// <summary>
        /// The Keystroke action of field is changed.
        /// </summary>
        private void keystrokePdfActionEditorControl_ActionChanged(object sender, EventArgs e)
        {
            PdfJavaScriptAction jsAction = keystrokePdfActionEditorControl.Action as PdfJavaScriptAction;
            if (keystrokePdfActionEditorControl.Action != null && jsAction == null)
            {
                string message = "The Keystroke action of form field is not derived from PdfJavaScriptAction.";
                keystrokePdfActionEditorControl.Action = null;
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
                _field.AdditionalActions.Keystroke = jsAction;
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
