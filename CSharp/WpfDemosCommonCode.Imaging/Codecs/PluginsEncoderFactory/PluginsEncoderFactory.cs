using System.Windows.Forms;

using Vintasoft.Imaging.Codecs.Encoders;

using WpfDemosCommonCode.Imaging.Codecs.Dialogs;


namespace WpfDemosCommonCode.Imaging.Codecs
{
    /// <summary>
    /// Provides the ability to create the encoder (<see cref="EncoderBase"/> or 
    /// <see cref="MultipageEncoderBase"/>) for filename or encoder name.
    /// </summary>
    public class PluginsEncoderFactory : ImagingEncoderFactory
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginsEncoderFactory"/> class.
        /// </summary>
        public PluginsEncoderFactory()
            : base()
        {
        }

        #endregion



        #region Properties

        static PluginsEncoderFactory _default = null;
        /// <summary>
        /// Gets the default <see cref="PluginsEncoderFactory"/>.
        /// </summary>
        public static new PluginsEncoderFactory Default
        {
            get
            {
                if (_default == null)
                    _default = new PluginsEncoderFactory();

                return _default;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Shows the encoder settings dialog.
        /// </summary>
        /// <param name="encoder">The encoder.</param>
        /// <param name="canAddImagesToExistingFile">The value indicating whether encoder can add images to the existing multipage image file.</param>
        /// <returns>
        /// <b>True</b> if settings are applied to the encoder; otherwise, <b>false</b>.
        /// </returns>
        protected override bool ShowEncoderSettingsDialog(
            ref EncoderBase encoder,
            bool canAddImagesToExistingFile)
        {
            if (base.ShowEncoderSettingsDialog(ref encoder, canAddImagesToExistingFile))
                return true;

            // set encoder settings
            switch (encoder.Name)
            {
                case "Jpeg2000":
                    Jpeg2000EncoderSettingsWindow jpeg2000EncoderSettingsWindow = new Jpeg2000EncoderSettingsWindow();
                    SetEncoderSettingsDialogProperties(jpeg2000EncoderSettingsWindow);
                    if (jpeg2000EncoderSettingsWindow.ShowDialog() == false)
                        return false;

                    ((IJpeg2000Encoder)encoder).Settings = jpeg2000EncoderSettingsWindow.EncoderSettings;
                    return true;

#if !REMOVE_PDF_PLUGIN && !REMOVE_DOCCLEANUP_PLUGIN
                case "Pdf":
                    PdfEncoderSettingsWindow pdfEncoderSettingsWindow = new PdfEncoderSettingsWindow();
                    SetEncoderSettingsDialogProperties(pdfEncoderSettingsWindow);
                    pdfEncoderSettingsWindow.AppendExistingDocumentEnabled = canAddImagesToExistingFile;
                    if (pdfEncoderSettingsWindow.ShowDialog() == false)
                        return false;
                    if (pdfEncoderSettingsWindow.MrcCompressionSettings != null &&
                        pdfEncoderSettingsWindow.MrcCompressionSettings.EnableMrcCompression)
                    {
                        encoder.Dispose();
                        encoder = new PdfMrcEncoder();
                        ((PdfMrcEncoder)encoder).MrcCompressionSettings = pdfEncoderSettingsWindow.MrcCompressionSettings;
                    }
                        ((IPdfEncoder)encoder).Settings = pdfEncoderSettingsWindow.EncoderSettings;
                    ((MultipageEncoderBase)encoder).CreateNewFile = !pdfEncoderSettingsWindow.AppendExistingDocument;
                    return true;
#endif

                case "Jbig2":
                    Jbig2EncoderSettingsWindow jbig2EncoderSettingsWindow = new Jbig2EncoderSettingsWindow();
                    SetEncoderSettingsDialogProperties(jbig2EncoderSettingsWindow);
                    jbig2EncoderSettingsWindow.AppendExistingDocumentEnabled = canAddImagesToExistingFile;
                    if (jbig2EncoderSettingsWindow.ShowDialog() == false)
                        return false;

                    ((IJbig2Encoder)encoder).Settings = jbig2EncoderSettingsWindow.EncoderSettings;
                    ((MultipageEncoderBase)encoder).CreateNewFile = !jbig2EncoderSettingsWindow.AppendExistingDocument;
                    return true;
            }

            return false;
        }

        #endregion

    }
}
