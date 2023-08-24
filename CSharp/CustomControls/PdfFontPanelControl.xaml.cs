using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree.Fonts;

using WpfDemosCommonCode;


namespace WpfDemosCommonCode.CustomControls
{
    /// <summary>
    /// A panel that allows to show the selected PDF font and
    /// select PDF font.
    /// </summary>
    [DefaultEvent("PdfFontChanged")]
    [DefaultProperty("PdfFont")]
    public partial class PdfFontPanelControl : UserControl
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfFontPanelControl"/> class.
        /// </summary>
        public PdfFontPanelControl()
        {
            InitializeComponent();
        }

        #endregion



        #region Properties

        PdfFont _pdfFont = null;
        /// <summary>
        /// Gets or sets the font.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        [Browsable(false)]
        public PdfFont PdfFont
        {
            get
            {
                return _pdfFont;
            }
            set
            {
                if (_pdfFont != value)
                {
                    _pdfFont = value;

                    string fontName = string.Empty;
                    if (_pdfFont != null)
                        fontName = _pdfFont.FontName;
                    fontNameTextBox.Text = fontName;
                }
            }
        }

        public PdfDocument _document = null;
        /// <summary>
        /// Gets or sets the PDF document.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public PdfDocument PdfDocument
        {
            get
            {
                if (_pdfFont == null)
                    return _document;
                else
                    return _pdfFont.Document;
            }
            set
            {
                if (_pdfFont == null)
                    _document = value;
                else
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets or sets the button width.
        /// </summary>
        [Description("The button width.")]
        [DefaultValue(25)]
        public double ButtonWidth
        {
            get
            {
                return fontNameButton.Width;
            }
            set
            {
                fontNameButton.Width = Math.Max(1, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether button is visible.
        /// </summary>
        /// <value>
        /// <b>True</b> if button is visible; otherwise, <b>false</b>.
        /// </value>
        [Description("Indicates whether button is visible.")]
        [DefaultValue(true)]
        public bool ButtonVisible
        {
            get
            {
                return fontNameButton.Visibility == Visibility.Visible;
            }
            set
            {
                if (value)
                    fontNameButton.Visibility = Visibility.Visible;
                else
                    fontNameButton.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Gets or sets the button margin.
        /// </summary>
        /// <value>
        /// Default value is <b>3</b>.
        /// </value>
        [Description("The button margin.")]
        [DefaultValue(3)]
        public double ButtonMargin
        {
            get
            {
                return fontNameButton.Margin.Left;
            }
            set
            {
                fontNameButton.Margin = new Thickness(
                    Math.Max(0, value),
                    fontNameButton.Margin.Top,
                    fontNameButton.Margin.Right,
                    fontNameButton.Margin.Bottom);
            }
        }

        /// <summary>
        /// Gets or sets the button text.
        /// </summary>
        /// <value>
        /// Default value is <b>...</b>.
        /// </value>
        [Description("The button text.")]
        [DefaultValue("...")]
        public string ButtonText
        {
            get
            {
                return fontNameButton.Content.ToString();
            }
            set
            {
                if (value != null)
                    fontNameButton.Content = value;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// "..." button is clicked.
        /// </summary>
        private void fontNameButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFontDialog();
        }

        /// <summary>
        /// Mouse is double clicked on the text box.
        /// </summary>
        private void fontNameTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                ShowFontDialog();
        }

        /// <summary>
        /// Shows the font dialog.
        /// </summary>
        public void ShowFontDialog()
        {
            CreateFontWindow window = new CreateFontWindow(PdfDocument, PdfFont);            
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = Window.GetWindow(this);
            if (window.ShowDialog() == true)
            {
                _pdfFont = window.SelectedFont;
                fontNameTextBox.Text = _pdfFont.FontName;

                if (PdfFontChanged != null)
                    PdfFontChanged(this, EventArgs.Empty);
            }
        }

        #endregion



        #region Events

        /// <summary>
        /// Occurs when font is changed.
        /// </summary>
        public event EventHandler PdfFontChanged;

        #endregion

    }
}
