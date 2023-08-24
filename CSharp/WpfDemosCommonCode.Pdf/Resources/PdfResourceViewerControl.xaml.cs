using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Metadata;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Drawing.GraphicsFigures;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Wpf.UI;
using Vintasoft.Imaging.UI;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view PDF resource.
    /// </summary>
    public partial class PdfResourceViewerControl : UserControl
    {

        #region Fields

        /// <summary>
        /// The PDF content editor tool.
        /// </summary>
        WpfPdfContentEditorTool _editorTool = null;

        /// <summary>
        /// The character encoding of text data.
        /// </summary>
        Encoding _textDataEncoding = Encoding.ASCII;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfResourceViewerControl"/> class.
        /// </summary>
        public PdfResourceViewerControl()
        {
            InitializeComponent();

            ImageViewerAvailableZoomValues = new int[] { 25, 50, 100, 200, 400 };
#if !REMOVE_PDFVISUALEDITOR_PLUGIN
            _editorTool = new WpfPdfContentEditorTool();
            _editorTool.RenderFiguresWhenImageIndexChanging = false;
            resourceImageViewer.VisualTool = _editorTool;
#endif

            ShowResource(null);
        }

        #endregion



        #region Properties

        PdfResource _resource = null;
        /// <summary>
        /// Gets or sets the PDF resource.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        [Browsable(false)]
        public PdfResource Resource
        {
            get
            {
                return _resource;
            }
            set
            {
                if (_resource != value)
                {
                    _resource = value;

                    ShowResource(_resource);
                }
            }
        }

        bool _showSizeModeComboBox = true;
        /// <summary>
        /// Gets or sets a value indicating whether combo box of size mode is visible.
        /// </summary>
        [DefaultValue(true)]
        public bool ShowSizeModeComboBox
        {
            get
            {
                return _showSizeModeComboBox;
            }
            set
            {
                _showSizeModeComboBox = value;

                if (resourceImageViewerPanel.Visibility == Visibility.Visible)
                {
                    if (value)
                        sizeModeComboBox.Visibility = Visibility.Visible;
                    else
                        sizeModeComboBox.Visibility = Visibility.Collapsed;
                }
                else
                    sizeModeComboBox.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating how an image is positioned within the viewer.
        /// </summary>
        [DefaultValue(ImageSizeMode.Normal)]
        public ImageSizeMode ImageViewerSizeMode
        {
            get
            {
                return (ImageSizeMode)sizeModeComboBox.SelectedItem;
            }
            set
            {
                if (value != ImageSizeMode.Normal &&
                    value != ImageSizeMode.BestFit)
                    throw new ArgumentOutOfRangeException();

                sizeModeComboBox.SelectedItem = value;
            }
        }

        /// <summary>
        /// Gets or sets the color of viewer border.
        /// </summary>
        public Color ImageViewerBorderColor
        {
            get
            {
                SolidColorBrush brush = (SolidColorBrush)resourceImageViewerPanel.BorderBrush;
                return brush.Color;
            }
            set
            {
                resourceImageViewerPanel.BorderBrush = new SolidColorBrush(value);
            }
        }

        /// <summary>
        /// Gets or sets the thickness of border on viewer.
        /// </summary>
        public Thickness ImageViewerBorderThickness
        {
            get
            {
                return resourceImageViewerPanel.BorderThickness;
            }
            set
            {
                resourceImageViewer.BorderThickness = value;
            }
        }

        int[] _imageViewerAvailableZoomValues;
        /// <summary>
        /// Gets or sets an array of zoom values, which can be used in image viewer.
        /// </summary>
        /// <value>
        /// Default value is <b>25, 50, 100, 200, 400</b>.
        /// </value>
        public int[] ImageViewerAvailableZoomValues
        {
            get
            {
                return _imageViewerAvailableZoomValues;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                _imageViewerAvailableZoomValues = value;

                sizeModeComboBox.Items.Clear();
                sizeModeComboBox.Items.Add(ImageSizeMode.Normal);
                sizeModeComboBox.Items.Add(ImageSizeMode.BestFit);
                for (int i = 0; i < _imageViewerAvailableZoomValues.Length; i++)
                    sizeModeComboBox.Items.Add(string.Format("{0}%", _imageViewerAvailableZoomValues[i]));

                sizeModeComboBox.SelectedItem = ImageSizeMode.Normal;
            }
        }

        private bool _showResourceName = true;
        /// <summary>
        /// Gets or sets a value indicating whether the resource name can be shown.
        /// </summary>
        /// <value>
        /// <b>true</b> the resource name can be shown; otherwise, <b>false</b>.
        /// </value>
        [DefaultValue(true)]
        [Description("Indicates whether the resource name can be shown.")]
        public bool ShowResourceName
        {
            get
            {
                return _showResourceName;
            }
            set
            {
                _showResourceName = value;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Reloads the resource in viewer.
        /// </summary>
        public void ReloadResource()
        {
            if (resourceImageViewer.Visibility == Visibility.Visible && 
                resourceImageViewer.Image != null)
                resourceImageViewer.Image.Reload(false);
        }

        #endregion


        #region PROTECTED

        /// <summary>
        /// Invoked when the parent of this element in the visual tree is changed.
        /// </summary>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            if (oldParent != null)
            {
                Window parentWindow = Window.GetWindow(oldParent);
                if (parentWindow != null)
                    parentWindow.Closed -= parentWindow_Closed;
            }

            base.OnVisualParentChanged(oldParent);

            if (this.Parent != null)
            {
                Window parentWindow = Window.GetWindow(this.Parent);
                if (parentWindow != null)
                    parentWindow.Closed += new EventHandler(parentWindow_Closed);
            }
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Parent window is closed.
        /// </summary>
        void parentWindow_Closed(object sender, EventArgs e)
        {
            if (_editorTool != null)
                _editorTool.FigureViewCollection.Clear();
            resourceImageViewer.Images.ClearAndDisposeItems();
        }

        /// <summary>
        /// Shows the resource of PDF document.
        /// </summary>
        /// <param name="resource">The resource of PDF document.</param>
        private void ShowResource(PdfResource resource)
        {
            if (_editorTool != null)
                _editorTool.FigureViewCollection.Clear();
            resourceTextBox.Text = string.Empty;
            string resourceName = string.Empty;

            if (resource != null)
            {
                if (_showResourceName)
                {
                    string typeName = ProcessingDemosTools.GetReadableTypeName(resource.GetType());
                    resourceName = string.Format("{0} (object {1})", typeName, resource.ObjectNumber);
                }

                RectangleFigure resourceFigure = CreateGraphicsFigure(resource);
                if (resourceFigure == null)
                {
                    byte[] resourceData = resource.GetBytes();
                    string resourceDataStr = _textDataEncoding.GetString(resourceData);

                    if (resourceDataStr.Contains("\n"))
                        resourceDataStr = resourceDataStr.Replace("\n", Environment.NewLine);
                    else
                        resourceDataStr = resourceDataStr.Replace("\r", Environment.NewLine);

                    resourceTextBox.Text = resourceDataStr;
                }
                else
                {
                    System.Drawing.SizeF resourceFigureSize = resourceFigure.Size;

                    if (resourceImageViewer.Image == null)
                    {
                        MemoryStream documentStream = new MemoryStream();
                        using (PdfDocument document = new PdfDocument())
                        {
                            document.Pages.Add(resourceFigureSize);
                            document.Save(documentStream);
                        }
                        resourceImageViewer.Images.Add(documentStream, true);
                    }
                    else
                    {
                        VintasoftImage image = resourceImageViewer.Image;
                        PdfPageMetadata metadata = (PdfPageMetadata)image.Metadata.MetadataTree;
                        metadata.MediaBox = new System.Drawing.RectangleF(System.Drawing.PointF.Empty, resourceFigureSize);
                    }

                    if (resourceFigure is FormXObjectFigure)
                    {
                        FormXObjectFigure formXObjectFigure = (FormXObjectFigure)resourceFigure;
                        PdfPage currentPage = PdfDocumentController.GetPageAssociatedWithImage(resourceImageViewer.Image);

                        if (formXObjectFigure.FormXObject.Document != currentPage.Document)
                            formXObjectFigure.FormXObject = formXObjectFigure.FormXObject.CreateCopy(currentPage.Document);
                    }

                    if (_editorTool != null)
                    {
                        WpfGraphicsFigureView resourceFigureView = WpfGraphicsFigureViewFactory.CreateView(resourceFigure);
                        resourceFigureView.Transformer = null;
                        resourceFigureView.Builder = null;
                        _editorTool.FigureViewCollection.Add(resourceFigureView);
                    }
                }
            }

            resourceNameLabel.Content = resourceName;
            resourceNameLabel.ToolTip = resourceName;

            if (resource == null ||
                resource is PdfImageResource ||
                resource is PdfFormXObjectResource)
            {
                if (ShowSizeModeComboBox)
                    sizeModeComboBox.Visibility = Visibility.Visible;
                else
                    sizeModeComboBox.Visibility = Visibility.Collapsed;

                resourceImageViewerPanel.Visibility = Visibility.Visible;
                resourceTextBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                sizeModeComboBox.Visibility = Visibility.Collapsed;
                resourceImageViewerPanel.Visibility = Visibility.Collapsed;
                resourceTextBox.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Creates the graphics figure.
        /// </summary>
        /// <param name="resource">The resource.</param>
        private RectangleFigure CreateGraphicsFigure(PdfResource resource)
        {
            RectangleFigure figure = null;

            if (resource is PdfImageResource)
            {
                PdfImageResource imageResource = (PdfImageResource)resource;

                ImageFigure imageFigure = new ImageFigure();
                imageFigure.ImageResource = imageResource;
                imageFigure.Size = new System.Drawing.SizeF(imageResource.Width, imageResource.Height);

                figure = imageFigure;
            }
            else if (resource is PdfFormXObjectResource)
            {
                PdfFormXObjectResource formResource = (PdfFormXObjectResource)resource;

                FormXObjectFigure formFigure = new FormXObjectFigure();
                formFigure.FormXObject = formResource;

                float width = Math.Max(1, formFigure.Size.Width);
                float height = Math.Max(1, formFigure.Size.Height);

                if (formFigure.Size.Width != width ||
                    formFigure.Size.Height != height)
                    formFigure.Size = new System.Drawing.SizeF(width, height);

                figure = formFigure;
            }

            return figure;
        }

        /// <summary>
        /// Size mode is changed.
        /// </summary>
        private void sizeModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSizeMode();
        }

        /// <summary>
        /// Updates the size mode.
        /// </summary>
        private void UpdateSizeMode()
        {
            if (sizeModeComboBox.SelectedItem != null)
            {
                string text = sizeModeComboBox.SelectedItem.ToString();
                if (text.EndsWith("%", StringComparison.InvariantCulture))
                {
                    string currentZoom = text.TrimEnd('%');
                    int zoom;
                    if (int.TryParse(currentZoom, out zoom))
                    {
                        resourceImageViewer.SizeMode = ImageSizeMode.Zoom;
                        resourceImageViewer.Zoom = zoom;
                    }
                }
                else
                {
                    ImageSizeMode sizeMode = (ImageSizeMode)sizeModeComboBox.SelectedItem;
                    resourceImageViewer.SizeMode = sizeMode;
                }
            }
        }

        #endregion

        #endregion

    }
}
