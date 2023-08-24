using System;
using System.Diagnostics;
using System.Windows;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Executor of "URI" actions that opens URL using the default internet browser.
    /// </summary>
    public class PdfUriActionExecutor : PdfActionExecutorBase
    {

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="args">The <see cref="PdfTriggerEventArgs"/> instance
        /// containing the event data.</param>
        /// <returns><b>True</b> if action is executed; otherwise, <b>false</b>.</returns>
        public override bool ExecuteAction(PdfAction action, PdfTriggerEventArgs args)
        {
            PdfUriAction uriAction = action as PdfUriAction;
            if (uriAction != null)
            {
                if (uriAction.URI != "")
                {
                    if (MessageBox.Show(string.Format("Open URL '{0}' ?", uriAction.URI), "Open URL", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            DemosTools.OpenBrowser(uriAction.URI);
                        }
                        catch (Exception exc)
                        {
                            DemosTools.ShowErrorMessage(exc);
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

    }
}
