using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Vintasoft.Imaging;
#if !REMOVE_ANNOTATION_PLUGIN
using Vintasoft.Imaging.Annotation.Comments;
using Vintasoft.Imaging.Annotation.Wpf.UI.VisualTools;
#endif
using Vintasoft.Imaging.Pdf.Drawing.GraphicsFigures;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;
using Vintasoft.Imaging.Pdf.Wpf.UI;
using Vintasoft.Imaging.Pdf.Wpf.UI.Annotations;
using Vintasoft.Imaging.Text;
using Vintasoft.Imaging.UIActions;
using Vintasoft.Imaging.Wpf.UI;
using Vintasoft.Imaging.Wpf.UI.VisualTools;
using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;

using WpfDemosCommonCode.Annotation;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit annotations and PDF interactive form of PDF document.
    /// </summary>
    public partial class PdfAnnotationToolControl : UserControl
    {

        #region Fields

        /// <summary>
        /// The context menu of annotation tool.
        /// </summary>
        ContextMenu _annotationContextMenu = null;

        /// <summary>
        /// Dictionary: "State name" => "Menu item".
        /// </summary>
        Dictionary<string, MenuItem> _commentStateNameToMenuItem;

        /// <summary>
        /// Hovered <see cref="WpfPdfAnnotationView"/>.
        /// </summary>
        /// <remarks>
        /// Field is updated when the annotation context menu is opened.
        /// </remarks>
        WpfPdfAnnotationView _hoveredAnnotationView;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAnnotationToolControl"/> class.
        /// </summary>
        public PdfAnnotationToolControl()
        {
            InitializeComponent();

            ToolTip hoveredAnnotationToolTip = new ToolTip();
            ToolTipService.SetInitialShowDelay(hoveredAnnotationToolTip, 750);
            HoveredAnnotationToolTip = hoveredAnnotationToolTip;

            interactionModeViewRadioButton.Tag = WpfPdfAnnotationInteractionMode.View;
            interactionModeMarkupRadioButton.Tag = WpfPdfAnnotationInteractionMode.Markup;
            interactionModeEditRadioButton.Tag = WpfPdfAnnotationInteractionMode.Edit;
            interactionModeNoneRadioButton.Tag = WpfPdfAnnotationInteractionMode.None;

            _annotationContextMenu = (ContextMenu)Resources["annotationContextMenu"];

#if !REMOVE_ANNOTATION_PLUGIN
            _commentStateNameToMenuItem = new Dictionary<string, MenuItem>();
            MenuItem setStatusMenuItem = (MenuItem)_annotationContextMenu.Items[1];
            _commentStateNameToMenuItem.Add(CommentTools.CommentStateReviewAccepted, (MenuItem)setStatusMenuItem.Items[0]);
            _commentStateNameToMenuItem.Add(CommentTools.CommentStateReviewRejected, (MenuItem)setStatusMenuItem.Items[1]);
            _commentStateNameToMenuItem.Add(CommentTools.CommentStateReviewCancelled, (MenuItem)setStatusMenuItem.Items[2]);
            _commentStateNameToMenuItem.Add(CommentTools.CommentStateReviewCompleted, (MenuItem)setStatusMenuItem.Items[3]);
            _commentStateNameToMenuItem.Add(CommentTools.CommentStateReviewNone, (MenuItem)setStatusMenuItem.Items[4]);
            foreach (string stateName in _commentStateNameToMenuItem.Keys)
                _commentStateNameToMenuItem[stateName].Tag = stateName;
#endif

        }

        #endregion



        #region Properties

        WpfPdfAnnotationTool _annotationTool;
        /// <summary>
        /// Gets or sets the annotation tool.
        /// </summary>
        public WpfPdfAnnotationTool AnnotationTool
        {
            get
            {
                return _annotationTool;
            }
            set
            {
                if (_annotationTool != null)
                    UnsubscribeFromPdfAnnotationToolEvents();

                _annotationTool = value;
                AnnotationsControl.AnnotationTool = value;
                InteractiveFormControl.AnnotationTool = value;

                if (_annotationTool != null)
                {
                    SubscribeToPdfAnnotationToolEvents();

                    if (_annotationTool.AnnotationViewCollection != null)
                    {
                        foreach (WpfPdfAnnotationView annotationView in _annotationTool.AnnotationViewCollection)
                            ToolTipService.SetToolTip(annotationView, _hoveredAnnotationToolTip);
                    }
                }

                UpdateUI();
            }
        }

        ToolTip _hoveredAnnotationToolTip = null;
        /// <summary>
        /// Gets or sets the tooltip of hovered annotation.
        /// </summary>
        public ToolTip HoveredAnnotationToolTip
        {
            get
            {
                return _hoveredAnnotationToolTip;
            }
            set
            {
                if (_hoveredAnnotationToolTip != value)
                {
                    _hoveredAnnotationToolTip = value;

                    if (_hoveredAnnotationToolTip != null)
                    {
                        if (_annotationTool != null &&
                           _annotationTool.AnnotationViewCollection != null)
                        {
                            foreach (WpfPdfAnnotationView annotationView in _annotationTool.AnnotationViewCollection)
                                ToolTipService.SetToolTip(annotationView, _hoveredAnnotationToolTip);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the copy action.
        /// </summary>
        public UIAction CopyAction
        {
            get
            {
                if (_annotationTool.ImageViewer != null && _annotationTool.ImageViewer.VisualTool != null)
                    return PdfDemosTools.GetUIAction<CopyItemUIAction>(_annotationTool.ImageViewer.VisualTool);
                return null;
            }
        }

        /// <summary>
        /// Gets the cut action.
        /// </summary>
        public UIAction CutAction
        {
            get
            {
                if (_annotationTool.ImageViewer != null && _annotationTool.ImageViewer.VisualTool != null)
                    return PdfDemosTools.GetUIAction<CutItemUIAction>(_annotationTool.ImageViewer.VisualTool);
                return null;
            }
        }

        /// <summary>
        /// Gets the paste action.
        /// </summary>
        public UIAction PasteAction
        {
            get
            {
                if (_annotationTool.ImageViewer != null && _annotationTool.ImageViewer.VisualTool != null)
                    return PdfDemosTools.GetUIAction<PasteItemUIAction>(_annotationTool.ImageViewer.VisualTool);
                return null;
            }
        }

        /// <summary>
        /// Gets the delete action.
        /// </summary>
        public UIAction DeleteAction
        {
            get
            {
                if (_annotationTool.ImageViewer != null && _annotationTool.ImageViewer.VisualTool != null)
                    return PdfDemosTools.GetUIAction<DeleteItemUIAction>(_annotationTool.ImageViewer.VisualTool);
                return null;
            }
        }

        /// <summary>
        /// Gets the bring to back action.
        /// </summary>
        public UIAction BringToBackAction
        {
            get
            {
                if (_annotationTool.ImageViewer != null && _annotationTool.ImageViewer.VisualTool != null)
                    return PdfDemosTools.GetUIAction<BringToBackItemUIAction>(_annotationTool.ImageViewer.VisualTool);
                return null;
            }
        }

        /// <summary>
        /// Gets the bring to front action.
        /// </summary>
        public UIAction BringToFrontAction
        {
            get
            {
                if (_annotationTool.ImageViewer != null && _annotationTool.ImageViewer.VisualTool != null)
                    return PdfDemosTools.GetUIAction<BringToFrontItemUIAction>(_annotationTool.ImageViewer.VisualTool);
                return null;
            }
        }

#if !REMOVE_ANNOTATION_PLUGIN
        WpfCommentVisualTool _commentTool;
        /// <summary>
        /// Sets the <see cref="WpfCommentVisualTool"> that allows to operate the annotation comments.
        /// </summary>
        public WpfCommentVisualTool CommentTool
        {
            get
            {
                return _commentTool;
            }
            set
            {
                _commentTool = value;
            }
        }
#endif

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Updates the User Interface.
        /// </summary>
        public void UpdateUI()
        {
            AnnotationsControl.UpdateUI();
            InteractiveFormControl.UpdateUI();
            if (AnnotationTool != null)
            {
                multiSelectCheckBox.IsChecked = AnnotationTool.AllowMultipleSelection;
                highlightObjectsCheckBox.IsChecked = AnnotationTool.EditorModeHighlight;
                highlightFieldsCheckBox.IsChecked = AnnotationTool.FieldHighlight;
                tabNavigationLoopedOnPageCheckBox.IsChecked = AnnotationTool.IsNavigationLoopedOnFocusedPage;
                UpdateInteractionMode();
            }
        }

        /// <summary>
        /// Shows the form fields tab page.
        /// </summary>
        public void ShowFormFieldsTab()
        {
            toolsTabControl.SelectedItem = formFieldsTabPage;
        }

        /// <summary>
        /// Shows the annotations tab page.
        /// </summary>
        public void ShowAnnotationsTab()
        {
            toolsTabControl.SelectedItem = annotationsTabPage;
        }

        #endregion


        #region PRIVATE

        #region Interaction mode

        /// <summary>
        /// Interaction mode of PDF annotation tool is changed.
        /// </summary>
        private void interactionModeRadioButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AnnotationTool != null)
                {
                    AnnotationTool.CancelBuilding();

                    if (((RadioButton)sender).IsChecked.Value)
                    {
                        AnnotationTool.InteractionMode = (WpfPdfAnnotationInteractionMode)((FrameworkElement)sender).Tag;
                        editModeSettingsGroupBox.IsEnabled = AnnotationTool.InteractionMode == WpfPdfAnnotationInteractionMode.Edit;
                    }
                }
            }
            catch (Exception exception)
            {
                DemosTools.ShowErrorMessage(exception.Message);
            }
        }

        #endregion


        #region Edit mode settings

        /// <summary>
        /// Enables or disables multi selection of annotations.
        /// </summary>
        private void multiSelectCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (AnnotationTool != null)
                AnnotationTool.AllowMultipleSelection = multiSelectCheckBox.IsChecked.Value;
        }

        /// <summary>
        /// Enables or disables the highlighting of interactive fields in Editor mode.
        /// </summary>
        private void highlightObjectsCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (AnnotationTool != null)
                AnnotationTool.EditorModeHighlight = highlightObjectsCheckBox.IsChecked.Value;
        }

        #endregion


        /// <summary>
        /// Enables or disables the highlighting of interactive fields.
        /// </summary>
        private void highlightFieldsCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (AnnotationTool != null)
                AnnotationTool.FieldHighlight = highlightFieldsCheckBox.IsChecked.Value;
        }

        /// <summary>
        /// Tab page (annotations / interactive fields) is changed.
        /// </summary>
        private void toolsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // if current tab of tab control is changed
            if (e.Source == toolsTabControl)
            {
                AnnotationTool.CancelBuilding();

                UpdateUI();

                if (toolsTabControl.SelectedItem == annotationsTabPage)
                    AnnotationsControl.RefreshAnnotationList();
                else if (toolsTabControl.SelectedItem == formFieldsTabPage)
                    InteractiveFormControl.RefreshInteractiveFormTree();
            }
        }



        /// <summary>
        /// Enables or disables the TAB navigation looping on page.
        /// </summary>
        private void tabNavigationLoopedOnPageCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (AnnotationTool != null)
                AnnotationTool.IsNavigationLoopedOnFocusedPage = (bool)tabNavigationLoopedOnPageCheckBox.IsChecked;
        }


        /// <summary>
        /// Enables or disables building annotations continuously.
        /// </summary>
        private void buildContinuouslyCheckBox_Click(object sender, RoutedEventArgs e)
        {
            AnnotationsControl.AnnotationBuilderControl.NeedBuildAnnotationsContinuously = (bool)buildContinuouslyCheckBox.IsChecked;
            InteractiveFormControl.InteractiveFormFieldBuilder.NeedBuildFormFieldsContinuously = (bool)buildContinuouslyCheckBox.IsChecked;
        }


        #region Annotation context menu

        /// <summary>
        /// Context menu is opened.
        /// </summary>
        private void annotationContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = (ContextMenu)sender;
            MenuItem replyMenuItem = (MenuItem)menu.Items[0];
            MenuItem setStatusMenuItem = (MenuItem)menu.Items[1];
            Separator menuSeparator = (Separator)menu.Items[2];
            MenuItem bringToBackMenuItem = (MenuItem)menu.Items[11];
            MenuItem bringToFrontMenuItem = (MenuItem)menu.Items[10];
            MenuItem propertiesMenuItem = (MenuItem)menu.Items[14];
            MenuItem transformersMenuItem = (MenuItem)menu.Items[13];
            MenuItem pasteMenuItem = (MenuItem)menu.Items[7];
            MenuItem copyMenuItem = (MenuItem)menu.Items[6];
            MenuItem deleteMenuItem = (MenuItem)menu.Items[8];
            MenuItem cutMenuItem = (MenuItem)menu.Items[5];
            MenuItem textMarkupMenuItem = (MenuItem)menu.Items[3];

            bool hasFocusedAnnotation = AnnotationTool.FocusedAnnotationView != null;
            if (!hasFocusedAnnotation)
            {
                // get the cursor point in viewer coordinates
                Point cursorPosition = Mouse.GetPosition(AnnotationTool.ImageViewer);

                // get the hovered annotation view
                _hoveredAnnotationView = AnnotationTool.FindAnnotationView(cursorPosition, false);

                hasFocusedAnnotation = _hoveredAnnotationView != null;
            }

            // text is selected ?
            bool hasSelectedText = false;
            WpfTextSelectionTool textSelection = WpfVisualTool.FindVisualTool<WpfTextSelectionTool>(AnnotationTool.ImageViewer.VisualTool);
            if (textSelection != null)
                hasSelectedText = textSelection.HasSelectedText;

            bringToBackMenuItem.IsEnabled = CanUseAction(BringToBackAction);
            bringToFrontMenuItem.IsEnabled = CanUseAction(BringToFrontAction);
            propertiesMenuItem.IsEnabled = hasFocusedAnnotation;
            transformersMenuItem.IsEnabled = hasFocusedAnnotation;
            pasteMenuItem.IsEnabled = CanUseAction(PasteAction);
            copyMenuItem.IsEnabled = CanUseAction(CopyAction);
            deleteMenuItem.IsEnabled = CanUseAction(DeleteAction);
            cutMenuItem.IsEnabled = CanUseAction(CutAction);
            textMarkupMenuItem.IsEnabled = hasSelectedText;

            bool showCommentSubMenu = false;

