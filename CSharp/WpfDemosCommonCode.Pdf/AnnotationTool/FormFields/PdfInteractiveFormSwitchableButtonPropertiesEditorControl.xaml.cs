using System;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit properties of
    /// the <see cref="PdfInteractiveFormSwitchableButtonField"/>.
    /// </summary>
    public partial class PdfInteractiveFormSwitchableButtonPropertiesEditorControl : UserControl, IPdfInteractiveFormPropertiesEditor
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of
        /// the <see cref="PdfInteractiveFormSwitchableButtonPropertiesEditorControl"/> class.
        /// </summary>
        public PdfInteractiveFormSwitchableButtonPropertiesEditorControl()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        PdfInteractiveFormSwitchableButtonField _field = null;
        /// <summary>
        /// Gets or sets the <see cref="PdfInteractiveFormSwitchableButtonField"/>.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public PdfInteractiveFormSwitchableButtonField Field
        {
            get
            {
                return _field;
            }
            set
            {
                _field = value;

                if (_field != null && _field.Annotation != null)
                {
                    pdfAnnotationAppearancesEditorControl.Annotation = _field.Annotation;

                    UpdateFieldInfo();
                }
            }
        }

        /// <summary>
        /// Gets or sets the PDF interactive form field.
        /// </summary>
        PdfInteractiveFormField IPdfInteractiveFormPropertiesEditor.Field
        {
            get
            {
                return Field;
            }
            set
            {
                Field = value as PdfInteractiveFormSwitchableButtonField;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates information about the field.
        /// </summary>
        public void UpdateFieldInfo()
        {
            currentAppearanceStateComboBox.BeginInit();
            currentAppearanceStateComboBox.Items.Clear();
            if (_field != null)
            {
                string[] names = _field.GetAvailableAppearanceStateNames();
                foreach (string name in names)
                    currentAppearanceStateComboBox.Items.Add(name);

                string appearanceStateName = _field.Value;
                if (Array.IndexOf(names, appearanceStateName) == -1)
                    appearanceStateName = "Off";
                currentAppearanceStateComboBox.SelectedItem = appearanceStateName;
            }
            currentAppearanceStateComboBox.EndInit();

            pdfAnnotationAppearancesEditorControl.UpdateAppearance();
        }

        /// <summary>
        /// Name of apeearance state is changed.
        /// </summary>
        private void currentAppearanceStateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string value = currentAppearanceStateComboBox.SelectedItem.ToString();

            if (_field.Value != value)
            {
                _field.Value = value;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// Appearance of annotation is changed.
        /// </summary>
        private void pdfAnnotationAppearancesEditorControl_AppearanceChanged(object sender, EventArgs e)
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
