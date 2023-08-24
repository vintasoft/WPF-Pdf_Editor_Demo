using System;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree.Annotations;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit common properties of the <see cref="PdfAnnotation"/>.
    /// </summary>
    public partial class PdfAnnotationCommonPropertiesEditorControl : UserControl, IPdfAnnotationPropertiesEditor
    {

        #region Fields

        /// <summary>
        /// Determines that the user interface is updating.
        /// </summary>
        bool _isUiUpdating = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAnnotationCommonPropertiesEditorControl"/> class.
        /// </summary>
        public PdfAnnotationCommonPropertiesEditorControl()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        PdfAnnotation _annotation;
        /// <summary>
        /// Gets or sets the annotation.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public PdfAnnotation Annotation
        {
            get
            {
                return _annotation;
            }
            set
            {
                if (_annotation != value)
                {
                    _annotation = value;

                    borderStyleControl.Annotation = value;

                    opacityPanel.IsEnabled = _annotation is PdfMarkupAnnotation;
                    UpdateAnnotationInfo();
                }
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Updates the information about annotation.
        /// </summary>
        public void UpdateAnnotationInfo()
        {
            _isUiUpdating = true;

            try
            {
                borderStyleControl.UpdateBorderInfo();

                if (_annotation == null)
                {
                    nameTextBox.Text = string.Empty;
                    subjectTextBox.Text = string.Empty;
                    titleTextBox.Text = string.Empty;
                }
                else
                {
                    nameTextBox.Text = _annotation.Name;
                    subjectTextBox.Text = _annotation.Subject;
                    titleTextBox.Text = _annotation.Title;

                    lockedCheckBox.IsChecked = _annotation.IsLocked;
                    printableCheckBox.IsChecked = _annotation.IsPrintable;
                    readOnlyCheckBox.IsChecked = _annotation.IsReadOnly;

                    PdfMarkupAnnotation markupAnnotation = _annotation as PdfMarkupAnnotation;
                    if (markupAnnotation == null)
                        opacityNumericUpDown.Value = opacityNumericUpDown.Maximum;
                    else
                        opacityNumericUpDown.Value = Convert.ToInt32(markupAnnotation.Opacity * opacityNumericUpDown.Maximum);
                }
            }
            finally
            {
                _isUiUpdating = false;
            }
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Annotation opacity is changed.
        /// </summary>
        private void opacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = Convert.ToInt32(opacitySlider.Value);
            if (opacityNumericUpDown.Value != value)
                opacityNumericUpDown.Value = value;
        }

        /// <summary>
        /// Annotation opacity is changed.
        /// </summary>
        private void opacityNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            int sliderValue = Convert.ToInt32(opacitySlider.Value);
            if (sliderValue != opacityNumericUpDown.Value)
                opacitySlider.Value = opacityNumericUpDown.Value;

            if (!_isUiUpdating)
            {
                PdfMarkupAnnotation markupAnnotation = _annotation as PdfMarkupAnnotation;

                float opacity = (float)opacityNumericUpDown.Value / (float)opacityNumericUpDown.Maximum;
                markupAnnotation.Opacity = opacity;

                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// Annotation name is changed.
        /// </summary>
        private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUiUpdating)
                return;

            _annotation.Name = nameTextBox.Text;
        }

        /// <summary>
        /// Annotation subject is changed.
        /// </summary>
        private void subjectTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUiUpdating)
                return;

            _annotation.Subject = subjectTextBox.Text;
        }

        /// <summary>
        /// Annotation title is changed.
        /// </summary>
        private void titleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUiUpdating)
                return;

            _annotation.Title = titleTextBox.Text;
        }

        /// <summary>
        /// IsLocked flag of annotation is changed.
        /// </summary>
        private void lockedCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_isUiUpdating)
                return;

            if (lockedCheckBox.IsChecked.Value == true)
                _annotation.IsLocked = true;
            else
                _annotation.IsLocked = false;
        }

        /// <summary>
        /// IsPrintable flag of annotation is changed.
        /// </summary>
        private void printableCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_isUiUpdating)
                return;

            if (printableCheckBox.IsChecked.Value == true)
                _annotation.IsPrintable = true;
            else
                _annotation.IsPrintable = false;
        }

        /// <summary>
        /// IsReadOnly flag of annotation is changed.
        /// </summary>
        private void readOnlyCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_isUiUpdating)
                return;

            if (readOnlyCheckBox.IsChecked.Value == true)
                _annotation.IsReadOnly = true;
            else
                _annotation.IsReadOnly = false;
        }

        /// <summary>
        /// Annotation border is changed.
        /// </summary>
        private void borderStyleControl_PropertyValueChanged(object sender, EventArgs e)
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

        #endregion



        #region Events

        /// <summary>
        /// Occurs when value of property is changed.
        /// </summary>
        public event EventHandler PropertyValueChanged;

        #endregion

    }
}
