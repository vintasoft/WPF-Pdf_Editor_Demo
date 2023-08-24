using System;
using System.Collections.Generic;
using System.Text;
using Vintasoft.Imaging.Pdf.JavaScriptApi;
using System.Windows.Controls;

namespace WpfDemosCommonCode.Pdf.JavaScript
{
    /// <summary>
    /// Textbox for PDF JavaScript console.
    /// </summary>
    public class TextBoxPdfJsConsole : PdfJsConsole
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBoxPdfJsConsole"/> class.
        /// </summary>
        public TextBoxPdfJsConsole()
        {
        }

        #endregion



        #region Properties

        TextBox _textBox;
        /// <summary>
        /// Gets or sets the text box.
        /// </summary>
        public TextBox TextBox
        {
            get
            {
                return _textBox;
            }
            set
            {
                _textBox = value;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Shows the console window.
        /// </summary>
        public override void show()
        {
        }

        /// <summary>
        /// Closes the console window.
        /// </summary>
        public override void hide()
        {
        }

        /// <summary>
        /// Prints a string value to the console window with an accompanying carriage return.
        /// </summary>
        /// <param name="cMessage">A string message to print.</param>
        public override void println(string cMessage)
        {
            if (_textBox != null)
            {
                if (cMessage != null)
                    _textBox.AppendText(cMessage);
                _textBox.AppendText("\r\n");
                _textBox.ScrollToEnd();
            }
        }

        /// <summary>
        /// Clears the buffer of console window.
        /// </summary>
        public override void clear()
        {
            if (_textBox != null)
                _textBox.Clear();
        }

        #endregion

    }
}
