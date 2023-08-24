using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view and edit a list of PDF interactive form fields.
    /// </summary>
    public partial class PdfInteractiveFormFieldListEditorControl : UserControl
    {

        #region Nested class

        /// <summary>
        /// List box item.
        /// </summary>
        class ListBoxItem
        {

            /// <summary>
            /// Initializes a new instance of the <see cref="ListBoxItem"/> class.
            /// </summary>
            /// <param name="field">The field.</param>
            public ListBoxItem(PdfInteractiveFormField field)
            {
                Field = field;
            }



            /// <summary>
            /// Gets or sets the field.
            /// </summary>
            public PdfInteractiveFormField Field
            {
                get;
                set;
            }



            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return Field.FullyQualifiedName;
            }

        }

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of
        /// the <see cref="PdfInteractiveFormFieldListEditorControl"/> class.
        /// </summary>
        public PdfInteractiveFormFieldListEditorControl()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets or sets the PDF interactive form.
        /// </summary>
        public PdfDocumentInteractiveForm InteractiveForm
        {
            get
            {
                return pdfInteractiveFormFieldTree.InteractiveForm;
            }
            set
            {
                pdfInteractiveFormFieldTree.InteractiveForm = value;
                pdfInteractiveFormFieldTree.RefreshInteractiveFormTreeSafely();
            }
        }

        /// <summary>
        /// Gets or sets an array of selected PDF interactive form fields.
        /// </summary>
        public PdfInteractiveFormField[] SelectedFields
        {
            get
            {
                ItemCollection items = selectedFieldsListBox.Items;
                PdfInteractiveFormField[] result =
                    new PdfInteractiveFormField[items.Count];

                for (int i = 0; i < result.Length; i++)
                    result[i] = ((ListBoxItem)items[i]).Field;

                return result;
            }
            set
            {
                selectedFieldsListBox.BeginInit();
                try
                {
                    ItemCollection items = selectedFieldsListBox.Items;
                    items.Clear();

                    if (value != null)
                    {
                        foreach (PdfInteractiveFormField field in value)
                        {
                            items.Add(new ListBoxItem(field));
                        }
                    }
                }
                finally
                {
                    selectedFieldsListBox.EndInit();
                }

                UpdateUI();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether tree view must show only form fields
        /// that can export their values.
        /// </summary>
        public bool ShowOnlyExportableFields
        {
            get
            {
                return pdfInteractiveFormFieldTree.ShowOnlyExportableFields;
            }
            set
            {
                pdfInteractiveFormFieldTree.ShowOnlyExportableFields = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether tree view must show only form fields
        /// that can reset their values.
        /// </summary>
        public bool ShowOnlyResettableFields
        {
            get
            {
                return pdfInteractiveFormFieldTree.ShowOnlyResettableFields;
            }
            set
            {
                pdfInteractiveFormFieldTree.ShowOnlyResettableFields = value;
            }
        }

        #endregion



        #region Methods

        #region Buttons

        /// <summary>
        /// Adds PDF interactive form field to the array of selected PDF interactive form fields.
        /// </summary>
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            AddSelectedField();
        }

        /// <summary>
        /// Adds PDF interactive form field to the array of selected PDF interactive form fields.
        /// </summary>
        private void AddSelectedField()
        {
            selectedFieldsListBox.BeginInit();
            try
            {
                PdfInteractiveFormField selectedField = pdfInteractiveFormFieldTree.SelectedField;

                ListBoxItem item = new ListBoxItem(selectedField);
                selectedFieldsListBox.Items.Add(item);
                selectedFieldsListBox.SelectedIndex = selectedFieldsListBox.Items.Count - 1;

                UpdateUI();
            }
            finally
            {
                selectedFieldsListBox.EndInit();
            }
        }

        /// <summary>
        /// Remove PDF interactive form field from the array of selected PDF interactive form fields.
        /// </summary>
        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedField();
        }

        /// <summary>
        /// Remove PDF interactive form field from the array of selected PDF interactive form fields.
        /// </summary>
        private void RemoveSelectedField()
        {
            int newIndex = selectedFieldsListBox.SelectedIndex;
            if (newIndex == selectedFieldsListBox.Items.Count - 1)
                newIndex--;

            selectedFieldsListBox.Items.Remove(selectedFieldsListBox.SelectedItem);
            selectedFieldsListBox.SelectedIndex = newIndex;

            UpdateUI();
        }

        /// <summary>
        /// Removes all PDF interactive form fields from
        /// the array of selected PDF interactive form fields.
        /// </summary>
        private void removeAllButton_Click(object sender, RoutedEventArgs e)
        {
            selectedFieldsListBox.Items.Clear();
            UpdateUI();
        }

        #endregion


        #region PdfInteractiveFormFieldTree

        /// <summary>
        /// Mouse is double clicked in PdfInteractiveFormFieldTree.
        /// </summary>
         private void pdfInteractiveFormFieldTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (pdfInteractiveFormFieldTree.SelectedField != null && !SelectedFieldContainedListBox())
                AddSelectedField();
        }

        /// <summary>
        /// Handles the SelectedItemChanged event of the PdfInteractiveFormFieldTree control.
        /// </summary>
        private void pdfInteractiveFormFieldTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UpdateUI();
        }

        #endregion


        #region SelectedFieldsListBox

        /// <summary>
        /// Selected PDF interactive form field is changed.
        /// </summary>
        private void selectedFieldsListBox_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Mouse is double click in SelectedFieldsListBox.
        /// </summary>
        private void selectedFieldsListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (selectedFieldsListBox.SelectedItem != null)
                RemoveSelectedField();
        }

        #endregion


        /// <summary>
        /// Updates the user interface of this control.
        /// </summary>
        private void UpdateUI()
        {
            addButton.IsEnabled = pdfInteractiveFormFieldTree.SelectedField != null &&
                                  !SelectedFieldContainedListBox();
            removeButton.IsEnabled = selectedFieldsListBox.SelectedItem != null;
            removeAllButton.IsEnabled = selectedFieldsListBox.Items.Count > 0;
        }


        /// <summary>
        /// Determines that SelectedFieldsListBox contains field from  PdfInteractiveFormFieldTree.
        /// </summary>
        /// <returns>
        /// <b>True</b> - SelectedFieldsListBox contains field from PdfInteractiveFormFieldTree;
        /// <b>false</b> - SelectedFieldsListBox does NOT contain field from PdfInteractiveFormFieldTree.
        /// </returns>
        private bool SelectedFieldContainedListBox()
        {
            // get selected field
            PdfInteractiveFormField selectedField = pdfInteractiveFormFieldTree.SelectedField;
            // if selected fiels exist
            if (selectedField != null)
            {
                // get items
                ItemCollection items = selectedFieldsListBox.Items;
                // find selected field
                foreach (ListBoxItem item in items)
                {
                    if (item.Field == selectedField)
                        return true;
                }
            }

            return false;
        }

        #endregion

    }
}
