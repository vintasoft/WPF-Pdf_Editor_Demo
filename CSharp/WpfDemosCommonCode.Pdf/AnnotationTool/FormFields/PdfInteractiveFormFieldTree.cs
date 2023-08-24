using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;
using Vintasoft.Imaging.Pdf.Wpf.UI.Annotations;
using Vintasoft.Imaging.Utils;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Represents the tree view of PDF interactive form.
    /// </summary>
    public class PdfInteractiveFormFieldTree : TreeView
    {

        #region Fields

        /// <summary>
        /// The format string for icon resources.
        /// </summary>
        public static readonly string ResourceNameFormatString = "{0}.png";

        /// <summary>
        /// Dictionary: PDF interactive field => image.
        /// </summary>
        Dictionary<PdfInteractiveFormField, VintasoftImage> _fieldToImageTable = new Dictionary<PdfInteractiveFormField, VintasoftImage>();

        /// <summary>
        /// Dictionary: PDF interactive field => TreeNode.
        /// </summary>
        Dictionary<PdfInteractiveFormField, TreeViewItem> _fieldToNodeTable = new Dictionary<PdfInteractiveFormField, TreeViewItem>();

        Dictionary<string, BitmapSource> _imageList;

        bool _isSelectedFieldChanging = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfInteractiveFormFieldTree"/> class.
        /// </summary>
        public PdfInteractiveFormFieldTree()
            : base()
        {
            _imageList = new Dictionary<string, BitmapSource>();
            string[] resourceNames = new string[]
            {
                "InteractiveField",
                "CheckBoxField",
                "BarcodeField",
                "ButtonField",
                "FieldGroup",
                "ListBoxField",
                "RadioButtonField",
                "SignatureField",
                "TextField",
                "ComboBoxField",
                "CheckBoxGroup",
                "RadioButtonGroup",
            };
            for (int i = 0; i < resourceNames.Length; i++)
            {
                BitmapSource resourceBitmap = DemosResourcesManager.GetResourceAsBitmap(
                    string.Format(ResourceNameFormatString, resourceNames[i]));
                _imageList.Add(resourceNames[i], resourceBitmap);
            }
        }

        #endregion



        #region Properties

        PdfDocumentInteractiveForm _interactiveForm;
        /// <summary>
        /// Gets or sets the source PDF interactive form.
        /// </summary>
        public PdfDocumentInteractiveForm InteractiveForm
        {
            get
            {
                if (AnnotationTool != null)
                    return AnnotationTool.FocusedInteractiveForm;
                return _interactiveForm;
            }
            set
            {
                if (AnnotationTool != null)
                    throw new InvalidOperationException();

                if (_interactiveForm != null)
                    _interactiveForm.FieldTreeChanged -= new CollectionChangeEventHandler<PdfInteractiveFormField>(InteractiveForm_FieldTreeChanged);

                _interactiveForm = value;

                if (_interactiveForm != null)
                    _interactiveForm.FieldTreeChanged += new CollectionChangeEventHandler<PdfInteractiveFormField>(InteractiveForm_FieldTreeChanged);
            }
        }

        WpfPdfAnnotationTool _annotationTool;
        /// <summary>
        /// Gets or sets the annotation tool that is used as a source of PDF interactive forms.
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
                _interactiveForm = null;

                if (_annotationTool != null)
                {
                    foreach (PdfDocument document in _annotationTool.DocumentSet)
                    {
                        if (document.InteractiveForm != null)
                            UnsubscribeFromInteractiveFormEvents(document.InteractiveForm);
                    }
                    _annotationTool.DocumentSet.Changed -= new EventHandler<ObjectSetListenerEventArgs<PdfDocument>>(DocumentSet_Changed);
                }

                _annotationTool = value;

                if (_annotationTool != null)
                {
                    _annotationTool.DocumentSet.Changed += new EventHandler<ObjectSetListenerEventArgs<PdfDocument>>(DocumentSet_Changed);
                    foreach (PdfDocument document in _annotationTool.DocumentSet)
                    {
                        if (document.InteractiveForm != null)
                            SubscribeToInteractiveFormEvents(document.InteractiveForm);
                    }
                }
            }
        }

        bool _groupFormFieldsByPages = false;
        /// <summary>
        /// Gets or sets a value indicating whether tree view must show the form fields grouped by pages.
        /// </summary>
        public bool GroupFormFieldsByPages
        {
            get
            {
                return _groupFormFieldsByPages;
            }
            set
            {
                if (_groupFormFieldsByPages != value)
                {
                    _groupFormFieldsByPages = value;
                    RefreshInteractiveFormTreeSafely();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected field.
        /// </summary>
        public PdfInteractiveFormField SelectedField
        {
            get
            {
                TreeViewItem selectedItem = SelectedItem as TreeViewItem;
                if (selectedItem != null && selectedItem.Tag is PdfInteractiveFormField)
                    return (PdfInteractiveFormField)selectedItem.Tag;
                return null;
            }
            set
            {
                _isSelectedFieldChanging = true;
                try
                {
                    if (value == null)
                    {
                        TreeViewItem selectedItem = SelectedItem as TreeViewItem;
                        if (selectedItem != null)
                            selectedItem.IsSelected = false;
                    }
                    else
                    {
                        if (!_fieldToNodeTable.ContainsKey(value))
                            RefreshInteractiveFormTreeSafely();
                        if (_fieldToNodeTable.ContainsKey(value))
                        {
                            _fieldToNodeTable[value].IsSelected = true;
                        }
                        else
                        {
                            TreeViewItem selectedItem = SelectedItem as TreeViewItem;
                            if (selectedItem != null)
                                selectedItem.IsSelected = false;
                        }
                    }
                }
                finally
                {
                    _isSelectedFieldChanging = false;
                }
            }
        }

        bool _showOnlyExportableFields = false;
        /// <summary>
        /// Gets or sets a value indicating whether the tree view must show only form fields
        /// that can export their values.
        /// </summary>
        public bool ShowOnlyExportableFields
        {
            get
            {
                return _showOnlyExportableFields;
            }
            set
            {
                _showOnlyExportableFields = value;
            }
        }

        bool _showOnlyResettableFields = false;
        /// <summary>
        /// Gets or sets a value indicating whether the tree view must show only form fields
        /// that can reset their values.
        /// </summary>
        public bool ShowOnlyResettableFields
        {
            get
            {
                return _showOnlyResettableFields;
            }
            set
            {
                _showOnlyResettableFields = value;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Refreshes the interactive form tree.
        /// </summary>
        public void RefreshInteractiveFormTreeSafely()
        {
            if (Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                RefreshInteractiveFormTree();
            }
            else
            {
                Dispatcher.Invoke(new RefreshInteractiveFormTreeDelegate(RefreshInteractiveFormTree));
            }
        }

        /// <summary>
        /// Refreshes the interactive form tree.
        /// </summary>
        public void RefreshInteractiveFormTree()
        {
            _isSelectedFieldChanging = true;
            BeginInit();
            try
            {
                Items.Clear();
                _fieldToNodeTable.Clear();
                _fieldToImageTable.Clear();
                if (AnnotationTool != null && AnnotationTool.ImageViewer != null)
                {
                    ImageCollection images = AnnotationTool.ImageViewer.Images;
                    for (int i = 0; i < images.Count; i++)
                    {
                        PdfPage page = PdfDocumentController.GetPageAssociatedWithImage(images[i]);
                        if (page != null)
                        {
                            AddFields(images[i], page, string.Format("Page {0}", i + 1));
                        }
                    }
                }
                else if (InteractiveForm != null)
                {
                    PdfDocument document = InteractiveForm.Document;
                    for (int i = 0; i < document.Pages.Count; i++)
                        AddFields(null, document.Pages[i], string.Format("Page {0}", i + 1));
                }

            }
            finally
            {
                EndInit();
                _isSelectedFieldChanging = false;
            }
        }

        /// <summary>
        /// Updates name of the field. 
        /// </summary>
        /// <param name="field">The field.</param>
        public void UpdateField(PdfInteractiveFormField field)
        {
            TreeViewItem node = null;

            if (_fieldToNodeTable.TryGetValue(field, out node))
            {
                StackPanel stackPanel = (StackPanel)node.Header;
                TextBlock textBlock = (TextBlock)stackPanel.Children[1];
                textBlock.Text = GetFieldName(field);
            }
        }

        #endregion


        #region PROTECTED

        /// <summary>
        /// Disposes the PdfInteractiveFormFieldTree.
        /// </summary>
        ~PdfInteractiveFormFieldTree()
        {
            if (_interactiveForm != null)
                _interactiveForm.FieldTreeChanged -= new CollectionChangeEventHandler<PdfInteractiveFormField>(InteractiveForm_FieldTreeChanged);
            else if (_annotationTool != null)
            {
                foreach (PdfDocument document in _annotationTool.DocumentSet)
                {
                    if (document.InteractiveForm != null)
                        UnsubscribeFromInteractiveFormEvents(document.InteractiveForm);
                }
                _annotationTool.DocumentSet.Changed -= new EventHandler<ObjectSetListenerEventArgs<PdfDocument>>(DocumentSet_Changed);
            }
        }


        /// <summary>
        /// Raises the <see cref="E:SelectedItemChanged" /> event.
        /// </summary>
        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            if (_isSelectedFieldChanging)
                return;

            TreeViewItem newValue = e.NewValue as TreeViewItem;
            if (newValue != null)
            {

                PdfInteractiveFormField field = newValue.Tag as PdfInteractiveFormField;
                if (field != null && AnnotationTool != null && AnnotationTool.AllowMultipleSelection)
                {
                    SetFocusedFieldInAnnotationTool(field);
                    AnnotationTool.PerformSelection(AnnotationTool.GetAnnotationViewsAssociatedWith(field));
                }
            }

            if (IsEnabled)
            {
                base.OnSelectedItemChanged(e);

                SetFocusedFieldInAnnotationTool(SelectedField);
            }
        }

        #endregion


        #region INTERNAL

        /// <summary>
        /// Returns the image key, which is associated with the specified field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>The image key, which is associated with the specified field.</returns>
        internal static string GetImageKey(PdfInteractiveFormField field)
        {
            if (field.IsTerminalField)
            {
                if (field is PdfInteractiveFormCheckBoxField)
                    return "CheckBoxField";
                if (field is PdfInteractiveFormBarcodeField)
                    return "BarcodeField";
                if (field is PdfInteractiveFormPushButtonField)
                    return "ButtonField";
                if (field is PdfInteractiveFormListBoxField)
                    return "ListBoxField";
                if (field is PdfInteractiveFormRadioButtonField)
                    return "RadioButtonField";
                if (field is PdfInteractiveFormSignatureField)
                    return "SignatureField";
                if (field is PdfInteractiveFormTextField)
                    return "TextField";
                if (field is PdfInteractiveFormComboBoxField)
                    return "ComboBoxField";
                return "InteractiveField";
            }
            if (field is PdfInteractiveFormCheckBoxGroupField)
                return "CheckBoxGroup";
            if (field is PdfInteractiveFormRadioButtonGroupField)
                return "RadioButtonGroup";
            return "FieldGroup";
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Sets the focused field in annotation tool.
        /// </summary>
        /// <param name="field">The field.</param>
        private void SetFocusedFieldInAnnotationTool(PdfInteractiveFormField field)
        {
            if (AnnotationTool != null && field != null)
            {
                if (!field.IsTerminalField)
                {
                    PdfAnnotation[] annotations = field.GetAnnotations();
                    if (annotations.Length > 0)
                        field = ((PdfWidgetAnnotation)annotations[0]).Field;
                    else
                        return;
                }
                if (_fieldToImageTable.ContainsKey(field))
                {
                    if (AnnotationTool.ImageViewer.Image != _fieldToImageTable[field])
                    {
                        int index = AnnotationTool.ImageViewer.Images.IndexOf(_fieldToImageTable[field]);
                        if (index < 0)
                            return;
                        AnnotationTool.ImageViewer.FocusedIndex = index;
                    }
                    if (AnnotationTool.FocusedField != field)
                    {
                        AnnotationTool.FocusedField = field;
                        AnnotationTool.ScrollToFocusedItem();
                    }
                }
                else
                {
                    AnnotationTool.FocusedField = field;
                }
            }
        }

        /// <summary>
        /// Handles the Changed event of the DocumentSet object.
        /// </summary>
        private void DocumentSet_Changed(
            object sender,
            ObjectSetListenerEventArgs<PdfDocument> e)
        {
            RefreshInteractiveFormTreeSafely();
            foreach (PdfDocument document in e.RemovedElements)
                if (document.InteractiveForm != null)
                    UnsubscribeFromInteractiveFormEvents(document.InteractiveForm);
            foreach (PdfDocument document in e.NewElements)
                if (document.InteractiveForm != null)
                    SubscribeToInteractiveFormEvents(document.InteractiveForm);
        }

        /// <summary>
        /// Adds the fields that locates ons specified image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="name">The image name.</param>
        private void AddFields(VintasoftImage image, PdfPage page, string name)
        {
            if (page.Document.InteractiveForm != null)
            {
                ItemCollection rootCollection = Items;
                if (_groupFormFieldsByPages)
                {
                    TreeViewItem pageNode = new TreeViewItem();
                    pageNode.Header = GetItemHeader("FieldGroup", name);
                    Items.Add(pageNode);
                    rootCollection = pageNode.Items;
                }
                PdfInteractiveFormField[] fields = page.Document.InteractiveForm.GetFieldsLocatedOnPage(page);
                foreach (PdfInteractiveFormField field in fields)
                {
                    if (image != null)
                        _fieldToImageTable[field] = image;
                    AddField(rootCollection, field);
                }
            }
        }

        /// <summary>
        /// Adds the field to tree view.
        /// </summary>
        /// <param name="rootCollection">The root collection.</param>
        /// <param name="field">The field.</param>
        private TreeViewItem AddField(ItemCollection rootCollection, PdfInteractiveFormField field)
        {
            if (_fieldToNodeTable.ContainsKey(field))
                return _fieldToNodeTable[field];

            ItemCollection parentNodes;
            string name = GetFieldName(field);
            if (_groupFormFieldsByPages)
            {
                if (!field.IsTerminalField && !(field is PdfInteractiveFormSwitchableButtonGroupField))
                    return null;

                parentNodes = rootCollection;
                if (field is PdfInteractiveFormSwitchableButtonField && field.Parent != null)
                {
                    TreeViewItem parentNode = AddField(rootCollection, field.Parent);
                    if (parentNode != null)
                        parentNodes = parentNode.Items;
                }

                if (SuppressFieldAddition(field))
                    return null;
            }
            else
            {
                if (field.Parent != null)
                {
                    TreeViewItem parentNode = AddField(rootCollection, field.Parent);
                    if (parentNode == null)
                        return null;
                    parentNodes = parentNode.Items;
                }
                else
                {
                    parentNodes = rootCollection;
                }

                if (SuppressFieldAddition(field))
                    return null;
            }

            TreeViewItem node = new TreeViewItem();
            node.Header = GetItemHeader(GetImageKey(field), name);
            node.Tag = field;
            _fieldToNodeTable[field] = node;
            parentNodes.Add(node);
            return node;
        }

        /// <summary>
        /// Returns the name of the field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>The name of the field.</returns>
        private string GetFieldName(PdfInteractiveFormField field)
        {
            string name = string.Empty;
            if (_groupFormFieldsByPages)
            {
                if (field.PartialName == "" && field is PdfInteractiveFormSwitchableButtonField)
                    name = ((PdfInteractiveFormSwitchableButtonField)field).ButtonValue;
                else
                    name = field.FullyQualifiedName;
            }
            else
            {
                name = field.PartialName;
                if (name == "")
                {
                    if (field is PdfInteractiveFormSwitchableButtonField)
                        name = ((PdfInteractiveFormSwitchableButtonField)field).ButtonValue;
                }
            }

            return name;
        }

        /// <summary>
        /// Determines that the field addition must be suppressed.
        /// </summary>
        /// <param name="field">The field.</param>
        private bool SuppressFieldAddition(PdfInteractiveFormField field)
        {
            if (ShowOnlyExportableFields)
            {
                if (IsElementOfSwitchableGroup(field))
                    return true;
                if (field.IsNoExport)
                    return true;
            }

            if (ShowOnlyResettableFields)
            {
                if (IsElementOfSwitchableGroup(field))
                    return true;
                if (field.GetType() == typeof(PdfInteractiveFormPushButtonField))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines that the specified field is an element of a switchable group,
        /// e.g., a single radio button in a group of radio buttons.
        /// </summary>
        /// <param name="field">The field.</param>
        private bool IsElementOfSwitchableGroup(PdfInteractiveFormField field)
        {
            return
                field is PdfInteractiveFormSwitchableButtonField &&
                field.Parent is PdfInteractiveFormSwitchableButtonGroupField;
        }

        /// <summary>
        /// Handles the FieldTreeChanged event of the InteractiveForm.
        /// </summary>
        private void InteractiveForm_FieldTreeChanged(object sender, CollectionChangeEventArgs<PdfInteractiveFormField> e)
        {
            RefreshInteractiveFormTreeSafely();
        }

        /// <summary>
        /// Subscribes to interactive form events.
        /// </summary>
        /// <param name="interactiveForm">The interactive form.</param>
        private void SubscribeToInteractiveFormEvents(PdfDocumentInteractiveForm interactiveForm)
        {
            interactiveForm.FieldTreeChanged += new CollectionChangeEventHandler<PdfInteractiveFormField>(InteractiveForm_FieldTreeChanged);
        }

        /// <summary>
        /// Unsubscribes from interactive form events.
        /// </summary>
        /// <param name="interactiveForm">The interactive form.</param>
        private void UnsubscribeFromInteractiveFormEvents(PdfDocumentInteractiveForm interactiveForm)
        {
            interactiveForm.FieldTreeChanged -= new CollectionChangeEventHandler<PdfInteractiveFormField>(InteractiveForm_FieldTreeChanged);
        }

        private object GetItemHeader(string imageKey, string text)
        {
            StackPanel header = new StackPanel();
            header.Orientation = Orientation.Horizontal;
            Image itemImage = GetItemImage(imageKey);
            header.Children.Add(itemImage);
            TextBlock captionTextBlock = new TextBlock();
            captionTextBlock.Text = text;
            header.Children.Add(captionTextBlock);
            return header;
        }

        private Image GetItemImage(string imageKey)
        {
            Image itemImage = new Image();
            itemImage.Source = _imageList[imageKey];
            return itemImage;
        }

        #endregion

        #endregion



        #region Delegates

        /// <summary>
        /// Represents the <see cref="RefreshInteractiveFormTree"/> method.
        /// </summary>
        private delegate void RefreshInteractiveFormTreeDelegate();

        #endregion

    }
}
