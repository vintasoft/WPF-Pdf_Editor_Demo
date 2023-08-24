using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf.Drawing.GraphicsFigures;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;
using Vintasoft.Imaging.Pdf.Wpf.UI.Annotations;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit the PDF interactive form of PDF document.
    /// </summary>
    public partial class PdfInteractiveFormControl : UserControl
    {

        #region Fields

        /// <summary>
        /// An array of copied PDF interactive form fields.
        /// </summary>
        PdfInteractiveFormField[] _fieldClipboard;

        /// <summary>
        /// Indicates that CUT operation is active.
        /// </summary>
        bool _isCutOperationActive = false;

        /// <summary>
        /// Dictionary: the menu item => interactive form field type.
        /// </summary>
        Dictionary<MenuItem, InteractiveFormFieldType> _menuItemToInteractiveFormFieldType =
            new Dictionary<MenuItem, InteractiveFormFieldType>();


        #region Context Menu

        /// <summary>
        /// The "Cut" context menu item.
        /// </summary>
        MenuItem _cutFieldMenuItem;

        /// <summary>
        /// The "Copy" context menu item.
        /// </summary>
        MenuItem _copyFieldMenuItem;

        /// <summary>
        /// The "Paste" context menu item.
        /// </summary>
        MenuItem _pasteAnnotationOrFieldMenuItem;

        /// <summary>
        /// The "Delete" context menu item.
        /// </summary>
        MenuItem _deleteFieldMenuItem;

        /// <summary>
        /// The "Add Text Field" context menu item.
        /// </summary>
        MenuItem _addTextFieldMenuItem;

        /// <summary>
        /// The "Add Check Box" context menu item.
        /// </summary>
        MenuItem _addCheckBoxMenuItem;

        /// <summary>
        /// The "Add Button" context menu item.
        /// </summary>
        MenuItem _addButtonMenuItem;

        /// <summary>
        /// The "Add List Box" context menu item.
        /// </summary>
        MenuItem _addListBoxMenuItem;

        /// <summary>
        /// The "Add Combo Box" context menu item.
        /// </summary>
        MenuItem _addComboBoxMenuItem;

        /// <summary>
        /// The "Add Radio Button" context menu item.
        /// </summary>
        MenuItem _addRadioButtonMenuItem;

        /// <summary>
        /// The "Add Barcode" context menu item.
        /// </summary>
        MenuItem _addBarcodeMenuItem;

        /// <summary>
        /// The "Add Digital Signature" context menu item.
        /// </summary>
        MenuItem _addDigitalSignatureMenuItem;

        /// <summary>
        /// The "Properties..." context menu item.
        /// </summary>
        MenuItem _propertiesMenuItem;

        #endregion

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfInteractiveFormControl"/> class.
        /// </summary>
        public PdfInteractiveFormControl()
        {
            InitializeComponent();

            ContextMenu contextMenu = (ContextMenu)Resources["ContextMenu"];
            InitializeInteractiveFormFieldButtons(contextMenu);

            interactiveFormFieldTree.ContextMenu = contextMenu;
        }

        #endregion



        #region Properties

        WpfPdfAnnotationTool _annotationTool;
        /// <summary>
        /// Gets or sets the PDF annotation tool.
        /// </summary>
        [DefaultValue((object)null)]
        public WpfPdfAnnotationTool AnnotationTool
        {
            get
            {
                return _annotationTool;
            }
            set
            {
                if (_annotationTool != value)
                {
                    if (_annotationTool != null)
                        UnsctibeFromPdfAnnotationToolEvents(_annotationTool);
                    _annotationTool = value;
                    if (_annotationTool != null)
                    {
                        SubsctibeToPdfAnnotationToolEvents(_annotationTool);
                        mainPanel.IsEnabled = _annotationTool.ImageViewer != null;
                    }
                    InteractiveFormFieldBuilder.AnnotationTool = value;
                    interactiveFormFieldTree.AnnotationTool = value;
                    UpdateUI();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the control can delete field.
        /// </summary>
        public bool CanDeleteField
        {
            get
            {
                return AnnotationTool != null && interactiveFormFieldTree.SelectedField != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the control can copy field.
        /// </summary>
        public bool CanCopyField
        {
            get
            {
                return AnnotationTool != null && interactiveFormFieldTree.SelectedField != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the control can paste field.
        /// </summary>
        public bool CanPasteField
        {
            get
            {
                return AnnotationTool.FocusedPage != null && AnnotationTool.FocusedPage != null && _fieldClipboard != null;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Returns the appearance generator for specified annotation view.
        /// </summary>
        /// <param name="view">The annotation view.</param>
        /// <returns>The appearance generator.</returns>
        public PdfAnnotationAppearanceGenerator GetAppearanceGenerator(WpfPdfAnnotationView view)
        {
            return InteractiveFormFieldBuilder.AnnotationAppearanceGeneratorController.GetAppearanceGenerator(view);
        }

        /// <summary>
        /// Copies the field that is selected in interactive form field tree.
        /// </summary>
        public void CopyField()
        {
            try
            {
                _isCutOperationActive = false;
                if (CanCopyField)
                {
                    _fieldClipboard = new PdfInteractiveFormField[] { interactiveFormFieldTree.SelectedField };
                    AnnotationTool.Clipboard.Clear();
                }
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Cuts the field that is selected in interactive form field tree.
        /// </summary>
        public void CutField()
        {
            CopyField();
            _isCutOperationActive = true;
            DeleteField();
        }

        /// <summary>
        /// Pastes the field into the field, which is selected in interactive form field tree.
        /// </summary>
        public void PasteField()
        {
            try
            {
                if (CanPasteField)
                {
                    for (int i = 0; i < _fieldClipboard.Length; i++)
                    {
                        PdfInteractiveFormFieldList targetList = GetFieldList(AnnotationTool.FocusedField, _fieldClipboard[i]);
                        if (!_isCutOperationActive || _fieldClipboard[i].Document != targetList.Document)
                        {
                            _fieldClipboard[i] = AnnotationTool.InteractiveFormEditorManager.AddCopy(
                                targetList, AnnotationTool.AnnotationCollection, _fieldClipboard[i], 10, -10);
                        }
                        else
                        {
                            AnnotationTool.InteractiveFormEditorManager.Add(
                                targetList, AnnotationTool.AnnotationCollection, _fieldClipboard[i], 10, -10);
                        }
                        if (AnnotationTool.AllowMultipleSelection)
                        {
                            AnnotationTool.SelectedAnnotationViewCollection.Clear();
                            foreach (PdfInteractiveFormField field in _fieldClipboard)
                                AddToSelection(field.GetAnnotations());
                        }
                        PdfAnnotation[] annotations = _fieldClipboard[0].GetAnnotations();
                        if (annotations != null && annotations.Length > 0)
                            AnnotationTool.FocusedAnnotationView = AnnotationTool.GetAnnotationViewAssociatedWith(annotations[0]);
                    }
                }
                _isCutOperationActive = false;
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Deletes the field that is selected in interactive form field tree.
        /// </summary>
        public void DeleteField()
        {
            try
            {
                if (interactiveFormFieldTree.SelectedField != null)
                    AnnotationTool.InteractiveFormEditorManager.Remove(interactiveFormFieldTree.SelectedField);
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Updates the User Interface.
        /// </summary>
        public void UpdateUI()
        {
            if (_annotationTool != null)
            {
                mainPanel.IsEnabled = true;
                showFieldNamesCheckBox.IsChecked = _annotationTool.ShowFieldNames;
            }
            else
            {
                mainPanel.IsEnabled = false;
            }

            InteractiveFormFieldBuilder.UpdateUI();
        }

        /// <summary>
        /// Updates the field.
        /// </summary>
        /// <param name="field">The field to update.</param>
        public void UpdateField(PdfInteractiveFormField field)
        {
            interactiveFormFieldTree.UpdateField(field);
        }

        /// <summary>
        /// Refreshes the interactive form tree.
        /// </summary>
        public void RefreshInteractiveFormTree()
        {
            interactiveFormFieldTree.RefreshInteractiveFormTreeSafely();
        }

        #endregion


        #region PRIVATE

        #region Checkboxes

        /// <summary>
        /// "Show field names" checkbox is checked/unchecked.
        /// </summary>
        private void showFieldNamesCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            AnnotationTool.ShowFieldNames = showFieldNamesCheckBox.IsChecked.Value;
        }

        /// <summary>
        /// "Group Form Fields by Page" checkbox is checked/unchecked.
        /// </summary>
        private void groupFormFieldsByPagesCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            interactiveFormFieldTree.GroupFormFieldsByPages = groupFormFieldsByPagesCheckBox.IsChecked.Value;
        }


        #endregion


        #region PDF interactive form fField tree

        /// <summary>
        /// Mouse is double clicked in InteractiveFormFieldTree.
        /// </summary>
        private void interactiveFormFieldTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // if annotation is focused
            if (AnnotationTool.FocusedAnnotationView != null)
            {
                WpfPdfAnnotationView view = AnnotationTool.FocusedAnnotationView;
                // create annotation properties dialog
                PdfAnnotationPropertiesWindow annotationProperties = new PdfAnnotationPropertiesWindow(view);
                annotationProperties.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                annotationProperties.Owner = Window.GetWindow(this);
                annotationProperties.ShowDialog();
                if (view.Figure is PdfWidgetAnnotationGraphicsFigure)
                    interactiveFormFieldTree.UpdateField(((PdfWidgetAnnotationGraphicsFigure)view.Figure).Field);
            }
        }

        #endregion


        #region Context menu

        /// <summary>
        /// The FieldContextMenuStrip is opening.
        /// </summary>
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            _cutFieldMenuItem.IsEnabled = CanCopyField;
            _copyFieldMenuItem.IsEnabled = CanCopyField;
            _pasteAnnotationOrFieldMenuItem.IsEnabled = CanPasteField;
            _deleteFieldMenuItem.IsEnabled = CanDeleteField;
            _propertiesMenuItem.IsEnabled = true;
        }


        /// <summary>
        /// "Cut" menu in FieldContextMenuStrip is clicked.
        /// </summary>
        private void cutFieldMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CutField();
        }

        /// <summary>
        /// "Copy" menu in FieldContextMenuStrip is clicked.
        /// </summary>
        private void copyFieldMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CopyField();
        }

        /// <summary>
        /// "Delete" menu in FieldContextMenuStrip is clicked.
        /// </summary>
        private void deleteFieldMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteField();
        }

        /// <summary>
        /// "Paste" menu in FieldContextMenuStrip is clicked.
        /// </summary>
        private void pasteAnnotationOrFieldMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PasteField();
        }


        /// <summary>
        /// Adds an interactive form field to the AnnotationTool.
        /// </summary>
        private void addInteractiveFormFieldToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            InteractiveFormFieldType interactiveFormFieldType =
                _menuItemToInteractiveFormFieldType[menuItem];

            PdfInteractiveFormField parentField = GetFocusedNonTerminalField();

            InteractiveFormFieldBuilder.AddAndBuildInteractiveFormField(
                interactiveFormFieldType, parentField);
        }


        /// <summary>
        /// "Properties" menu in FieldContextMenuStrip is clicked.
        /// </summary>
        private void propertiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfAnnotationPropertiesWindow annotationProperties = null;
            // get selected field
            PdfInteractiveFormField field = interactiveFormFieldTree.SelectedField;
            // if selected field exists
            if (field != null)
            {
                WpfPdfAnnotationView view = null;

                // if field has annotation
                if (field.Annotation != null)
                {
                    // find field view
                    foreach (WpfPdfAnnotationView annoView in AnnotationTool.AnnotationViewCollection)
                    {
                        if (annoView.Annotation == field.Annotation)
                        {
                            view = annoView;
                            break;
                        }
                    }
                }

                // if field annotation exists
                if (view != null)
                    annotationProperties = new PdfAnnotationPropertiesWindow(view);
                else
                    annotationProperties = new PdfAnnotationPropertiesWindow(field);
            }

            if (annotationProperties != null)
            {
                annotationProperties.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                annotationProperties.Owner = Window.GetWindow(this);
                annotationProperties.ShowDialog();
                interactiveFormFieldTree.UpdateField(field);
            }
        }


        /// <summary>
        /// Initializes the interactive form field buttons.
        /// </summary>
        /// <param name="contextMenu">The context menu.</param>
        private void InitializeInteractiveFormFieldButtons(ContextMenu contextMenu)
        {
            _cutFieldMenuItem = FindMenuItemByName(contextMenu, "cutFieldMenuItem");
            _copyFieldMenuItem = FindMenuItemByName(contextMenu, "copyFieldMenuItem");
            _pasteAnnotationOrFieldMenuItem = FindMenuItemByName(contextMenu, "pasteAnnotationOrFieldMenuItem");
            _deleteFieldMenuItem = FindMenuItemByName(contextMenu, "deleteFieldMenuItem");


            _addTextFieldMenuItem = FindMenuItemByName(contextMenu, "addTextFieldMenuItem");
            _menuItemToInteractiveFormFieldType.Add(
                _addTextFieldMenuItem, InteractiveFormFieldType.TextField);

            _addCheckBoxMenuItem = FindMenuItemByName(contextMenu, "addCheckBoxMenuItem");
            _menuItemToInteractiveFormFieldType.Add(
                _addCheckBoxMenuItem, InteractiveFormFieldType.CheckBox);

            _addButtonMenuItem = FindMenuItemByName(contextMenu, "addButtonMenuItem");
            _menuItemToInteractiveFormFieldType.Add(
                _addButtonMenuItem, InteractiveFormFieldType.PushButton);

            _addListBoxMenuItem = FindMenuItemByName(contextMenu, "addListBoxMenuItem");
            _menuItemToInteractiveFormFieldType.Add(
                _addListBoxMenuItem, InteractiveFormFieldType.ListBox);

            _addComboBoxMenuItem = FindMenuItemByName(contextMenu, "addComboBoxMenuItem");
            _menuItemToInteractiveFormFieldType.Add(
                _addComboBoxMenuItem, InteractiveFormFieldType.ComboBox);

            _addRadioButtonMenuItem = FindMenuItemByName(contextMenu, "addRadioButtonMenuItem");
            _menuItemToInteractiveFormFieldType.Add(
                _addRadioButtonMenuItem, InteractiveFormFieldType.RadioButton);

            _addBarcodeMenuItem = FindMenuItemByName(contextMenu, "addBarcodeMenuItem");
            _menuItemToInteractiveFormFieldType.Add(
                _addBarcodeMenuItem, InteractiveFormFieldType.BarcodeField);

            _addDigitalSignatureMenuItem = FindMenuItemByName(contextMenu, "addDigitalSignatureMenuItem");
            _menuItemToInteractiveFormFieldType.Add(
                _addDigitalSignatureMenuItem, InteractiveFormFieldType.SignatureField);


            _propertiesMenuItem = FindMenuItemByName(contextMenu, "propertiesMenuItem");
        }

        #endregion


        #region PDF annotation tool

        /// <summary>
        /// Subsctibes to the PDF annotation tool events.
        /// </summary>
        /// <param name="annotationTool">The annotation tool.</param>
        private void SubsctibeToPdfAnnotationToolEvents(WpfPdfAnnotationTool annotationTool)
        {
            annotationTool.FocusedFieldChanged += new PropertyChangedEventHandler<PdfInteractiveFormField>(pdfAnnotationTool_FocusedFieldChanged);
            annotationTool.AnnotationMouseButtonDown += pdfAnnotationTool_MouseDown;
            annotationTool.Activated += new EventHandler(pdfAnnotationTool_Activated);
            annotationTool.Deactivated += new EventHandler(pdfAnnotationTool_Deactivated);
            annotationTool.BuildingStarted += new EventHandler<WpfPdfAnnotationViewEventArgs>(pdfAnnotationTool_BuildingStarted);
            annotationTool.BuildingFinished += new EventHandler<WpfPdfAnnotationViewEventArgs>(pdfAnnotationTool_BuildingFinished);
            annotationTool.BuildingCanceled += new EventHandler<WpfPdfAnnotationViewEventArgs>(pdfAnnotationTool_BuildingCanceled);
        }

        /// <summary>
        /// Unsctibes from the PDF annotation tool events.
        /// </summary>
        /// <param name="annotationTool">The annotation tool.</param>
        private void UnsctibeFromPdfAnnotationToolEvents(WpfPdfAnnotationTool annotationTool)
        {
            annotationTool.FocusedFieldChanged -= new PropertyChangedEventHandler<PdfInteractiveFormField>(pdfAnnotationTool_FocusedFieldChanged);
            annotationTool.AnnotationMouseButtonDown -= pdfAnnotationTool_MouseDown;
            annotationTool.Activated -= new EventHandler(pdfAnnotationTool_Activated);
            annotationTool.Deactivated -= new EventHandler(pdfAnnotationTool_Deactivated);
            annotationTool.BuildingStarted -= pdfAnnotationTool_BuildingStarted;
            annotationTool.BuildingFinished -= pdfAnnotationTool_BuildingFinished;
            annotationTool.BuildingCanceled -= pdfAnnotationTool_BuildingCanceled;
        }

        /// <summary>
        /// PDF annotation tool is activated.
        /// </summary>
        private void pdfAnnotationTool_Activated(object sender, EventArgs e)
        {
            mainPanel.IsEnabled = true;
        }

        /// <summary>
        /// PDF annotation tool is deactivated.
        /// </summary>
        private void pdfAnnotationTool_Deactivated(object sender, EventArgs e)
        {
            mainPanel.IsEnabled = false;
        }

        /// <summary>
        /// Annotation building is started.
        /// </summary>
        private void pdfAnnotationTool_BuildingStarted(object sender, WpfPdfAnnotationViewEventArgs e)
        {
            interactiveFormFieldTree.IsEnabled = false;
        }

        /// <summary>
        /// Annotation building is canceled.
        /// </summary>
        private void pdfAnnotationTool_BuildingCanceled(object sender, WpfPdfAnnotationViewEventArgs e)
        {
            interactiveFormFieldTree.IsEnabled = true;
        }

        /// <summary>
        /// Annotaiton building is finished.
        /// </summary>
        private void pdfAnnotationTool_BuildingFinished(object sender, WpfPdfAnnotationViewEventArgs e)
        {
            if (!AnnotationTool.IsFocusedAnnotationBuilding)
                interactiveFormFieldTree.IsEnabled = true;
        }

        /// <summary>
        /// Mouse is down.
        /// </summary>
        private void pdfAnnotationTool_MouseDown(object sender, WpfPdfAnnotationViewMouseEventArgs e)
        {
            // if left mouse button is down
            if (e.MouseEventArgs.LeftButton == MouseButtonState.Pressed &&
                e.MouseEventArgs.MiddleButton == MouseButtonState.Released &&
                e.MouseEventArgs.RightButton == MouseButtonState.Released)
            {
                // if focused annotation view is Signature Field
                if (e.AnnotationView is WpfPdfSignatureWidgetAnnotationView)
                {
                    InteractiveFormFieldBuilder.ShowSignatureDialog((WpfPdfSignatureWidgetAnnotationView)e.AnnotationView);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Focused field is changed.
        /// </summary>
        private void pdfAnnotationTool_FocusedFieldChanged(
            object sender,
            PropertyChangedEventArgs<PdfInteractiveFormField> e)
        {
            if (e.NewValue != null)
                interactiveFormFieldTree.SelectedField = e.NewValue;
        }

        #endregion


        /// <summary>
        /// Adds annotations to selection.
        /// </summary>
        /// <param name="annotations">The annotations.</param>
        private void AddToSelection(IEnumerable<PdfAnnotation> annotations)
        {
            foreach (PdfAnnotation annotation in annotations)
            {
                WpfPdfAnnotationView view = AnnotationTool.GetAnnotationViewAssociatedWith(annotation);
                if (view != null)
                    AnnotationTool.SelectedAnnotationViewCollection.Add(view);
            }
        }

        /// <summary>
        /// Finds the menu item by name.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="name">The name.</param>
        private MenuItem FindMenuItemByName(ItemsControl control, string name)
        {
            if (control.HasItems)
            {
                foreach (object item in control.Items)
                {
                    MenuItem menuItem = item as MenuItem;
                    if (menuItem != null)
                    {
                        if (String.Equals(menuItem.Name, name, StringComparison.InvariantCultureIgnoreCase))
                            return menuItem;
                    }

                    ItemsControl itemsControl = item as ItemsControl;
                    if (itemsControl != null)
                    {
                        MenuItem result = FindMenuItemByName(itemsControl, name);

                        if (result != null)
                            return result;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the field list for paste operation.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="targetField">The target field.</param>
        /// <returns>The field list for paste operation.</returns>
        private PdfInteractiveFormFieldList GetFieldList(
            PdfInteractiveFormField field,
            PdfInteractiveFormField targetField)
        {
            if (field == null)
                return targetField.Document.InteractiveForm.Fields;
            if (field.Kids != null)
            {
                if (field is PdfInteractiveFormCheckBoxGroupField &&
                    targetField is PdfInteractiveFormCheckBoxField)
                    return field.Kids;
                if (field is PdfInteractiveFormRadioButtonGroupField &&
                    targetField is PdfInteractiveFormRadioButtonField)
                    return field.Kids;
                if (!(field is PdfInteractiveFormSwitchableButtonGroupField))
                    return field.Kids;
            }
            if (field.Parent == null)
                return targetField.Document.InteractiveForm.Fields;
            return GetFieldList(field.Parent, targetField);
        }

        /// <summary>
        /// Returns the focused non-terminal field.
        /// </summary>
        /// <returns>The focused non-terminal field.</returns>
        private PdfInteractiveFormField GetFocusedNonTerminalField()
        {
            PdfInteractiveFormField field = interactiveFormFieldTree.SelectedField;
            if (field != null && !field.IsTerminalField)
                return field;
            return null;
        }

        #endregion

        #endregion

    }
}
