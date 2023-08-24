using System;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree.Annotations;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit properties of the <see cref="PdfLinkAnnotation"/>.
    /// </summary>
    public partial class PdfLinkAnnotationPropertiesEditorControl : UserControl, IPdfAnnotationPropertiesEditor
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of
        /// the <see cref="PdfLinkAnnotationPropertiesEditorControl"/> class.
        /// </summary>
        public PdfLinkAnnotationPropertiesEditorControl()
        {
            InitializeComponent();

            foreach (PdfAnnotationHighlightingMode mode in Enum.GetValues(typeof(PdfAnnotationHighlightingMode)))
                highlightingModeComboBox.Items.Add(mode);
        }

        #endregion



        #region Properties

        PdfLinkAnnotation _annotation;
        /// <summary>
        /// Gets or sets the annotation.
        /// </summary>
        public PdfLinkAnnotation Annotation
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
                Annotation = value as PdfLinkAnnotation;
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

            highlightingModeComboBox.SelectedItem = _annotation.HighlightingMode;
            if (pdfActionEditorControl.Document == null)
                pdfActionEditorControl.Document = _annotation.Document;
            pdfActionEditorControl.Action = _annotation.ActivateAction;
        }

        /// <summary>
        /// Field highlighting mode is changed.
        /// </summary>
        private void highlightingModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PdfAnnotationHighlightingMode highlightingMode =
                (PdfAnnotationHighlightingMode)highlightingModeComboBox.SelectedItem;
            if (_annotation.HighlightingMode != highlightingMode)
            {
                _annotation.HighlightingMode = highlightingMode;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// Field active action is changed.
        /// </summary>
        private void pdfActionEditorControl_ActionChanged(object sender, EventArgs e)
        {
            _annotation.ActivateAction = pdfActionEditorControl.Action;
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
