using System.Windows;

using Vintasoft.Imaging.ImageColors;
using Vintasoft.Imaging.Pdf.Content;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Text;
using Vintasoft.Imaging.Wpf;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to edit the PDF content graphics properties.
    /// </summary>
    public partial class PdfContentGraphicsPropertiesWindow : Window
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfResourceGraphicsPropertiesWindow"/> class.
        /// </summary>
        public PdfContentGraphicsPropertiesWindow()
        {
            InitializeComponent();

            textRenderingModeComboBox.Items.Add(TextRenderingMode.AddClipPath);
            textRenderingModeComboBox.Items.Add(TextRenderingMode.Fill);
            textRenderingModeComboBox.Items.Add(TextRenderingMode.FillAndAddClipPath);
            textRenderingModeComboBox.Items.Add(TextRenderingMode.FillAndStroke);
            textRenderingModeComboBox.Items.Add(TextRenderingMode.FillAndStrokeAndAddClipPath);
            textRenderingModeComboBox.Items.Add(TextRenderingMode.Invisible);
            textRenderingModeComboBox.Items.Add(TextRenderingMode.Stroke);
            textRenderingModeComboBox.Items.Add(TextRenderingMode.StrokeAndAddClipPath);
            textRenderingModeComboBox.SelectedItem = TextRenderingMode.Fill;

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
        /// Handles the CheckedChanged event of strokePropertiesCheckBox object.
        /// </summary>
        private void strokePropertiesCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (strokePropertiesCheckBox.IsChecked == true)
                strokeGroupBox.IsEnabled = true;
            else
                strokeGroupBox.IsEnabled = false;
        }

        /// <summary>
        /// Handles the CheckedChanged event of fillPropertiesCheckBox object.
        /// </summary>
        private void fillPropertiesCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (fillPropertiesCheckBox.IsChecked == true)
                fillGroupBox.IsEnabled = true;
            else
                fillGroupBox.IsEnabled = false;
        }

        /// <summary>
        /// Handles the CheckedChanged event of textPropertiesCheckBox object.
        /// </summary>
        private void textPropertiesCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (textPropertiesCheckBox.IsChecked == true)
                textPropertiesGroupBox.IsEnabled = true;
            else
                textPropertiesGroupBox.IsEnabled = false;
        }

        /// <summary>
        /// Handles the CheckedChanged event of colorBlendingCheckBox object.
        /// </summary>
        private void colorBlendingCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (colorBlendingCheckBox.IsChecked == true)
                colorBlendingGroupBox.IsEnabled = true;
            else
                colorBlendingGroupBox.IsEnabled = false;
        }

        /// <summary>
        /// Handles the Click event of buttonOk object.
        /// </summary>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            if (fillPropertiesCheckBox.IsChecked == true)
            {
                Argb32Color newColor = Argb32Color.FromColor(WpfObjectConverter.CreateDrawingColor(fillColorPanelControl.Color));
                if (GraphicsProperties.FillColor != newColor)
                    GraphicsProperties.FillColor = newColor;
            }
            else
            {
                GraphicsProperties.FillColor = null;
            }

            if (strokePropertiesCheckBox.IsChecked == true)
            {
                Argb32Color newColor = Argb32Color.FromColor(WpfObjectConverter.CreateDrawingColor(strokeColorPanelControl.Color));
                if (GraphicsProperties.StrokeColor != newColor)
                    GraphicsProperties.StrokeColor = newColor;
                GraphicsProperties.LineWidth = (float)lineWidthNumericUpDown.Value;
            }
            else
            {
                GraphicsProperties.StrokeColor = null;
                GraphicsProperties.LineWidth = null;
            }

            if (textPropertiesCheckBox.IsChecked == true)
            {
                GraphicsProperties.TextRenderingMode = (TextRenderingMode)textRenderingModeComboBox.SelectedItem;
            }
            else
            {
                GraphicsProperties.TextRenderingMode = null;
            }

            if (colorBlendingCheckBox.IsChecked == true)
            {
                GraphicsProperties.ColorBlendingMode = (GraphicsStateBlendMode)colorBlendingComboBox.SelectedItem;
            }
            else
            {
                GraphicsProperties.ColorBlendingMode = null;
            }

            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of buttonCancel object.
        /// </summary>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion


        #region UI State

        /// <summary>
        /// Updates the User Interface.
        /// </summary>
        internal void UpdateUI()
        {
            if (GraphicsProperties.FillColorSpace != null)
            {
                fillPropertiesCheckBox.IsChecked = true;
                fillColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(GraphicsProperties.FillColor.ToColor());
            }
            else
            {
                fillPropertiesCheckBox.IsChecked = false;
            }
            fillGroupBox.IsEnabled = fillPropertiesCheckBox.IsChecked.Value;

            if (GraphicsProperties.StrokeColorSpace != null)
            {
                strokePropertiesCheckBox.IsChecked = true;
                strokeColorPanelControl.Color = WpfObjectConverter.CreateWindowsColor(GraphicsProperties.StrokeColor.ToColor());
                lineWidthNumericUpDown.Value = GraphicsProperties.LineWidth ?? 0;
            }
            else
            {
                strokePropertiesCheckBox.IsChecked = false;
            }
            strokeGroupBox.IsEnabled = strokePropertiesCheckBox.IsChecked.Value;

            if (GraphicsProperties.TextRenderingMode != null)
            {
                textPropertiesCheckBox.IsChecked = true;
                textRenderingModeComboBox.SelectedItem = GraphicsProperties.TextRenderingMode.Value;
            }
            else
            {
                textPropertiesCheckBox.IsChecked = false;
            }
            textPropertiesGroupBox.IsEnabled = textPropertiesCheckBox.IsChecked.Value;

            if (GraphicsProperties.ColorBlendingMode != null)
            {
                colorBlendingCheckBox.IsChecked = true;
                colorBlendingComboBox.SelectedItem = GraphicsProperties.ColorBlendingMode.Value;
            }
            else
            {
                colorBlendingCheckBox.IsChecked = false;
            }
            colorBlendingGroupBox.IsEnabled = colorBlendingCheckBox.IsChecked.Value;
        }

        #endregion

        #endregion

    }
}
