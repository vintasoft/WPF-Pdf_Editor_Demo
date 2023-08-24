using System.Windows;

using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view and edit PDF reset form action.
    /// </summary>
    public partial class PdfResetFormActionEditorWindow : Window
    {

        #region Fields

        /// <summary>
        /// The PDF reset form action.
        /// </summary>
        PdfResetFormAction _action;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfResetFormActionEditorForm"/> class.
        /// </summary>
        /// <param name="action">The PDF reset form action.</param>
        public PdfResetFormActionEditorWindow(PdfResetFormAction action)
        {
            InitializeComponent();

            _action = action;

            pdfInteractiveFormFieldListEditorControl.InteractiveForm = action.Document.InteractiveForm;
            excludeSelectedFieldsCheckBox.IsChecked = action.FieldsIsExclude;

            if (action.Fields != null)
            {
                selectedFieldsRadioButton.IsChecked = true;
                pdfInteractiveFormFieldListEditorControl.SelectedFields = action.Fields.ToArray();
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// "Ok" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
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
        /// Radio button is clicked.
        /// </summary>
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            selectedFieldsGrid.IsEnabled = selectedFieldsRadioButton.IsChecked.Value;
        }

        #endregion

    }
}
