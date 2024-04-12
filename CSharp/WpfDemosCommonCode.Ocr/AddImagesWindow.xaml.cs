using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;

using Microsoft.Win32;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Codecs.Decoders;
using Vintasoft.Imaging.ImageProcessing;
using Vintasoft.Imaging.ImageProcessing.Color;
using Vintasoft.Imaging.ImageProcessing.Document;
using Vintasoft.Imaging.ImageProcessing.Info;
using Vintasoft.Imaging.ImageProcessing.Transforms;

using WpfDemosCommonCode.Imaging;
using WpfDemosCommonCode.Imaging.Codecs;
using WpfDemosCommonCode.Twain;

namespace WpfDemosCommonCode.Ocr
{
    /// <summary>
    /// A window that allows to add images.
    /// </summary>
    public partial class AddImagesWindow : Window
    {

        #region Fields

        /// <summary>
        /// The simple TWAIN manager to acquire images from scanner.
        /// </summary>
        WpfSimpleTwainManager _simpleTwainManager;

        /// <summary>
        /// The open file dialog.
        /// </summary>
        OpenFileDialog _openImageFileDialog = new OpenFileDialog();

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AddImagesWindow"/> class.
        /// </summary>
        public AddImagesWindow()
        {
            InitializeComponent();

            // set filters in open dialog
            CodecsFileFilters.SetFilters(_openImageFileDialog);

            // set CustomFontProgramsController for all opened documents
            CustomFontProgramsController.SetDefaultFontProgramsController();

            // set the initial directory in open dialog
            DemosTools.SetTestFilesFolder(_openImageFileDialog);

            _simpleTwainManager = new WpfSimpleTwainManager(this, thumbnailViewer1.Images);

            imageViewer1.MasterViewer = thumbnailViewer1;
            // disable management of images rendering settings
            imageViewer1.ImageRenderingSettings = null;

            // update the UI
            UpdateUI();
        }

        #endregion



        #region Properties

        Dictionary<VintasoftImage, ReadOnlyCollection<ImageRegion>> _segmentationResults;
        /// <summary>
        /// Gets the segmentation results.
        /// </summary>
        public Dictionary<VintasoftImage, ReadOnlyCollection<ImageRegion>> SegmentationResults
        {
            get
            {
                return _segmentationResults;
            }
        }

        bool _openFileWhenShown = false;
        /// <summary>
        /// Gets or sets a value indicating whether
        /// to open image from file when form is shown.
        /// </summary>
        public bool OpenFileWhenShown
        {
            get
            {
                return _openFileWhenShown;
            }
            set
            {
                _openFileWhenShown = value;
            }
        }

        bool _scanImageWhenShown = false;
        /// <summary>
        /// Gets or sets a value indicating whether
        /// to acquire image from scanner when form is shown.
        /// </value>
        public bool ScanImageWhenShown
        {
            get
            {
                return _scanImageWhenShown;
            }
            set
            {
                _scanImageWhenShown = value;
            }
        }

        /// <summary>
        /// Gets currently opened images.
        /// </summary>
        public ImageCollection Images
        {
            get
            {
                return thumbnailViewer1.Images;
            }
        }

        bool _isImageOpening = false;
        /// <summary>
        /// Gets or sets a value indicating whether image is opening.
        /// </summary>
        bool IsImageOpening
        {
            get
            {
                return _isImageOpening;
            }
            set
            {
                _isImageOpening = value;
                UpdateUI();
            }
        }

        bool _isImageProcessing = false;
        /// <summary>
        /// Gets or sets a value indicating whether image is processing.
        /// </summary>
        bool IsImageProcessing
        {
            get
            {
                return _isImageProcessing;
            }
            set
            {
                _isImageProcessing = value;
                UpdateUI();
            }
        }

        #endregion



        #region Methods

        #region Add Images

        /// <summary>
        /// Handles the Click event of addFromFileButton object.
        /// </summary>
        private void addFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            // add image(s) from file
            AddImagesFromFiles();
        }

        /// <summary>
        /// Handles the Click event of addFromScannerButton object.
        /// </summary>
        private void addFromScannerButton_Click(object sender, RoutedEventArgs e)
        {
            // add image(s) from scanner
            AddImageFromScanner();
        }

        #endregion


        #region Image Processing

        /// <summary>
        /// Handles the Click event of invertButton object.
        /// </summary>
        private void invertButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteProcessingCommand(new InvertCommand());
        }

        /// <summary>
        /// Handles the Click event of adaptiveBinarizeButton object.
        /// </summary>
        private void adaptiveBinarizeButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteProcessingCommand(new ChangePixelFormatToBlackWhiteCommand(BinarizationMode.Adaptive));
        }

        /// <summary>
        /// Handles the Click event of globalBinarizeButton object.
        /// </summary>
        private void globalBinarizeButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteProcessingCommand(new ChangePixelFormatToBlackWhiteCommand(BinarizationMode.Global));
        }

        /// <summary>
        /// Handles the Click event of thresholdBinarizeButton object.
        /// </summary>
        private void thresholdBinarizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int threshold = int.Parse(thresholdComboBox.Text, CultureInfo.InvariantCulture);
                ExecuteProcessingCommand(new ChangePixelFormatToBlackWhiteCommand(threshold));
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of rotateButton object.
        /// </summary>
        private void rotateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double angle = double.Parse(rotateAngleComboBox.Text, CultureInfo.InvariantCulture);
                ExecuteProcessingCommand(new RotateCommand(angle));
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of removeTablesButton object.
        /// </summary>
        private void removeTablesButton_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_DOCCLEANUP_PLUGIN
            ExecuteProcessingCommand(new LineRemovalCommand(LinesType.Tables));
