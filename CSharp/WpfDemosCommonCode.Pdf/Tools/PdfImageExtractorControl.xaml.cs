using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Win32;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Content.ImageExtraction;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Wpf.UI;
using Vintasoft.Imaging.Wpf.UI;

using WpfDemosCommonCode.Imaging.Codecs;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to extract image-resources from PDF page.
    /// </summary>
    public partial class PdfImageExtractorControl : UserControl
    {

        #region Fields

        /// <summary>
        /// Determines that image is selected in image viewer.
        /// </summary>
        bool _isImageSelected = false;

        /// <summary>
        /// Determines that visual tool is activated.
        /// </summary>
        bool _isVisualToolActivated = false;

        /// <summary>
        /// The context menu of content images.
        /// </summary>
        ContextMenu _imageExtractorContextMenu;

        bool _isImageMouseSelecting = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfImageExtractorControl"/> class.
        /// </summary>
        public PdfImageExtractorControl()
        {
            InitializeComponent();

            _imageExtractorContextMenu = (ContextMenu)Resources["ImageExtractorContextMenu"];
            DemosTools.SetTestFilesFolder(ImageSaveFileDialog);
            CodecsFileFilters.SetFilters(ImageSaveFileDialog, false);
        }

        #endregion



        #region Properties

        WpfPdfImageExtractorTool _imageExtractorTool = null;
        /// <summary>
        /// Gets or sets the PDF image extractor tool.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public WpfPdfImageExtractorTool ImageExtractorTool
        {
            get
            {
                return _imageExtractorTool;
            }
            set
            {
                if (_imageExtractorTool == value)
                    return;

                if (_imageExtractorTool != null)
                    UnsubscribeFromVisualToolEvents(_imageExtractorTool);

                _imageExtractorTool = value;
                _isVisualToolActivated = false;

                if (_imageExtractorTool != null)
                    SubscribeToVisualToolEvents(_imageExtractorTool);

                UpdateImageResourcesListBox();
                UpdateUI();
            }
        }

        SaveFileDialog _imageSaveFileDialog = new SaveFileDialog();
        /// <summary>
        /// Gets the save file dialog.
        /// </summary>
        private SaveFileDialog ImageSaveFileDialog
        {
            get
            {
                return _imageSaveFileDialog;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the user interface of this control.
        /// </summary>
        private void UpdateUI()
        {
            bool containsTool = _imageExtractorTool != null;

            mainPanel.IsEnabled = containsTool &&
                _imageExtractorTool.ImageViewer != null &&
                _imageExtractorTool.ImageViewer.Image != null &&
                PdfDocumentController.GetPageAssociatedWithImage(_imageExtractorTool.ImageViewer.Image) != null;

            if (mainPanel.IsEnabled)
            {
                bool resourceIsSelected = false;
                if (containsTool)
                    resourceIsSelected = _imageExtractorTool.SelectedImage != null;
                bool containsImages = containsTool && _imageExtractorTool.ContentImages != null;
                if (containsImages)
                    containsImages = _imageExtractorTool.ContentImages.Length > 0;

                saveGroupBox.IsEnabled = resourceIsSelected;
                viewContentImageButton.IsEnabled = resourceIsSelected;
            }
        }


        /// <summary>
        /// Returns the description of content image.
        /// </summary>
        /// <param name="contentImage">The content image.</param>
        /// <returns>The description of content image.</returns>
        private string GetContentImageDescription(ContentImage contentImage)
        {
            string result = string.Empty;

            PdfImageResource resource = contentImage.ImageResource;
            // if resource is inline
            if (resource.IsInline)
                result = "Inline, ";
            else
                result = string.Format("Resource {0,3}, ", resource.ObjectNumber.ToString());

            // size of resource
            result += string.Format("{0}x{1} px, ", resource.Width, resource.Height);
            // compression of resource
            result += string.Format("compression={0}, ", resource.Compression);
            // compressed size of resource
            result += string.Format("{0} bytes", resource.Length);
            return result;
        }

        /// <summary>
        /// Returns the zoom of transformed image.
        /// </summary>
        /// <param name="imageViewer">The image viewer.</param>
        /// <returns>The zoom of transformed image.</returns>
        private float GetTransformedImageZoom(WpfImageViewer imageViewer)
        {
            return Convert.ToSingle((96.0 / 72.0) * (imageViewer.Zoom / 100.0));
        }


        #region ImageResourcesListBox

        /// <summary>
        /// Image resource is changed in list box.
        /// </summary>
        private void imageResourcesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isImageMouseSelecting)
                return;

            ContentImage contentImage = null;

            if (imageResourcesListBox.SelectedIndex != -1)
                contentImage = _imageExtractorTool.ContentImages[imageResourcesListBox.SelectedIndex];

            _imageExtractorTool.SelectedImage = contentImage;
            _isImageSelected = contentImage != null;

            UpdateUI();
        }

        /// <summary>
        /// Image resource is clicked.
        /// </summary>
        private void imageResourcesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (imageResourcesListBox.SelectedIndex != -1)
                viewContentImageButton_Click(sender, e);
        }

        /// <summary>
        /// Updates the image resources in list box.
        /// </summary>
        private void UpdateImageResourcesListBox()
        {
            imageResourcesListBox.BeginInit();
            try
            {
                // clear items
                imageResourcesListBox.Items.Clear();
                // if visual tool is activated
                if (_imageExtractorTool != null && _isVisualToolActivated)
                {
                    // get content images of current PDF page
                    ContentImage[] contentImages = _imageExtractorTool.ContentImages;
                    // if content images exist
                    if (contentImages != null)
                    {
                        // add all content images
                        foreach (ContentImage contentImage in contentImages)
                        {
                            ListBoxItem item = new ListBoxItem();
                            item.Content = GetContentImageDescription(contentImage);
                            item.MouseDoubleClick += new MouseButtonEventHandler(imageResourcesListBox_MouseDoubleClick);
                            imageResourcesListBox.Items.Add(item);
                        }
                    }
                }
            }
            finally
            {
                imageResourcesListBox.EndInit();
            }
        }

        /// <summary>
        /// Updates the description of focused image.
        /// </summary>
        private void UpdateFocusedImageDescription()
        {
            int selectedIndex = imageResourcesListBox.SelectedIndex;

            if (selectedIndex != -1)
            {
                ListBoxItem item = (ListBoxItem)imageResourcesListBox.Items[selectedIndex];
                item.Content = GetContentImageDescription(_imageExtractorTool.SelectedImage);
            }
        }

        #endregion


        #region Buttons

        /// <summary>
        /// "View content image" button is clicked.
        /// </summary>
        private void viewContentImageButton_Click(object sender, RoutedEventArgs e)
        {
            // get the image viewer
            WpfImageViewer imageViewer = _imageExtractorTool.ImageViewer;
            // get the transformed zoom of content image
            float zoom = GetTransformedImageZoom(imageViewer);

            // get selected image
            ContentImage contentImage = _imageExtractorTool.SelectedImage;

            PdfCompression previousCompression = contentImage.ImageResource.Compression;
            // create a dialog with information about content image
            ViewContentImageWindow viewContentImageWindow = new ViewContentImageWindow(contentImage, zoom);
            // show the dialog
            viewContentImageWindow.ShowDialog();


            if (previousCompression != contentImage.ImageResource.Compression)
            {
                UpdateFocusedImageDescription();

                imageViewer.ReloadImage();
            }
        }

        /// <summary>
        /// "Save image resource" button is clicked.
        /// </summary>
        private void saveImageResourceButton_Click(object sender, RoutedEventArgs e)
        {
            // show dialog for file selection
            if (ImageSaveFileDialog.ShowDialog().Value)
            {
                // get image resource of selected image
                PdfImageResource imageResource = _imageExtractorTool.SelectedImage.ImageResource;

                // get image of image resource
                using (VintasoftImage image = imageResource.GetImage())
                {
                    // save image to a file
                    image.Save(ImageSaveFileDialog.FileName);
                }
            }
        }

        /// <summary>
        /// "Save transformed image" button is clicked.
        /// </summary>
        private void saveTransformedImageButton_Click(object sender, RoutedEventArgs e)
        {
            // show dialog for file selection
            if (ImageSaveFileDialog.ShowDialog().Value)
            {
                // get image of selected image
                ContentImage contentImage = _imageExtractorTool.SelectedImage;
                // get zoom of transformed image
                float zoom = GetTransformedImageZoom(_imageExtractorTool.ImageViewer);
                // get image
                using (VintasoftImage image = contentImage.RenderImage(zoom))
                {
                    // save image to a file
                    image.Save(ImageSaveFileDialog.FileName);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the "Copy Image to Clipboard" menu item.
        /// </summary>
        private void copyImageToClipboardToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _imageExtractorTool.CopyAction.Execute();
        }

        #endregion


        #region Visual tool

        /// <summary>
        /// Subscribes to the events of visual tool.
        /// </summary>
        /// <param name="visualTool">The visual tool.</param>
        private void SubscribeToVisualToolEvents(WpfPdfImageExtractorTool visualTool)
        {
            visualTool.Activated += new EventHandler(VisualTool_Activated);
            visualTool.Deactivated += new EventHandler(VisualTool_Deactivated);
            visualTool.ImageMouseEnter += new EventHandler<WpfPdfImageExtractorEventArgs>(VisualTool_ImageMouseEnter);
            visualTool.ImageMouseLeave += new EventHandler<WpfPdfImageExtractorEventArgs>(VisualTool_ImageMouseLeave);
            visualTool.MouseDown += new MouseButtonEventHandler(VisualTool_MouseDown);
            visualTool.MouseDoubleClick += new MouseButtonEventHandler(VisualTool_MouseDoubleClick);
            visualTool.SelectedImageChanged += new EventHandler(VisualTool_SelectedImageChanged);

            if (visualTool.ImageViewer != null)
            {
                SubscribeToImageViewerEvents(visualTool.ImageViewer);
                _isVisualToolActivated = true;
            }
        }

        /// <summary>
        /// Unsubscribes from the events of visual tool.
        /// </summary>
        /// <param name="visualTool">The visual tool.</param>
        private void UnsubscribeFromVisualToolEvents(WpfPdfImageExtractorTool visualTool)
        {
            visualTool.Activated -= VisualTool_Activated;
            visualTool.Deactivated -= VisualTool_Deactivated;
            visualTool.ImageMouseEnter -= VisualTool_ImageMouseEnter;
            visualTool.ImageMouseLeave -= VisualTool_ImageMouseLeave;
            visualTool.MouseDown -= VisualTool_MouseDown;
            visualTool.MouseDoubleClick -= VisualTool_MouseDoubleClick;
            visualTool.SelectedImageChanged -= VisualTool_SelectedImageChanged;

            if (visualTool.ImageViewer != null)
            {
                UnsubscribeFromImageViewerEvents(visualTool.ImageViewer);
                _isVisualToolActivated = false;
            }
        }

        /// <summary>
        /// Visual tool is activated.
        /// </summary>
        private void VisualTool_Activated(object sender, EventArgs e)
        {
            WpfPdfVisualTool visualTool = (WpfPdfVisualTool)sender;

            _isImageSelected = false;
            SubscribeToImageViewerEvents(visualTool.ImageViewer);
            _isVisualToolActivated = true;
            UpdateImageResourcesListBox();
            UpdateUI();
        }

        /// <summary>
        /// Visual tool is deactivated.
        /// </summary>
        private void VisualTool_Deactivated(object sender, EventArgs e)
        {
            WpfPdfVisualTool visualTool = (WpfPdfVisualTool)sender;

            UnsubscribeFromImageViewerEvents(visualTool.ImageViewer);
            _isVisualToolActivated = false;
            UpdateImageResourcesListBox();
            mainPanel.IsEnabled = false;
        }

        /// <summary>
        /// Mouse pointer enters an image on PDF page.
        /// </summary>
        private void VisualTool_ImageMouseEnter(object sender, WpfPdfImageExtractorEventArgs e)
        {
            if (!IsEnabled || !mainPanel.IsEnabled || _isImageSelected)
                return;

            _isImageMouseSelecting = true;
            _imageExtractorTool.SelectedImage = e.ContentImage;
            imageResourcesListBox.SelectedIndex = Array.IndexOf(_imageExtractorTool.ContentImages, e.ContentImage);
            _isImageMouseSelecting = false;
        }

        /// <summary>
        /// Mouse pointer leaves an image on PDF page.
        /// </summary>
        private void VisualTool_ImageMouseLeave(object sender, WpfPdfImageExtractorEventArgs e)
        {
            if (!IsEnabled || !mainPanel.IsEnabled || _isImageSelected)
                return;

            _isImageMouseSelecting = true;
            _imageExtractorTool.SelectedImage = null;
            imageResourcesListBox.SelectedIndex = -1;
            _isImageMouseSelecting = false;
        }

        /// <summary>
        /// Mouse is down on an image on PDF page.
        /// </summary>
        private void VisualTool_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled || !mainPanel.IsEnabled)
                return;

            // if left or right mouse button is clicked
            if (e.ChangedButton == MouseButton.Left ||
                e.ChangedButton == MouseButton.Right)
            {
                Point location = e.GetPosition(_imageExtractorTool.ImageViewer);
                ContentImage image = _imageExtractorTool.FindImageInViewerSpace(location);

                _isImageSelected = image != null;

                // set image as selected image
                _imageExtractorTool.SelectedImage = image;
                // update resource index in list box
                imageResourcesListBox.SelectedIndex = Array.IndexOf(_imageExtractorTool.ContentImages, image);
                UpdateUI();
            }

            // if right mouse button is clicked
            if (e.ChangedButton == MouseButton.Right)
                // show context menu
                _imageExtractorContextMenu.IsOpen = true;
        }

        /// <summary>
        /// Mouse is double click on an image on PDF page.
        /// </summary>
        void VisualTool_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled || !mainPanel.IsEnabled)
                return;

            // get selected image
            ContentImage contentImage = _imageExtractorTool.SelectedImage;

            if (contentImage != null &&
                e.ChangedButton == MouseButton.Left)
            {
                // get the image viewer
                WpfImageViewer imageViewer = _imageExtractorTool.ImageViewer;
                // get the transformed zoom of content image
                float zoom = GetTransformedImageZoom(imageViewer);

                PdfCompression previousCompression = contentImage.ImageResource.Compression;
                // create a dialog with information about content image
                ViewContentImageWindow viewContentImageForm = new ViewContentImageWindow(contentImage, zoom);
                // show the dialog
                viewContentImageForm.ShowDialog();

                if (previousCompression != contentImage.ImageResource.Compression)
                {
                    UpdateFocusedImageDescription();

                    imageViewer.ReloadImage();
                }
            }
        }
        
        /// <summary>
        /// Handles the SelectedImageChanged event of VisualTool object.
        /// </summary>
        void VisualTool_SelectedImageChanged(object sender, EventArgs e)
        {
            if (!IsEnabled || !mainPanel.IsEnabled)
                return;

            imageResourcesListBox.SelectedIndex = Array.IndexOf(_imageExtractorTool.ContentImages, _imageExtractorTool.SelectedImage);
        }

        #endregion


        #region Image viewer

        /// <summary>
        /// Subscribes to the events of image viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer.</param>
        private void SubscribeToImageViewerEvents(WpfImageViewer imageViewer)
        {
            imageViewer.FocusedIndexChanged += new PropertyChangedEventHandler<int>(ImageViewer_FocusedIndexChanged);
        }

        /// <summary>
        /// Unsubscribes from the events of image viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer.</param>
        private void UnsubscribeFromImageViewerEvents(WpfImageViewer imageViewer)
        {
            imageViewer.FocusedIndexChanged -= ImageViewer_FocusedIndexChanged;
        }

        /// <summary>
        /// Focused image in image viewer is changed.
        /// </summary>
        private void ImageViewer_FocusedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            _isImageMouseSelecting = false;
            _isImageSelected = false;
            _imageExtractorTool.SelectedImage = null;

            UpdateImageResourcesListBox();
            UpdateUI();
        }

        #endregion

        #endregion

    }
}
