namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// Specifies available tree types of processing result.
    /// </summary>
    public enum ProcessingResultTreeType
    {
        /// <summary>
        /// All nodes of the processing result tree will be displayed.
        /// </summary>
        Detailed,

        /// <summary>
        /// Only the triggers or the results of executed commands will be displayed.
        /// The tree nodes will be grouped by document or page.
        /// </summary>
        ByPage,
    }
}
