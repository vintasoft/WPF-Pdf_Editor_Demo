using System;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Wpf;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit properties of the <see cref="PdfPolygonalAnnotation"/>.
    /// </summary>
    public partial class PdfPolygonalAnnotationPropertiesEditorControl : UserControl, IPdfAnnotationPropertiesEditor
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of 
        /// the <see cref="PdfPolygonalAnnotationPropertiesEditorControl"/> class.
        /// </summary>
        public PdfPolygonalAnnotationPropertiesEditorControl()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        PdfPolygonalAnnotation _annotation;
        /// <summary>
        /// Gets or sets the annotation.
        /// </summary>
        public PdfPolygonalAnnotation Annotation
        {
            get
            {
                return _annotation;
            }
            set
            {
                _annotation = value;

                pdfAnnotationBorderEffectEditorControl1.Annotation = value;
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
                Annotation = value as PdfPolygonalAnnotation;
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

            interiorColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(_annotation.InteriorColor);

            pdfAnnotationBorderEffectEditorControl1.UpdateAnnotationInfo();
        }

        /// <summary>
        /// Field interior color is changed.
        /// </summary>
        private void interiorColorPanelControl_ColorChanged(object sender, EventArgs e)
        {
            _annotation.InteriorColor = WpfObjectConverter.CreateDrawingColor(interiorColorPanelControl.Color);
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Field border effect is changed.
        /// </summary>
        private void pdfAnnotationBorderEffectEditorControl1_PropertyValueChanged(object sender, EventArgs e)
        {
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
