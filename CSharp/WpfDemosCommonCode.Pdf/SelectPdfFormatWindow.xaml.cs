using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Security;
using WpfDemosCommonCode.Pdf.Security;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to select format of PDF document.
    /// </summary>
    public partial class SelectPdfFormatWindow : Window
    {

        #region Constructor

        public SelectPdfFormatWindow(PdfFormat initialFormat, EncryptionSystem initialEncryptionSettings)
        {
            InitializeComponent();
            _format = initialFormat;
            pdfVersion.Text = _format.Version;
            if (pdfVersion.SelectedIndex > 4)
            {
                compressedCrossReferenceTableCheckBox.IsChecked = _format.CompressedCrossReferenceTable;
                compressedObjectStreamsCheckBox.IsChecked = _format.CompressedObjectStreams;
            }
            else
            {
                compressedCrossReferenceTableCheckBox.IsChecked = false;
                compressedObjectStreamsCheckBox.IsChecked = false;
            }
            binaryFormatCheckBox.IsChecked = _format.BinaryFormat;
            _newEncryptionSettings = initialEncryptionSettings;

            if (initialFormat.VersionNumber >= 12)
                linearizedCheckBox.IsChecked = _format.LinearizedFormat;
        }

        #endregion



        #region Properties

        PdfFormat _format;
        /// <summary>
        /// Gets the PDF document format.
        /// </summary>
        public PdfFormat Format
        {
            get
            {
                return _format;
            }
        }

        EncryptionSystem _newEncryptionSettings;
        /// <summary>
        /// Gets the new encryption settings of PDF document.
        /// </summary>
        public EncryptionSystem NewEncryptionSettings
        {
            get
            {
                return _newEncryptionSettings;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of okButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            _format = new PdfFormat(pdfVersion.Text,
                compressedCrossReferenceTableCheckBox.IsChecked.Value == true,
                compressedObjectStreamsCheckBox.IsChecked.Value == true,
                binaryFormatCheckBox.IsChecked.Value == true,
                linearizedCheckBox.IsChecked == true);
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Handles the Click event of cancelButton object.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handles the SelectionChanged event of pdfVersion object.
        /// </summary>
        private void pdfVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            securityButton.IsEnabled = pdfVersion.SelectedIndex >= 1;
            binaryFormatCheckBox.IsChecked = true;
            if (pdfVersion.SelectedIndex > 4)
            {
                compressedObjectStreamsCheckBox.IsChecked = true;
                compressedObjectStreamsCheckBox.IsEnabled = true;
                compressedCrossReferenceTableCheckBox.IsChecked = true;
                compressedCrossReferenceTableCheckBox.IsEnabled = true;
            }
            else
            {
                compressedObjectStreamsCheckBox.IsChecked = false;
                compressedObjectStreamsCheckBox.IsEnabled = false;
                compressedCrossReferenceTableCheckBox.IsChecked = false;
                compressedCrossReferenceTableCheckBox.IsEnabled = false;
            }
            if (pdfVersion.SelectedIndex >= 2)
            {
                linearizedCheckBox.IsChecked = _format.LinearizedFormat;
                linearizedCheckBox.IsEnabled = true;
            }
            else
            {
                linearizedCheckBox.IsChecked = false;
                linearizedCheckBox.IsEnabled = false;
            }
        }


        /// <summary>
        /// Handles the Click event of securityButton object.
        /// </summary>
        private void securityButton_Click(object sender, RoutedEventArgs e)
        {
            SecuritySettingsWindow securitySettings = new SecuritySettingsWindow(_newEncryptionSettings);
            if (securitySettings.ShowDialog().Value)
                _newEncryptionSettings = securitySettings.NewEncryptionSettings;
        }

        #endregion

    }
}
