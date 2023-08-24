using System;
using System.Windows;
using System.Windows.Input;

using Vintasoft.Imaging.Wpf.UI.VisualTools;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Observes mouse focus and mouse location in visual tool.
    /// </summary>
    public class VisualToolMouseObserver
    {

        #region Properties

        WpfVisualTool _visualTool;
        /// <summary>
        /// Gets or sets the visual tool, which is monitored.
        /// </summary>
        public WpfVisualTool VisualTool
        {
            get
            {
                return _visualTool;
            }
            set
            {
                if (_visualTool != value)
                {
                    if (_visualTool != null)
                    {
                        _visualTool.MouseEnter -= new MouseEventHandler(visualTool_MouseEnter);
                        _visualTool.MouseLeave -= new MouseEventHandler(visualTool_MouseLeave);
                        _visualTool.MouseMove -= new MouseEventHandler(visualTool_MouseMove);
                    }

                    _visualTool = value;

                    if (_visualTool != null)
                    {
                        _visualTool.MouseEnter += new MouseEventHandler(visualTool_MouseEnter);
                        _visualTool.MouseLeave += new MouseEventHandler(visualTool_MouseLeave);
                        _visualTool.MouseMove += new MouseEventHandler(visualTool_MouseMove);
                    }
                }
            }
        }

        Point _visualToolMouseLocation;
        /// <summary>
        /// Gets the mouse location in visual tool.
        /// </summary>
        public Point VisualToolMouseLocation
        {
            get
            {
                return _visualToolMouseLocation;
            }
        }

        bool _visualToolHasMouseFocus;
        /// <summary>
        /// Gets a value indicating whether the visual tool has mouse focus.
        /// </summary>
        public bool VisualToolHasMouseFocus
        {
            get
            {
                return _visualToolHasMouseFocus;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the MouseMove event of the VisualTool.
        /// </summary>
        private void visualTool_MouseMove(object sender, MouseEventArgs e)
        {
            _visualToolMouseLocation = e.GetPosition((WpfVisualTool)sender);
        }

        /// <summary>
        /// Handles the MouseLeave event of the VisualTool.
        /// </summary>
        private void visualTool_MouseLeave(object sender, MouseEventArgs e)
        {
            _visualToolHasMouseFocus = false;
        }

        /// <summary>
        /// Handles the MouseEnter event of the VisualTool.
        /// </summary>
        private void visualTool_MouseEnter(object sender, MouseEventArgs e)
        {
            _visualToolHasMouseFocus = true;
        }

        #endregion

    }
}
