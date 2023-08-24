using System;
using System.Windows.Media;

using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Wpf;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit border properties of the <see cref="PdfWidgetAnnotation"/>.
    /// </summary>
    public partial class PdfWidgetAnnotationBorderStyleControl : PdfAnnotationBorderStyleControl
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfWidgetAnnotationBorderStyleControl"/> class.
        /// </summary>
        public PdfWidgetAnnotationBorderStyleControl()
        {
        }

        #endregion



        #region Properties

        PdfWidgetAnnotation _annotation = null;
        /// <summary>
        /// Gets or sets the annotation.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if
        /// annotation is not a widget anotation</exception>
        public override PdfAnnotation Annotation
        {
            get
            {
                return base.Annotation;
            }
            set
            {
                _annotation = value as PdfWidgetAnnotation;
                if (value != null && _annotation == null)
                    throw new InvalidOperationException();

                base.Annotation = value;
            }
        }

        /// <summary>
        /// Gets or sets the border color.
        /// </summary>
        protected override Color BorderColor
        {
            get
            {
                if (_annotation.AppearanceCharacteristics == null)
                    _annotation.AppearanceCharacteristics = new PdfAnnotationAppearanceCharacteristics(_annotation.Document);
                return WpfObjectConverter.CreateWindowsColor(_annotation.AppearanceCharacteristics.BorderColor);
            }
            set
            {
                if (_annotation.AppearanceCharacteristics == null)
                    _annotation.AppearanceCharacteristics = new PdfAnnotationAppearanceCharacteristics(_annotation.Document);
                _annotation.AppearanceCharacteristics.BorderColor = WpfObjectConverter.CreateDrawingColor(value);
            }
        }

        #endregion
    }
}
