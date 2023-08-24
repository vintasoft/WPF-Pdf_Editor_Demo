using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to show information about annotations of PDF Page.
    /// </summary>
    public partial class PdfPageAnnotationsControl : ListView
    {

        #region Nested class

        /// <summary>
        /// Represents a list view row data.
        /// </summary>
        public class ListViewRowData
        {

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ListViewRowData"/> class.
            /// </summary>
            public ListViewRowData(PdfAnnotation annotation, string name, BitmapSource iconImage)
            {
                _annotation = annotation;
                _name = name;
                _iconImage = iconImage;
            }

            #endregion



            #region Properties

            PdfAnnotation _annotation;
            /// <summary>
            /// Gets the annotation associated with current row of a grid view.
            /// </summary>
            public PdfAnnotation Annotation
            {
                get
                {
                    return _annotation;
                }
            }

            BitmapSource _iconImage;
            /// <summary>
            /// Gets the icon image of current row.
            /// </summary>
            public BitmapSource IconImage
            {
                get
                {
                    return _iconImage;
                }
            }

            /// <summary>
            /// Gets the type of the annotation.
            /// </summary>
            public string AnnotationType
            {
                get
                {
                    return _annotation.AnnotationType.ToString();
                }
            }

            string _name;
            /// <summary>
            /// Gets or sets the name of the annotation.
            /// </summary>
            public string Name
            {
                get
                {
                    return _name;
                }
                set
                {
                    _name = value;
                }
            }

            /// <summary>
            /// Gets the rectangle of the annotation.
            /// </summary>
            public string Rectangle
            {
                get
                {
                    return _annotation.Rectangle.ToString();
                }
            }

            #endregion

        }

        #endregion



        #region Fields

        Dictionary<string, BitmapSource> _imageList;

        /// <summary>
        /// A view mode that displays data items in columns for a ListView control.
        /// </summary>
        GridView _gridView;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAnnotationListControl"/> class.
        /// </summary>
        public PdfPageAnnotationsControl()
        {
            InitializeComponent();

            _imageList = new Dictionary<string, BitmapSource>();
            string[] widgetResourceNames = new string[]{
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
            string widgetResourceNameFormatString = "WpfDemosCommonCode.Pdf.AnnotationTool.FormFields.Resources.{0}.png";
            // load resources
            for (int i = 0; i < widgetResourceNames.Length; i++)
            {
                BitmapSource resourceBitmap = DemosResourcesManager.GetResourceAsBitmap(
                    string.Format(widgetResourceNameFormatString, widgetResourceNames[i]));
                _imageList.Add(widgetResourceNames[i], resourceBitmap);
            }

            string[] annotationResourceNames = new string[] {
                     "Ellipse",
                     "FreeText",
                     "Highlight",
                     "StrikeOut",
                     "Underline",
                     "Squiggly",
                     "Line",
                     "Link",
                     "Polyline",
                     "Polygon",
                     "Screen",
                     "Rectangle",
                     "Stamp",
                     "Text_Comment",
                     "Ink",
                     "FileAttachment",
                     "Popup",
                     "Caret",
            };

            string annotationResourceNameFormatString = "WpfDemosCommonCode.Pdf.AnnotationTool.Annotations.Resources.{0}.png";
            // load resources
            for (int i = 0; i < annotationResourceNames.Length; i++)
            {
                BitmapSource resourceBitmap = DemosResourcesManager.GetResourceAsBitmap(
                    string.Format(annotationResourceNameFormatString, annotationResourceNames[i]));
                _imageList.Add(annotationResourceNames[i], resourceBitmap);
            }

            _gridView = (GridView)Resources["GridView"];

            Style = (Style)Resources["GridStyle"];
        }

        #endregion



        #region Properties

        PdfAnnotationList _annotationList = null;
        /// <summary>
        /// Gets or sets the annotation list.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public virtual PdfAnnotationList AnnotationList
        {
            get
            {
                return _annotationList;
            }
            set
            {
                if (_annotationList != value)
                {
                    if (_annotationList != null)
                        _annotationList.Changed -= annotationList_Changed;

                    _annotationList = value;

                    if (_annotationList != null)
                        _annotationList.Changed += new CollectionChangeEventHandler<PdfAnnotation>(annotationList_Changed);

                    Update(_annotationList);
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected annotation.
        /// </summary>
        public virtual PdfAnnotation SelectedAnnotation
        {
            get
            {
                if (SelectedItems.Count > 0)
                {
                    ListViewRowData rowData = (ListViewRowData)SelectedItems[0];
                    return rowData.Annotation;
                }
                return null;
            }
            set
            {
                BeginInit();
                try
                {
                    SelectedItems.Clear();

                    if (value != null)
                    {
                        foreach (ListViewRowData item in this.Items)
                        {
                            if (item.Annotation == value)
                            {
                                SelectedItems.Add(item);
                                return;
                            }
                        }
                    }
                }
                finally
                {
                    EndInit();
                }
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the annotation name.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        public void UpdateAnnotation(PdfAnnotation annotation)
        {
            foreach (ListViewRowData item in Items)
            {
                if (item.Annotation == annotation)
                {
                    item.Name = PdfDemosTools.GetAnnotationName(annotation);
                    Items.Refresh();
                    break;
                }
            }
        }

        /// <summary>
        /// Updates the control.
        /// </summary>
        /// <param name="tool">The annotation list.</param>
        private void Update(PdfAnnotationList annotations)
        {
            // begin update
            BeginInit();
            try
            {
                // clear items
                Items.Clear();
                // clear columns
                _gridView.Columns.Clear();

                // if annotation list contains annotations
                if (annotations != null && annotations.Count > 0)
                {
                    // add columns
                    AddColumn(_gridView, "Type", "GridNameView", null);
                    AddColumn(_gridView, "Name", null, "Name");
                    AddColumn(_gridView, "Rectangle", null, "Rectangle");

                    foreach (PdfAnnotation annotation in annotations)
                    {
                        if (annotation != null)
                            AddAnnotation(annotation);
                    }
                }
            }
            finally
            {
                // end update
                EndInit();
            }
        }

        /// <summary>
        /// Adds a column to the specified GridView.
        /// </summary>
        /// <param name="gridView">The GridView.</param>
        /// <param name="header">The text of the header.</param>
        /// <param name="cellTemplateResourceName">Name of the cell template resource.</param>
        /// <param name="displayMemberBindingPath">The DisplayMemberBinding path.</param>
        private void AddColumn(
            GridView gridView,
            string header,
            string cellTemplateResourceName,
            string displayMemberBindingPath)
        {
            GridViewColumn column = new GridViewColumn();
            column.Header = header;
            column.HeaderContainerStyle = (Style)Resources["GridViewColumnHeaderStyle"];
            if (cellTemplateResourceName != null)
                column.CellTemplate = (DataTemplate)Resources[cellTemplateResourceName];
            if (displayMemberBindingPath != null)
                column.DisplayMemberBinding = new Binding(displayMemberBindingPath);
            gridView.Columns.Add(column);
        }

        /// <summary>
        /// Annotation list is changed.
        /// </summary>
        private void annotationList_Changed(object sender, CollectionChangeEventArgs<PdfAnnotation> e)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
                Dispatcher.Invoke(new UpdateDelegate(Update), _annotationList);
            else
                Update(_annotationList);
        }

        /// <summary>
        /// Adds the annotation.
        /// </summary>
        /// <param name="annotation">The annotation to add.</param>
        private void AddAnnotation(PdfAnnotation annotation)
        {
            // name of annotation
            string name = PdfDemosTools.GetAnnotationName(annotation);
            // icon for annotation
            BitmapSource iconImage = GetIconImage(annotation);
            // create row data for grid view
            ListViewRowData rowData = CreateItem(annotation, name, iconImage);
            // add the item to list view
            Items.Add(rowData);
        }

        /// <summary>
        /// Returns the image for the specified annotation.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <returns>The image for the specified annotation.</returns>
        private BitmapSource GetIconImage(PdfAnnotation annotation)
        {
            string imageKey = string.Empty;

            switch (annotation.AnnotationType)
            {
                case PdfAnnotationType.Circle:
                    imageKey = "Ellipse";
                    break;

                case PdfAnnotationType.FreeText:
                    imageKey = "FreeText";
                    break;

                case PdfAnnotationType.Highlight:
                    imageKey = "Highlight";
                    break;

                case PdfAnnotationType.StrikeOut:
                    imageKey = "StrikeOut";
                    break;

                case PdfAnnotationType.Underline:
                    imageKey = "Underline";
                    break;

                case PdfAnnotationType.Squiggly:
                    imageKey = "Squiggly";
                    break;

                case PdfAnnotationType.Line:
                    imageKey = "Line";
                    break;

                case PdfAnnotationType.Link:
                    imageKey = "Link";
                    break;

                case PdfAnnotationType.PolyLine:
                    imageKey = "Polyline";
                    break;

                case PdfAnnotationType.Polygon:
                    imageKey = "Polygon";
                    break;

                case PdfAnnotationType.Screen:
                    imageKey = "Screen";
                    break;

                case PdfAnnotationType.Square:
                    imageKey = "Rectangle";
                    break;

                case PdfAnnotationType.Stamp:
                    imageKey = "Stamp";
                    break;

                case PdfAnnotationType.Text:
                    imageKey = "Text_Comment";
                    break;

                case PdfAnnotationType.Ink:
                    imageKey = "Ink";
                    break;

                case PdfAnnotationType.FileAttachment:
                    imageKey = "FileAttachment";
                    break;

                case PdfAnnotationType.Popup:
                    imageKey = "Popup";
                    break;

                case PdfAnnotationType.Caret:
                    imageKey = "Caret";
                    break;

                case PdfAnnotationType.Widget:
                    PdfWidgetAnnotation widgetAnnotation = (PdfWidgetAnnotation)annotation;
                    // get image key for widget annotation
                    imageKey = PdfInteractiveFormFieldTree.GetImageKey(widgetAnnotation.Field);
                    break;
            }

            if (!string.IsNullOrEmpty(imageKey))
                return _imageList[imageKey];

            return null;
        }

        /// <summary>
        /// Creates the item.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <param name="name">The name of the annotation.</param>
        /// <param name="iconImage">The icon image for the annotation.</param>
        private ListViewRowData CreateItem(PdfAnnotation annotation, string name, BitmapSource iconImage)
        {
            return new ListViewRowData(annotation, name, iconImage);
        }

        #endregion



        #region Delegates

        private delegate void UpdateDelegate(PdfAnnotationList annotations);

        #endregion

    }
}