#endif
        }

        /// <summary>
        /// Handles the Click event of autoTextInvertButton object.
        /// </summary>
        private void autoTextInvertButton_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_DOCCLEANUP_PLUGIN
            ExecuteProcessingCommand(new AutoTextInvertCommand());
#endif
        }

        /// <summary>
        /// Handles the Click event of autoOrientationButton object.
        /// </summary>
        private void autoOrientationButton_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_DOCCLEANUP_PLUGIN
            ExecuteProcessingCommand(new AutoTextOrientationCommand());
#endif
        }

        #endregion

        /// <summary>
        /// Handles the CheckedChanged event of ocrPreprocessingCheckBox object.
        /// </summary>
        private void ocrPreprocessingCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            ocrPreprocessingGroupBox.IsEnabled = ocrPreprocessingCheckBox.IsChecked.Value;
        }

        /// <summary>
        /// Handles the Loaded event of Window object.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (OpenFileWhenShown)
                AddImagesFromFiles();

            if (ScanImageWhenShown)
                AddImageFromScanner();

            okButton.Focus();

            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Handles the ThumbnailAdded event of thumbnailViewer1 object.
        /// </summary>
        private void thumbnailViewer1_ThumbnailAdded(object sender, Vintasoft.Imaging.Wpf.UI.ThumbnailImageItemEventArgs e)
        {
            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Handles the ThumbnailRemoved event of thumbnailViewer1 object.
        /// </summary>
        private void thumbnailViewer1_ThumbnailRemoved(object sender, Vintasoft.Imaging.Wpf.UI.ThumbnailImageItemEventArgs e)
        {
            // update the UI
            UpdateUI();
        }

        /// <summary>
        /// Handles the Click event of okButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (ocrPreprocessingCheckBox.IsChecked == true)
                ExecuteOcrPreprocessing();

            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of buttonCancel object.
        /// </summary>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            thumbnailViewer1.Images.ClearAndDisposeItems();
            DialogResult = false;
        }


        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            // get the current status of application
            bool isImagesOpening = IsImageOpening;
            bool isImagesEmpty = Images.Count == 0;
            bool isImagesProcessing = IsImageProcessing;

            // Add from File...
            addFromFileButton.IsEnabled = !isImagesOpening && !isImagesProcessing;

            // Scan...
            addFromScannerButton.IsEnabled = !isImagesOpening && !isImagesProcessing;

            // Processing
            processingGroupBox.IsEnabled = !isImagesOpening && !isImagesProcessing && !isImagesEmpty;
            // "All Images" radio button
            allImagesRadioButton.IsEnabled = Images.Count > 1;
            // "Current Image" radio button
            if (Images.Count == 1)
                currentImageRadioButton.IsChecked = true;

            // OCR Preprocessing
            ocrPreprocessingGroupBox.IsEnabled = !isImagesOpening && !isImagesProcessing && !isImagesEmpty;
            ocrPreprocessingCheckBox.IsEnabled = ocrPreprocessingGroupBox.IsEnabled;

            // "OK" button
            okButton.IsEnabled = !isImagesOpening && !isImagesProcessing && !isImagesEmpty;

            // "Cancel" button
            buttonCancel.IsEnabled = !isImagesOpening && !isImagesProcessing;

            thumbnailViewer1.IsEnabled = !isImagesOpening && !isImagesProcessing;
            imageViewer1.IsEnabled = !isImagesOpening && !isImagesProcessing;
        }

        /// <summary>
        /// Adds image(s) from file.
        /// </summary>
        private void AddImagesFromFiles()
        {
            IsImageOpening = true;

            RenderingSettingsWindow renderingSettings = null;

            if (_openImageFileDialog.ShowDialog() == true)
            {
                for (int i = 0; i < _openImageFileDialog.FileNames.Length; i++)
                {
                    try
                    {
                        using (ImageCollection images = new ImageCollection())
                        {
                            DocumentPasswordWindow.EnableAuthentication(images);
                            try
                            {
                                // load images
                                images.Add(_openImageFileDialog.FileNames[i]);
                                // check images format
                                for (int j = 0; j < images.Count; j++)
                                {
                                    switch (images[j].PixelFormat)
                                    {
                                        case Vintasoft.Imaging.PixelFormat.Bgr555:
                                        case Vintasoft.Imaging.PixelFormat.Bgr565:
                                            ChangePixelFormatToBgrCommand to24 = new ChangePixelFormatToBgrCommand(Vintasoft.Imaging.PixelFormat.Bgr24);
                                            to24.ExecuteInPlace(images[j]);
                                            break;
                                    }
                                }

                                // if image is vector
                                if (images[0].SourceInfo.Decoder.IsVectorDecoder)
                                {
                                    // show rendering settings form
                                    if (renderingSettings == null)
                                    {
                                        renderingSettings = new RenderingSettingsWindow(images[0].RenderingSettings);
                                        renderingSettings.ShowDialog();
                                    }

                                    // set rendering settings
                                    for (int j = 0; j < images.Count; j++)
                                        images[j].RenderingSettings = (RenderingSettings)renderingSettings.RenderingSettings.Clone();
                                }

                                // add images
                                thumbnailViewer1.Images.AddRange(images.ToArray());
                            }
                            finally
                            {
                                DocumentPasswordWindow.DisableAuthentication(images);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DemosTools.ShowErrorMessage(ex);
                    }
                }
            }

            IsImageOpening = false;
        }

        /// <summary>
        /// Adds an image from scanner.
        /// </summary>
        private void AddImageFromScanner()
        {
            IsImageOpening = true;

            try
            {
                _simpleTwainManager.SelectDeviceAndAcquireImage();
            }
            catch (Exception e)
            {
                DemosTools.ShowErrorMessage(e);
            }

            IsImageOpening = false;
        }

        /// <summary>
        /// Handler of the OcrPreprocessingCommand.Progress event.
        /// </summary>
        private void preprocessingCommand_Progress(object sender, ImageProcessingProgressEventArgs e)
        {
            currentImageProgressBar.Value = e.Progress;
        }

        /// <summary>
        /// Executes the OCR preprocessing.
        /// </summary>
        private void ExecuteOcrPreprocessing()
        {
#if !REMOVE_DOCCLEANUP_PLUGIN
            IsImageProcessing = true;
            OcrPreprocessingCommand preprocessingCommand = new OcrPreprocessingCommand();

            // leave only checked commands
            preprocessingCommand.Binarization = null;
            if (autoInvertCheckBox.IsChecked == false)
                preprocessingCommand.AutomaticalInvert = null;
            if (halftoneRemovalCheckBox.IsChecked == false)
                preprocessingCommand.HalftoneRemoval = null;
            if (clearBorderCheckBox.IsChecked == false)
                preprocessingCommand.BorderClear = null;
            if (despeckleCheckBox.IsChecked == false)
                preprocessingCommand.Despeckle = null;
            if (deskewChekBox.IsChecked == false)
                preprocessingCommand.Deskew = null;
            if (holePunchRemovalCheckBox.IsChecked == false)
                preprocessingCommand.HolePunchRemoval = null;
            if (autoOrientationCheckBox.IsChecked == false)
                preprocessingCommand.AutomaticalOrientation = null;
            else
                preprocessingCommand.AutomaticalOrientation = new AutoTextOrientationCommand();

            if (segmentationCheckBox.IsChecked == false)
            {
                preprocessingCommand.Segmentation = null;
            }
            else
            {
                _segmentationResults = new Dictionary<VintasoftImage, ReadOnlyCollection<ImageRegion>>();
                preprocessingCommand.Segmentation.BorderSize = 2;
            }

            if (preprocessingCommand.SupportedPixelFormats[0] == Vintasoft.Imaging.PixelFormat.Undefined)
                return;

            preprocessingCommand.Progress += new EventHandler<ImageProcessingProgressEventArgs>(preprocessingCommand_Progress);

            allImagesProgressBar.Value = 0;
            allImagesProgressBar.Maximum = Images.Count;

            // for each image
            for (int i = 0; i < Images.Count; i++)
            {
                allImagesProgressBar.Value = i + 1;
                try
                {
                    // execute processing command
                    preprocessingCommand.ExecuteInPlace(Images[i]);
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }

                if (preprocessingCommand.Segmentation != null)
                {
                    _segmentationResults.Add(Images[i], preprocessingCommand.SegmentationTextRegions);
                }
                DemosTools.DoEvents();
            }
            IsImageProcessing = false;
#endif
        }

        /// <summary>
        /// Executes the processing command.
        /// </summary>
        /// <param name="command">The processing command.</param>
        private void ExecuteProcessingCommand(ProcessingCommandBase command)
        {
            IsImageProcessing = true;

            try
            {
                if (currentImageRadioButton.IsChecked == true)
                {
                    if (imageViewer1.Image != null)
                    {
                        // execute command on current image
                        command.ExecuteInPlace(imageViewer1.Image);
                    }
                }
                else
                {
                    for (int i = 0; i < Images.Count; i++)
                    {
                        // execute command on all images
                        command.ExecuteInPlace(Images[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
            finally
            {
                IsImageProcessing = false;
            }
        }

        #endregion

    }
}
