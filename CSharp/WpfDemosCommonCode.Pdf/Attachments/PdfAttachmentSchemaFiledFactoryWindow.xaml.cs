using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree.FileAttachments;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Window that allows to create a new field of attachment collection schema.
    /// </summary>
    public partial class PdfAttachmentSchemaFieldFactoryWindow : Window
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAttachmentSchemaFieldFactoryWindow"/> class.
        /// </summary>
        public PdfAttachmentSchemaFieldFactoryWindow()
        {
            InitializeComponent();
            fieldTypeComboBox.Items.Add(AttachmentCollectionSchemaFieldDataType.Filename);
            fieldTypeComboBox.Items.Add(AttachmentCollectionSchemaFieldDataType.CompressedSize);
            fieldTypeComboBox.Items.Add(AttachmentCollectionSchemaFieldDataType.UncompressedSize);
            fieldTypeComboBox.Items.Add(AttachmentCollectionSchemaFieldDataType.FileDescription);
            fieldTypeComboBox.Items.Add(AttachmentCollectionSchemaFieldDataType.CreationDate);
            fieldTypeComboBox.Items.Add(AttachmentCollectionSchemaFieldDataType.ModificationDate);
            fieldTypeComboBox.Items.Add(AttachmentCollectionSchemaFieldDataType.String);
            fieldTypeComboBox.Items.Add(AttachmentCollectionSchemaFieldDataType.Number);
            fieldTypeComboBox.Items.Add(AttachmentCollectionSchemaFieldDataType.Date);
            fieldTypeComboBox.SelectedItem = AttachmentCollectionSchemaFieldDataType.String;
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets the data type of new field.
        /// </summary>
        public AttachmentCollectionSchemaFieldDataType DataType
        {
            get
            {
                return (AttachmentCollectionSchemaFieldDataType)fieldTypeComboBox.SelectedItem;
            }
        }

        /// <summary>
        /// Gets the displayed name of new field.
        /// </summary>
        public string DisplayedName
        {
            get
            {
                return displayedNameTextBox.Text;
            }
        }

        /// <summary>
        /// Gets the name of new field.
        /// </summary>
        public string FieldName
        {
            get
            {
                return nameTextBox.Text;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Creates new schema field.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="name">The name.</param>
        /// <param name="owner">The owner.</param>
        /// <returns>A new instance of PdfAttachmentCollectionSchemaField class.</returns>
        public static PdfAttachmentCollectionSchemaField CreateSchemaField(PdfDocument document, out string name, Window owner)
        {
            name = "";
            PdfAttachmentSchemaFieldFactoryWindow window = new PdfAttachmentSchemaFieldFactoryWindow();
            window.Owner = owner;
            if (window.ShowDialog().Value)
            {
                name = window.FieldName;
                return new PdfAttachmentCollectionSchemaField(document, window.DisplayedName, window.DataType);
            }
            return null;
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Cancels new field creation and closes this window.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Closes this form.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// Handles the TextChanged event of the displayedNameTextBox control.
        /// </summary>
        private void displayedNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Handles the TextChanged event of the nameTextBox control.
        /// </summary>
        private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Updates the User Interface.
        /// </summary>
        private void UpdateUI()
        {
            okButton.IsEnabled = DisplayedName != "" && FieldName != null;
        }

        #endregion

        #endregion

    }
}