#if !REMOVE_ANNOTATION_PLUGIN
            if (CommentTool != null)
            {
                Comment focusedComment = GetFocusedComment();

                showCommentSubMenu =
                    CommentTool.Enabled &&
                    ((interactionModeViewRadioButton.IsChecked.Value == true) || (interactionModeMarkupRadioButton.IsChecked.Value == true)) &&
                    _annotationTool.FocusedAnnotationView is WpfPdfMarkupAnnotationView &&
                    focusedComment != null && !focusedComment.IsState;

                if (showCommentSubMenu)
                {
                    UpdateStatesUI(focusedComment);
                }
            }
#endif
            if (showCommentSubMenu)
            {
                replyMenuItem.Visibility = Visibility.Visible;
                setStatusMenuItem.Visibility = Visibility.Visible;
                menuSeparator.Visibility = Visibility.Visible;
            }
            else
            {
                replyMenuItem.Visibility = Visibility.Collapsed;
                setStatusMenuItem.Visibility = Visibility.Collapsed;
                menuSeparator.Visibility = Visibility.Collapsed;
            }
        }


        #region Comments
#if !REMOVE_ANNOTATION_PLUGIN
        /// <summary>
        /// Returns the comment, that is focused.
        /// </summary>
        /// <returns>Replied <see cref="Comment">.</returns>
        private Comment GetFocusedComment()
        {
            CommentCollection comments = CommentTool.CommentController.GetComments(CommentTool.CommentController.ImageViewer.Image);

            if (comments != null && _annotationTool.FocusedAnnotationView != null)
                return comments.FindBySource(_annotationTool.FocusedAnnotationView.Annotation);

            return null;
        }
