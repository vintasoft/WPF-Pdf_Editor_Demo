using System.Windows;

using Vintasoft.Imaging.Pdf;

using WpfDemosCommonCode.Imaging;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to set the compression parameters of PDF image-resource.
    /// </summary>
    public partial class ImageResourceCompressionParamsWindow : Window
    {

        #region Constructor

        public ImageResourceCompressionParamsWindow()
        {
            InitializeComponent();

            compressionComboBox.Items.Add(PdfCompression.Auto);
            compressionComboBox.Items.Add(PdfCompression.Jpeg);
            compressionComboBox.Items.Add(PdfCompression.Zip);
            compressionComboBox.Items.Add(PdfCompression.CcittFax);
            compressionComboBox.Items.Add(PdfCompression.Jbig2);
            compressionComboBox.Items.Add(PdfCompression.Lzw);
            compressionComboBox.Items.Add(PdfCompression.RunLength);
            compressionComboBox.SelectedIndex = 0;
        }

        #endregion



        #region Properties

        PdfCompression _compression = PdfCompression.Auto;
        public PdfCompression Compression
        {
            get
            {
                return _compression;
            }
        }

        PdfCompressionSettings _settings = null;
        public PdfCompressionSettings CompressionSettings
        {
            get
            {
                return _settings;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of OkButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            _compression = (PdfCompression)compressionComboBox.SelectedItem;
            if (_settings == null)
                _settings = PdfCompressionSettings.DefaultSettings;
            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of CompressionParamsButton object.
        /// </summary>
        private void compressionParamsButton_Click(object sender, RoutedEventArgs e)
        {
            _settings = new PdfCompressionSettings();
            PropertyGridWindow propertyGridWindow = new PropertyGridWindow(_settings, "Compression Settings");
            propertyGridWindow.Topmost = true;
            propertyGridWindow.ShowDialog();
        }

        #endregion

    }
}
