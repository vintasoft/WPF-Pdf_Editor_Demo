
namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Contains information about PDF viewer named action.
    /// </summary>
    public class PdfViewerNamedAction
    {

        #region Fields

        /// <summary>
        /// Handler of the action.
        /// </summary>
        ViewerActionDelegate _viewerActionHandler;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfViewerNamedAction"/> class.
        /// </summary>
        /// <param name="actionName">Action name.</param>
        /// <param name="viewerActionHandler">Action handler.</param>
        public PdfViewerNamedAction(string actionName, ViewerActionDelegate viewerActionHandler)
        {
            _actionName = actionName;
            _viewerActionHandler = viewerActionHandler;
        }

        #endregion



        #region Properties

        string _actionName;
        /// <summary>
        /// Gets the action name.
        /// </summary>
        public string ActionName
        {
            get
            {
                return _actionName;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Executes the action.
        /// </summary>
        public void Execute()
        {
            _viewerActionHandler();
        }

        #endregion



        #region Delegates

        /// <summary>
        /// Delegate that executes an action.
        /// </summary>
        public delegate void ViewerActionDelegate();

        #endregion

    }
}
