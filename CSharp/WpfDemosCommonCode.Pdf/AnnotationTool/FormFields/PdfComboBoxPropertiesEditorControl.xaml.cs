using System;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit properties of the <see cref="PdfInteractiveFormComboBoxField"/>.
    /// </summary>
    public partial class PdfComboBoxPropertiesEditorControl : UserControl, IPdfInteractiveFormPropertiesEditor
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfComboBoxPropertiesEditorControl"/> class.
        /// </summary>
        public PdfComboBoxPropertiesEditorControl()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        PdfInteractiveFormComboBoxField _field = null;
        /// <summary>
        /// Gets or sets the <see cref="PdfInteractiveFormComboBoxField"/>.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public PdfInteractiveFormComboBoxField Field
        {
            get
            {
                return _field;
            }
            set
            {
                _field = value;
                mainTabControl.IsEnabled = _field != null;                
                pdfInteractiveFormChoiceFieldEditorControl.Field = _field;

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

                    editableCheckBox.IsChecked = _field.IsEdit;
                    spellCheckCheckBox.IsChecked = !_field.IsDoNotSpellCheck;
                }
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
                Field = value as PdfInteractiveFormComboBoxField;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the field information.
        /// </summary>
        public void UpdateFieldInfo()
        {
            editableCheckBox.IsChecked = _field.IsEdit;
            pdfInteractiveFormChoiceFieldEditorControl.UpdateFieldInfo();

            UpdateActionEditorControl();
        }

        /// <summary>
        /// IsEdit flag is changed.
        /// </summary>
        private void editableCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool isEdit = false;
            if (editableCheckBox.IsChecked.Value == true)
                isEdit = true;

            if (_field.IsEdit != isEdit)
            {
                _field.IsEdit = isEdit;
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
        /// The property value is changed.
        /// </summary>
        private void pdfInteractiveFormChoiceFieldEditorControl_PropertyValueChanged(object sender, EventArgs e)
        {
            OnPropertyValueChanged();
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



        #region Events

        /// <summary>
        /// Occurs when the property value is changed.
        /// </summary>
        public event EventHandler PropertyValueChanged;

        #endregion

    }
}
