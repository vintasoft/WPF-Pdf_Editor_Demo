using System;
using System.Windows;

using Microsoft.Win32;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Content.ImageExtraction;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view PDF contant image.
    /// </summary>
    public partial class ViewContentImageWindow : Window
    {

        #region Fields

        ContentImage _contentImage;
        float _zoom;
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        #endregion



        #region Constructor

        public ViewContentImageWindow(ContentImage contentImage, float zoom)           
        {
            InitializeComponent();

            _zoom = zoom;
            _contentImage = contentImage;
            
            UpdateContentImageInfo();
            transformedImageRadioButton.IsChecked = true;

            saveFileDialog.Filter = "PNG files|*.png";
            saveFileDialog.DefaultExt = "png";
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Checked event of ViewImageRadioButton object.
        /// </summary>
        private void viewImageRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ReloadResourceImage();
        }

        private void ReloadResourceImage()
        {
            try
            {
                VintasoftImage img = imageViewer.Image;

                if (originalImageRadioButton.IsChecked.Value == true)
                    imageViewer.Image = _contentImage.ImageResource.GetImage();
                else if (transformedImageRadioButton.IsChecked.Value == true)
                    imageViewer.Image = _contentImage.RenderImage(_zoom);

                if (img != null)
                    img.Dispose();
            }
            catch (Exception exc)
            {
                DemosTools.ShowErrorMessage(exc);
            }
        }

        /// <summary>
        /// Handles the Click event of SaveAsButton object.
        /// </summary>
        private void saveAsButton_Click(object sender, RoutedEventArgs e)
        {
            if (saveFileDialog.ShowDialog() == true)
                imageViewer.Image.Save(saveFileDialog.FileName);
        }

        /// <summary>
        /// Handles the Click event of ViewResourceButton object.
        /// </summary>
        private void viewResourceButton_Click(object sender, RoutedEventArgs e)
        {
            PdfCompression previousCompression = _contentImage.ImageResource.Compression;
            PdfResourcesViewerWindow window =
               new PdfResourcesViewerWindow(_contentImage.ImageResource);
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();

            if (previousCompression != _contentImage.ImageResource.Compression)
            {
                UpdateContentImageInfo();
                ReloadResourceImage();
            }
        }

        private void UpdateContentImageInfo()
        {
            compressionLabel.Content = string.Format("{0}", _contentImage.ImageResource.Compression);
            compressedSizeLabel.Content = string.Format("{0} bytes", _contentImage.ImageResource.Length);
            originalSizeLabel.Content = string.Format("{0}x{1} px", _contentImage.ImageResource.Width, _contentImage.ImageResource.Height);
            RegionF region = _contentImage.Region;
            LTlabel.Content = string.Format("({0};{1})", region.LeftTop.X, region.LeftTop.Y);
            LBlabel.Content = string.Format("({0};{1})", region.LeftBottom.X, region.LeftBottom.Y);
            RTlabel.Content = string.Format("({0};{1})", region.RightTop.X, region.RightTop.Y);
            RBlabel.Content = string.Format("({0};{1})", region.RightBottom.X, region.RightBottom.Y);
            regionGroupBox.Header = string.Format("Region in page content (Resolution: {0})", _contentImage.Resolution);
        }

        #endregion

    }
}
