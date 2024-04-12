using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Drawing;
using Vintasoft.Imaging.Pdf.Tree;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to set the blending settings for PDF pen or PDF brush.
    /// </summary>
    public partial class SelectHighlightWindow : Window
    {

        #region Fields

        PdfPen _pen;
        PdfBrush _brush;

        #endregion



        #region Constructor

        private SelectHighlightWindow()
        {
            InitializeComponent();
        }

        public SelectHighlightWindow(PdfPen pen)
            : this()
        {
            _pen = pen;
            propertyGrid.SelectedObject = _pen;
            this.Title = "Pen";
        }

        public SelectHighlightWindow(PdfBrush brush)
            : this()
        {
            _brush = brush;
            propertyGrid.SelectedObject = _brush;
            this.Title = "Brush";
        }

        #endregion



        #region Properties

        public int ModeIndex
        {
            get
            {
                return modeComboBox.SelectedIndex;
            }
            set
            {
                modeComboBox.SelectedIndex = value;
                modeComboBox_SelectionChanged(modeComboBox, null);
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Handles the Click event of okButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of cancelButton object.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Handles the SelectionChanged event of modeComboBox object.
        /// </summary>
        private void modeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GraphicsStateBlendMode blendingMode = GraphicsStateBlendMode.Normal;
            System.Drawing.Color blendColor = System.Drawing.Color.Black;
            switch (modeComboBox.SelectedIndex)
            {
                //Text highlight (yellow)
                case 0:
                    blendingMode = GraphicsStateBlendMode.Multiply;
                    blendColor = System.Drawing.Color.Yellow;
                    break;
                //Text highlight (red)
                case 1:
                    blendingMode = GraphicsStateBlendMode.Multiply;
                    blendColor = System.Drawing.Color.Red;
                    break;
                //Text highlight (green)
                case 2:
                    blendingMode = GraphicsStateBlendMode.Multiply;
                    blendColor = System.Drawing.Color.Lime;
                    break;
                //Invert
                case 3:
                    blendingMode = GraphicsStateBlendMode.Difference;
                    blendColor = System.Drawing.Color.White;
                    break;
                //Soft light
                case 4:
                    blendingMode = GraphicsStateBlendMode.SoftLight;
                    blendColor = System.Drawing.Color.White;
                    break;
                //Soft light (red)
                case 5:
                    blendingMode = GraphicsStateBlendMode.SoftLight;
                    blendColor = System.Drawing.Color.Red;
                    break;
                //Soft light (green)
                case 6:
                    blendingMode = GraphicsStateBlendMode.SoftLight;
                    blendColor = System.Drawing.Color.Lime;
                    break;
                //Soft light (blue)
                case 7:
                    blendingMode = GraphicsStateBlendMode.SoftLight;
                    blendColor = System.Drawing.Color.Blue;
                    break;
                //Hue (red)
                case 8:
                    blendingMode = GraphicsStateBlendMode.Hue;
                    blendColor = System.Drawing.Color.Red;
                    break;
                //Hue (green)
                case 9:
                    blendingMode = GraphicsStateBlendMode.Hue;
                    blendColor = System.Drawing.Color.Lime;
                    break;
                //Hue (blue)
                case 10:
                    blendingMode = GraphicsStateBlendMode.Hue;
                    blendColor = System.Drawing.Color.Blue;
                    break;
            }
            if (_pen != null)
            {
                _pen.BlendMode = blendingMode;
                _pen.Color = blendColor;
                propertyGrid.SelectedObject = _pen;
            }
            else if(_brush != null)
            {
                _brush.BlendMode = blendingMode;
                _brush.Color = blendColor;
                propertyGrid.SelectedObject = _brush;
            }
        }

        #endregion

    }
}
