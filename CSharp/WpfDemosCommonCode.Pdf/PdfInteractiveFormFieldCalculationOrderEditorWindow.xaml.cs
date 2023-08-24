using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Form that allows to change the calculation order of PDF interactive form.
    /// </summary>
    public partial class PdfInteractiveFormFieldCalculationOrderEditorWindow : Window
    {
        
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfInteractiveFormFieldCalculationOrderEditorWindow"/> class.
        /// </summary>
        public PdfInteractiveFormFieldCalculationOrderEditorWindow()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        PdfDocumentInteractiveForm _interactiveForm = null;
        /// <summary>
        /// Gets or sets the PDF document interactive form.
        /// </summary>
        public PdfDocumentInteractiveForm InteractiveForm
        {
            get
            {
                return _interactiveForm;
            }
            set
            {
                _interactiveForm = value;

                // if interactive form is NOT empty
                if (_interactiveForm != null)
                {
                    // a list of fields with "Calculate" action
                    List<PdfInteractiveFormField> fieldsWithCalculateAction = new List<PdfInteractiveFormField>();

                    // if form has calculation order
                    if (_interactiveForm.CalculationOrder != null)
                    {
                        // for each field in caculation order
                        foreach (PdfInteractiveFormField field in _interactiveForm.CalculationOrder)
                        {
                            // if field has the "Calculate" action
                            if (field.IsCalculated)
                            {
                                ListBoxItem item = new ListBoxItem();
                                item.Content = field.FullyQualifiedName;
                                item.Tag = field;
                                // add field to a form list box
                                interactiveFormListBox.Items.Add(item);
                                // add field to a list of fields with "Calculate" action
                                fieldsWithCalculateAction.Add(field);
                            }
                        }
                    }

                    // get all interactive form fields of interactive form
                    PdfInteractiveFormField[] interactiveFormFields = _interactiveForm.GetFields();
                    // for each field
                    foreach (PdfInteractiveFormField field in interactiveFormFields)
                    {
                        // if field has "Calculate" action
                        if (field.IsCalculated)
                        {
                            // if list of fields with "Calculate" action does NOT contain field
                            if (!fieldsWithCalculateAction.Contains(field))
                            {
                                ListBoxItem item = new ListBoxItem();
                                item.Content = field.FullyQualifiedName;
                                item.Tag = field;
                                // add field to a form list box
                                interactiveFormListBox.Items.Add(item);
                            }
                        }
                    }
                }

                UpdateUI();
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the user interface of this control.
        /// </summary>
        private void UpdateUI()
        {
            int selectedIndex = interactiveFormListBox.SelectedIndex;

            moveDownButton.IsEnabled = selectedIndex != -1 && selectedIndex != interactiveFormListBox.Items.Count - 1;
            moveUpButton.IsEnabled = selectedIndex != -1 && selectedIndex > 0;
            okButton.IsEnabled = _interactiveForm != null;
        }

        /// <summary>
        /// "Ok" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (interactiveFormListBox.Items.Count == 0 &&
                _interactiveForm.CalculationOrder == null)
                return;

            if (_interactiveForm.CalculationOrder == null)
                _interactiveForm.CalculationOrder = new PdfInteractiveFormFieldList(_interactiveForm.Document);
            else
                _interactiveForm.CalculationOrder.Clear();

            PdfInteractiveFormFieldList list = _interactiveForm.CalculationOrder;

            foreach (ListBoxItem item in interactiveFormListBox.Items)
                list.Add((PdfInteractiveFormField)item.Tag);

            DialogResult = true;
        }

        /// <summary>
        /// "Cancel" button is clicked.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// PDF interactive field is moved up.
        /// </summary>
        private void moveUpButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSelected(true);
        }

        /// <summary>
        /// PDF interactive field is moved down.
        /// </summary>
        private void moveDownButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSelected(false);
        }

        /// <summary>
        /// Current interactive field is changed.
        /// </summary>
        private void interactiveFormListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Moves the selected interactive field in list box.
        /// </summary>
        /// <param name="moveUp">Determines that interactive field must be moved up.</param>
        private void MoveSelected(bool moveUp)
        {
            // get selected index
            int index = interactiveFormListBox.SelectedIndex;

            // if interactive field is move up
            if (moveUp)
                index--;
            else
                index++;

            // get selected item
            object item = interactiveFormListBox.SelectedItem;
            // remove selected item
            interactiveFormListBox.Items.Remove(item);
            // insert selected item
            interactiveFormListBox.Items.Insert(index, item);
            // set selected item
            interactiveFormListBox.SelectedItem = item;
        }

        #endregion

    }
}
