using System;
using System.Windows;

using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view and edit PDF form submit action.
    /// </summary>
    public partial class PdfSubmitFormActionEditorWindow : Window
    {

        #region Fields

        /// <summary>
        /// The PDF form submit action.
        /// </summary>
        PdfSubmitFormAction _action;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfSubmitFormActionEditorForm"/> class.
        /// </summary>
        /// <param name="action">The PDF form submit action.</param>
        public PdfSubmitFormActionEditorWindow(PdfSubmitFormAction action)
        {
            InitializeComponent();

            foreach (PdfInteractiveFormFieldSubmitFormat format in Enum.GetValues(typeof(PdfInteractiveFormFieldSubmitFormat)))
                submitFormatComboBox.Items.Add(format);

            _action = action;

            submitFormatComboBox.SelectedItem = action.SubmitFormat;
            submitUrlTextBox.Text = action.Url;
            okButton.IsEnabled = !string.IsNullOrEmpty(submitUrlTextBox.Text);
            pdfInteractiveFormFieldListEditorControl.InteractiveForm = action.Document.InteractiveForm;
            excludeSelectedFieldsCheckBox.IsChecked = action.FieldsIsExclude;

            if (_action.Fields != null)
            {
                selectedFieldsRadioButton.IsChecked = true;
                pdfInteractiveFormFieldListEditorControl.SelectedFields = _action.Fields.ToArray();
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// "Ok" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            pdfInteractiveFormFieldListEditorControl.InteractiveForm = null;

            _action.SubmitFormat = (PdfInteractiveFormFieldSubmitFormat)submitFormatComboBox.SelectedItem;
            _action.Url = submitUrlTextBox.Text;

            if (allFieldsRadioButton.IsChecked.Value == true)
            {
                _action.Fields = null;
            }
            else
            {
                _action.FieldsIsExclude = excludeSelectedFieldsCheckBox.IsChecked.Value;
                if (_action.Fields == null)
                    _action.Fields = new PdfInteractiveFormFieldList(_action.Document);
                else
                    _action.Fields.Clear();

                _action.Fields.AddRange(pdfInteractiveFormFieldListEditorControl.SelectedFields);
            }

            DialogResult = true;
        }

        /// <summary>
        /// Radio button is checked.
        /// </summary>
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (selectedFieldsRadioButton != null)
            {
                if (selectedFieldsRadioButton.IsChecked.Value == true)
                    selectedFieldsGrid.IsEnabled = true;
                else
                    selectedFieldsGrid.IsEnabled = false;
            }
        }

        /// <summary>
        /// The text of text box is changed.
        /// </summary>
        private void submitUrlTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            okButton.IsEnabled = !string.IsNullOrEmpty(submitUrlTextBox.Text);
        }

        #endregion

    }
}
