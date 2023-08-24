using System.Text;

#if !REMOVE_PDF_PLUGIN
using Vintasoft.Imaging.Pdf.Tree.Fonts;
#endif

using Vintasoft.Imaging.Text;

namespace WpfDemosCommonCode
{
    /// <summary>
    /// Contains collection of helper-algorithms with fonts for demo applications.
    /// </summary>
    class FontDemosTools
    {

        /// <summary>
        /// Returns the font information.
        /// </summary>
        /// <param name="font">The font.</param>
        /// <returns>The font information.</returns>
        public static string GetFontInfo(ITextFont font)
        {
#if !REMOVE_PDF_PLUGIN 
            PdfFont pdfFont = font as PdfFont;
            if (pdfFont != null)
            {
                StringBuilder fontProgramType = new StringBuilder(pdfFont.FontType.ToString());
                if (pdfFont.StandardFontType != PdfStandardFontType.NotStandard)
                {
                    fontProgramType.Append(", Standard");
                }
                else
                {
                    if (pdfFont.IsFullyDefined)
                        fontProgramType.Append(", Embedded");
                    else
                        fontProgramType.Append(", External");
                    if (pdfFont.IsVertical)
                        fontProgramType.Append(", Vertical");
                }
                return string.Format("[{1}] {0} ({2})", pdfFont.FontName, pdfFont.ObjectNumber, fontProgramType);
            }
#endif
            return font.Name;
        }

    }
}