#endif

        /// <summary>
        /// Adds reply to comment.
        /// </summary>
        private void replyMenuItem_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_ANNOTATION_PLUGIN
            Comment comment = GetFocusedComment();
            if (comment != null)
            {
                Comment reply = comment.AddReply(System.Drawing.Color.Yellow, GetCurrentUserName());
                CommentControl replyControl = (CommentControl)CommentTool.FindCommentControl(reply);
                if (replyControl != null)
                    replyControl.NeedSetFocusWhenLoaded = true;
            }
#endif
        }

        /// <summary>
        /// Returns the name of the current user.
        /// </summary>
        /// <returns>The name of the current user.</returns>
        private string GetCurrentUserName()
        {
            return Environment.UserName;
        }

        /// <summary>
        /// Sets the state of comment.
        /// </summary>
        private void setStatusMenuItem_Click(object sender, RoutedEventArgs e)
        {
#if !REMOVE_ANNOTATION_PLUGIN
            MenuItem menuItem = (MenuItem)sender;
            string stateName = (string)menuItem.Tag;

            string[] parsedName = stateName.Split('.');
            string stateModel = parsedName[0];
            string state = parsedName[1];

            Comment repliedComment = GetFocusedComment();
            if (repliedComment != null)
            {
                Comment stateComment = repliedComment.SetState(System.Drawing.Color.Yellow, GetCurrentUserName(), stateModel, state, CommentTools.SplitStatesByUserName);
                stateComment.IsOpen = false;
                stateComment.Text = string.Format("{0} sets by {1}", state, stateComment.UserName);
            }
#endif
        }

