using System;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;
using Vintasoft.Imaging.Wpf;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit properties of the <see cref="PdfFreeTextAnnotation"/>.
    /// </summary>
    public partial class PdfFreeTextAnnotationPropertiesEditorControl : UserControl, IPdfAnnotationPropertiesEditor
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of
        /// the <see cref="PdfFreeTextAnnotationPropertiesEditorControl"/> class.
        /// </summary>
        public PdfFreeTextAnnotationPropertiesEditorControl()
        {
            InitializeComponent();

            foreach (TextQuaddingType type in Enum.GetValues(typeof(TextQuaddingType)))
                textQuaddingComboBox.Items.Add(type);

            foreach (PdfAnnotationLineEndingStyle style in Enum.GetValues(typeof(PdfAnnotationLineEndingStyle)))
                lineEndingStyleComboBox.Items.Add(style);
        }

        #endregion



        #region Properties

        PdfFreeTextAnnotation _annotation;
        /// <summary>
        /// Gets or sets the annotation.
        /// </summary>
        public PdfFreeTextAnnotation Annotation
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
                Annotation = value as PdfFreeTextAnnotation;
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

            textBox.Text = _annotation.Contents;
            textQuaddingComboBox.SelectedItem = _annotation.TextQuadding;
            backColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(_annotation.Color);
            pdfFontPanelControl.PdfFont = _annotation.Font;
            if (pdfFontPanelControl.PdfDocument == null)
                pdfFontPanelControl.PdfDocument = _annotation.Document;
            fontSizeNumericUpDown.Value = Convert.ToInt32(_annotation.FontSize);
            lineEndingStyleComboBox.SelectedItem = _annotation.LineEndingStyle;

            if (_annotation.CalloutLinePoints == null ||
                _annotation.CalloutLinePoints.Length == 0)
                lineEndingStyleComboBox.IsEnabled = false;
            else
                lineEndingStyleComboBox.IsEnabled = true;

            pdfAnnotationBorderEffectEditorControl1.UpdateAnnotationInfo();
        }

        /// <summary>
        /// Field text is changed.
        /// </summary>
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_annotation.Contents != textBox.Text)
            {
                _annotation.Contents = textBox.Text;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// Field text quadding is changed.
        /// </summary>
        private void textQuaddingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextQuaddingType textQuadding = (TextQuaddingType)textQuaddingComboBox.SelectedItem;
            if (_annotation.TextQuadding != textQuadding)
            {
                _annotation.TextQuadding = textQuadding;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// Field color is changed.
        /// </summary>
        private void backColorPanelControl_ColorChanged(object sender, EventArgs e)
        {
            System.Drawing.Color color = WpfObjectConverter.CreateDrawingColor(backColorPanelControl.Color);
            if (_annotation.Color != color)
            {
                _annotation.Color = color;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// Field font is changed.
        /// </summary>
        private void pdfFontPanelControl_PdfFontChanged(object sender, EventArgs e)
        {
            _annotation.Font = pdfFontPanelControl.PdfFont;
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Field font size is changed.
        /// </summary>
        private void fontSizeNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            float fontSize = Convert.ToSingle(fontSizeNumericUpDown.Value);
            if (_annotation.FontSize != fontSize)
            {
                _annotation.FontSize = fontSize;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// Field border effect is changed.
        /// </summary>
        private void PdfAnnotationBorderEffectEditorControl_PropertyValueChanged(
            object sender,
            EventArgs e)
        {
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Field line ending style is changed.
        /// </summary>
        private void lineEndingStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PdfAnnotationLineEndingStyle lineEndingStyle =
                (PdfAnnotationLineEndingStyle)lineEndingStyleComboBox.SelectedItem;
            if (_annotation.LineEndingStyle != lineEndingStyle)
            {
                _annotation.LineEndingStyle = lineEndingStyle;
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
