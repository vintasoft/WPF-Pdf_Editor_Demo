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

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit properties of
    /// the <see cref="PdfInteractiveFormRadioButtonField"/>.
    /// </summary>
    public partial class PdfRadioButtonPropertiesEditorControl : UserControl, IPdfInteractiveFormPropertiesEditor
    {

        #region Fields

        /// <summary>
        /// A string that contains name of the current appearance state.
        /// </summary>
        string _currentAppearanceState;

        /// <summary>
        /// A string that contains name of the "Off" appearance state.
        /// </summary>
        string _offAppearanceState;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of 
        /// the <see cref="PdfRadioButtonPropertiesEditorControl"/> class.
        /// </summary>
        public PdfRadioButtonPropertiesEditorControl()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        PdfInteractiveFormRadioButtonField _field = null;
        /// <summary>
        /// Gets or sets the <see cref="PdfInteractiveFormRadioButtonField"/>.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public PdfInteractiveFormRadioButtonField Field
        {
            get
            {
                return _field;
            }
            set
            {
                _field = value;

                // if radio button field is set
                if (_field != null)
                {
                    // get the appearance states of the field
                    string[] availableAppearanceStates = _field.GetAvailableAppearanceStateNames();
                    // set the the current appearance state to the the field appearance state
                    _currentAppearanceState = _field.Value;
                    // if the current appearance state is not present in appearance states of the field
                    if (Array.IndexOf(availableAppearanceStates, _currentAppearanceState) == -1)
                        // set the current appearance state to "Off"
                        _currentAppearanceState = "Off";

                    // if radio button field is a part of radio button group
                    if (_field.Parent is PdfInteractiveFormRadioButtonGroupField)
                    {
                        // get the radio button group of this radio button
                        PdfInteractiveFormRadioButtonGroupField radioButtonGroup =
                            (PdfInteractiveFormRadioButtonGroupField)_field.Parent;

                        // set the "Off" appearance state to
                        // the checked appearance state of radio button group
                        _offAppearanceState = radioButtonGroup.CheckedAppearanceStateName;
                        // if the "Off" appearance state and current appearance state are equal
                        if (_offAppearanceState == _currentAppearanceState)
                        {
                            // set the "Off" appearance state to
                            // the default checked appearance state of radio button group
                            _offAppearanceState = radioButtonGroup.DefaultCheckedAppearanceStateName;
                            // if the "Off" appearance state and current appearance state are equal
                            if (_offAppearanceState == _currentAppearanceState)
                            {
                                // for each field in radio button group
                                foreach (PdfInteractiveFormField formField in radioButtonGroup.Kids)
                                {
                                    // if field is radio button
                                    if (formField is PdfInteractiveFormRadioButtonField)
                                    {
                                        // get the radio button
                                        PdfInteractiveFormRadioButtonField radioButtonField =
                                            (PdfInteractiveFormRadioButtonField)formField;

                                        // get the checked appearance state of radio button
                                        string checkedAppearanceStateName = radioButtonField.GetCheckedAppearanceStateName();
                                        // the "Off" appearance state does not equal
                                        // the checked appearance state of radio button
                                        if (_offAppearanceState != checkedAppearanceStateName)
                                        {
                                            // set the "Off" checked appearance state to
                                            // the checked appearance state of radio button
                                            _offAppearanceState = checkedAppearanceStateName;
                                            break;
                                        }
                                    }
                                }

                                // if the "Off" appearance state and the field checked appearance state are equals
                                if (_offAppearanceState == _field.GetCheckedAppearanceStateName())
                                    // clear the "Off" appearance state
                                    _offAppearanceState = string.Empty;
                            }
                        }
                    }
                }

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
                Field = value as PdfInteractiveFormRadioButtonField;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates information about the field.
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
            // if the appearance state of radio button field is changed
            if (_field.Value != _currentAppearanceState)
            {
                // if radio button field is a part of radio button group
                if (_field.Parent is PdfInteractiveFormRadioButtonGroupField)
                {
                    // save the radio button appearance state as the current appearance state
                    _currentAppearanceState = _field.Value;

                    // get the radio button group of this radio button
                    PdfInteractiveFormRadioButtonGroupField radioButtonGroup =
                        (PdfInteractiveFormRadioButtonGroupField)_field.Parent;

                    // if current appearance state is "Off"
                    if (_currentAppearanceState == "Off")
                        // check the default radio button in radio button group
                        radioButtonGroup.CheckedAppearanceStateName = _offAppearanceState;
                    else
                        // check the current radio button
                        radioButtonGroup.CheckedAppearanceStateName = _currentAppearanceState;
                }
            }

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
