using System.Windows;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Codecs.Decoders;
using Vintasoft.Imaging.Drawing;
#if !REMOVE_PDF_PLUGIN
using Vintasoft.Imaging.Pdf;
#endif
using Vintasoft.Imaging.Wpf;

namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A window that allows to view and edit the composite rendering settings.
    /// </summary>
    public partial class CompositeRenderingSettingsWindow : Window
    {

        #region Fields

        /// <summary>
        /// Contains image/document rendering settings.
        /// </summary>
        RenderingSettings _renderingSettings;

        #endregion



        #region Constructors

        /// <summary>
        /// Inititalizes new instance of <see cref="CompositeRenderingSettingsWindow"/>.
        /// </summary>
        public CompositeRenderingSettingsWindow()
        {
            InitializeComponent();

            // init "Text Rendering Hint"
            textRenderingHintComboBox.Items.Add(TextRenderingHint.Auto);
            textRenderingHintComboBox.Items.Add(TextRenderingHint.AntiAlias);
            textRenderingHintComboBox.Items.Add(TextRenderingHint.AntiAliasGridFit);
            textRenderingHintComboBox.Items.Add(TextRenderingHint.ClearTypeGridFit);
            textRenderingHintComboBox.Items.Add(TextRenderingHint.SingleBitPerPixel);
            textRenderingHintComboBox.Items.Add(TextRenderingHint.SingleBitPerPixelGridFit);
            textRenderingHintComboBox.Items.Add(TextRenderingHint.SystemDefault);

            // init "Interpolation Mode"
            interpolationModeComboBox.Items.Add(ImageInterpolationMode.Bicubic);
            interpolationModeComboBox.Items.Add(ImageInterpolationMode.Bilinear);
            interpolationModeComboBox.Items.Add(ImageInterpolationMode.HighQualityBicubic);
            interpolationModeComboBox.Items.Add(ImageInterpolationMode.HighQualityBilinear);
            interpolationModeComboBox.Items.Add(ImageInterpolationMode.NearestNeighbor);

            // init "Smoothing Mode"
            smoothingModeComboBox.Items.Add(DrawingSmoothingMode.AntiAlias);
            smoothingModeComboBox.Items.Add(DrawingSmoothingMode.None);
        }

        /// <summary>
        /// Inititalizes new instance of <see cref="CompositeRenderingSettingsWindow"/>.
        /// </summary>
        /// <param name="renderingSettings">Current rendering settings.</param>
        public CompositeRenderingSettingsWindow(RenderingSettings renderingSettings)
            : this()
        {
            if (renderingSettings == null)
                renderingSettings = RenderingSettings.Empty;

            _renderingSettings = renderingSettings;

            // init common settings

            if (renderingSettings.IsEmpty || renderingSettings.Resolution.IsEmpty())
            {
                resolutionSettingsCheckBox.IsChecked = false;
            }
            else
            {
                resolutionSettingsCheckBox.IsChecked = true;
                horizontalResolutionNumericUpDown.Value = (int)renderingSettings.Resolution.Horizontal;
                verticalResolutionNumericUpDown.Value = (int)renderingSettings.Resolution.Vertical;
            }

            smoothingModeComboBox.SelectedItem = renderingSettings.SmoothingMode;
            interpolationModeComboBox.SelectedItem = renderingSettings.InterpolationMode;
            optimizeImageDrawingCheckBox.IsChecked = renderingSettings.OptimizeImageDrawing;

            // init PDF settings
            bool isPdfSettingsEnabled= false;

#if !REMOVE_PDF_PLUGIN
            // get the PDF rendering settings from image rendering settings
            PdfRenderingSettings pdfSettings = CompositeRenderingSettings.GetRenderingSettings<PdfRenderingSettings>(renderingSettings);

            // if PDF rendering settings are found
            if (pdfSettings != null)
            {
                // set the form properties according to the PDF rendering settings

                PdfAnnotationRenderingMode renderingMode = pdfSettings.AnnotationRenderingMode;
                vintasoftAnnotationsCheckBox.IsChecked = (renderingMode & PdfAnnotationRenderingMode.VintasoftAnnotations) != 0;
                nonMarkupAnnotationsCheckBox.IsChecked = (renderingMode & PdfAnnotationRenderingMode.NonMarkupAnnotations) != 0;
                markupAnnotationsCheckBox.IsChecked = (renderingMode & PdfAnnotationRenderingMode.MarkupAnnotations) != 0;
                renderInvisibleCheckBox.IsChecked = (renderingMode & PdfAnnotationRenderingMode.RenderInvisible) != 0;
                renderHiddenCheckBox.IsChecked = (renderingMode & PdfAnnotationRenderingMode.RenderHidden) != 0;
                renderPrintableCheckBox.IsChecked = (renderingMode & PdfAnnotationRenderingMode.RenderPrintable) != 0;
                renderNoViewCheckBox.IsChecked = (renderingMode & PdfAnnotationRenderingMode.RenderNoView) != 0;
                renderDisplayableCheckBox.IsChecked = (renderingMode & PdfAnnotationRenderingMode.RenderDisplayable) != 0;

                ignoreImageInterpolationFlagCheckBox.IsChecked = pdfSettings.IgnoreImageInterpolateFlag;
                optimizePatternRenderingCheckBox.IsChecked = pdfSettings.OptimizePatternRendering;

                isPdfSettingsEnabled = true;
            }
#endif

            // if PDF document rendering settings are not enabled
            if (!isPdfSettingsEnabled)
                // remove tab page with PDF document rendering settings from the form
                pdfRenderingSettingsTabItem.Visibility = Visibility.Collapsed;

            // init the Office document settings
            bool isOfficeDocumentSettingsEnabled = false;

#if !REMOVE_OFFICE_PLUGIN
            // get the Office document markup rendering settings from image rendering settings
            MarkupRenderingSettings markupSettings = CompositeRenderingSettings.GetRenderingSettings<MarkupRenderingSettings>(renderingSettings);

            // if Office document markup rendering settings are found
            if (markupSettings != null)
            {
                // set the form properties according to the Office document markup rendering settings
                textRenderingHintComboBox.SelectedItem = markupSettings.TextRenderingHint;
                showInvisibleTableBordersCheckBox.IsChecked = markupSettings.ShowInvisibleTableBorders;
                invisibleTableBordersColorPanelControl.CanSetColor = true;
                invisibleTableBordersColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(markupSettings.InvisibleTableBordersColor);
                invisibleTableBordersGroupBox.IsEnabled = showInvisibleTableBordersCheckBox.IsChecked.Value == true;
                showNonPrintableCharactersCheckBox.IsChecked = markupSettings.ShowNonPrintableCharacters;

                isOfficeDocumentSettingsEnabled = true;
            }
#endif

            // if Office document rendering settings are not enabled
            if (!isOfficeDocumentSettingsEnabled)
                // remove tab page with Office document rendering settings from the form
                officeRenderingSettingsTabItem.Visibility = Visibility.Collapsed;
        }

        #endregion



        #region Methods

        /// <summary>
        /// "OK" button is clicked.
        /// </summary>
        private void btOk_Click(object sender, RoutedEventArgs e)
        {
            // set common settings
            // if resolution is specified
            if (resolutionSettingsCheckBox.IsChecked.Value == true)
            {
                // get rendering resolution
                _renderingSettings.Resolution = new Resolution(
                    (float)horizontalResolutionNumericUpDown.Value,
                    (float)verticalResolutionNumericUpDown.Value);
            }
            else
            {
                // set default resolution
                _renderingSettings.Resolution = RenderingSettings.Empty.Resolution;
            }

            // get rendering interpolation mode
            _renderingSettings.InterpolationMode = (ImageInterpolationMode)interpolationModeComboBox.SelectedItem;
            // get rendering smotthing mode
            _renderingSettings.SmoothingMode = (DrawingSmoothingMode)smoothingModeComboBox.SelectedItem;
            // get image drawing optimization flag
            _renderingSettings.OptimizeImageDrawing = optimizeImageDrawingCheckBox.IsChecked.Value == true;


#if !REMOVE_PDF_PLUGIN
            // if tab page with PDF rendering settings presents
            if (pdfRenderingSettingsTabItem.Visibility == Visibility.Visible)
            {
                // get all of the PDF rendering settings objects from rendering settings
                PdfRenderingSettings[] pdfSettings = CompositeRenderingSettings.FindRenderingSettings<PdfRenderingSettings>(_renderingSettings);

                foreach (PdfRenderingSettings settings in pdfSettings)
                {
                    // copy setting from dialog to each PDF rendering settings object
                    settings.AnnotationRenderingMode = GetAnnotationRenderingMode();
                    settings.IgnoreImageInterpolateFlag = ignoreImageInterpolationFlagCheckBox.IsChecked.Value == true;
                    settings.OptimizePatternRendering = optimizePatternRenderingCheckBox.IsChecked.Value == true;
                }
            }
#endif

#if !REMOVE_OFFICE_PLUGIN
            // if tab page with Office document rendering settings presents
            if (officeRenderingSettingsTabItem.Visibility == Visibility.Visible)
            {
                // get all of the markup rendering settings objects from rendering settings
                MarkupRenderingSettings[] markupSettings = CompositeRenderingSettings.FindRenderingSettings<MarkupRenderingSettings>(_renderingSettings);

                foreach (MarkupRenderingSettings settings in markupSettings)
                {
                    // copy setting from dialog to each markup rendering settings object
                    settings.TextRenderingHint = (TextRenderingHint)textRenderingHintComboBox.SelectedItem;
                    settings.ShowInvisibleTableBorders = showInvisibleTableBordersCheckBox.IsChecked.Value == true;
                    settings.ShowNonPrintableCharacters = showNonPrintableCharactersCheckBox.IsChecked.Value == true;
                    settings.InvisibleTableBordersColor = WpfObjectConverter.CreateDrawingColor(invisibleTableBordersColorPanelControl.Color);
                }
            }
#endif
            DialogResult = true;
        }

        /// <summary>
        /// "Cancel" button is clicked.
        /// </summary>
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

#if !REMOVE_PDF_PLUGIN
        /// <summary>
        /// Returns the PDF annotation rendering mode, according to selected values on the form.
        /// </summary>
        /// <returns>PDF annotation rendering mode, according to selected values on the form.</returns>
        private PdfAnnotationRenderingMode GetAnnotationRenderingMode()
        {
            PdfAnnotationRenderingMode result = PdfAnnotationRenderingMode.None;

            if (vintasoftAnnotationsCheckBox.IsChecked.Value == true)
                result |= PdfAnnotationRenderingMode.VintasoftAnnotations;
            if (nonMarkupAnnotationsCheckBox.IsChecked.Value == true)
                result |= PdfAnnotationRenderingMode.NonMarkupAnnotations;
            if (markupAnnotationsCheckBox.IsChecked.Value == true)
                result |= PdfAnnotationRenderingMode.MarkupAnnotations;
            if (renderInvisibleCheckBox.IsChecked.Value == true)
                result |= PdfAnnotationRenderingMode.RenderInvisible;
            if (renderHiddenCheckBox.IsChecked.Value == true)
                result |= PdfAnnotationRenderingMode.RenderHidden;
            if (renderPrintableCheckBox.IsChecked.Value == true)
                result |= PdfAnnotationRenderingMode.RenderPrintable;
            if (renderNoViewCheckBox.IsChecked.Value == true)
                result |= PdfAnnotationRenderingMode.RenderNoView;
            if (renderDisplayableCheckBox.IsChecked.Value == true)
                result |= PdfAnnotationRenderingMode.RenderDisplayable;

            return result;
        }
#endif

        /// <summary>
        /// The resolution settings is enabled/disabled.
        /// </summary>
        private void resolutionSettingsCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            resolutionSettingsGroupBox.IsEnabled = resolutionSettingsCheckBox.IsChecked.Value == true;
        }

        /// <summary>
        /// The invisible table borders color panel control is enabled/disabled.
        /// </summary>
        private void showInvisibleTableBordersCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            invisibleTableBordersGroupBox.IsEnabled = showInvisibleTableBordersCheckBox.IsChecked.Value == true;
        }

        #endregion

    }
}
