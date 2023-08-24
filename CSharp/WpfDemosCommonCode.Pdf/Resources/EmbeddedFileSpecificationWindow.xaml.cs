using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Represents an editor of PDF Embedded File Specification object.
    /// </summary>
    public partial class EmbeddedFileSpecificationWindow : Window
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedFileSpecificationWindow"/> class.
        /// </summary>
        public EmbeddedFileSpecificationWindow()
        {
            InitializeComponent();

            compressionComboBox.Items.Add(PdfCompression.None);
            compressionComboBox.Items.Add(PdfCompression.Zip);
            compressionComboBox.Items.Add(PdfCompression.Lzw);
            compressionComboBox.Items.Add(PdfCompression.Ascii85);
            compressionComboBox.Items.Add(PdfCompression.AsciiHex);
            compressionComboBox.Items.Add(PdfCompression.Zip | PdfCompression.Ascii85);
        }

        #endregion



        #region Properties

        PdfEmbeddedFileSpecification _embeddedFileSpecification;
        /// <summary>
        /// Gets or sets the embedded file specification.
        /// </summary>
        [Browsable(false)]
        public PdfEmbeddedFileSpecification EmbeddedFileSpecification
        {
            get
            {
                return _embeddedFileSpecification;
            }
            set
            {
                _embeddedFileSpecification = value;
                UpdateUI();
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the User Interface.
        /// </summary>
        private void UpdateUI()
        {
            // if embedded file specification is not empty
            if (_embeddedFileSpecification != null)
            {
                // set filename to the filename text box
                filenameTextBox.Text = _embeddedFileSpecification.Filename;
                // set description to the decription text box
                fileDescriptionTextBox.Text = _embeddedFileSpecification.Description;
                // if embedded file is empty
                if (_embeddedFileSpecification.EmbeddedFile == null)
                {
                    // disable "Save As..." button 
                    saveAsButton.IsEnabled = false;
                    // set empty string to the size text box
                    sizeTextBox.Text = "";
                    // set empty string to the compressed size text box
                    compressedSizeTextBox.Text = "";
                    // disable compression combo box
                    compressionComboBox.IsEnabled = false;
                    // set PDF compression "None" as selected compression
                    compressionComboBox.SelectedItem = PdfCompression.None;
                }
                // if embedded file is not empty
                else
                {
                    // enable "Save As..." button
                    saveAsButton.IsEnabled = true;
                    // if embedded file uncompressed size is greater than 0
                    if (_embeddedFileSpecification.UncompressedSize > 0)
                    {
                        // set the information to the size text box
                        sizeTextBox.Text = string.Format("{0} bytes", _embeddedFileSpecification.UncompressedSize);
                    }
                    else
                    {
                        // set empty string to the size text box
                        sizeTextBox.Text = "";
                    }
                    // if embedded file compressed size is greater than 0
                    if (_embeddedFileSpecification.CompressedSize > 0)
                    {
                        // set the information to the compressed size text box
                        compressedSizeTextBox.Text = string.Format("{0} bytes", _embeddedFileSpecification.CompressedSize);
                    }
                    else
                    {
                        // set empty string to the size text box
                        compressedSizeTextBox.Text = "";
                    }
                    // enable compression combo box
                    compressionComboBox.IsEnabled = true;
                    // if compression combo box does not contain compression of embedded file
                    if (!compressionComboBox.Items.Contains(_embeddedFileSpecification.EmbeddedFile.Compression))
                    {
                        // add compression of embedded file to the compression combo box
                        compressionComboBox.Items.Add(_embeddedFileSpecification.EmbeddedFile.Compression);
                    }
                    // set embedded file compression as selected compression
                    compressionComboBox.SelectedItem = _embeddedFileSpecification.EmbeddedFile.Compression;
                }
            }
        }

        /// <summary>
        /// "OK" button is pressed.
        /// </summary>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            _embeddedFileSpecification.Filename = filenameTextBox.Text;
            _embeddedFileSpecification.Description = fileDescriptionTextBox.Text;
            DialogResult = true;
        }

        /// <summary>
        /// "Cancel" button is pressed.
        /// </summary>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Embedded file compression is changed.
        /// </summary>
        private void compressionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // embedded file is not empty
            if (_embeddedFileSpecification.EmbeddedFile != null)
            {
                // set selected PDF compression to the embedded file
                _embeddedFileSpecification.EmbeddedFile.Compression = (PdfCompression)compressionComboBox.SelectedItem;
                // update user interface
                UpdateUI();
            }
        }

        /// <summary>
        /// "Load..." button is clicked.
        /// </summary>
        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // create open file dialog
                OpenFileDialog openFile = new OpenFileDialog();
                // if dialog result is true
                if (openFile.ShowDialog() == true)
                {
                    // set new embedded file
                    _embeddedFileSpecification.EmbeddedFile = new PdfEmbeddedFile(_embeddedFileSpecification.Document, openFile.FileName);
                    // set new embedded file name
                    _embeddedFileSpecification.Filename = Path.GetFileName(openFile.FileName);
                    // update user interface
                    UpdateUI();
                }
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// "Save As..." button is clicked.
        /// </summary>
        private void saveAsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // create save file dialog
                SaveFileDialog saveFile = new SaveFileDialog();
                // get embedded file name
                string filename = Path.GetFileName(_embeddedFileSpecification.Filename);
                // set default save extension
                saveFile.DefaultExt = Path.GetExtension(filename);
                // set save file name
                saveFile.FileName = filename;
                // if dialog result is true
                if (saveFile.ShowDialog() == true)
                {
                    // save embedded file
                    File.WriteAllBytes(saveFile.FileName, _embeddedFileSpecification.EmbeddedFile.GetBytes());
                }
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        #endregion

    }
}
