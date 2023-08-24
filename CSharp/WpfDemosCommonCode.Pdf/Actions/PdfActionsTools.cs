using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;
using Vintasoft.Imaging.Pdf.Wpf.UI;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Contains common static functions for PDF actions.
    /// </summary>
    public static class PdfActionsTools
    {

        /// <summary>
        /// Gets the action description.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>Action description.</returns>
        public static string GetActionDescription(PdfAction action)
        {
            if (action is PdfGotoAction)
            {
                PdfGotoAction gotoAction = (PdfGotoAction)action;
                WpfPdfGotoActionExecutor gotoActionExecutor =
                    PdfActionExecutorManager.ApplicationActionExecutor.FindExecutor<WpfPdfGotoActionExecutor>();
                if (gotoActionExecutor != null)
                {
                    VintasoftImage destImage = gotoActionExecutor.GetImageAssociatedWithAction(gotoAction);
                    if (destImage != null)
                    {
                        return string.Format("{0} ({1}): page number {2}",
                               gotoAction.GetType().Name,
                               gotoAction.Destination.GetType().Name,
                               gotoActionExecutor.ImageViewer.Images.IndexOf(destImage) + 1);
                    }
                    if (gotoAction.Destination != null && gotoAction.Destination.Page != null)
                    {
                        return string.Format("{0} ({1}): document page number {2}",
                           gotoAction.GetType().Name,
                           gotoAction.Destination.GetType().Name,
                           gotoAction.Document.Pages.IndexOf(gotoAction.Destination.Page) + 1);
                    }
                }
            }
            else if (action is PdfNamedAction)
            {
                return string.Format("Named Action: '{0}'",
                    ((PdfNamedAction)action).ActionName);
            }
            else if (action is PdfLaunchAction)
            {
                return string.Format("Launch Application: '{0}'",
                    ((PdfLaunchAction)action).WinCommandLine);
            }
            else if (action is PdfUriAction)
            {
                return string.Format("Open URL: '{0}'",
                    ((PdfUriAction)action).URI);
            }
            else if (action is PdfSubmitFormAction)
            {
                return string.Format("Submit Interactive Form ({0}): '{1}'",
                    ((PdfSubmitFormAction)action).SubmitFormat,
                    ((PdfSubmitFormAction)action).Url);
            }
            return string.Format("{0} action ({1})", action.ActionType, action.GetType().Name);
        }

        /// <summary>
        /// Gets the activate action description.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <returns>The activate action description.</returns>
        public static string GetActivateActionDescription(PdfAnnotation annotation)
        {
            PdfAction action = annotation.GetAction("Activate");
            if (annotation.AdditionalActions != null)
            {
                if (action == null)
                    action = annotation.AdditionalActions.MouseButtonUp;
                if (action == null)
                    action = annotation.AdditionalActions.MouseButtonDown;
            }
            if (action != null)
                return GetActionDescription(action);
            return "";
        }

    }
}
