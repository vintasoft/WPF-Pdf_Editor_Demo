using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Codecs.Encoders;

using WpfDemosCommonCode.Imaging.Codecs.Dialogs;

namespace WpfPdfEditorDemo
{
    /// <summary>
    /// A window that allows to specify parameters for image encoding.
    /// </summary>
    public partial class SetCompressionParamsWindow : Window
    {

        #region Fields

        PdfEncoderSettings _encoderSettings;

        #endregion



        #region Constructor

        public SetCompressionParamsWindow(int index, VintasoftImage image, PdfEncoderSettings encoderSettings)
        {
            InitializeComponent();

            _encoderSettings = encoderSettings;
            compressionComboBox.Items.Add(PdfImageCompression.Auto);
            compressionComboBox.Items.Add(PdfImageCompression.Zip);
            compressionComboBox.Items.Add(PdfImageCompression.Lzw);
            compressionComboBox.Items.Add(PdfImageCompression.Jpeg);
            if (AvailableEncoders.IsEncoderAvailable("Jpeg2000"))
                compressionComboBox.Items.Add(PdfImageCompression.Jpeg2000);
            compressionComboBox.Items.Add(PdfImageCompression.CcittFax);
            if (AvailableEncoders.IsEncoderAvailable("Jbig2"))
                compressionComboBox.Items.Add(PdfImageCompression.Jbig2);
            compressionComboBox.Items.Add(PdfImageCompression.None);
            _encoderSettings = encoderSettings;
            compressionComboBox.SelectedItem = encoderSettings.Compression;

            jpegQualityNumericUpDown.Value = encoderSettings.JpegQuality;
            jpegGrayscaleCheckBox.IsChecked = encoderSettings.JpegSaveAsGrayscale;
            jbig2LossyCheckBox.IsChecked = encoderSettings.Jbig2Settings.Lossy;
            jbig2UseGlobalsCheckBox.IsChecked = encoderSettings.Jbig2UseGlobals;

            imageNumberLabel.Content = (index + 1).ToString();
            pixelFormatLabel.Content = image.PixelFormat.ToString();
            sizeLabel.Content = string.Format("{0}x{1} pixels", image.Width, image.Height);
        }

        #endregion



        #region Properties

        bool _useCompressionForAllImages = false;
        public bool UseCompressionForAllImages
        {
            get
            {
                return _useCompressionForAllImages;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of OkButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            SetParams();
            DialogResult = true;
        }

        private void SetParams()
        {
            _encoderSettings.JpegQuality = (int)jpegQualityNumericUpDown.Value;
            _encoderSettings.JpegSaveAsGrayscale = jpegGrayscaleCheckBox.IsChecked.Value == true;
            _encoderSettings.Jbig2Settings.Lossy = jbig2LossyCheckBox.IsChecked.Value == true;
            _encoderSettings.Jbig2UseGlobals = jbig2LossyCheckBox.IsChecked.Value == true;
        }

        /// <summary>
        /// Handles the Click event of ForAllButton object.
        /// </summary>
        private void forAllButton_Click(object sender, RoutedEventArgs e)
        {
            _useCompressionForAllImages = true;
            SetParams();
            DialogResult = true;
        }

        /// <summary>
        /// Handles the SelectionChanged event of CompressionComboBox object.
        /// </summary>
        private void compressionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _encoderSettings.Compression = (PdfImageCompression)compressionComboBox.SelectedItem;
            jpegParamsGroupBox.Visibility = Visibility.Collapsed;
            jbig2ParamsGroupBox.Visibility = Visibility.Collapsed;
            jpeg2000CompressionGroupBox.Visibility = Visibility.Collapsed;
            switch (_encoderSettings.Compression)
            {
                case PdfImageCompression.Jpeg:
                    jpegParamsGroupBox.Visibility = Visibility.Visible;
                    break;
                case PdfImageCompression.Jbig2:
                    jbig2ParamsGroupBox.Visibility = Visibility.Visible;
                    break;
                case PdfImageCompression.Jpeg2000:
                    jpeg2000CompressionGroupBox.Visibility = Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// Handles the Click event of Jpeg200SettingsButton object.
        /// </summary>
        private void jpeg200SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Jpeg2000EncoderSettingsWindow jpeg2000SettingsDialog = new Jpeg2000EncoderSettingsWindow();
            jpeg2000SettingsDialog.EncoderSettings = _encoderSettings.Jpeg2000Settings;
            jpeg2000SettingsDialog.ShowDialog();
        }

        #endregion

    }
}
