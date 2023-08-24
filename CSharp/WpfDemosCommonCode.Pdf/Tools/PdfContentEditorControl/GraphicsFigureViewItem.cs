using Vintasoft.Imaging.Pdf.Drawing.GraphicsFigures;
using Vintasoft.Imaging.Pdf.Wpf.UI;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Provides GraphicsFigureView item.
    /// </summary>
    public class GraphicsFigureViewItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsFigureViewItem"/> class.
        /// </summary>
        /// <param name="figureView">The figure view.</param>
        public GraphicsFigureViewItem(WpfGraphicsFigureView figureView)
        {
            _figureView = figureView;
        }



        WpfGraphicsFigureView _figureView;
        /// <summary>
        /// Gets the figure view.
        /// </summary>
        public WpfGraphicsFigureView FigureView
        {
            get
            {
                return _figureView;
            }
        }



        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return GetDescription(FigureView);
        }

        /// <summary>
        /// Returns description of specified figure view.
        /// </summary>
        /// <param name="figureView">The figure view.</param>
        /// <returns>Description of figure view.</returns>
        public static string GetDescription(WpfGraphicsFigureView figureView)
        {
            GraphicsFigure figure = figureView.Figure;
            if (figure is ContentStreamGraphicsFigure)
                return string.Format("Content ({0})", figure.ContentType);
            if (figure is ContentStreamGraphicsFigureTextGroup)
                return "Text content group";
            if (figure is ContentStreamGraphicsFigureGroup)
                return string.Format("Content group ({0})", figure.ContentType);
            return figureView.Figure.GetType().Name;
        }

    }
}
