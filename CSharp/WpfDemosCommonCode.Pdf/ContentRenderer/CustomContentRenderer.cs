using System.Drawing;
using System.Drawing.Drawing2D;

using Vintasoft.Imaging;
using Vintasoft.Imaging.ImageProcessing;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Content;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Patterns;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Drawing;
using Vintasoft.Imaging.Text;

namespace WpfDemosCommonCode.Pdf
{
        /// <summary>
    /// Class that provides access to the rendering algorithm of PDF content.
    /// </summary>
    public class CustomContentRenderer : PdfContentRenderer
    {

        #region Fields

        /// <summary>
        /// Determines that string is drawing.
        /// </summary>
        bool _stringDrawing = false;

        #endregion



        #region Properties

        bool _drawAnnotations = true;
        /// <summary>
        /// Gets or sets a value indicating whether the renderer must draw 
        /// annotations on PDF page.
        /// </summary>        
        public bool DrawAnnotations
        {
            get
            {
                return _drawAnnotations;
            }
            set
            {
                _drawAnnotations = value;
            }
        }

        bool _drawForms = true;
        /// <summary>
        /// Gets or sets a value indicating whether the renderer must draw 
        /// form XObjects on PDF page.
        /// </summary>
        public bool DrawForms
        {
            get
            {
                return _drawForms;
            }
            set
            {
                _drawForms = value;
            }
        }

        bool _drawImages = true;
        /// <summary>
        /// Gets or sets a value indicating whether the renderer must draw 
        /// images on PDF page.
        /// </summary>
        public bool DrawImages
        {
            get
            {
                return _drawImages;
            }
            set
            {
                _drawImages = value;
            }
        }

        bool _drawInlineImages = true;
        /// <summary>
        /// Gets or sets a value indicating whether the renderer must draw 
        /// inline images on PDF page.
        /// </summary>
        public bool DrawInlineImages
        {
            get
            {
                return _drawInlineImages;
            }
            set
            {
                _drawInlineImages = value;
            }
        }

        bool _drawPaths = true;
        /// <summary>
        /// Gets or sets a value indicating whether the renderer must draw 
        /// graphics paths on PDF page.
        /// </summary>
        public bool DrawPaths
        {
            get
            {
                return _drawPaths;
            }
            set
            {
                _drawPaths = value;
            }
        }

        bool _fillPaths = true;
        /// <summary>
        /// Gets or sets a value indicating whether the renderer must fill 
        /// graphics paths on PDF page.
        /// </summary>
        public bool FillPaths
        {
            get
            {
                return _fillPaths;
            }
            set
            {
                _fillPaths = value;
            }
        }

        bool _fillPathsUseTilingPatterns = true;
        /// <summary>
        /// Gets or sets a value indicating whether the renderer must fill
        /// graphics paths use tiling patterns on PDF page.
        /// </summary>
        public bool FillPathsUseTilingPatterns
        {
            get
            {
                return _fillPathsUseTilingPatterns;
            }
            set
            {
                _fillPathsUseTilingPatterns = value;
            }
        }

        bool _fillPathsUseShadingPatterns = true;
        /// <summary>
        /// Gets or sets a value indicating whether the renderer must fill
        /// graphics paths use shading patterns on PDF page.
        /// </summary>
        public bool FillPathsUseShadingPatterns
        {
            get
            {
                return _fillPathsUseShadingPatterns;
            }
            set
            {
                _fillPathsUseShadingPatterns = value;
            }
        }


        bool _fillAreaUseShadingPatterns = true;
        /// <summary>
        /// Gets or sets a value indicating whether the renderer must fill
        /// all visible area use tiling patterns on PDF page.
        /// </summary>
        public bool FillAreaUseShadingPatterns
        {
            get
            {
                return _fillAreaUseShadingPatterns;
            }
            set
            {
                _fillAreaUseShadingPatterns = value;
            }
        }

        bool _drawText = true;
        /// <summary>
        /// Gets or sets a value indicating whether the renderer must draw 
        /// text on PDF page.
        /// </summary>
        public bool DrawText
        {
            get
            {
                return _drawText;
            }
            set
            {
                _drawText = value;
            }
        }

