using System;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree.Annotations;

using WpfDemosCommonCode.Pdf;


namespace WpfDemosCommonCode.CustomControls
{
    /// <summary>
    /// A control that allows to view and edit properties of
    /// the <see cref="PdfAnnotationBorderEffect"/>.
    /// </summary>
    public partial class PdfAnnotationBorderEffectEditorControl : UserControl, IPdfAnnotationPropertiesEditor
    {
        #region Constants

        /// <summary>
        /// The annotation border effect style is is not specified.
        /// </summary>
        const string STYLE_NONE = "None";

        /// <summary>
        /// The annotation border effect style is small cloud.
        /// </summary>
        const string STYLE_SMALL_CLOUD = "Small Cloud";

        /// <summary>
        /// The annotation border effect style is cloud.
        /// </summary>
        const string STYLE_CLOUD = "Cloud";

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAnnotationBorderEffectEditorControl"/> class.
        /// </summary>
        public PdfAnnotationBorderEffectEditorControl()
        {
            InitializeComponent();

            styleComboBox.Items.Add(STYLE_NONE);
            styleComboBox.Items.Add(STYLE_SMALL_CLOUD);
            styleComboBox.Items.Add(STYLE_CLOUD);
        }

        #endregion



        #region Properties

        PdfAnnotation _annotation;
        /// <summary>
        /// Gets or sets the annotation.
        /// </summary>
        public PdfAnnotation Annotation
        {
            get
            {
                return _annotation;
            }
            set
            {
                _annotation = value;

                styleComboBox.IsEnabled = BorderEffect != PdfAnnotationBorderEffectType.Unsupported;

                UpdateAnnotationInfo();
            }
        }

        /// <summary>
        /// Gets or sets the border effect of annotation.
        /// </summary>
        private PdfAnnotationBorderEffectType BorderEffect
        {
            get
            {
                PdfAnnotationBorderEffectType borderEffect = PdfAnnotationBorderEffectType.Unsupported;
                if (_annotation is PdfPolygonAnnotation)
                    borderEffect = ((PdfPolygonAnnotation)_annotation).BorderEffect;
                else if (_annotation is PdfRectangularAnnotation)
                    borderEffect = ((PdfRectangularAnnotation)_annotation).BorderEffect;
                else if (_annotation is PdfFreeTextAnnotation)
                    borderEffect = ((PdfFreeTextAnnotation)_annotation).BorderEffect;
                return borderEffect;
            }
            set
            {
                if (_annotation is PdfPolygonAnnotation)
                    ((PdfPolygonAnnotation)_annotation).BorderEffect = value;
                else if (_annotation is PdfRectangularAnnotation)
                    ((PdfRectangularAnnotation)_annotation).BorderEffect = value;
                else if (_annotation is PdfFreeTextAnnotation)
                    ((PdfFreeTextAnnotation)_annotation).BorderEffect = value;
            }
        }

        /// <summary>
        /// Gets or sets the border effect intensity of annotation.
        /// </summary>
        private float BorderEffectIntensity
        {
            get
            {
                float borderEffectIntensity = 0f;
                if (_annotation is PdfPolygonAnnotation)
                    borderEffectIntensity = ((PdfPolygonAnnotation)_annotation).BorderEffectIntensity;
                else if (_annotation is PdfRectangularAnnotation)
                    borderEffectIntensity = ((PdfRectangularAnnotation)_annotation).BorderEffectIntensity;
                else if (_annotation is PdfFreeTextAnnotation)
                    borderEffectIntensity = ((PdfFreeTextAnnotation)_annotation).BorderEffectIntensity;
                return borderEffectIntensity;
            }
            set
            {
                if (_annotation is PdfPolygonAnnotation)
                    ((PdfPolygonAnnotation)_annotation).BorderEffectIntensity = value;
                else if (_annotation is PdfRectangularAnnotation)
                    ((PdfRectangularAnnotation)_annotation).BorderEffectIntensity = value;
                else if (_annotation is PdfFreeTextAnnotation)
                    ((PdfFreeTextAnnotation)_annotation).BorderEffectIntensity = value;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the information about annotation.
        /// </summary>
        public void UpdateAnnotationInfo()
        {
            if (BorderEffect == PdfAnnotationBorderEffectType.Unsupported)
                return;

            if (BorderEffect == PdfAnnotationBorderEffectType.Cloudy)
            {
                if (BorderEffectIntensity >= 2)
                    styleComboBox.SelectedItem = STYLE_CLOUD;
                else
                    styleComboBox.SelectedItem = STYLE_SMALL_CLOUD;
            }
            else
                styleComboBox.SelectedItem = STYLE_NONE;
        }

        /// <summary>
        /// The border style of annotation is changed.
        /// </summary>
        private void styleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedItem = string.Empty;
            if (styleComboBox.SelectedItem != null)
                selectedItem = styleComboBox.SelectedItem.ToString();

            bool propertyIsChanged = false;

            switch (selectedItem)
            {
                case STYLE_SMALL_CLOUD:
                    if (BorderEffect != PdfAnnotationBorderEffectType.Cloudy ||
                        BorderEffectIntensity != 1f)
                    {
                        propertyIsChanged = true;

                        BorderEffect = PdfAnnotationBorderEffectType.Cloudy;
                        BorderEffectIntensity = 1f;
                    }
                    break;

                case STYLE_CLOUD:
                    if (BorderEffect != PdfAnnotationBorderEffectType.Cloudy ||
                        BorderEffectIntensity != 2f)
                    {
                        propertyIsChanged = true;

                        BorderEffect = PdfAnnotationBorderEffectType.Cloudy;
                        BorderEffectIntensity = 2f;
                    }
                    break;

                default:
                    if (BorderEffect != PdfAnnotationBorderEffectType.Solid)
                    {
                        propertyIsChanged = true;

                        BorderEffect = PdfAnnotationBorderEffectType.Solid;
                    }
                    break;
            }

            if (propertyIsChanged)
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
        /// Occurs when the property value is changed.
        /// </summary>
        public event EventHandler PropertyValueChanged;

        #endregion

    }
}
