namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Specifies available types of <see cref="PdfAnnotation"/>, which can be built.
    /// </summary>
    internal enum AnnotationType
    {
        /// <summary>
        /// Unknown type.
        /// </summary>
        Unknown,

        /// <summary>
        /// The line annotation.
        /// </summary>
        Line,

        /// <summary>
        /// The line annnotation with arrow.
        /// </summary>
        LineWithArrow,

        /// <summary>
        /// The line annnotation with arrows.
        /// </summary>
        LineWithArrows,

        /// <summary>
        /// The freehand lines annotation.
        /// </summary>
        Ink,

        /// <summary>
        /// The rectangle annotation.
        /// </summary>
        Rectangle,

        /// <summary>
        /// The filled rectangle annotation.
        /// </summary>
        FilledRectangle,

        /// <summary>
        /// The rectangle annotation with cloud border.
        /// </summary>
        CloudRectangle,

        /// <summary>
        /// The filled rectangle annotation with cloud border.
        /// </summary>
        CloudFilledRectangle,

        /// <summary>
        /// The ellipse annotation.
        /// </summary>
        Ellipse,

        /// <summary>
        /// The filled ellipse annotation.
        /// </summary>
        FilledEllipse,

        /// <summary>
        /// The ellipse annotation with cloud border.
        /// </summary>
        CloudEllipse,

        /// <summary>
        /// The filled ellipse annotation with cloud border.
        /// </summary>
        CloudFilledEllipse,

        /// <summary>
        /// The polyline annotation.
        /// </summary>
        Polyline,

        /// <summary>
        /// The polyline annotation with arrow.
        /// </summary>
        PolylineWithArrow,

        /// <summary>
        /// The polyline annotation with arrows.
        /// </summary>
        PolylineWithArrows,

        /// <summary>
        /// The freehand polyline annotation.
        /// </summary>
        FreehandPolyline,

        /// <summary>
        /// The polygon annotation.
        /// </summary>
        Polygon,

        /// <summary>
        /// The filled polygon annotation.
        /// </summary>
        FilledPolygon,

        /// <summary>
        /// The polygon annotation with cloud border.
        /// </summary>
        CloudPolygon,

        /// <summary>
        /// The filled polygon annotation with cloud border.
        /// </summary>
        CloudFilledPolygon,

        /// <summary>
        /// The freehand polygon annotation.
        /// </summary>
        FreehandPolygon,

        /// <summary>
        /// The link annotation.
        /// </summary>
        Link,

        /// <summary>
        /// The label annotation.
        /// </summary>
        Label,

        /// <summary>
        /// The text annotation.
        /// </summary>
        Text,

        /// <summary>
        /// The text annotation with cloud border.
        /// </summary>
        CloudText,

        /// <summary>
        /// The text annotation and line annotation with arrow.
        /// </summary>
        FreeText,

        /// <summary>
        /// The text annotation with cloud border and line annotation with arrow.
        /// </summary>
        CloudFreeText,

        /// <summary>
        /// The file attachment annotation.
        /// </summary>
        FileAttachment,

        /// <summary>
        /// The file attachment annotation with 'Graph' icon.
        /// </summary>
        GraphFileAttachment,

        /// <summary>
        /// The file attachment annotation with 'Pin' icon.
        /// </summary>
        PushPinFileAttachment,

        /// <summary>
        /// The file attachment annotation with 'Paperclip' icon.
        /// </summary>
        PaperclipFileAttachment,

        /// <summary>
        /// The file attachment annotation with 'Tag' icon.
        /// </summary>
        TagFileAttachment,


        /// <summary>
        /// The text annotation with stick name "Comment".
        /// </summary>
        Text_Comment,

        /// <summary>
        /// The text annotation with stick name "Check".
        /// </summary>
        Text_Check,

        /// <summary>
        /// The text annotation with stick name "Checkmark".
        /// </summary>
        Text_Checkmark,

        /// <summary>
        /// The text annotation with stick name "Circle".
        /// </summary>
        Text_Circle,

        /// <summary>
        /// The text annotation with stick name "Rectangle".
        /// </summary>
        Text_Rectangle,

        /// <summary>
        /// The text annotation with stick name "Cross".
        /// </summary>
        Text_Cross,

        /// <summary>
        /// The text annotation with stick name "Cross hairs".
        /// </summary>
        Text_CrossHairs,

        /// <summary>
        /// The text annotation with stick name "Help".
        /// </summary>
        Text_Help,

        /// <summary>
        /// The text annotation with stick name "Insert".
        /// </summary>
        Text_Insert,

        /// <summary>
        /// The text annotation with stick name "Key".
        /// </summary>
        Text_Key,

        /// <summary>
        /// The text annotation with stick name "New paragraph".
        /// </summary>
        Text_NewParagraph,

        /// <summary>
        /// The text annotation with stick name "Note".
        /// </summary>
        Text_Note,

        /// <summary>
        /// The text annotation with stick name "Paragraph".
        /// </summary>
        Text_Paragraph,

        /// <summary>
        /// The text annotation with stick name "Right arrow".
        /// </summary>
        Text_RightArrow,

        /// <summary>
        /// The text annotation with stick name "Right pointer".
        /// </summary>
        Text_RightPointer,

        /// <summary>
        /// The text annotation with stick name "Star".
        /// </summary>
        Text_Star,

        /// <summary>
        /// The text annotation with stick name "Up arrow".
        /// </summary>
        Text_UpArrow,

        /// <summary>
        /// The text annotation with stick name "Up left arrow".
        /// </summary>
        Text_UpLeftArrow,

        /// <summary>
        /// Office Document: empty document.
        /// </summary>
        EmptyOfficeDocument,

        /// <summary>
        /// Office Document: chart.
        /// </summary>
        Chart,

        /// <summary>
        /// Office Document.
        /// </summary>
        OfficeDocument,
    }
}
