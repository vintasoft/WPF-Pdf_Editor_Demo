using System.Windows;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Contains common static functions for editing of PDF actions.
    /// </summary>
    public static class PdfActionsEditorTool
    {
        
        /// <summary>
     /// Edits the PDF action.
     /// </summary>
     /// <param name="action">The PDF action.</param>
     /// <param name="imageCollection">Image collection, which is associated with
     /// PDF document.</param>
     /// <param name="parentWindow">Window that must be used as a parent window for dialog
     /// that edits PDF action.</param>
        public static bool EditAction(PdfAction action, ImageCollection imageCollection, Window parentWindow)
        {
            Window dialog = null;

            switch (action.ActionType)
            {
                case PdfActionType.GoTo:
                    dialog = new PdfGotoActionEditorWindow((PdfGotoAction)action, imageCollection);
                    break;

                case PdfActionType.Hide:
                    dialog = new PdfAnnotationHideActionEditorWindow((PdfAnnotationHideAction)action);
                    break;

                case PdfActionType.JavaScript:
                    dialog = new PdfJavaScriptActionEditorWindow((PdfJavaScriptAction)action);
                    break;

                case PdfActionType.Launch:
                    dialog = new PdfLaunchActionEditorWindow((PdfLaunchAction)action);
                    break;

                case PdfActionType.Named:
                    dialog = new PdfNamedActionEditorWindow((PdfNamedAction)action);
                    break;

                case PdfActionType.ResetForm:
                    dialog = new PdfResetFormActionEditorWindow((PdfResetFormAction)action);
                    break;

                case PdfActionType.SubmitForm:
                    dialog = new PdfSubmitFormActionEditorWindow((PdfSubmitFormAction)action);
                    break;

                case PdfActionType.URI:
                    dialog = new PdfUriActionEditorWindow((PdfUriAction)action);
                    break;
            }

            bool result = false;

            if (dialog != null)
            {
                // set the dialog owner
                if (parentWindow != null)
                    dialog.Owner = parentWindow;
                // show dialog
                if (dialog.ShowDialog() == true)
                    result = true;
            }

            return result;
        }


    }
}