        bool _drawInvisibleText = false;
        /// <summary>
        /// Gets or sets a value indicating whether the renderer must draw 
        /// invisible text on PDF page.
        /// </summary>
        public bool DrawInvisibleText
        {
            get
            {
                return _drawInvisibleText;
            }
            set
            {
                _drawInvisibleText = value;
            }
        }

        bool _setClipPaths = true;
        /// <summary>
        /// Gets or sets a value indicating whether the renderer must set 
        /// clip paths.
        /// </summary>
        public bool SetClipPaths
        {
            get
            {
                return _setClipPaths;
            }
            set
            {
                _setClipPaths = value;
            }
        }

        float _linesWeigth = 1;
        /// <summary>
        /// Gets or sets the lines weigth, in percents.
        /// </summary>
        public float LinesWeigth
        {
            get
            {
                return _linesWeigth;
            }
            set
            {
                _linesWeigth = value;
            }
        }

        ProcessingCommandBase _imageProcessing = null;
        /// <summary>
        /// Gets or sets the image processing command that apply 
        /// before an image draws on PDF page.
        /// </summary>
        public ProcessingCommandBase ImageProcessing
        {
            get
            {
                return _imageProcessing;
            }
            set
            {
                _imageProcessing = value;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Draws a text string.
        /// </summary>
        /// <param name="context">The rendering context.</param>
        /// <param name="charCodes">The codes, in font encoding, of text characters.</param>
        public override void DrawString(PdfContentRenderingContext context, ulong[] charCodes)
        {
            // if text must be drawn AND text is visible
            if (_drawText && context.GraphicsState.TextRenderingMode != TextRenderingMode.Invisible)
            {
                _stringDrawing = true;
                base.DrawString(context, charCodes);
                _stringDrawing = false;
            }
            // if invisible text must be drawn AND text is invisible
            else if (_drawInvisibleText && context.GraphicsState.TextRenderingMode == TextRenderingMode.Invisible)
            {
                context.GraphicsState.TextRenderingMode = TextRenderingMode.Fill;
                base.DrawString(context, charCodes);
                context.GraphicsState.TextRenderingMode = TextRenderingMode.Invisible;
            }
        }

        /// <summary>
        /// Draws path using specified pen.
        /// </summary>
        /// <param name="context">The rendering context.</param>
        /// <param name="path">The path to draw.</param>
        /// <param name="pen">The pen to use for drawing path.</param>
        public override void DrawPath(
            PdfContentRenderingContext context,
            IGraphicsPath path,
            IDrawingPen pen)
        {
            // if path must be drawn
            if ((_drawPaths && !_stringDrawing) || (_drawText && _stringDrawing))
            {
                // if path is not a path of text symbol
                if (_linesWeigth != 1 && !_stringDrawing)
                {
                    float oldWidth = pen.Width;
                    pen.Width *= _linesWeigth;
                    base.DrawPath(context, path, pen);
                    pen.Width = oldWidth;
                }
                else
                {
                    base.DrawPath(context, path, pen);
                }
            }
        }

        /// <summary>
        /// Fills path using specified brush.
        /// </summary>
        /// <param name="context">The rendering context.</param>
        /// <param name="path">The path to fill.</param>
        /// <param name="brush">The brush to use for filling path.</param>
        public override void FillPath(
            PdfContentRenderingContext context,
            IGraphicsPath path,
            IDrawingBrush brush)
        {
            // if path must be filled
            if ((_fillPaths && !_stringDrawing) || (_drawText && _stringDrawing))
                base.FillPath(context, path, brush);
        }

        /// <summary>
        /// Fills path using specified pattern.
        /// </summary>
        /// <param name="context">The rendering context.</param>
        /// <param name="path">The path to fill.</param>
        /// <param name="pattern">The pattern to use for filling path.</param>
        public override void FillPath(
            PdfContentRenderingContext context,
            IGraphicsPath path,
            PdfGraphicalPattern pattern)
        {
            if (!_fillPathsUseShadingPatterns && pattern is ShadingPattern)
                return;
            if (!_fillPathsUseTilingPatterns && pattern is TilingPattern)
                return;
            base.FillPath(context, path, pattern);
        }

        /// <summary>
        /// Fills all visible areas using specified shading pattern.
        /// </summary>
        /// <param name="context">The rendering context.</param>
        /// <param name="shadingPattern">The shading pattern.</param>
        public override void FillArea(
            PdfContentRenderingContext context,
            Vintasoft.Imaging.Pdf.Tree.ShadingPatterns.PdfShadingPattern shadingPattern)
        {
            if (_fillAreaUseShadingPatterns)
                base.FillArea(context, shadingPattern);
        }

        /// <summary>
        /// Draws a form XObject.
        /// </summary>
        /// <param name="context">The rendering context.</param>
        /// <param name="formResource">The form XObject to draw.</param>
        public override void DrawFormXObject(
            PdfContentRenderingContext context,
            PdfFormXObjectResource formResource)
        {
            if (_drawForms)
                base.DrawFormXObject(context, formResource);
        }

        /// <summary>
        /// Draws an annotation.
        /// </summary>
        /// <param name="context">The rendering context.</param>
        /// <param name="annotation">The annotation to draw.</param>
        /// <param name="appearanceForm">The annotation appearance form.</param>
        public override void DrawAnnotation(
            PdfContentRenderingContext context,
            PdfAnnotation annotation,
            PdfFormXObjectResource appearanceForm)
        {
            if (_drawAnnotations)
                base.DrawAnnotation(context, annotation, appearanceForm);
        }

        /// <summary>
        /// Intersects current clip region with specified clip path.
        /// </summary>
        /// <param name="context">The rendering context.</param>
        /// <param name="clipPath">The clip path.</param>
        public override void IntersectClip(
            PdfContentRenderingContext context,
            IGraphicsPath clipPath)
        {
            if (_setClipPaths)
                base.IntersectClip(context, clipPath);
        }

        /// <summary>
        /// Draws an image resource.
        /// </summary>
        /// <param name="context">The rendering context.</param>
        /// <param name="imageResource">The image XObject resource to draw.</param>
        public override void DrawImageResource(
            PdfContentRenderingContext context,
            PdfImageResource imageResource)
        {
            if (!_drawImages && !imageResource.IsInline)
                return;
            if (!_drawInlineImages && imageResource.IsInline)
                return;
            base.DrawImageResource(context, imageResource);
        }

        /// <summary>
        /// Draws an image.
        /// </summary>
        /// <param name="context">The rendering context.</param>
        /// <param name="image">The image to draw.</param>
        /// <param name="points">Array of three System.Drawing.PointF structures that
        /// define a parallelogram on rendering content where image must be drawn.</param>
        public override void DrawImage(
            PdfContentRenderingContext context,
            Vintasoft.Imaging.VintasoftImage image,
            PointF[] points)
        {
            // if image must be processed
            if (ImageProcessing != null &&
                ImageProcessing.IsPixelFormatSupported(image.PixelFormat))
            {
                lock (image)
                    using (VintasoftImage tempImage = ImageProcessing.Execute(image))
                        base.DrawImage(context, tempImage, points);
            }
            else
            {
                base.DrawImage(context, image, points);
            }
        }

        /// <summary>
        /// Creates a new object that is a copy of this instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override object Clone()
        {
            CustomContentRenderer result = new CustomContentRenderer();

            result._drawAnnotations = _drawAnnotations;
            result._drawForms = _drawForms;
            result._drawImages = _drawImages;
            result._drawInlineImages = _drawInlineImages;
            result._drawInvisibleText = _drawInvisibleText;
            result._drawPaths = _drawPaths;
            result._drawText = _drawText;
            result._fillAreaUseShadingPatterns = _fillAreaUseShadingPatterns;
            result._fillPaths = _fillPaths;
            result._fillPathsUseShadingPatterns = _fillPathsUseShadingPatterns;
            result._fillPathsUseTilingPatterns = _fillPathsUseTilingPatterns;
            if (_imageProcessing != null)
                result._imageProcessing = (ProcessingCommandBase)_imageProcessing.Clone();
            result._linesWeigth = _linesWeigth;
            result._setClipPaths = _setClipPaths;

            return result;
        }

        #endregion

    }
}
