using System;

using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Defines an interface for controls, which can change properties of PDF interactive form field.
    /// </summary>
    public interface IPdfInteractiveFormPropertiesEditor
    {

        /// <summary>
        /// Gets or sets the PDF interactive form field.
        /// </summary>
        PdfInteractiveFormField Field { get; set; }



        /// <summary>
        /// Updates information about the field.
        /// </summary>
        void UpdateFieldInfo();
    }
}
