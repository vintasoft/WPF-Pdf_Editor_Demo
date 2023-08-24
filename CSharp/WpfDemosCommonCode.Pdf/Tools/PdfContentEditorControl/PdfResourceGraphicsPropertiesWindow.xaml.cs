using System.Windows;

using Vintasoft.Imaging.Pdf.Content;
using Vintasoft.Imaging.Pdf.Tree;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to edit the PDF content graphics properties of XObject resource.
    /// </summary>
    public partial class PdfResourceGraphicsPropertiesWindow : Window
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfResourceGraphicsPropertiesWindow"/> class.
        /// </summary>
        public PdfResourceGraphicsPropertiesWindow()
        {
            InitializeComponent();

            colorBlendingComboBox.Items.Add(GraphicsStateBlendMode.Normal);
            colorBlendingComboBox.Items.Add(GraphicsStateBlendMode.Color);
            colorBlendingComboBox.Items.Add(GraphicsStateBlendMode.ColorBurn);
            colorBlendingComboBox.Items.Add(GraphicsStateBlendMode.ColorDodge);
            colorBlendingComboBox.Items.Add(GraphicsStateBlendMode.Darken);
            colorBlendingComboBox.Items.Add(GraphicsStateBlendMode.Difference);
            colorBlendingComboBox.Items.Add(GraphicsStateBlendMode.Exclusion);
            colorBlendingComboBox.Items.Add(GraphicsStateBlendMode.HardLight);
            colorBlendingComboBox.Items.Add(GraphicsStateBlendMode.Hue);
            colorBlendingComboBox.Items.Add(GraphicsStateBlendMode.Lighten);
            colorBlendingComboBox.Items.Add(GraphicsStateBlendMode.Luminosity);
            colorBlendingComboBox.Items.Add(GraphicsStateBlendMode.Multiply);
            colorBlendingComboBox.Items.Add(GraphicsStateBlendMode.Overlay);
            colorBlendingComboBox.Items.Add(GraphicsStateBlendMode.Saturation);
            colorBlendingComboBox.Items.Add(GraphicsStateBlendMode.Screen);
            colorBlendingComboBox.Items.Add(GraphicsStateBlendMode.SoftLight);
            colorBlendingComboBox.SelectedItem = GraphicsStateBlendMode.Normal;

            GraphicsProperties = new PdfContentGraphicsProperties();
        }

        #endregion



        #region Properties

        PdfContentGraphicsProperties _graphicsProperties;
        /// <summary>
        /// Gets or sets the PDF content graphics properties.
        /// </summary>
        public PdfContentGraphicsProperties GraphicsProperties
        {
            get
            {
                return _graphicsProperties;
            }
            set
            {
                _graphicsProperties = value;
                UpdateUI();
            }
        }

        #endregion



        #region Methods

        #region UI

        /// <summary>
        /// Handles the CheckedChanged event of ColorBlendingCheckBox object.
        /// </summary>
        private void colorBlendingCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (colorBlendingCheckBox.IsChecked == true)
                colorBlendingGroupBox.IsEnabled = true;
            else
                colorBlendingGroupBox.IsEnabled = false;
        }

        /// <summary>
        /// Handles the CheckedChanged event of AlphaConstantCheckBox object.
        /// </summary>
        private void alphaConstantCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (alphaConstantCheckBox.IsChecked == true)
                alphaConstantValueEditor.IsEnabled = true;
            else
                alphaConstantValueEditor.IsEnabled = false;
        }

        /// <summary>
        /// Handles the Click event of ButtonOk object.
        /// </summary>
        private void buttonOk_Click(object sender, System.EventArgs e)
        {
            if (colorBlendingCheckBox.IsChecked == true)
            {
                GraphicsProperties.ColorBlendingMode = (GraphicsStateBlendMode)colorBlendingComboBox.SelectedItem;
            }
            else
            {
                GraphicsProperties.ColorBlendingMode = null;
            }

            if (alphaConstantCheckBox.IsChecked == true)
            {
                GraphicsProperties.FillAlphaConstant = (float)(alphaConstantValueEditor.Value / alphaConstantValueEditor.MaxValue);
            }
            else
            {
                GraphicsProperties.FillAlphaConstant = null;
            }

            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of ButtonCancel object.
        /// </summary>
        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = false;
        }

        #endregion


        #region UI state

        /// <summary>
        /// Updates the User Interface.
        /// </summary>
        internal void UpdateUI()
        {
            if (GraphicsProperties.ColorBlendingMode != null)
            {
                colorBlendingCheckBox.IsChecked = true;
                colorBlendingComboBox.SelectedItem = GraphicsProperties.ColorBlendingMode.Value;
            }
            else
            {
                colorBlendingCheckBox.IsChecked = false;
            }

            if (GraphicsProperties.FillAlphaConstant != null)
            {
                alphaConstantCheckBox.IsChecked = true;
                alphaConstantValueEditor.Value = GraphicsProperties.FillAlphaConstant.Value * alphaConstantValueEditor.MaxValue;
            }
            else
            {
                alphaConstantCheckBox.IsChecked = false;
            }
        }

        #endregion

        #endregion

    }
}
