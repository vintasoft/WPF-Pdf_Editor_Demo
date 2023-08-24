using System;

using Vintasoft.Imaging.Pdf.Tree.Annotations;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Defines an interface for controls, which can change properties of PDF annotation.
    /// </summary>
    public interface IPdfAnnotationPropertiesEditor
    {

        /// <summary>
        /// Gets or sets the annotation.
        /// </summary>
        PdfAnnotation Annotation { get; set; }


        /// <summary>
        /// Updates information about the annotation.
        /// </summary>
        void UpdateAnnotationInfo();
    }
}
