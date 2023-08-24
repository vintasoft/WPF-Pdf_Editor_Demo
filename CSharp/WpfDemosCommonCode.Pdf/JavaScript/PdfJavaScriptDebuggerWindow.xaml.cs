using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Wpf.UI;

using WpfDemosCommonCode.Pdf.JavaScript;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Window that allows to debug PDF JavaScript interperter.
    /// </summary>
    public partial class PdfJavaScriptDebuggerWindow : Window
    {

        #region Fields

        /// <summary>
        /// The image viewer.
        /// </summary>
        WpfImageViewer _imageViewer;

        /// <summary>
        /// The JavaScript action executor.
        /// </summary>
        WpfPdfJavaScriptActionExecutor _javaScriptActionExecutor;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfJavaScriptDebuggerWindow"/> class.
        /// </summary>
        public PdfJavaScriptDebuggerWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfJavaScriptDebuggerWindow"/> class.
        /// </summary>
        /// <param name="javaScriptActionExecutor">The java script action executor.</param>
        /// <param name="imageViewer">The image viewer.</param>
        public PdfJavaScriptDebuggerWindow(
            WpfPdfJavaScriptActionExecutor javaScriptActionExecutor,
            WpfImageViewer imageViewer)
            : this()
        {
            _javaScriptActionExecutor = javaScriptActionExecutor;
            _javaScriptActionExecutor.ConsoleTextBox = ConsoleTextBox;
            _javaScriptActionExecutor.LogTextBox = LogTextBox;
            _imageViewer = imageViewer;
            debugModecheckBox.IsChecked = PdfJavaScriptManager.JavaScriptActionExecutor.DebugMode;
            topMostCheckBox.IsChecked = Topmost;
        }

        #endregion



        #region Methods

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.Closing" /> event.
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            base.OnClosing(e);
        }

        /// <summary>
        /// Handles the KeyDown event of the watchTextBox control.
        /// </summary>
        private void expressionTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    int caretIndex = expressionTextBox.CaretIndex;
                    string newLine = Environment.NewLine;
                    if (expressionTextBox.Text.Length == caretIndex)
                        expressionTextBox.AppendText(newLine);
                    else
                        expressionTextBox.Text = expressionTextBox.Text.Insert(caretIndex, newLine);
                    expressionTextBox.CaretIndex = caretIndex + newLine.Length;
                }
                else
                {
                    EvaluateExpression();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        private void EvaluateExpression()
        {
            evaluateButton.IsEnabled = false;
            try
            {
                PdfPage page = null;
                if (_imageViewer.Image != null)
                    page = PdfDocumentController.GetPageAssociatedWithImage(_imageViewer.Image);
                PdfDocument document = null;
                if (page != null)
                    document = page.Document;
                Object result = null;
                expressionTextBox.IsReadOnly = true;
                result = _javaScriptActionExecutor.ExecuteScript(
                    document,
                    expressionTextBox.Text,
                    true);
                expressionTextBox.IsReadOnly = false;
                evaluateResultTextBox.Clear();
                if (result == null || result.ToString() == null)
                    evaluateResultTextBox.AppendText("null");
                else
                    evaluateResultTextBox.AppendText(result.ToString());
                watchResultPropertyGrid.SelectedObject = result;
            }
            finally
            {
                evaluateButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the debugModecheckBox.
        /// </summary>
        private void debugModecheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            PdfJavaScriptManager.JavaScriptActionExecutor.DebugMode = debugModecheckBox.IsChecked.Value;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the topMostCheckBox.
        /// </summary>
        private void topMostCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Topmost = topMostCheckBox.IsChecked.Value;
        }

        /// <summary>
        /// Clears console.
        /// </summary>
        private void clearConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            ConsoleTextBox.Text = "";
        }

        /// <summary>
        /// Clears interpreter log.
        /// </summary>
        private void clearEngineLogButton_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Text = "";
        }

        /// <summary>
        /// Evaluates a JavaScript.
        /// </summary>
        private void evaluateButton_Click(object sender, RoutedEventArgs e)
        {
            EvaluateExpression();
        }

        /// <summary>
        /// Clears EvaluateExpression text box.
        /// </summary>
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            evaluateResultTextBox.Text = "";
            watchResultPropertyGrid.SelectedObject = null;
            expressionTextBox.Text = "";
        }

        #endregion

    }
}
