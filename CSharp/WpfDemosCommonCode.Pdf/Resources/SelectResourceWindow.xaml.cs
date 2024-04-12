using System.Collections.ObjectModel;
using System.Windows;

using Microsoft.Win32;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to select type of PDF image-resource.
    /// </summary>
    public partial class SelectResourceWindow : Window
    {

        #region Fields

        PdfImageResource _resource;
        PdfDocument _document;
        OpenFileDialog openImageFileDialog = new OpenFileDialog();

        #endregion



        #region Constructor

        public SelectResourceWindow(PdfDocument document)
        {
            InitializeComponent();

            _document = document;
            PdfImageResource[] images = _document.GetImages();
            exisingResourceButton.IsEnabled = images.Length > 0;

            openImageFileDialog.Filter = "All Image Files|*.png;*.gif;*.tif;*.tiff;*.jpg;*.jpeg;*.jb;*.jbig;*.bmp";
        }

        #endregion



        #region Properties

        public PdfImageResource Resource
        {
            get
            {
                return _resource;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of cancelButton object.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Handles the Click event of exisingResourceButton object.
        /// </summary>
        private void exisingResourceButton_Click(object sender, RoutedEventArgs e)
        {
            PdfResourcesViewerWindow resourcesViewerDialog = new PdfResourcesViewerWindow(_document);
            resourcesViewerDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            resourcesViewerDialog.Owner = this;
            resourcesViewerDialog.ShowFormResources = false;

            if (resourcesViewerDialog.ShowDialog() == true)
            {
                _resource = resourcesViewerDialog.SelectedResource as PdfImageResource;
                if (_resource != null)
                {
                    DialogResult = true;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of newResourceButton object.
        /// </summary>
        private void newResourceButton_Click(object sender, RoutedEventArgs e)
        {
            if (openImageFileDialog.ShowDialog().Value)
            {
                ImageResourceCompressionParamsWindow compressionParams = new ImageResourceCompressionParamsWindow();
                if (compressionParams.ShowDialog().Value)
                {
                    try
                    {
                        using (VintasoftImage image = new VintasoftImage(openImageFileDialog.FileName))
                            _resource = new PdfImageResource(_document, image, compressionParams.Compression, compressionParams.CompressionSettings);
                        DialogResult = true;
                    }
                    catch (System.Exception exc)
                    {
                        DemosTools.ShowErrorMessage(exc);
                    }
                }
            }
        }

        #endregion

    }
}
