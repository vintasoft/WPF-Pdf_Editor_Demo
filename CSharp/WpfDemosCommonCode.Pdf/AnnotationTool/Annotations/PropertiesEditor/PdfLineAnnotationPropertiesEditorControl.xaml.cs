using System;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Wpf;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit properties of the <see cref="PdfLineAnnotation"/>.
    /// </summary>
    public partial class PdfLineAnnotationPropertiesEditorControl : UserControl, IPdfAnnotationPropertiesEditor
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of
        /// the <see cref="PdfLineAnnotationPropertiesEditorControl"/> class.
        /// </summary>
        public PdfLineAnnotationPropertiesEditorControl()
        {
            InitializeComponent();

            foreach (PdfAnnotationLineEndingStyle style in Enum.GetValues(typeof(PdfAnnotationLineEndingStyle)))
            {
                startPointLineEndingStyleComboBox.Items.Add(style);
                endPointLineEndingStyleComboBox.Items.Add(style);
            }
        }

        #endregion



        #region Properties

        PdfLineAnnotation _annotation;
        /// <summary>
        /// Gets or sets the annotation.
        /// </summary>
        public PdfLineAnnotation Annotation
        {
            get
            {
                return _annotation;
            }
            set
            {
                _annotation = value;

                mainGrid.IsEnabled = _annotation != null;

                UpdateAnnotationInfo();
            }
        }

        /// <summary>
        /// Gets or sets the annotation.
        /// </summary>
        PdfAnnotation IPdfAnnotationPropertiesEditor.Annotation
        {
            get
            {
                return Annotation;
            }
            set
            {
                Annotation = value as PdfLineAnnotation;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the information about annotation.
        /// </summary>
        public void UpdateAnnotationInfo()
        {
            if (_annotation == null)
                return;

            startPointLineEndingStyleComboBox.SelectedItem = _annotation.StartPointLineEndingStyle;
            endPointLineEndingStyleComboBox.SelectedItem = _annotation.EndPointLineEndingStyle;
            interiorColorColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(_annotation.InteriorColor);
            UpdateUI();
        }

        /// <summary>
        /// Updates the user interface of this control.
        /// </summary>
        private void UpdateUI()
        {
            PdfAnnotationLineEndingStyle startPointLineEndingStyle = PdfAnnotationLineEndingStyle.None;
            if (startPointLineEndingStyleComboBox.SelectedItem != null)
                startPointLineEndingStyle = (PdfAnnotationLineEndingStyle)startPointLineEndingStyleComboBox.SelectedItem;

            PdfAnnotationLineEndingStyle endPointLineEndingStyle = PdfAnnotationLineEndingStyle.None;
            if (endPointLineEndingStyleComboBox.SelectedItem != null)
                endPointLineEndingStyle = (PdfAnnotationLineEndingStyle)endPointLineEndingStyleComboBox.SelectedItem;

            interiorColorColorPanelControl.IsEnabled =
                PdfAnnotationsTools.IsClosedLineEndingStyle(startPointLineEndingStyle) ||
                PdfAnnotationsTools.IsClosedLineEndingStyle(endPointLineEndingStyle);
        }

        /// <summary>
        /// Field start point line ending style is changed.
        /// </summary>
        private void startPointLineEndingStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _annotation.StartPointLineEndingStyle = (PdfAnnotationLineEndingStyle)startPointLineEndingStyleComboBox.SelectedItem;
            UpdateUI();
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Field end point line ending style is changed.
        /// </summary>
        private void endPointLineEndingStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _annotation.EndPointLineEndingStyle = (PdfAnnotationLineEndingStyle)endPointLineEndingStyleComboBox.SelectedItem;
            UpdateUI();
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Field interior color is changed.
        /// </summary>
        private void InteriorColorColorPanelControl_ColorChanged(object sender, EventArgs e)
        {
            _annotation.InteriorColor = WpfObjectConverter.CreateDrawingColor(interiorColorColorPanelControl.Color);
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Raises the PropertyValueChanged event.
        /// </summary>
        private void OnPropertyValueChanged()
        {
            if (PropertyValueChanged != null)
                PropertyValueChanged(this, EventArgs.Empty);
        }

        #endregion



        #region Events

        /// <summary>
        /// Occurs when value of property is changed.
        /// </summary>
        public event EventHandler PropertyValueChanged;

        #endregion

    }
}
