using System;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit properties of
    /// the <see cref="PdfInteractiveFormCheckBoxField"/>.
    /// </summary>
    public partial class PdfCheckBoxPropertiesEditorControl : UserControl, IPdfInteractiveFormPropertiesEditor
    {

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfCheckBoxPropertiesEditorControl"/> class.
        /// </summary>
        public PdfCheckBoxPropertiesEditorControl()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        PdfInteractiveFormCheckBoxField _field = null;
        /// <summary>
        /// Gets or sets the <see cref="PdfInteractiveFormCheckBoxField"/>.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public PdfInteractiveFormCheckBoxField Field
        {
            get
            {
                return _field;
            }
            set
            {
                _field = value;

                pdfInteractiveFormSwitchableButtonPropertiesEditorControl.Field = _field;
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
                Field = value as PdfInteractiveFormCheckBoxField;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates information of the field.
        /// </summary>
        public void UpdateFieldInfo()
        {
            pdfInteractiveFormSwitchableButtonPropertiesEditorControl.UpdateFieldInfo();
        }

        /// <summary>
        /// Property of interactive form is changed.
        /// </summary>
        private void pdfInteractiveFormSwitchableButtonPropertiesEditorControl_PropertyValueChanged(object sender, EventArgs e)
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
        /// Occurs when the property value is changed.
        /// </summary>
        public event EventHandler PropertyValueChanged;

        #endregion
        
    }
}
