using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view and edit the PDF hide action.
    /// </summary>
    public partial class PdfAnnotationHideActionEditorWindow : Window
    {

        #region Nested classes

        /// <summary>
        /// List box item.
        /// </summary>
        class ListBoxItem
        {

            /// <summary>
            /// The PDF page index.
            /// </summary>
            int _pageIndex;



            /// <summary>
            /// Initializes a new instance of the <see cref="ListBoxItem"/> class.
            /// </summary>
            /// <param name="annotation">The PDF annotation.</param>
            /// <param name="pageIndex">Index of the PDF page.</param>
            public ListBoxItem(PdfAnnotation annotation, int pageIndex)
            {
                _pageIndex = pageIndex;
                Annotation = annotation;
            }



            /// <summary>
            /// Gets or sets the PDF annotation.
            /// </summary>
            public PdfAnnotation Annotation
            {
                get;
                set;
            }



            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                string name = "Unknown";

                // if annotation is widget annotation
                if (Annotation is PdfWidgetAnnotation)
                {
                    PdfWidgetAnnotation widgetAnnotation = (PdfWidgetAnnotation)Annotation;
                    // get interactive form field
                    PdfInteractiveFormField field = widgetAnnotation.Field;

                    // get name of annotation
                    name = field.PartialName;
                    // if name is empty
                    if (string.IsNullOrEmpty(name))
                    {
                        // if field is switchable button
                        if (field is PdfInteractiveFormSwitchableButtonField)
                            // get value of button
                            name = ((PdfInteractiveFormSwitchableButtonField)field).ButtonValue;
                    }
                }
                else
                {
                    name = Annotation.Name;
                }
                return string.Format("(Page:{0,3}) {1}", _pageIndex + 1, name);
            }

        }

        #endregion



        #region Fields

        /// <summary>
        /// The PDF annotation hide action.
        /// </summary>
        PdfAnnotationHideAction _action;

        /// <summary>
        /// The PDF pages, which have annotations.
        /// </summary>
        List<PdfPage> _annotatedPages = new List<PdfPage>();

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAnnotationHideActionEditorWindow"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        public PdfAnnotationHideActionEditorWindow(PdfAnnotationHideAction action)
        {
            InitializeComponent();

            _action = action;

            hideCheckBox.IsChecked = action.Hide;

            PdfPageCollection pages = action.Document.Pages;
            pagesComboBox.BeginInit();
            for (int i = 0; i < pages.Count; i++)
            {
                PdfPage page = pages[i];
                if (page.Annotations != null && page.Annotations.Count > 0)
                {
                    _annotatedPages.Add(page);
                    pagesComboBox.Items.Add(string.Format("Page {0}", i + 1));
                }
            }
            pagesComboBox.SelectedIndex = 0;
            pagesComboBox.EndInit();

            if (action.Annotations != null)
            {
                foreach (PdfAnnotation annotation in action.Annotations)
                    AddAnnotation(annotation);
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Adds annotation from PdfPageAnnotationsControl to the annotation list in AnnotationsListBox.
        /// </summary>
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            AddAnnotation();
        }

        /// <summary>
        /// Adds annotation from PdfPageAnnotationsControl to the annotation list in AnnotationsListBox.
        /// </summary>
        private void AddAnnotation()
        {
            // get PDF page annotation
            PdfAnnotation pageAnnotation = pdfPageAnnotationsControl.SelectedAnnotation;
            // adds PDF page annotation to a list of selected annotations
            AddAnnotation(pageAnnotation);

            UpdateUI();
        }

        /// <summary>
        /// Removes annotation from annotation list in AnnotationsListBox.
        /// </summary>
        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveAnnotation();
        }

        /// <summary>
        /// Removes annotation from annotation list in AnnotationsListBox.
        /// </summary>
        private void RemoveAnnotation()
        {
            int newIndex = annotationsListBox.SelectedIndex;
            if (newIndex == annotationsListBox.Items.Count - 1)
                newIndex--;

            annotationsListBox.Items.Remove(annotationsListBox.SelectedItem);
            annotationsListBox.SelectedIndex = newIndex;

            UpdateUI();
        }

        /// <summary>
        /// Removes all PDF annotations from a list of selected annotations.
        /// </summary>
        private void removeAllButton_Click(object sender, RoutedEventArgs e)
        {
            annotationsListBox.Items.Clear();

            UpdateUI();
        }

        /// <summary>
        /// "Ok" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (hideCheckBox.IsChecked.Value == true)
                _action.Hide = true;
            else
                _action.Hide = false;

            ItemCollection items = annotationsListBox.Items;
            List<PdfAnnotation> annotations = new List<PdfAnnotation>();

            foreach (ListBoxItem item in items)
                annotations.Add(item.Annotation);

            _action.Annotations = annotations.ToArray();
            DialogResult = true;
        }

        /// <summary>
        /// Mouse is double clicked on annotation in PdfPageAnnotationsControl.
        /// </summary>
        private void pdfPageAnnotationsControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CanAddAnnotationFromPdfPageAnnotationsControlToSelectedAnnotations())
                // add annotation from PdfPageAnnotationsControl to the annotation list in AnnotationsListBox
                AddAnnotation();
        }

        /// <summary>
        /// Mouse is double clicked on annotation in AnnotationsListBox.
        /// </summary>
        private void annotationsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (annotationsListBox.SelectedItem != null)
                // remove annotation from annotation list in AnnotationsListBox
                RemoveAnnotation();
        }

        /// <summary>
        /// PDF page index is changed.
        /// </summary>
        private void pagesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pagesComboBox.SelectedIndex == -1)
            {
                pdfPageAnnotationsControl.AnnotationList = null;
            }
            else
            {
                PdfPage page = _annotatedPages[pagesComboBox.SelectedIndex];
                pdfPageAnnotationsControl.AnnotationList = page.Annotations;
            }
        }

        /// <summary>
        /// Selected annotation is changed in AnnotationsListBox.
        /// </summary>
        private void annotationsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Selected annotation is changed in PdfPageAnnotationsControl.
        /// </summary>
        private void pdfPageAnnotationsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUI();
        }


        /// <summary>
        /// Updates the user interface of this control.
        /// </summary>
        private void UpdateUI()
        {
            addButton.IsEnabled = CanAddAnnotationFromPdfPageAnnotationsControlToSelectedAnnotations();
            removeButton.IsEnabled = annotationsListBox.SelectedIndex != -1;
            removeAllButton.IsEnabled = annotationsListBox.Items.Count > 0;
        }

        /// <summary>
        /// Determines that the annotation from PdfPageAnnotationsControl can be added
        /// to the annotation list in AnnotationsListBox.
        /// </summary>
        private bool CanAddAnnotationFromPdfPageAnnotationsControlToSelectedAnnotations()
        {
            // get annotation from PdfPageAnnotationsControl
            PdfAnnotation selectedAnnotation = pdfPageAnnotationsControl.SelectedAnnotation;
            // if annotation exist
            if (selectedAnnotation != null)
            {
                // get list box items
                ItemCollection items = annotationsListBox.Items;
                // for each list box item
                foreach (ListBoxItem item in items)
                {
                    // if annotation, associated with list box item, is equal to the selected annotation
                    if (item.Annotation == selectedAnnotation)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Adds the annotation to a list box of selected annotations.
        /// </summary>
        private void AddAnnotation(PdfAnnotation annotation)
        {
            annotationsListBox.Items.Add(
                new ListBoxItem(annotation, _annotatedPages.IndexOf(annotation.Page)));
        }

        #endregion


    }
}
