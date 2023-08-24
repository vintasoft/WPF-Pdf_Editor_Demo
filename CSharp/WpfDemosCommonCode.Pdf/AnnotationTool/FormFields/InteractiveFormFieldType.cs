namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Specifies available types of <see cref="PdfInteractiveFormField"/>, which can be built.
    /// </summary>
    public enum InteractiveFormFieldType
    {
        /// <summary>
        /// Unknown type.
        /// </summary>
        Unknown,

        /// <summary>
        /// The text field.
        /// </summary>
        TextField,

        /// <summary>
        /// The check box.
        /// </summary>
        CheckBox,

        /// <summary>
        /// The push button.
        /// </summary>
        PushButton,

        /// <summary>
        /// The list box.
        /// </summary>
        ListBox,

        /// <summary>
        /// The combo box.
        /// </summary>
        ComboBox,

        /// <summary>
        /// The radio button.
        /// </summary>
        RadioButton,

        /// <summary>
        /// The field with barcode determines PDF specification.
        /// </summary>
        BarcodeField,

        /// <summary>
        /// The field with any barcode.
        /// (The barcodes is not supported PDF specification).
        /// </summary>
        VintasoftBarcodeField,

        /// <summary>
        /// The signature field.
        /// </summary>
        SignatureField,
    }
}
