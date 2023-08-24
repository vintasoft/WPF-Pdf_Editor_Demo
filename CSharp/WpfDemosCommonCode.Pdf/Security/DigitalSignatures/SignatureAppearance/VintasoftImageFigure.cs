using System;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Drawing;
using Vintasoft.Imaging.Pdf.Drawing.GraphicsFigures;
using Vintasoft.Imaging.Pdf.Tree;
using System.Windows.Media;
using System.Windows;


namespace WpfDemosCommonCode.Pdf.Security
{
    /// <summary>
    /// Represents a VintasoftImage graphics figure.
    /// </summary>
    /// <remarks>
    /// If image is associated with PDF page:
    /// <ul>
    /// <li>figure gets PDF page</li>
    /// <li>figure creates PDF form resource from PDF page</li>
    /// <li>figure draws PDF form resource on PDF graphics</li>
    /// </ul>
    /// <br />
    /// If image is NOT associated with PDF page:
    /// <ul>
    /// <li>figure creates PDF image resource from image</li>
    /// <li>figure draws PDF image resource on PDF graphics</li>
    /// </ul>
    /// </remarks>
    public class VintasoftImageFigure : RectangleFigure
    {

        #region Fields

        /// <summary>
        /// The resource that draws.
        /// </summary>
        PdfResource _resource;

        #endregion



        #region Properties

        Color _backgroundColor = Colors.Transparent;
        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public Color BackgroundColor
        {
            get
            {
                return _backgroundColor;
            }
            set
            {
                _backgroundColor = value;
                OnChanged();
            }
        }

        VintasoftImage _image = null;
        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        public VintasoftImage Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                _resource = null;
                OnChanged();
            }
        }
        
        bool _inlineImage = false;
        /// <summary>
        /// Gets or sets a value indicating whether an image must be inlined in PDF content.
        /// </summary>
        public bool InlineImage
        {
            get
            {
                return _inlineImage;
            }
            set
            {
                _inlineImage = value;
                OnChanged();
            }
        }

        PdfCompression _imageCompression = PdfCompression.Auto;
        /// <summary>
        /// Gets or sets an image compression.
        /// </summary>
        public PdfCompression ImageCompression
        {
            get
            {
                return _imageCompression;
            }
            set
            {
                _imageCompression = value;
                OnChanged();
            }
        }

        PdfCompressionSettings _imageCompressionSettings = new PdfCompressionSettings();
        /// <summary>
        /// Gets or sets the image compression settings.
        /// </summary>
        public PdfCompressionSettings ImageCompressionSettings
        {
            get
            {
                return _imageCompressionSettings;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                _imageCompressionSettings = value;
                OnChanged();
            }
        }

        bool _maintainAspectRatio = true;
        /// <summary>
        /// Gets or sets a value indicating whether graphics figure must maintain the aspect ratio of image.
        /// </summary>
        public bool MaintainAspectRatio
        {
            get
            {
                return _maintainAspectRatio;
            }
            set
            {
                if (_maintainAspectRatio != value)
                {
                    _maintainAspectRatio = value;
                    OnChanged();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this figure is visible.
        /// </summary>
        /// <value>
        /// <b>true</b> if this figure is visible; otherwise, <b>false</b>.
        /// </value>
        public override bool IsVisible
        {
            get
            {
                return Image != null || BackgroundColor != Colors.Transparent;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Draws the graphics figure on specified <see cref="PdfGraphics" />.
        /// </summary>
        /// <param name="graphics">The <see cref="PdfGraphics" />.</param>
        public override void DrawFigure(PdfGraphics graphics)
        {
            System.Drawing.RectangleF rect = GetDrawRectangle();

            if (BackgroundColor != Colors.Transparent)
            {
                System.Drawing.Color color = System.Drawing.Color.FromArgb(
                    _backgroundColor.A,
                    _backgroundColor.R,
                    _backgroundColor.G,
                    _backgroundColor.B);
                graphics.FillRectangle(new PdfBrush(color), rect);
            }

            if (Image != null)
            {
                if (_resource == null || _resource.Document != graphics.Document)
                    _resource = CreateResource(Image, graphics.Document);

                DrawResource(graphics, _resource, rect, true);
            }
        }

        /// <summary>
        /// Copies the state of the current object to the target object.
        /// </summary>
        /// <param name="target">Object to copy the state of the current object to.</param>
        protected override void CopyTo(GraphicsFigure target)
        {
            base.CopyTo(target);
            VintasoftImageFigure typedTarget = (VintasoftImageFigure)target;
            typedTarget._image = _image;
            typedTarget._imageCompression = _imageCompression;
            typedTarget._imageCompressionSettings = _imageCompressionSettings;
            typedTarget._inlineImage = _inlineImage;
            typedTarget._maintainAspectRatio = _maintainAspectRatio;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override object Clone()
        {
            VintasoftImageFigure figure = new VintasoftImageFigure();
            CopyTo(figure);
            return figure;
        }

        #endregion


        #region PRIVATE
                
        /// <summary>
        /// Creates a PDF resource from specified VintasoftImage.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="document">The document.</param>
        /// <returns>PDF resource.</returns>
        private PdfResource CreateResource(VintasoftImage image, PdfDocument document)
        {
            // get PDF page associated with image
            PdfPage page = PdfDocumentController.GetPageAssociatedWithImage(image);
            // if image presents PDF page
            if (page != null)
                // return PDF form resource based on PDF page
                return new PdfFormXObjectResource(document, page);

            // return PDF image resource based on image
            return new PdfImageResource(document, image, PdfCompression.Zip);
        }

        /// <summary>
        /// Draws the resource on specified PDF graphics.
        /// </summary>
        /// <param name="graphics">The PDF graphics.</param>
        /// <param name="resource">The PDF resource.</param>
        /// <param name="rect">The rectangle in which PDF resource must be drawn.</param>
        /// <param name="mantainAspectRatio">Indicates whether the aspect ratio of PDF resource
        /// must be maintained.</param>
        private void DrawResource(
            PdfGraphics graphics,
            PdfResource resource,
            System.Drawing.RectangleF rect,
            bool mantainAspectRatio)
        {
            // if the aspect ratio of PDF resource must be maintained
            if (mantainAspectRatio)
            {
                System.Drawing.SizeF resourceSize;
                if (resource is PdfFormXObjectResource)
                    resourceSize = ((PdfFormXObjectResource)resource).BoundingBox.Size;
                else if (resource is PdfImageResource)
                    resourceSize = new System.Drawing.SizeF(((PdfImageResource)resource).Width, ((PdfImageResource)resource).Height);
                else
                    throw new NotImplementedException();

                double dw = rect.Width / (double)resourceSize.Width;
                double dh = rect.Height / (double)resourceSize.Height;

                double d = Math.Min(dw, dh);

                if (dw != dh)
                {
                    rect.X += (float)((rect.Width - resourceSize.Width * d) / 2);
                    rect.Y += (float)((rect.Height - resourceSize.Height * d) / 2);
                    rect.Width = (float)(resourceSize.Width * d);
                    rect.Height = (float)(resourceSize.Height * d);
                }
            }

            // if PDF resource is PDF form resource
            if (resource is PdfFormXObjectResource)
                // draw PDF form on PDF graphics
                graphics.DrawForm((PdfFormXObjectResource)resource, rect);
            // if PDF resource is PDF image resource
            else if (resource is PdfImageResource)
                // draw image on PDF graphics
                graphics.DrawImage((PdfImageResource)resource, rect);
            else
                throw new NotImplementedException();
        }

        #endregion

        #endregion

    }
}
