using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;

using Vintasoft.Imaging.Pdf.JavaScriptApi;
using Vintasoft.Imaging.Print;
using Vintasoft.Imaging.Wpf.Print;
using Vintasoft.Imaging.Wpf.UI;
using Vintasoft.Imaging.Pdf.Wpf.UI.JavaScript;

namespace WpfDemosCommonCode.Pdf.JavaScript
{
    /// <summary>
    /// An JavaScript API "app" object that represents the WPF PDF Viewer application. 
    /// </summary>
    public class PdfViewerJsApp : WpfPdfJsApp
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfViewerJsApp"/> class.
        /// </summary>
        public PdfViewerJsApp()
            : base()
        {
        }

     

        /// <summary>
        /// Prints all or a specific number of pages of specified document.
        /// </summary>
        /// <param name="doc">The Document to print.</param>
        /// <param name="bUI">Indicates that the User Interface must be shown to the user
        /// to obtain printing information and confirm the action.</param>
        /// <param name="nStart">A zero-based index that defines the start of an inclusive range of pages.<br />
        /// If <i>nStart</i> and <i>nEnd</i> are not specified, all pages in the document are printed.<br />
        /// If only <i>nStart</i> is specified, the range of pages is the single page
        /// specified by <i>nStart</i>.<br />
        /// If <i>nStart</i> and <i>nEnd</i> parameters are used, <i>bUI</i> must be false.</param>
        /// <param name="nEnd">A zero-based index that defines the end of an inclusive page range.<br />
        /// If <i>nStart</i> and <i>nEnd</i> are not specified, all pages in the document are printed.<br />
        /// If only <i>nEnd</i> is specified, the range of a pages is 0 to <i>nEnd</i>.<br />
        /// If <i>nStart</i> and <i>nEnd</i> parameters are used, <i>bUI</i> must be <b>false</b>.</param>
        /// <param name="bSilent">If <b>true</b>, suppresses the cancel dialog box while the document is printing.</param>
        /// <param name="bShrinkToFit">If <b>true</b>, the page is shrunk (if necessary) to fit within the imageable area
        /// of the printed page. If <b>false</b>, it is not.</param>
        /// <param name="bPrintAsImage">If <b>true</b>, print pages as an image.</param>
        /// <param name="bReverse">If <b>true</b>, print from <i>nEnd</i> to <i>nStart</i>.</param>
        /// <param name="bAnnotations">If <b>true</b>, annotations are printed.</param>
        public override void PrintDoc(
            PdfJsDoc doc,
            bool bUI,
            int nStart,
            int nEnd,
            bool bSilent,
            bool bShrinkToFit,
            bool bPrintAsImage,
            bool bReverse,
            bool bAnnotations)
        {
            // get image viewer where document is displayed
            WpfImageViewer viewer = FindImageViewer(doc.Source);

            // page count of document
            int pageCount = viewer.Images.Count;
            int fromPage;
            int toPage;
            PageRangeSelection printRange;
            // if page range is specified
            if (nStart >= 0 || nEnd >= 0)
            {
                // if page range is invalid
                if (nStart >= pageCount || nEnd >= pageCount)
                    return;

                // if start page is specified
                if (nStart >= 0)
                {
                    fromPage = nStart + 1;
                    // if end page is specified
                    if (nEnd >= 0)
                    {
                        if (nEnd >= nStart)
                            toPage = nEnd + 1;
                        else
                            toPage = pageCount;
                    }
                    else
                    {
                        toPage = fromPage;
                    }
                }
                else
                {
                    fromPage = 1;
                    toPage = nEnd + 1;
                }

                printRange = PageRangeSelection.UserPages;
            }
            else
            {
                fromPage = 1;
                toPage = pageCount;
                printRange = PageRangeSelection.AllPages;
            }

            // create PrintManager
            using (WpfImagePrintManager printManager = new WpfImagePrintManager())
            {
                printManager.Images = viewer.Images;
                // set scale mode
                if (bShrinkToFit)
                    printManager.PrintScaleMode = PrintScaleMode.BestFit;
                else
                    printManager.PrintScaleMode = PrintScaleMode.CropToPageSize;

                // set dialog properties
                printManager.PrintDialog.UserPageRangeEnabled = true;
                printManager.PrintDialog.MinPage = 1;
                printManager.PrintDialog.MaxPage = (uint)pageCount;
                printManager.PrintDialog.PageRange = new PageRange(fromPage, toPage);
                printManager.PrintDialog.PageRangeSelection = printRange;

                // no padding
                printManager.PagePadding = new Thickness(0);

                // show print dialog and
                // start print if dialog result is OK
                if (!bUI || printManager.PrintDialog.ShowDialog() == true)
                {
                    try
                    {
                        string description = null;
                        FileStream documentFileStream = doc.Source.SourceStream as FileStream;
                        if (documentFileStream != null)
                            description = Path.GetFileName(documentFileStream.Name);
                        if (description == null)
                            description = "JavaScript print";
                        printManager.Print(description);
                    }
                    catch (Exception ex)
                    {
                        DemosTools.ShowErrorMessage(ex);
                    }
                }
            }
        }

       

    }
}
