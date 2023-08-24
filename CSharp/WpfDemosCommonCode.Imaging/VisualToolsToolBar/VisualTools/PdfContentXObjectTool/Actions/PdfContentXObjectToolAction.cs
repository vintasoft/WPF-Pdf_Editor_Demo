using System.Windows.Media;
using System.Windows.Media.Imaging;
using Vintasoft.Imaging.Pdf.Wpf.UI;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// Stores information about a <see cref="WpfPdfContentXObjectTool"/> action.
    /// </summary>
    public class PdfContentXObjectToolAction : VisualToolAction
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfContentXObjectToolAction"/> class.
        /// </summary>
        /// <param name="visualTool">The visual tool.</param>
        /// <param name="text">The action text.</param>
        /// <param name="toolTip">The action tool tip.</param>
        /// <param name="icon">The action icon.</param>
        /// <param name="subactions">The sub actions of the action.</param>
        public PdfContentXObjectToolAction(
            WpfPdfContentXObjectTool visualTool,
            string text,
            string toolTip,
            BitmapSource icon,
            params VisualToolAction[] subactions)
            : base(visualTool, text, toolTip, icon, subactions)
        {
            visualTool.SelectionImagesBrush = new SolidColorBrush(Color.FromArgb(32, 0, 0, 255));
            visualTool.SelectionImagesPen = new Pen(Brushes.Red, 1);
            visualTool.SelectionFormsBrush = new SolidColorBrush(Color.FromArgb(32, 0, 255, 0));
            visualTool.SelectionFormsPen = new Pen(Brushes.Blue, 1);
        }

        #endregion

    }
}
