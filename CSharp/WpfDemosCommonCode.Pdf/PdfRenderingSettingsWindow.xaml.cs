using System;
using System.Windows;
using Vintasoft.Imaging;
using Vintasoft.Imaging.Codecs.Decoders;
using Vintasoft.Imaging.Pdf;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view and edit the rendering settings of PDF document.
    /// </summary>
    public partial class PdfRenderingSettingsWindow : Window
    {

        #region Constructor

        public PdfRenderingSettingsWindow(PdfRenderingSettings settings)
        {
            InitializeComponent();

            okButton.Focus();

            _renderingSettings = settings;

            foreach (object value in Enum.GetValues(typeof(PdfRenderingMode)))
                renderingMode.Items.Add(value);
            renderingMode.SelectedItem = _renderingSettings.RenderingMode;

            if (settings.Resolution.IsEmpty())
            {
                dpiDefault.IsChecked = true;
            }
            else
            {
                dpiDefault.IsChecked = false;
                horizontalResolution.Value = (int)settings.Resolution.Horizontal;
                verticalResolution.Value = (int)settings.Resolution.Vertical;
            }
            dpiDefault_Click(dpiDefault, null);
            drawAnnotationsCheckBox.IsChecked = _renderingSettings.DrawPdfAnnotations || _renderingSettings.DrawVintasoftAnnotations;
            cacheResourcesCheckBox.IsChecked = _renderingSettings.CacheResources;
            cropPageAtCropBoxCheckBox.IsChecked = _renderingSettings.UseCropBox;
            useRotatePropertyCheckBox.IsChecked = _renderingSettings.UsePageRotateProperty;
        }

        #endregion



        #region Properties

        PdfRenderingSettings _renderingSettings;
        public PdfRenderingSettings RenderingSettings
        {
            get
            {
                return _renderingSettings;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of OkButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (dpiDefault.IsChecked.Value == true)
                _renderingSettings.Resolution = Resolution.Empty;
            else
                _renderingSettings.Resolution = new Resolution((float)horizontalResolution.Value, (float)verticalResolution.Value);
            _renderingSettings.RenderingMode = (PdfRenderingMode)renderingMode.SelectedItem;
            _renderingSettings.DrawPdfAnnotations = drawAnnotationsCheckBox.IsChecked.Value == true;
            _renderingSettings.DrawVintasoftAnnotations = drawAnnotationsCheckBox.IsChecked.Value == true;
            _renderingSettings.CacheResources = cacheResourcesCheckBox.IsChecked.Value == true;
            _renderingSettings.UseCropBox = cropPageAtCropBoxCheckBox.IsChecked.Value == true;
            _renderingSettings.UsePageRotateProperty = useRotatePropertyCheckBox.IsChecked.Value == true;
            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of DpiDefault object.
        /// </summary>
        private void dpiDefault_Click(object sender, RoutedEventArgs e)
        {
            horizontalResolution.IsEnabled = dpiDefault.IsChecked.Value == false;
            verticalResolution.IsEnabled = dpiDefault.IsChecked.Value == false;
        }

        /// <summary>
        /// Handles the Click event of CancelButton object.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Handles the ValueChanged event of HorizontalResolution object.
        /// </summary>
        private void horizontalResolution_ValueChanged(object sender, EventArgs e)
        {
            verticalResolution.Value = horizontalResolution.Value;
        }

        #endregion

    }
}
