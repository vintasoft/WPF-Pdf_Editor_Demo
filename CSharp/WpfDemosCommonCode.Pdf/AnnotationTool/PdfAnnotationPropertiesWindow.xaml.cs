using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

using Vintasoft.Imaging.Pdf.Drawing.GraphicsFigures;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.Fonts;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms.AppearanceGenerators;
using Vintasoft.Imaging.Pdf.Wpf.UI.Annotations;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Form that allows to edit properties of annotations.
    /// </summary>
    public partial class PdfAnnotationPropertiesWindow : Window
    {

        #region Fields

        /// <summary>
        /// The PDF interactive form field.
        /// </summary>
        PdfInteractiveFormField _field;

        /// <summary>
        /// The PDF annotation.
        /// </summary>
        PdfAnnotation _annotation;

        /// <summary>
        /// The view of PDF annotation.
        /// </summary>
        WpfPdfAnnotationView _annotationView;

        /// <summary>
        /// Determines that the control is updating.
        /// </summary>
        bool _isControlUpdating = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAnnotationPropertiesForm"/> class.
        /// </summary>
        public PdfAnnotationPropertiesWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAnnotationPropertiesForm"/> class.
        /// </summary>
        /// <param name="field">The PDF interactive form field.</param>
        public PdfAnnotationPropertiesWindow(PdfInteractiveFormField field)
            : this()
        {
            _field = field;
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAnnotationPropertiesForm"/> class.
        /// </summary>
        /// <param name="annotationView">The PDF annotation view.</param>
        public PdfAnnotationPropertiesWindow(WpfPdfAnnotationView annotationView)
            : this()
        {
            _annotationView = annotationView;
            _annotation = annotationView.Annotation;
            if (_annotationView.Figure is PdfWidgetAnnotationGraphicsFigure)
                _field = ((PdfWidgetAnnotationGraphicsFigure)_annotationView.Figure).Field;

            Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAnnotationPropertiesForm"/> class.
        /// </summary>
        /// <param name="annotation">The PDF annotation.</param>
        public PdfAnnotationPropertiesWindow(PdfAnnotation annotation)
            : this()
        {
            _annotation = annotation;

            Init();
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets the appearance generator of PDF annotation.
        /// </summary>
        private PdfAnnotationAppearanceGenerator AppearanceGenerator
        {
            get
            {
                if (_annotationView != null && _field != null)
                    return _annotation.AppearanceGenerator;

                return null;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Updates the appearance of PDF annotation view.
        /// </summary>
        /// <param name="annotationView">The PDF annotation view.</param>
        /// <param name="loadAppearancePropertiesToAppearanceGenerator">Indicates whether need load appearance properties to appearance generator.</param>       
        public void UpdateAppearance(WpfPdfAnnotationView annotationView,
            bool loadAppearancePropertiesToAppearanceGenerator)
        {
            // if annotation view is NOT empty
            if (annotationView != null)
            {
                // get annotation
                PdfAnnotation annotation = annotationView.Annotation;

                // if annotation has appearance generator
                if (AppearanceGenerator != null)
                {
                    // if need load appearance properties to appearance generator
                    if (loadAppearancePropertiesToAppearanceGenerator)
                        AppearanceGenerator.LoadPropertiesFromAnnotation(annotation);

                    // update the annotation appearance
                    AppearanceGenerator.SetAppearance(annotation);
                }

                // if annotation view must be refreshed
                if (refreshAnnotationCheckBox.IsChecked.Value)
                    if (annotationView.PointsEditor != null)
                        annotationView.PointsEditor.RefreshFigure();

                // if annotation appearance must be updated
                if (updateAnnotationAppearanceCheckBox.IsChecked.Value)
                    ((PdfAnnotationGraphicsFigure)annotationView.Figure).Update();
                annotationView.Invalidate();
            }
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Init()
        {
            // if annnotation exists
            if (_annotation != null)
            {
                // set object of property grid
                annotationPropertyGrid.SelectedObject = _annotation;
                // set tree node of triggers editor
                annotationTriggersEditorControl.TreeNode = _annotation;
                // update text of group box
                annotationPropertiesGroupBox.Header = string.Format("{0} (object {1})", _annotation.GetType().Name, _annotation.ObjectNumber);
                annotationFontButton.IsEnabled = _annotation is PdfFreeTextAnnotation;

                if (_annotation is PdfMarkupAnnotation)
                {
                    // markup annotations is not supports tigger events
                    annotationTabControl.Items.Remove(annotationsTriggersTabPage);
                }

                if (!(_annotation is PdfWidgetAnnotation))
                {
                    commonPropertiesGroupBox.Content = null;
                    commonPropertiesGroupBox.Header = "Annotation Common Properties";
                    PdfAnnotationCommonPropertiesEditorControl commonPropertiesEditorControl =
                        new PdfAnnotationCommonPropertiesEditorControl();
                    commonPropertiesEditorControl.Annotation = _annotation;
                    commonPropertiesEditorControl.PropertyValueChanged +=
                        new EventHandler(annotationPropertiesEditorControl_PropertyValueChanged);
                    commonPropertiesGroupBox.Content = commonPropertiesEditorControl;

                    customPropertiesGroupBox.Content = null;
                    customPropertiesGroupBox.Header = string.Empty;
                    System.Windows.Controls.Control annotationPropertiesEditorControl = null;

                    if (_annotation is PdfLineAnnotation)
                    {
                        PdfLineAnnotationPropertiesEditorControl pdfLineAnnotationPropertiesEditorControl =
                            new PdfLineAnnotationPropertiesEditorControl();
                        pdfLineAnnotationPropertiesEditorControl.PropertyValueChanged +=
                                new EventHandler(annotationPropertiesEditorControl_PropertyValueChanged);
                        annotationPropertiesEditorControl = pdfLineAnnotationPropertiesEditorControl;
                    }
                    else if (_annotation is PdfRectangularAnnotation)
                    {
                        PdfRectangularAnnotationPropertiesEditorControl pdfRectangularAnnotationPropertiesEditorControl =
                            new PdfRectangularAnnotationPropertiesEditorControl();
                        pdfRectangularAnnotationPropertiesEditorControl.PropertyValueChanged +=
                                new EventHandler(annotationPropertiesEditorControl_PropertyValueChanged);
                        annotationPropertiesEditorControl = pdfRectangularAnnotationPropertiesEditorControl;
                    }
                    else if (_annotation is PdfPolylineAnnotation)
                    {
                        PdfPolylineAnnotationPropertiesEditorControl pdfPolylineAnnotationPropertiesEditorControl =
                            new PdfPolylineAnnotationPropertiesEditorControl();
                        pdfPolylineAnnotationPropertiesEditorControl.PropertyValueChanged +=
                                new EventHandler(annotationPropertiesEditorControl_PropertyValueChanged);
                        annotationPropertiesEditorControl = pdfPolylineAnnotationPropertiesEditorControl;
                    }
                    else if (_annotation is PdfPolygonalAnnotation)
                    {
                        PdfPolygonalAnnotationPropertiesEditorControl pdfPolygonalAnnotationPropertiesEditorControl =
                            new PdfPolygonalAnnotationPropertiesEditorControl();
                        pdfPolygonalAnnotationPropertiesEditorControl.PropertyValueChanged +=
                                new EventHandler(annotationPropertiesEditorControl_PropertyValueChanged);
                        annotationPropertiesEditorControl = pdfPolygonalAnnotationPropertiesEditorControl;
                    }
                    else if (_annotation is PdfLinkAnnotation)
                    {
                        PdfLinkAnnotationPropertiesEditorControl pdfLinkAnnotationPropertiesEditorControl =
                            new PdfLinkAnnotationPropertiesEditorControl();
                        pdfLinkAnnotationPropertiesEditorControl.PropertyValueChanged +=
                                new EventHandler(annotationPropertiesEditorControl_PropertyValueChanged);
                        annotationPropertiesEditorControl = pdfLinkAnnotationPropertiesEditorControl;
                    }
                    else if (_annotation is PdfFreeTextAnnotation)
                    {
                        PdfFreeTextAnnotationPropertiesEditorControl pdfFreeTextAnnotationPropertiesEditorControl =
                            new PdfFreeTextAnnotationPropertiesEditorControl();
                        pdfFreeTextAnnotationPropertiesEditorControl.PropertyValueChanged +=
                                new EventHandler(annotationPropertiesEditorControl_PropertyValueChanged);
                        annotationPropertiesEditorControl = pdfFreeTextAnnotationPropertiesEditorControl;
                    }
                    else if (_annotation is PdfFileAttachmentAnnotation)
                    {
                        PdfFileAttachmentAnnotationPropertiesEditorControl pdfFileAttachmentAnnotationPropertiesEditorControl =
                            new PdfFileAttachmentAnnotationPropertiesEditorControl();
                        pdfFileAttachmentAnnotationPropertiesEditorControl.PropertyValueChanged +=
                                new EventHandler(annotationPropertiesEditorControl_PropertyValueChanged);
                        annotationPropertiesEditorControl = pdfFileAttachmentAnnotationPropertiesEditorControl;
                    }

                    if (annotationPropertiesEditorControl != null)
                    {
                        string name = _annotation.GetType().Name;
                        if (name.StartsWith("Pdf") && name.Length > 3)
                            name = name.Substring(3);
                        if (name.EndsWith("Annotation") && name.Length > 10)
                            name = name.Substring(0, name.Length - 10);

                        customPropertiesGroupBox.Header = name;
                        customPropertiesGroupBox.Content = annotationPropertiesEditorControl;
                        if (annotationPropertiesEditorControl.MinHeight == annotationPropertiesEditorControl.MaxHeight)
                            annotationPropertiesEditorControl.VerticalAlignment = VerticalAlignment.Top;

                        if (annotationPropertiesEditorControl is IPdfAnnotationPropertiesEditor)
                        {
                            IPdfAnnotationPropertiesEditor propertiesEditor =
                                (IPdfAnnotationPropertiesEditor)annotationPropertiesEditorControl;
                            propertiesEditor.Annotation = _annotation;
                        }
                    }
                }
            }
            else
            {
                // remove annotation tab page
                advancedTabControl.Items.Remove(annotationTabPage);
            }

            // if field is exist
            if (_field != null)
            {
                // set object of property grid
                fieldPropertyGrid.SelectedObject = _field;
                // set tree node of triggers editor
                fieldTriggersEditorControl.TreeNode = _field;
                // update text of group box
                fieldPropertiesGroupBox.Header = string.Format("{0} (object {1})", _field.GetType().Name, _field.ObjectNumber);

                if (_annotation == null || _annotation is PdfWidgetAnnotation)
                {
                    commonPropertiesGroupBox.Content = null;
                    commonPropertiesGroupBox.Header = "Field Common Properties";
                    PdfInteractiveFormCommonPropertiesEditorControl commonPropertiesEditorControl =
                        new PdfInteractiveFormCommonPropertiesEditorControl();
                    commonPropertiesEditorControl.Field = _field;
                    commonPropertiesEditorControl.PropertyValueChanged +=
                        new EventHandler(fieldPropertiesEditorControl_PropertyValueChanged);
                    commonPropertiesGroupBox.Content = commonPropertiesEditorControl;


                    customPropertiesGroupBox.Content = null;
                    customPropertiesGroupBox.Header = string.Empty;
                    System.Windows.Controls.Control interactiveFormPropertiesEditorControl = null;

                    if (_field is PdfInteractiveFormComboBoxField)
                    {
                        PdfComboBoxPropertiesEditorControl pdfComboBoxPropertiesEditorControl =
                            new PdfComboBoxPropertiesEditorControl();
                        pdfComboBoxPropertiesEditorControl.PropertyValueChanged +=
                                new EventHandler(fieldPropertiesEditorControl_PropertyValueChanged);
                        interactiveFormPropertiesEditorControl = pdfComboBoxPropertiesEditorControl;
                    }
                    else if (_field is PdfInteractiveFormListBoxField)
                    {
                        PdfListBoxPropertiesEditorControl pdfListBoxPropertiesEditorControl =
                            new PdfListBoxPropertiesEditorControl();
                        pdfListBoxPropertiesEditorControl.PropertyValueChanged +=
                                new EventHandler(fieldPropertiesEditorControl_PropertyValueChanged);
                        interactiveFormPropertiesEditorControl = pdfListBoxPropertiesEditorControl;
                    }
                    else if (_field is PdfInteractiveFormVintasoftBarcodeField)
                    {
                        PdfVintasoftBarcodeFieldPropertiesEditorControl pdfVintasoftBarcodeFieldPropertiesEditorControl =
                            new PdfVintasoftBarcodeFieldPropertiesEditorControl();
                        pdfVintasoftBarcodeFieldPropertiesEditorControl.PropertyValueChanged +=
                                new EventHandler(fieldPropertiesEditorControl_PropertyValueChanged);
                        interactiveFormPropertiesEditorControl = pdfVintasoftBarcodeFieldPropertiesEditorControl;
                    }
                    else if (_field is PdfInteractiveFormBarcodeField)
                    {
                        PdfBarcodeFieldPropertiesEditorControl pdfBarcodeFieldPropertiesEditorControl =
                            new PdfBarcodeFieldPropertiesEditorControl();
                        pdfBarcodeFieldPropertiesEditorControl.PropertyValueChanged +=
                                new EventHandler(fieldPropertiesEditorControl_PropertyValueChanged);
                        interactiveFormPropertiesEditorControl = pdfBarcodeFieldPropertiesEditorControl;
                    }
                    else if (_field is PdfInteractiveFormTextField)
                    {
                        PdfTextFieldPropertiesEditorControl pdfTextFieldPropertiesEditorControl =
                            new PdfTextFieldPropertiesEditorControl();
                        pdfTextFieldPropertiesEditorControl.PropertyValueChanged +=
                                new EventHandler(fieldPropertiesEditorControl_PropertyValueChanged);
                        interactiveFormPropertiesEditorControl = pdfTextFieldPropertiesEditorControl;
                    }
                    else if (_field is PdfInteractiveFormCheckBoxField)
                    {
                        PdfCheckBoxPropertiesEditorControl pdfCheckBoxPropertiesEditorControl =
                            new PdfCheckBoxPropertiesEditorControl();
                        pdfCheckBoxPropertiesEditorControl.PropertyValueChanged +=
                                new EventHandler(fieldPropertiesEditorControl_PropertyValueChanged);
                        interactiveFormPropertiesEditorControl = pdfCheckBoxPropertiesEditorControl;
                    }
                    else if (_field is PdfInteractiveFormRadioButtonField)
                    {
                        PdfRadioButtonPropertiesEditorControl pdfRadioButtonPropertiesEditorControl =
                            new PdfRadioButtonPropertiesEditorControl();
                        pdfRadioButtonPropertiesEditorControl.PropertyValueChanged +=
                                new EventHandler(fieldPropertiesEditorControl_PropertyValueChanged);
                        interactiveFormPropertiesEditorControl = pdfRadioButtonPropertiesEditorControl;
                    }
                    else if (_field is PdfInteractiveFormButtonField)
                    {
                        PdfPushButtonPropertiesEditorControl pdfPushButtonPropertiesEditorControl =
                            new PdfPushButtonPropertiesEditorControl();
                        pdfPushButtonPropertiesEditorControl.PropertyValueChanged +=
                                new EventHandler(fieldPropertiesEditorControl_PropertyValueChanged);
                        interactiveFormPropertiesEditorControl = pdfPushButtonPropertiesEditorControl;
                    }

                    if (interactiveFormPropertiesEditorControl != null)
                    {
                        if (interactiveFormPropertiesEditorControl is IPdfInteractiveFormPropertiesEditor)
                        {
                            IPdfInteractiveFormPropertiesEditor propertiesEditor =
                                (IPdfInteractiveFormPropertiesEditor)interactiveFormPropertiesEditorControl;
                            propertiesEditor.Field = _field;
                        }

                        string name = _field.GetType().Name;
                        name = name.Replace("PdfInteractiveForm", "");
                        if (name.EndsWith("Field") && name.Length > 5)
                            name = name.Substring(0, name.Length - 5);

                        customPropertiesGroupBox.Header = name;
                        customPropertiesGroupBox.Content = interactiveFormPropertiesEditorControl;
                    }
                }
            }
            else
            {
                // remove field tab page
                advancedTabControl.Items.Remove(fieldTabPage);
            }

            // if appearance generator is exist
            if (AppearanceGenerator != null)
            {
                // set object of property grid
                appearanceGeneratorPropertyGrid.SelectedObject = AppearanceGenerator;
                // update text of group box
                appearanceGeneratorGroupBox.Header = AppearanceGenerator.GetType().Name;
            }
            else
            {
                // remove field tab page
                advancedTabControl.Items.Remove(appearanceGeneratorTabPage);
            }
        }

        /// <summary>
        /// The annotation property is changed.
        /// </summary>
        void annotationPropertiesEditorControl_PropertyValueChanged(object sender, EventArgs e)
        {
            PdfRectangularAnnotation rectangularAnnotation = _annotationView.Annotation as PdfRectangularAnnotation;
            if (rectangularAnnotation != null)
            {
                PdfRectangularAnnotationPropertiesEditorControl rectangularAnnotationControl =
                    customPropertiesGroupBox.Content as PdfRectangularAnnotationPropertiesEditorControl;
                if (rectangularAnnotationControl != null &&
                    rectangularAnnotationControl.AutoUpdatePadding)
                {
                    PdfRectangularAnnotationAppearanceGenerator appearanceGenerator =
                    rectangularAnnotation.AppearanceGenerator as PdfRectangularAnnotationAppearanceGenerator;

                    Vintasoft.Imaging.PaddingF padding = appearanceGenerator.GetRequiredPadding(rectangularAnnotation);
                    rectangularAnnotation.Padding = padding;

                    rectangularAnnotationControl.UpdateAnnotationInfo();
                }
            }

            UpdateAppearance(_annotationView, true);
        }

        /// <summary>
        /// The field property is changed.
        /// </summary>
        void fieldPropertiesEditorControl_PropertyValueChanged(object sender, EventArgs e)
        {
            UpdateAppearance(_annotationView, true);

            if (sender is PdfTextFieldPropertiesEditorControl)
            {
                PdfInteractiveFormCommonPropertiesEditorControl interactiveFormCommonPropertiesEditorControl =
                    commonPropertiesGroupBox.Content as PdfInteractiveFormCommonPropertiesEditorControl;
                if (interactiveFormCommonPropertiesEditorControl != null)
                    interactiveFormCommonPropertiesEditorControl.UpdateFieldInfo();
            }
        }

        /// <summary>
        /// Updates the main tab page.
        /// </summary>
        private void mainTabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count == 1 && (e.AddedItems[0] is TabItem) &&
                e.RemovedItems != null && e.RemovedItems.Count == 1 && (e.RemovedItems[0] is TabItem))
            {
                if (mainTabControl.SelectedItem == propertiesTabPage)
                {
                    if (_isControlUpdating)
                        return;
                    _isControlUpdating = true;

                    if (commonPropertiesGroupBox.Content != null)
                    {
                        PdfInteractiveFormCommonPropertiesEditorControl interactiveFormCommonPropertiesEditorControl =
                            commonPropertiesGroupBox.Content as PdfInteractiveFormCommonPropertiesEditorControl;
                        if (interactiveFormCommonPropertiesEditorControl != null)
                            interactiveFormCommonPropertiesEditorControl.UpdateFieldInfo();

                        PdfAnnotationCommonPropertiesEditorControl annotationCommonPropertiesEditorControl =
                            commonPropertiesGroupBox.Content as PdfAnnotationCommonPropertiesEditorControl;
                        if (annotationCommonPropertiesEditorControl != null)
                            annotationCommonPropertiesEditorControl.UpdateAnnotationInfo();
                    }

                    if (customPropertiesGroupBox.Content != null)
                    {
                        if (customPropertiesGroupBox.Content is IPdfInteractiveFormPropertiesEditor)
                            ((IPdfInteractiveFormPropertiesEditor)customPropertiesGroupBox.Content).UpdateFieldInfo();
                        else if (customPropertiesGroupBox.Content is IPdfAnnotationPropertiesEditor)
                            ((IPdfAnnotationPropertiesEditor)customPropertiesGroupBox.Content).UpdateAnnotationInfo();
                    }
                    _isControlUpdating = false;
                }
                else
                {
                    UpdateAdvancedTabPage();
                }
            }
        }

        /// <summary>
        /// Selected tab, in advanced tab control, is changed.
        /// </summary>
        private void advancedTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAdvancedTabPage();
        }

        /// <summary>
        /// Updates the advanced tab page.
        /// </summary>
        private void UpdateAdvancedTabPage()
        {
            if (advancedTabControl.SelectedItem == fieldTabPage)
            {
                UpdateFieldTabPage();
            }
            else if (advancedTabControl.SelectedItem == annotationTabPage)
            {
                UpdateAnnotationTabPage();
            }
            else if (advancedTabControl.SelectedItem == appearanceGeneratorTabPage)
                appearanceGeneratorPropertyGrid.Refresh();
        }

        /// <summary>
        /// Selected tab, in field tab control, is changed.
        /// </summary>
        private void fieldTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFieldTabPage();
        }

        /// <summary>
        /// Updates the field tab page.
        /// </summary>
        private void UpdateFieldTabPage()
        {
            if (fieldTabControl.SelectedItem == fieldPropertiesTabPage)
                fieldPropertyGrid.Refresh();
            else
                fieldTriggersEditorControl.UpdateTriggersInfo();
        }

        /// <summary>
        /// Selected tab, in annotation tab control, is changed.
        /// </summary>
        private void annotationTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAnnotationTabPage();
        }

        /// <summary>
        /// Updates the annotation tab page.
        /// </summary>
        private void UpdateAnnotationTabPage()
        {
            if (annotationTabControl.SelectedItem == annotationPropertiesTabPage)
                annotationPropertyGrid.Refresh();
            else
                annotationTriggersEditorControl.UpdateTriggersInfo();
        }

        /// <summary>
        /// The "Close" button is clicked.
        /// </summary>
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// The property of appearance generator is changed.
        /// </summary>
        private void appearanceGeneratorPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            UpdateAppearance(_annotationView, false);
        }

        /// <summary>
        /// The property of interactive form field is changed.
        /// </summary>
        private void fieldPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            UpdateAppearance(_annotationView, true);
        }

        /// <summary>
        /// The property of annotation is changed.
        /// </summary>
        private void annotationPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            UpdateAppearance(_annotationView, true);
        }

        /// <summary>
        /// The button of appearance generator is clicked.
        /// </summary>
        private void setFontButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppearanceGenerator is PdfWidgetAnnotationAppearanceGenerator)
            {
                // get appearance generator
                PdfWidgetAnnotationAppearanceGenerator appearanceGenrator = AppearanceGenerator as PdfWidgetAnnotationAppearanceGenerator;
                // if appearance generator has font
                if (appearanceGenrator.Font != null)
                {
                    // create a font window
                    CreateFontWindow createFont = new CreateFontWindow(_annotation.Document, appearanceGenrator.Font);
                    if (createFont.ShowDialog().Value)
                    {
                        // update font of appearance generator
                        appearanceGenrator.Font = createFont.SelectedFont;
                        appearanceGeneratorPropertyGrid.Refresh();
                        UpdateAppearance(_annotationView, false);
                    }
                }
            }
        }

        /// <summary>
        /// The button of form field is clicked.
        /// </summary>
        private void fieldFontButton_Click(object sender, RoutedEventArgs e)
        {
            PdfFont font = null;
            System.Drawing.Color textColor;
            float fontSize;
            _field.GetTextDefaultAppearance("", out font, out fontSize, out textColor);
            // create a font window
            CreateFontWindow createFont = new CreateFontWindow(_field.Document, font);
            if (createFont.ShowDialog().Value)
            {
                // update font of appearance generator
                font = createFont.SelectedFont;
                _field.SetTextDefaultAppearance(font, fontSize, textColor);
                UpdateAppearance(_annotationView, true);
            }
        }

        /// <summary>
        /// The button of annotation is clicked.
        /// </summary>
        private void annotationFontButton_Click(object sender, RoutedEventArgs e)
        {
            PdfFreeTextAnnotation freeText = _annotation as PdfFreeTextAnnotation;
            // if annotation is FreeTextAnnotation
            if (freeText != null)
            {
                // create font window
                CreateFontWindow createFont = new CreateFontWindow(freeText.Document, freeText.Font);
                if (createFont.ShowDialog().Value)
                {
                    // update font of appearance generator
                    freeText.Font = createFont.SelectedFont;
                    UpdateAppearance(_annotationView, true);
                }
            }
        }

        #endregion

        #endregion

    }
}
