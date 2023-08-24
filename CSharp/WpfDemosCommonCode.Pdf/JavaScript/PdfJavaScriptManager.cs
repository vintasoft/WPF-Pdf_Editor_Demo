using System.Collections.Generic;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.JavaScriptApi;
using Vintasoft.Imaging.Wpf.UI;

namespace WpfDemosCommonCode.Pdf.JavaScript
{
    /// <summary>
    /// PDF JavaScript engine manager.
    /// </summary>
    public static class PdfJavaScriptManager
    {


        #region Constructors

        /// <summary>
        /// Initializes the <see cref="PdfJavaScriptManager"/> class.
        /// </summary>
        static PdfJavaScriptManager()
        {
            _jsApp = new PdfViewerJsApp();
            _javaScriptActionExecutor = new WpfPdfJavaScriptActionExecutor(_jsApp);
            _jsApp.ActionExecutor = _javaScriptActionExecutor;
            _javaScriptActionExecutor.IsEnabled = IsJavaScriptEnabled;
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether JavaScript is enabled.
        /// </summary>
        public static bool IsJavaScriptEnabled
        {
            get
            {
                return _javaScriptActionExecutor.IsEnabled;
            }
            set
            {
                _javaScriptActionExecutor.IsEnabled = value;
            }
        }

        static WpfPdfJavaScriptActionExecutor _javaScriptActionExecutor;
        /// <summary>
        /// Gets the JavaScript action executor.
        /// </summary>
        public static WpfPdfJavaScriptActionExecutor JavaScriptActionExecutor
        {
            get
            {
                return _javaScriptActionExecutor;
            }
        }

        static PdfViewerJsApp _jsApp;
        /// <summary>
        /// Gets the global JavaScript API "app" object.
        /// </summary>
        public static PdfViewerJsApp JsApp
        {
            get
            {
                return _jsApp;
            }
        }

        #endregion

    }
}

