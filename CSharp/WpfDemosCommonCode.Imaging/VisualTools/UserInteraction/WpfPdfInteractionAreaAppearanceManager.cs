using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;
using Vintasoft.Imaging.Pdf.Wpf.UI.Annotations;

using Vintasoft.Imaging.Wpf.UI.VisualTools.UserInteraction;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// Manages and stores the settings for interaction areas of PDF visual tool.
    /// </summary>
    public class WpfPdfInteractionAreaAppearanceManager : WpfInteractionAreaAppearanceManager
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfPdfInteractionAreaAppearanceManager"/> class.
        /// </summary>
        public WpfPdfInteractionAreaAppearanceManager()
        {
        }

        #endregion



        #region Properties

        bool _isEnabledFormFieldSpellChecking = true;
        /// <summary>
        /// Gets or sets a value indicating whether the spell checking of form fields is enabled.
        /// </summary>
        /// <value>
        /// <b>true</b> if spell checking of form fields is enabled; otherwise, <b>false</b>.
        /// </value>
        public bool IsEnabledFormFieldSpellChecking
        {
            get
            {
                return _isEnabledFormFieldSpellChecking;
            }
            set
            {
                if (_isEnabledFormFieldSpellChecking != value)
                {
                    _isEnabledFormFieldSpellChecking = value;

                    ApplyAppearanceSettings();
                }
            }
        }

        bool _isEnabledAnnotationsSpellChecking = true;
        /// <summary>
        /// Gets or sets a value indicating whether the spell checking of annotations is enabled.
        /// </summary>
        /// <value>
        /// <b>true</b> if spell checking of annotations is enabled; otherwise, <b>false</b>.
        /// </value>
        public bool IsEnabledAnnotationsSpellChecking
        {
            get
            {
                return _isEnabledAnnotationsSpellChecking;
            }
            set
            {
                if (_isEnabledAnnotationsSpellChecking != value)
                {
                    _isEnabledAnnotationsSpellChecking = value;

                    ApplyAppearanceSettings();
                }
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Sets the text box settings.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        public override void SetTextBoxSettings(TextBox textBox)
        {
            base.SetTextBoxSettings(textBox);

            bool isSpellCheckEnabled = false;

            // if visual tool is PdfAnnotationTool
            if (VisualTool is WpfPdfAnnotationTool)
            {
                // get PDF annotation tool
                WpfPdfAnnotationTool pdfAnnotationTool = (WpfPdfAnnotationTool)VisualTool;

                // get the spell check status for the focused PDF interactive form field
                isSpellCheckEnabled = GetSpellCheckStatusForPdfInteractiveFormField(pdfAnnotationTool.FocusedField);
            }

            // update spell check status for text box
            textBox.SpellCheck.IsEnabled = isSpellCheckEnabled;
        }


        /// <summary>
        /// Returns the spell check status for PDF interactive form field.
        /// </summary>
        /// <param name="field">PDF interactive form field.</param>
        /// <returns>The spell check status for PDF interactive form field.</returns>
        private bool GetSpellCheckStatusForPdfInteractiveFormField(PdfInteractiveFormField field)
        {
            // if focused field is text field
            if (field is PdfInteractiveFormTextField)
            {
                // get text field
                PdfInteractiveFormTextField textField = (PdfInteractiveFormTextField)field;

                // if field can NOT be changed
                if (textField.IsReadOnly)
                {
                    return false;
                }
                else
                {
                    // if field must NOT be spell checked
                    if (textField.IsDoNotSpellCheck)
                        return false;

                    return IsEnabledFormFieldSpellChecking;
                }
            }
            // if focused field is combo box field
            else if (field is PdfInteractiveFormComboBoxField)
            {
                // get combo box field
                PdfInteractiveFormComboBoxField comboBoxField = (PdfInteractiveFormComboBoxField)field;

                // if field can NOT be changed
                if (comboBoxField.IsReadOnly)
                {
                    return false;
                }
                else
                {
                    // if field must NOT be spell checked
                    if (comboBoxField.IsDoNotSpellCheck)
                        return false;

                    return IsEnabledFormFieldSpellChecking;
                }
            }

            return IsEnabledAnnotationsSpellChecking;
        }

        #endregion

    }
}