#if !REMOVE_ANNOTATION_PLUGIN
        /// <summary>
        /// Updates the states UI.
        /// </summary>
        private void UpdateStatesUI(Comment currentComment)
        {
            if (currentComment == null)
                return;

            Comment[] states = currentComment.GetStates(CommentTools.SplitStatesByUserName);

            // update items of state menu item
            foreach (MenuItem item in _commentStateNameToMenuItem.Values)
            {
                item.IsEnabled = !currentComment.IsReadOnly;
                item.IsChecked = false;
            }
            if (states != null)
            {
                foreach (Comment state in states)
                {
                    if (!CommentTools.SplitStatesByUserName || state.UserName == GetCurrentUserName())
                    {
                        MenuItem item = null;
                        string stateName = string.Format("{0}.{1}", state.StateModel, state.ParentState);
                        if (_commentStateNameToMenuItem.TryGetValue(stateName, out item))
                            item.IsChecked = true;
                    }
                }
            }
        }
#endif

        #endregion


        /// <summary>
        /// Determines whether specified action can be used.
        /// </summary>
        private bool CanUseAction(UIAction action)
        {
            if (action == null)
                return false;
            return action.IsEnabled;
        }
        /// <summary>
        /// Handles the Click event of textMarkupHighlightMenuItem object.
        /// </summary>
        private void textMarkupHighlightMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfTextMarkupTools.HighlightSelectedText(AnnotationTool.ImageViewer);
        }

        /// <summary>
        /// Handles the Click event of textMarkupStrinkoutMenuItem object.
        /// </summary>
        private void textMarkupStrinkoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfTextMarkupTools.StrikeoutSelectedText(AnnotationTool.ImageViewer);
        }

        /// <summary>
        /// Handles the Click event of textMarkupUnderlineMenuItem object.
        /// </summary>
        private void textMarkupUnderlineMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfTextMarkupTools.UnderlineSelectedText(AnnotationTool.ImageViewer);
        }

        /// <summary>
        /// Handles the Click event of textMarkupSquigglyUnderlineMenuItem object.
        /// </summary>
        private void textMarkupSquigglyUnderlineMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfTextMarkupTools.SquigglyUnderlineSelectedText(AnnotationTool.ImageViewer);
        }

        /// <summary>
        /// "Cut" menu is selected in context menu.
        /// </summary>
        private void cutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CutAction.Execute();
            if (toolsTabControl.SelectedItem == formFieldsTabPage)
                InteractiveFormControl.RefreshInteractiveFormTree();
        }

        /// <summary>
        /// "Copy" menu is selected in context menu.
        /// </summary>
        private void copyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CopyAction.Execute();
        }

        /// <summary>
        /// "Paste" menu is selected in context menu.
        /// </summary>
        private void pasteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PasteAction.Execute();
        }

        /// <summary>
        /// "Delete" menu is selected in context menu.
        /// </summary>
        private void deleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteAction.Execute();
            if (toolsTabControl.SelectedItem == formFieldsTabPage)
                InteractiveFormControl.RefreshInteractiveFormTree();
        }

        /// <summary>
        /// "Bring To Front" menu is selected in context menu.
        /// </summary>
        private void bringToFrontMenuItem_Click(object sender, RoutedEventArgs e)
        {
            BringToFrontAction.Execute();
        }

        /// <summary>
        /// "Bring To Back" menu is selected in context menu.
        /// </summary>
        private void bringToBackMenuItem_Click(object sender, RoutedEventArgs e)
        {
            BringToBackAction.Execute();
        }

        /// <summary>
        /// "Properties" menu is selected in context menu.
        /// </summary>
        private void propertiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WpfPdfAnnotationView view = AnnotationTool.FocusedAnnotationView;

            // if annotation is not focused
            if (view == null)
            {
                view = _hoveredAnnotationView;

                // if there is no hovered annotation
                if (view == null)
                    return;
            }

            // create annotation properties dialog
            PdfAnnotationPropertiesWindow annotationProperties = new PdfAnnotationPropertiesWindow(view);
            annotationProperties.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            annotationProperties.Owner = Window.GetWindow(this);
            annotationProperties.ShowDialog();

            // if read only property is set to true
            if (view.IsReadOnly)
            {
                // reset annotation focus
                AnnotationTool.FocusedAnnotationView = null;
            }

            UpdateAnnotationView(view);
        }

        /// <summary>
        /// "Transformers" menu is opened.
        /// </summary>
        private void transformersMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            MenuItem menu = (MenuItem)sender;
            MenuItem defaultMenuItem = (MenuItem)menu.Items[0];
            MenuItem moveResizeRotateMenuItem = (MenuItem)menu.Items[1];
            MenuItem pointsMoveResizeRotateMenuItem = (MenuItem)menu.Items[2];
            MenuItem distortionMenuItem = (MenuItem)menu.Items[3];
            MenuItem skewMenuItem = (MenuItem)menu.Items[4];
            MenuItem pointsMenuItem = (MenuItem)menu.Items[5];
            MenuItem noneMenuItem = (MenuItem)menu.Items[6];

            WpfPdfAnnotationView focusedView = AnnotationTool.FocusedAnnotationView;
            defaultMenuItem.IsEnabled = false;
            moveResizeRotateMenuItem.IsEnabled = false;
            pointsMoveResizeRotateMenuItem.IsEnabled = false;
            distortionMenuItem.IsEnabled = false;
            skewMenuItem.IsEnabled = false;
            pointsMenuItem.IsEnabled = false;
            noneMenuItem.IsEnabled = false;
            if (focusedView != null)
            {
                defaultMenuItem.IsEnabled = true;
                noneMenuItem.IsEnabled = true;
                if (focusedView is WpfPdfPolygonalAnnotationView ||
                    focusedView is WpfPdfInkAnnotationView)
                {
                    moveResizeRotateMenuItem.IsEnabled = true;
                    pointsMoveResizeRotateMenuItem.IsEnabled = true;
                    distortionMenuItem.IsEnabled = true;
                    skewMenuItem.IsEnabled = true;
                    pointsMenuItem.IsEnabled = true;
                }
                else if (focusedView is WpfPdfLineAnnotationView)
                {
                    moveResizeRotateMenuItem.IsEnabled = true;
                    pointsMoveResizeRotateMenuItem.IsEnabled = true;
                    pointsMenuItem.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// "Transformers -> Default" menu is selected in context menu.
        /// </summary>
        private void defaultMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AnnotationTool.FocusedAnnotationView.InteractionController =
                AnnotationTool.FocusedAnnotationView.Transformer;
        }

        /// <summary>
        /// "Transformers -> Move / Resize / Rotate" menu is selected in context menu.
        /// </summary>
        private void moveResizeRotateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AnnotationTool.FocusedAnnotationView.InteractionController =
                new WpfPointBasedObjectRectangularTransformer(AnnotationTool.FocusedAnnotationView);
        }

        /// <summary>
        /// "Transformers -> Points / Move / Resize / Rotate" menu is selected in context menu.
        /// </summary>
        private void pointsMoveResizeRotateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AnnotationTool.FocusedAnnotationView.InteractionController =
                new WpfCompositeInteractionController(
                    new WpfPointBasedObjectPointTransformer(AnnotationTool.FocusedAnnotationView),
                    new WpfPointBasedObjectRectangularTransformer(AnnotationTool.FocusedAnnotationView));
        }

        /// <summary>
        /// "Transformers -> Distortion" menu is selected in context menu.
        /// </summary>
        private void distortionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AnnotationTool.FocusedAnnotationView.InteractionController =
                new WpfPointBasedObjectDistortionTransformer(AnnotationTool.FocusedAnnotationView, false);
        }

        /// <summary>
        /// "Transformers -> Skew" menu is selected in context menu.
        /// </summary>
        private void skewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AnnotationTool.FocusedAnnotationView.InteractionController =
                new WpfPointBasedObjectDistortionTransformer(AnnotationTool.FocusedAnnotationView, true);
        }

        /// <summary>
        /// "Transformers -> Points" menu is selected in context menu.
        /// </summary>
        private void pointsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AnnotationTool.FocusedAnnotationView.InteractionController =
                new WpfPointBasedObjectPointTransformer(AnnotationTool.FocusedAnnotationView);
        }

        /// <summary>
        /// "Transformers -> None" menu is selected in context menu.
        /// </summary>
        private void noneMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AnnotationTool.FocusedAnnotationView.InteractionController = null;
        }

        #endregion


        #region PDF annotation tool

        /// <summary>
        /// Subscribes to the PDF annotation tool events.
        /// </summary>
        private void SubscribeToPdfAnnotationToolEvents()
        {
            _annotationTool.InteractionModeChanged += new PropertyChangedEventHandler<WpfPdfAnnotationInteractionMode>(pdfAnnotationTool_InteractionModeChanged);
            _annotationTool.MouseDoubleClick += new MouseButtonEventHandler(pdfAnnotationTool_MouseDoubleClick);
            _annotationTool.MouseDown += new MouseButtonEventHandler(pdfAnnotationTool_MouseDown);
            _annotationTool.AnnotationViewCollectionChanged += new CollectionChangeEventHandler<WpfPdfAnnotationView>(pdfAnnotationTool_AnnotationViewCollectionChanged);
            _annotationTool.HoveredAnnotationChanged += new EventHandler<PdfAnnotationEventArgs>(pdfAnnotationTool_HoveredAnnotationChanged);
        }

        /// <summary>
        /// Unsubscribes from the PDF annotation tool events.
        /// </summary>
        private void UnsubscribeFromPdfAnnotationToolEvents()
        {
            _annotationTool.InteractionModeChanged -= new PropertyChangedEventHandler<WpfPdfAnnotationInteractionMode>(pdfAnnotationTool_InteractionModeChanged);
            _annotationTool.MouseDoubleClick -= new MouseButtonEventHandler(pdfAnnotationTool_MouseDoubleClick);
            _annotationTool.MouseDown -= new MouseButtonEventHandler(pdfAnnotationTool_MouseDown);
            _annotationTool.AnnotationViewCollectionChanged -= new CollectionChangeEventHandler<WpfPdfAnnotationView>(pdfAnnotationTool_AnnotationViewCollectionChanged);
            _annotationTool.HoveredAnnotationChanged -= new EventHandler<PdfAnnotationEventArgs>(pdfAnnotationTool_HoveredAnnotationChanged);
        }

        /// <summary>
        /// Interaction mode of PDF annotation tool is changed.
        /// </summary>
        private void pdfAnnotationTool_InteractionModeChanged(object sender, PropertyChangedEventArgs<WpfPdfAnnotationInteractionMode> e)
        {
            AnnotationTool.CancelBuilding();

            UpdateInteractionMode();
        }

        /// <summary>
        /// Mouse is double clicked.
        /// </summary>
        private void pdfAnnotationTool_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!e.Handled && e.ChangedButton == MouseButton.Left)
            {
                if (AnnotationTool.InteractionMode != WpfPdfAnnotationInteractionMode.None)
                {
                    // if focused page is exist
                    if (AnnotationTool.FocusedPage != null)
                    {
                        Point position = e.GetPosition(AnnotationTool);
                        // find view of annotation
                        WpfPdfAnnotationView annotationView = AnnotationTool.FindAnnotationView(position);
                        // annotation is selected and is no FreeText annotation then
                        if (annotationView != null && !(annotationView is WpfPdfFreeTextAnnotationView))
                        {
                            // if annotation is not building
                            if (!annotationView.IsBuilding)
                            {
                                if (AnnotationTool.InteractionMode == WpfPdfAnnotationInteractionMode.Edit)
                                {
                                    // create annotation properties dialog
                                    PdfAnnotationPropertiesWindow annotationProperties = new PdfAnnotationPropertiesWindow(
                                        annotationView);
                                    annotationProperties.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                                    annotationProperties.Owner = Window.GetWindow(this);
                                    annotationProperties.ShowDialog();
                                    UpdateAnnotationView(annotationView);
                                    e.Handled = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Mouse is down.
        /// </summary>
        private void pdfAnnotationTool_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // if mouse button is right
            if (e.ChangedButton == MouseButton.Right)
            {
                // if annotation can be changed
                if (AnnotationTool.IsEnabled && AnnotationTool.InteractionMode != WpfPdfAnnotationInteractionMode.None)
                {
                    // if focused page is exist
                    if (AnnotationTool.FocusedPage != null)
                    {
                        AnnotationTool.CancelBuilding();

                        Point position = e.GetPosition(AnnotationTool);
                        // find view of annotation
                        AnnotationTool.FocusedAnnotationView = AnnotationTool.FindAnnotationView(position);

                        if (AnnotationTool.InteractionMode == WpfPdfAnnotationInteractionMode.Edit ||
                            AnnotationTool.FocusedAnnotationView == null ||
                            AnnotationTool.FocusedAnnotationView is WpfPdfMarkupAnnotationView)
                        {
                            // show annotationContextMenuStrip
                            _annotationContextMenu.IsOpen = true;
                        }

                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the HoveredAnnotationChanged event of the PDF AnnotationTool.
        /// </summary>
        private void pdfAnnotationTool_HoveredAnnotationChanged(object sender, PdfAnnotationEventArgs e)
        {
            if (_hoveredAnnotationToolTip != null)
            {
                if (e.Annotation == null)
                {
                    _hoveredAnnotationToolTip.Content = null;
                    _hoveredAnnotationToolTip.Visibility = Visibility.Hidden;
                }
                else
                {
                    string text = "";
                    if (toolTipCheckBox.IsChecked.Value == true)
                    {
                        switch (AnnotationTool.InteractionMode)
                        {
                            case WpfPdfAnnotationInteractionMode.Edit:
                                if (e.Annotation is PdfWidgetAnnotation)
                                {
                                    PdfInteractiveFormField field = ((PdfWidgetAnnotation)e.Annotation).Field;
                                    string name = field.FullyQualifiedName;
                                    if (field is PdfInteractiveFormSwitchableButtonField)
                                        if (string.IsNullOrEmpty(field.PartialName))
                                            name += string.Format(".{0}", ((PdfInteractiveFormSwitchableButtonField)field).ButtonValue);
                                    text = string.Format("{0} ({1})", name, field.GetType().Name);
                                }
                                else
                                {
                                    PdfAnnotation annotation = e.Annotation;
                                    if (annotation.Name == null)
                                        text = annotation.GetType().Name;
                                    else
                                        text = string.Format("{0} ({1})", annotation.Name, annotation.GetType().Name);
                                }
                                string activateAction = PdfActionsTools.GetActivateActionDescription(e.Annotation);
                                if (activateAction != "")
                                    text += string.Format(": {0}", activateAction);
                                break;
                            case WpfPdfAnnotationInteractionMode.Markup:
                            case WpfPdfAnnotationInteractionMode.View:
                                if (e.Annotation is PdfWidgetAnnotation)
                                {
                                    PdfInteractiveFormField field = ((PdfWidgetAnnotation)e.Annotation).Field;
                                    text = field.UserInterfaceName;
                                }
                                else if (e.Annotation is PdfMarkupAnnotation)
                                {
                                    PdfAnnotation annotation = e.Annotation;
                                    if (annotation is PdfFileAttachmentAnnotation)
                                    {
                                        text = ((PdfFileAttachmentAnnotation)annotation).Contents;
                                        if (text == null && ((PdfFileAttachmentAnnotation)annotation).FileSpecification != null)
                                            text = ((PdfFileAttachmentAnnotation)annotation).FileSpecification.Description;
                                    }
                                    else
                                    {
                                        if (annotation.Contents != null)
                                            text = annotation.Contents;
                                    }

                                }
                                else if (e.Annotation is PdfLinkAnnotation)
                                {
                                    text = PdfActionsTools.GetActivateActionDescription(e.Annotation);
                                }
                                break;
                        }
                    }

                    if ((_hoveredAnnotationToolTip.Content as string) != text)
                    {
                        if (string.IsNullOrEmpty(text))
                        {
                            _hoveredAnnotationToolTip.Content = null;
                            _hoveredAnnotationToolTip.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            _hoveredAnnotationToolTip.Content = text;
                            //_hoveredAnnotationToolTip.IsOpen = true;
                            _hoveredAnnotationToolTip.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Annotation collection is changed.
        /// </summary>
        private void pdfAnnotationTool_AnnotationViewCollectionChanged(
            object sender,
            CollectionChangeEventArgs<WpfPdfAnnotationView> e)
        {
            if (e.NewValue != null &&
              (e.Action == CollectionChangeActionType.SetItem ||
               e.Action == CollectionChangeActionType.InsertItem))
            {
                ToolTipService.SetToolTip(e.NewValue, _hoveredAnnotationToolTip);
            }
        }

        #endregion


        /// <summary>
        /// Updates information about interaction mode in application UI.
        /// </summary>
        private void UpdateInteractionMode()
        {
            if (AnnotationTool != null)
            {
                switch (AnnotationTool.InteractionMode)
                {
                    case WpfPdfAnnotationInteractionMode.View:
                        interactionModeViewRadioButton.IsChecked = true;
                        break;
                    case WpfPdfAnnotationInteractionMode.Markup:
                        interactionModeMarkupRadioButton.IsChecked = true;
                        break;
                    case WpfPdfAnnotationInteractionMode.Edit:
                        interactionModeEditRadioButton.IsChecked = true;
                        break;
                    case WpfPdfAnnotationInteractionMode.None:
                        interactionModeNoneRadioButton.IsChecked = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Updates the view of PDF annotation.
        /// </summary>
        /// <param name="view">The view of PDF annotation.</param>
        private void UpdateAnnotationView(WpfPdfAnnotationView view)
        {
            if (toolsTabControl.SelectedItem == annotationsTabPage)
            {
                AnnotationsControl.UpdateAnnotation(view.Annotation);
            }
            else
            {
                if (view.Figure is PdfWidgetAnnotationGraphicsFigure)
                {
                    PdfInteractiveFormField field = ((PdfWidgetAnnotationGraphicsFigure)view.Figure).Field;
                    InteractiveFormControl.UpdateField(field);
                }
            }
        }

        /// <summary>
        /// Handles the IsEnabledChanged event of UserControl object.
        /// </summary>
        private void UserControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateUI();
        }


        #endregion

        #endregion

    }
}
