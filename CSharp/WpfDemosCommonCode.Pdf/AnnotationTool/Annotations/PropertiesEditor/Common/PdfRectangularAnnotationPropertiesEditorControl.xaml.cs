using System;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Wpf;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit properties of
    /// the <see cref="PdfRectangularAnnotation"/>.
    /// </summary>
    public partial class PdfRectangularAnnotationPropertiesEditorControl : UserControl, IPdfAnnotationPropertiesEditor
    {

        #region Fields

        /// <summary>
        /// The value indicating whether the padding must be updated automatically.
        /// </summary>
        private static bool _autoUpdatePadding = true;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of
        /// the <see cref="PdfRectangularAnnotationPropertiesEditorControl"/> class.
        /// </summary>
        public PdfRectangularAnnotationPropertiesEditorControl()
        {
            InitializeComponent();

            autoUpdatePaddingCheckBox.IsChecked = _autoUpdatePadding;
        }

        #endregion



        #region Properties

        PdfRectangularAnnotation _annotation;
        /// <summary>
        /// Gets or sets the annotation.
        /// </summary>
        public PdfRectangularAnnotation Annotation
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
        /// Gets a value indicating whether the padding must be updated automatically.
        /// </summary>
        public bool AutoUpdatePadding
        {
            get
            {
                if (autoUpdatePaddingCheckBox.IsChecked.Value == true)
                    return true;
                else
                    return false;
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
                Annotation = value as PdfRectangularAnnotation;
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
            paddingPanelControl.PaddingValue = _annotation.Padding;

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
        /// Field padding is changed.
        /// </summary>
        private void paddingPanelControl_PaddingValueChanged(object sender, EventArgs e)
        {
            _annotation.Padding = paddingPanelControl.PaddingValue;
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Field border effect is changed.
        /// </summary>
        private void PdfAnnotationBorderEffectEditorControl_PropertyValueChanged(object sender, EventArgs e)
        {
            paddingPanelControl.PaddingValue = _annotation.Padding;
            OnPropertyValueChanged();
        }

        /// <summary>
        /// "Auto" check box is changed.
        /// </summary>
        private void autoUpdatePaddingCheckBox_CheckedChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                if (autoUpdatePaddingCheckBox.IsChecked.Value == true)
                    _autoUpdatePadding = true;
                else
                    _autoUpdatePadding = false;
                paddingPanelControl.IsEnabled = !_autoUpdatePadding;
                OnPropertyValueChanged();
            }
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
