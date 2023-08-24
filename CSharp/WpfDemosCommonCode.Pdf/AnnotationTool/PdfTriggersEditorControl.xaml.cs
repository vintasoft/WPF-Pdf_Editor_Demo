using System;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit triggers of PDF annotation or PDF interactive form field.
    /// </summary>
    public partial class PdfTriggersEditorControl : UserControl
    {

        #region Fields

        /// <summary>
        /// The available actions of annotation.
        /// </summary>
        string[] _annotationActions = new string[] {
            "Activate",
            "Focus",
            "Blur",
            "Mouse Up",
            "Mouse Down",
            "Mouse Enter",
            "Mouse Exit",
            "PageOpen",
            "PageClose",
            "PageVisible",
            "PageInvisible",
        };

        /// <summary>
        /// The available actions of an interactive form.
        /// </summary>
        string[] _interactiveFormFieldActions = new string[] 
        {
            "Calculate",
            "Keystroke",
            "Format",
            "Validate",
        };

        /// <summary>
        /// The available actions of PDF document catalog.
        /// </summary>
        string[] _documentCatalogActions = new string[]
        {
            "Open",
            "Printing",
            "Printed",
            "Saving",
            "Saved",
            "Closing",
        };

        /// <summary>
        /// The available actions of PDF page.
        /// </summary>
        string[] _pageActions = new string[]
        {
            "Open",
            "Close",
        };

        /// <summary>
        /// The actions of tree node.
        /// </summary>
        string[] _actions = null;

        /// <summary>
        /// Determines that the actions, of tree node, are updating.
        /// </summary>
        bool _actionsUpdating = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfTriggersEditorControl"/> class.
        /// </summary>
        public PdfTriggersEditorControl()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        PdfTreeNodeBase _treeNode;
        /// <summary>
        /// Gets or sets the tree node that contains actions.
        /// </summary>
        public PdfTreeNodeBase TreeNode
        {
            get
            {
                return _treeNode;
            }
            set
            {
                _treeNode = value;

                // if value is NOT empty
                if (value != null)
                {
                    // if value is interactive form
                    if (value is PdfInteractiveFormField)
                        _actions = _interactiveFormFieldActions;
                    // if value is annotation
                    else if (value is PdfAnnotation)
                        _actions = _annotationActions;
                    // if value is PDF document catalog
                    else if (value is PdfDocumentCatalog)
                        _actions = _documentCatalogActions;
                    // if value is PDF page
                    else if (value is PdfPage)
                        _actions = _pageActions;
                    else
                        throw new NotSupportedException();


                    // update list box of triggers
                    UpdateTriggersInfo();

                    // set PDF document
                    pdfActionEditorControl.Document = value.Document;
                }
                else
                    // clear list box of triggers
                    triggersListBox.Items.Clear();

                // update the user interface
                UpdateUI();
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the triggers information.
        /// </summary>
        internal void UpdateTriggersInfo()
        {
            if (!_actionsUpdating && _actions != null && _treeNode != null)
            {
                _actionsUpdating = true;
                triggersListBox.BeginInit();
                triggersListBox.Items.Clear();
                foreach (string action in _actions)
                    triggersListBox.Items.Add(GetActionName(action, _treeNode));
                triggersListBox.EndInit();
                _actionsUpdating = false;
                UpdateSelectedAction();
                UpdateUI();
            }
        }

        /// <summary>
        /// Returns the action name.
        /// </summary>
        /// <param name="actionPropertyName">Name of the action property.</param>
        /// <param name="treeNode">The tree node.</param>
        /// <returns>The action name.</returns>
        private string GetActionName(string actionPropertyName, PdfTreeNodeBase treeNode)
        {
            string result = actionPropertyName;
            PdfAction action = treeNode.GetAction(actionPropertyName);
            if (action != null)
                result = string.Format("{0}: {1}", actionPropertyName, PdfActionsTools.GetActionDescription(action));
            return result;
        }

        /// <summary>
        /// Updates the user interface of this control.
        /// </summary>
        private void UpdateUI()
        {
            triggersListBox.IsEnabled = _treeNode != null;
            triggerActionsGroupBox.IsEnabled = triggersListBox.SelectedIndex != -1;
            if (triggerActionsGroupBox.IsEnabled)
                triggerActionsGroupBox.Header = _actions[triggersListBox.SelectedIndex];
            else
                triggerActionsGroupBox.Header = string.Empty;
        }

        /// <summary>
        /// Selected trigger is changed.
        /// </summary>
        private void triggersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedAction();
            e.Handled = true;

            UpdateUI();
        }

        /// <summary>
        /// Updates the selected action.
        /// </summary>
        private void UpdateSelectedAction()
        {
            if (_actionsUpdating)
                return;

            _actionsUpdating = true;
            // if trigger is selected
            if (_treeNode != null && triggersListBox.SelectedIndex != -1)
            {
                // get action name
                string actionName = _actions[triggersListBox.SelectedIndex];
                // get action of trigger
                PdfAction action = _treeNode.GetAction(actionName);
                // set action in editor
                pdfActionEditorControl.Action = action;
            }
            else
            {
                pdfActionEditorControl.Action = null;
            }
            _actionsUpdating = false;
        }

        /// <summary>
        /// Action is changed.
        /// </summary>
        private void PdfActionEditorControl_ActionChanged(object sender, EventArgs e)
        {
            try
            {
                _actionsUpdating = true;
                // get selected index
                int selectedIndex = triggersListBox.SelectedIndex;
                // get action name
                string currentActionName = _actions[selectedIndex];
                // get changed action
                PdfAction action = pdfActionEditorControl.Action;
                // update action
                _treeNode.SetAction(currentActionName, action);
                // update action name
                triggersListBox.Items[selectedIndex] = GetActionName(currentActionName, _treeNode);
                triggersListBox.SelectedIndex = selectedIndex;
                _actionsUpdating = false;
            }
            catch (InvalidCastException exc)
            {
                string message = "Unexpected Action Type: " + exc.Message;
                DemosTools.ShowErrorMessage(message);

                pdfActionEditorControl.Action = null;
            }
            catch (Exception exc)
            {
                DemosTools.ShowErrorMessage(exc.Message);

                pdfActionEditorControl.Action = null;
            }
        }

        #endregion

    }
}
