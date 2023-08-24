using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;
using Vintasoft.Imaging.Pdf.Drawing;
using Vintasoft.Imaging.Wpf;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit properties of the <see cref="PdfInteractiveFormListBoxField"/>.
    /// </summary>
    public partial class PdfListBoxPropertiesEditorControl : UserControl, IPdfInteractiveFormPropertiesEditor
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfListBoxPropertiesEditorControl"/> class.
        /// </summary>
        public PdfListBoxPropertiesEditorControl()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        PdfInteractiveFormListBoxField _field = null;
        /// <summary>
        /// Gets or sets the <see cref="PdfInteractiveFormListBoxField"/>.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public PdfInteractiveFormListBoxField Field
        {
            get
            {
                return _field;
            }
            set
            {
                _field = value;
                mainGrid.IsEnabled = _field != null;
                pdfInteractiveFormChoiceFieldEditorControl.Field = _field;

                UpdateFieldInfo();
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
                Field = value as PdfInteractiveFormListBoxField;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the field information.
        /// </summary>
        public void UpdateFieldInfo()
        {
            if (_field == null)
                return;

            multiselectCheckBox.IsChecked = _field.IsMultiSelect;
            PdfBrush selectionBrush = _field.SelectionBrush;
            if (selectionBrush == null)
                selectionBrushColorPanelControl.Color = Colors.Transparent;
            else
                selectionBrushColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(selectionBrush.Color);
            pdfInteractiveFormChoiceFieldEditorControl.UpdateFieldInfo();
        }

        /// <summary>
        /// "Multiselect" flag is changed.
        /// </summary>
        private void multiselectCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (multiselectCheckBox.IsChecked.Value == true)
                _field.IsMultiSelect = true;
            else
                _field.IsMultiSelect = false;

            if (!_field.IsMultiSelect)
            {
                if (_field.FieldValue is string[])
                {
                    string[] fieldValue = (string[])_field.FieldValue;
                    if (fieldValue.Length > 1)
                        _field.FieldValue = fieldValue[0];
                }

                if (_field.FieldDefaultValue is string[])
                {
                    string[] fieldDefaultValue = (string[])_field.FieldDefaultValue;
                    if (fieldDefaultValue.Length > 1)
                        _field.FieldDefaultValue = fieldDefaultValue[0];
                }
            }

            pdfInteractiveFormChoiceFieldEditorControl.UpdateFieldInfo();
        }

        /// <summary>
        /// "SelectionBrush" property is changed.
        /// </summary>
        private void selectionBrushColorPanelControl_ColorChanged(object sender, EventArgs e)
        {
            _field.SelectionBrush = new PdfBrush(WpfObjectConverter.CreateDrawingColor(selectionBrushColorPanelControl.Color));
            OnPropertyValueChanged();
        }

        /// <summary>
        /// The property value is changed.
        /// </summary>
        private void pdfInteractiveFormChoiceFieldEditorControl_PropertyValueChanged(object sender, EventArgs e)
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
