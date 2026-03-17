using System;

namespace WpfCommonCode.Pdf
{
    /// <summary>
    /// Determines available view types for PDF resource tree.
    /// </summary>
    public enum PdfResourceTreeViewType
    {
        /// <summary>
        /// Resources of PDF tree will be shown as a hierarchial tree.
        /// </summary>
        Hierarchical,
        /// <summary>
        /// Resources of PDF tree will be shown as a list.
        /// </summary>
        Linear
    }
}
