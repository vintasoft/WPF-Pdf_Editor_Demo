using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

using Vintasoft.Imaging.ImageProcessing;
using Vintasoft.Imaging.ImageProcessing.Effects;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Window that allows to view and edit settings of custom content renderer.
    /// </summary>
    public partial class CustomContentRendererSettingsWindow : Window
    {

        #region Fields

        /// <summary>
        /// The content renderer.
        /// </summary>
        CustomContentRenderer _contentRenderer;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomContentRendererSettingsForm"/> class.
        /// </summary>
        public CustomContentRendererSettingsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomContentRendererSettingsForm"/> class.
        /// </summary>
        /// <param name="contentRenderer">The content renderer.</param>
        public CustomContentRendererSettingsWindow(CustomContentRenderer contentRenderer)
            : this()
        {
            _contentRenderer = contentRenderer;
            fillPathsCheckBox.IsChecked = contentRenderer.FillPaths;
            drawPathsCheckBox.IsChecked = contentRenderer.DrawPaths;
            useTilingPatternsCheckBox.IsChecked = contentRenderer.FillPathsUseTilingPatterns;
            useShadingPatternsCheckBox.IsChecked = contentRenderer.FillPathsUseShadingPatterns;
            useClipPathsCheckBox.IsChecked = contentRenderer.SetClipPaths;
            linesWeightNumericUpDown.Value = (int)(contentRenderer.LinesWeigth * 100);
            drawFormsCheckBox.IsChecked = contentRenderer.DrawForms;
            drawImageResourcesCheckBox.IsChecked = contentRenderer.DrawImages;
            drawInlineImagesCheckBox.IsChecked = contentRenderer.DrawInlineImages;
            fillAreaUseShadigPatternsCheckBox.IsChecked = contentRenderer.FillAreaUseShadingPatterns;
            drawTextCheckBox.IsChecked = contentRenderer.DrawText;
            drawInvisibleTextCheckBox.IsChecked = contentRenderer.DrawInvisibleText;
            if (contentRenderer.ImageProcessing != null)
            {
                ProcessingCommandBase[] commands;
                if (contentRenderer.ImageProcessing is CompositeCommand)
                    commands = ((CompositeCommand)contentRenderer.ImageProcessing).GetCommands();
                else
                    commands = new ProcessingCommandBase[] { contentRenderer.ImageProcessing };
                foreach (ProcessingCommandBase command in commands)
                {
                    if (command is ChangePixelFormatToGrayscaleCommand)
                        convertToGrayscaleCheckBox.IsChecked = true;
                    else if (command is AutoColorsCommand)
                        autoColorsCheckBox.IsChecked = true;
                    else if (command is AutoLevelsCommand)
                        autoLevelsCheckBox.IsChecked = true;
                    else if (command is AutoContrastCommand)
                        autoContrastCheckBox.IsChecked = true;
                }
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Saves the current setting to content renderer and closes the window.
        /// </summary>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            _contentRenderer.FillPaths = fillPathsCheckBox.IsChecked.Value;
            _contentRenderer.DrawPaths = drawPathsCheckBox.IsChecked.Value;
            _contentRenderer.FillPathsUseTilingPatterns = useTilingPatternsCheckBox.IsChecked.Value;
            _contentRenderer.FillPathsUseShadingPatterns = useShadingPatternsCheckBox.IsChecked.Value;
            _contentRenderer.SetClipPaths = useClipPathsCheckBox.IsChecked.Value;
            _contentRenderer.LinesWeigth = (float)linesWeightNumericUpDown.Value / 100f;
            _contentRenderer.DrawForms = drawFormsCheckBox.IsChecked.Value;
            _contentRenderer.DrawImages = drawImageResourcesCheckBox.IsChecked.Value;
            _contentRenderer.DrawInlineImages = drawInlineImagesCheckBox.IsChecked.Value;
            _contentRenderer.FillAreaUseShadingPatterns = fillAreaUseShadigPatternsCheckBox.IsChecked.Value;
            _contentRenderer.DrawText = drawTextCheckBox.IsChecked.Value;
            _contentRenderer.DrawInvisibleText = drawInvisibleTextCheckBox.IsChecked.Value;
            List<ProcessingCommandBase> processingCommands = new List<ProcessingCommandBase>();
            if (autoLevelsCheckBox.IsChecked.Value)
                processingCommands.Add(new AutoLevelsCommand());
            if (autoColorsCheckBox.IsChecked.Value)
                processingCommands.Add(new AutoColorsCommand());
            if (autoContrastCheckBox.IsChecked.Value)
                processingCommands.Add(new AutoContrastCommand());
            if (convertToGrayscaleCheckBox.IsChecked.Value)
                processingCommands.Add(new ChangePixelFormatToGrayscaleCommand());
            if (processingCommands.Count == 0)
                _contentRenderer.ImageProcessing = null;
            else if (processingCommands.Count == 1)
                _contentRenderer.ImageProcessing = processingCommands[0];
            else
                _contentRenderer.ImageProcessing = new CompositeCommand(processingCommands.ToArray());
            DialogResult = true;
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

    }
}
