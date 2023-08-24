using System;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree.FileAttachments;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Window that allows to create a new field for attachment data field.
    /// </summary>
    public partial class PdfAttachmentDataFieldFactoryWindow : Window
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAttachmentDataFieldFactoryWindow"/> class.
        /// </summary>
        public PdfAttachmentDataFieldFactoryWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAttachmentDataFieldFactoryWindow"/> class.
        /// </summary>
        /// <param name="schema">PDF attachment collection schema.</param>
        public PdfAttachmentDataFieldFactoryWindow(PdfAttachmentCollectionSchema schema)
            : this()
        {
            if (schema != null)
            {
                foreach (String fieldName in schema.Keys)
                {
                    AttachmentCollectionSchemaFieldDataType dataType = schema[fieldName].DataType;
                    if (dataType == AttachmentCollectionSchemaFieldDataType.String ||
                        dataType == AttachmentCollectionSchemaFieldDataType.Number ||
                        dataType == AttachmentCollectionSchemaFieldDataType.Date)
                    {
                        fieldNameComboBox.Items.Add(fieldName);
                    }
                }
            }
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets the name of new field.
        /// </summary>
        public string FieldName
        {
            get
            {
                return fieldNameComboBox.Text;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Creates new data field.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="name">The name.</param>
        /// <param name="owner">The owner.</param>
        /// <returns>A new instance of PdfAttachmentDataField class.</returns>
        public static PdfAttachmentDataField CreateDataField(PdfDocument document, out string name, Window owner)
        {
            name = "";
            PdfAttachmentDataFieldFactoryWindow window = new PdfAttachmentDataFieldFactoryWindow(document.Attachments.Schema);
            window.Owner = owner;
            if (window.ShowDialog().Value)
            {
                name = window.FieldName;
                return new PdfAttachmentDataField(document);
            }
            return null;
        }

        #endregion

        
        #region PRIVATE

        /// <summary>
        /// Closes this form.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// Cancels new field creation and closes this form.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Handles the SelectedValueChanged event of the fieldNameComboBox control.
        /// </summary>
        private void fieldNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Updates the User Interface.
        /// </summary>
        private void UpdateUI()
        {
            okButton.IsEnabled = FieldName != null;
        }

        #endregion

        #endregion


    }
}
