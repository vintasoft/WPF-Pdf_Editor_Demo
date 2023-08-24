using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Annotations;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view, add and remove embedded files of PDF document.
    /// </summary>
    public partial class EmbeddedFilesWindow : Window
    {

        #region Nested Class

        class FileInfo
        {

            #region Properties

            string _fileName;
            public string FileName
            {
                get
                {
                    return _fileName;
                }
                set
                {
                    _fileName = value;
                }
            }

            string _size;
            public string Size
            {
                get
                {
                    return _size;
                }
                set
                {
                    _size = value;
                }
            }

            string _compressedSize;
            public string CompressedSize
            {
                get
                {
                    return _compressedSize;
                }
                set
                {
                    _compressedSize = value;
                }
            }

            #endregion



            #region Constructor

            public FileInfo(PdfEmbeddedFileSpecification fileSpecification)
            {
                _fileName = fileSpecification.Filename;
                long compressedSize = fileSpecification.EmbeddedFile.Length;
                long uncompressedSize = fileSpecification.EmbeddedFile.UncompressedLength;
                if (uncompressedSize == 0)
                {
                    _size = "N/A";
                    _compressedSize = GetDataSize(compressedSize);
                }
                else
                {
                    _size = GetDataSize(uncompressedSize);
                    if (compressedSize > 0)
                    {
                        double compressionPercent = (1 - ((double)compressedSize) / uncompressedSize) * 100.0;
                        _compressedSize = string.Format("{0} ({1:F2}%)", GetDataSize(compressedSize), compressionPercent);
                    }
                    else
                    {
                        _compressedSize = "";
                    }
                }
            }

            #endregion



            #region Methods

            private string GetDataSize(long size)
            {
                if (size < 10000)
                    return string.Format("{0} Bytes", size);
                return string.Format("{0} KB", Math.Round(size / 1024.0));
            }

            #endregion

        }

        #endregion



        #region Fields

        const string DateTimeFormat = "dd.MM.yyyy hh:mm:ss";
        const string FormTitle = "Embedded Files";
        const string AddEmbeddedFileMessage = "Add embedded file '{0}'...";

        bool _showingFileProperties;

        SaveFileDialog saveFileDialog = new SaveFileDialog();
        OpenFileDialog openFileDialog = new OpenFileDialog();

        Dictionary<PdfEmbeddedFileSpecification, string> _embeddedFiles;
        Dictionary<PdfEmbeddedFileSpecification, PdfFileAttachmentAnnotation> _fileAttachments;

        #endregion



        #region Constructor

        public EmbeddedFilesWindow()
        {
            InitializeComponent();
            Title = FormTitle;
            compressionComboBox.Items.Add(PdfCompression.None);
            compressionComboBox.Items.Add(PdfCompression.Zip);
            compressionComboBox.Items.Add(PdfCompression.Lzw);
            compressionComboBox.Items.Add(PdfCompression.RunLength);
            openFileDialog.Multiselect = true;
        }

        #endregion



        #region Properties

        public bool CanEditEmbeddedFiles
        {
            get
            {
                return desriptionTextBox.IsEnabled == true;
            }
            set
            {
                desriptionTextBox.IsEnabled = value;
                removeButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                addButton.Visibility = removeButton.Visibility;
                compressionComboBox.IsEnabled = value;
            }
        }

        PdfDocument _document;
        public PdfDocument Document
        {
            get
            {
                return _document;
            }
            set
            {
                _document = value;
                embeddedFilesListView.Items.Clear();
                _embeddedFiles = new Dictionary<PdfEmbeddedFileSpecification, string>();
                if (_document.EmbeddedFiles != null)
                {
                    foreach (string name in _document.EmbeddedFiles.Keys)
                    {
                        PdfEmbeddedFileSpecification embeddedFileSpecification = _document.EmbeddedFiles[name];
                        _embeddedFiles.Add(embeddedFileSpecification, name);
                        AddRow(embeddedFileSpecification);
                    }
                }
                _fileAttachments = new Dictionary<PdfEmbeddedFileSpecification, PdfFileAttachmentAnnotation>();
                foreach (PdfPage page in _document.Pages)
                    if (page.Annotations != null)
                        foreach (PdfAnnotation annotation in page.Annotations)
                            if (annotation is PdfFileAttachmentAnnotation)
                            {
                                PdfFileAttachmentAnnotation fileAttachmentAnnotation = (PdfFileAttachmentAnnotation)annotation;
                                if (fileAttachmentAnnotation.FileSpecification != null)
                                    if (fileAttachmentAnnotation.FileSpecification.EmbeddedFile != null)
                                    {
                                        _fileAttachments.Add(fileAttachmentAnnotation.FileSpecification, fileAttachmentAnnotation);
                                        AddRow(fileAttachmentAnnotation.FileSpecification);
                                    }
                            }
            }
        }

        private PdfEmbeddedFileSpecification SelectedFileSpecification
        {
            get
            {
                if (embeddedFilesListView.SelectedIndex != -1)
                {
                    return (PdfEmbeddedFileSpecification)(embeddedFilesListView.SelectedItem as ListViewItem).Tag;
                }
                return null;
            }
        }

        #endregion



        #region Methods

        private ListViewItem AddRow(PdfEmbeddedFileSpecification fileSpecification)
        {
            ListViewItem item = new ListViewItem();
            item.Tag = fileSpecification;
            item.Content = new FileInfo(fileSpecification);
            embeddedFilesListView.Items.Add(item);

            return item;
        }

        private void UpdateRowInformation(ListViewItem row)
        {
            PdfEmbeddedFileSpecification fileSpecification = (PdfEmbeddedFileSpecification)row.Tag;
            row.Content = new FileInfo(fileSpecification);
        }

        /// <summary>
        /// Handles the Click event of OkButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of SaveAsButton object.
        /// </summary>
        private void saveAsButton_Click(object sender, RoutedEventArgs e)
        {
            PdfEmbeddedFileSpecification fileSpecification = SelectedFileSpecification;
            if (fileSpecification != null)
            {
                saveFileDialog.FileName = fileSpecification.Filename;
                if (saveFileDialog.ShowDialog().Value)
                    fileSpecification.EmbeddedFile.Save(saveFileDialog.FileName);
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of EmbeddedFilesGridView object.
        /// </summary>
        private void embeddedFilesGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PdfEmbeddedFileSpecification fileSpecification = SelectedFileSpecification;
            if (fileSpecification != null)
            {
                _showingFileProperties = true;
                compressionComboBox.SelectedItem = GetCompression(fileSpecification);
                desriptionTextBox.Text = fileSpecification.Description;
                modifyDateLabel.Content = fileSpecification.EmbeddedFile.ModifyDate.ToString(DateTimeFormat);
                createDateLabel.Content = fileSpecification.EmbeddedFile.CreationDate.ToString(DateTimeFormat);
                _showingFileProperties = false;
            }
        }

        private PdfCompression GetCompression(PdfEmbeddedFileSpecification fileSpecification)
        {
            PdfCompression compression = fileSpecification.EmbeddedFile.Compression;
            if ((compression & PdfCompression.Zip) != 0)
                return PdfCompression.Zip;
            if ((compression & PdfCompression.Lzw) != 0)
                return PdfCompression.Lzw;
            if ((compression & PdfCompression.RunLength) != 0)
                return PdfCompression.RunLength;
            return PdfCompression.None;
        }

        /// <summary>
        /// Handles the SelectionChanged event of CompressionComboBox object.
        /// </summary>
        private void compressionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_showingFileProperties)
            {
                PdfEmbeddedFileSpecification fileSpecification = SelectedFileSpecification;
                try
                {
                    if (fileSpecification != null)
                    {
                        fileSpecification.EmbeddedFile.Compression = (PdfCompression)compressionComboBox.SelectedItem;
                        UpdateRowInformation(embeddedFilesListView.SelectedItem as ListViewItem);
                    }
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }
        }

        /// <summary>
        /// Handles the TextChanged event of DesriptionTextBox object.
        /// </summary>
        private void desriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_showingFileProperties)
            {
                PdfEmbeddedFileSpecification fileSpecification = SelectedFileSpecification;
                if (fileSpecification != null)
                {
                    fileSpecification.Description = desriptionTextBox.Text;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of RemoveButton object.
        /// </summary>
        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (embeddedFilesListView.SelectedIndex != -1)
            {
                ListViewItem row = embeddedFilesListView.SelectedItem as ListViewItem;
                PdfEmbeddedFileSpecification embeddedFile = (PdfEmbeddedFileSpecification)row.Tag;
                if (_embeddedFiles.ContainsKey(embeddedFile))
                    Document.EmbeddedFiles.Remove(_embeddedFiles[embeddedFile]);
                else if (_fileAttachments[embeddedFile].Page != null && _fileAttachments[embeddedFile].Page.Annotations != null)
                    _fileAttachments[embeddedFile].Page.Annotations.Remove(_fileAttachments[embeddedFile]);               
                embeddedFilesListView.Items.Remove(row);

                compressionComboBox.SelectedIndex = -1;
                desriptionTextBox.Text = "";
                modifyDateLabel.Content = "00.00.0000 00:00:00";
                createDateLabel.Content = "00.00.0000 00:00:00";
            }
        }

        /// <summary>
        /// Handles the Click event of AddButton object.
        /// </summary>
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog().Value)
            {
                if (Document.EmbeddedFiles == null)
                    Document.EmbeddedFiles = new PdfEmbeddedFileSpecificationDictionary(Document);

                controlButtonsDockPanel.IsEnabled = false;
                foreach (string filename in openFileDialog.FileNames)
                {
                    try
                    {
                        // update form title
                        Title = string.Format(AddEmbeddedFileMessage, System.IO.Path.GetFileName(filename));

                        // create PDF embedded file
                        PdfCompression comression = PdfCompression.Auto;
                        PdfEmbeddedFile embeddedFile;
                        if (encodeFilesImmediatelyCheckBox.IsChecked.Value)
                        {
                            try
                            {
                                embeddedFile = new PdfEmbeddedFile(Document, filename, comression);
                            }
                            catch (OverflowException ex)
                            {
                                throw new Exception(string.Format("{0}\nDisable 'Encode Files Immediately' option.", ex.Message), ex);
                            }
                        }
                        else
                        {
                            embeddedFile = new PdfEmbeddedFile(Document, File.OpenRead(filename), true, comression, PdfCompressionSettings.DefaultSettings);
                        }

                        // create PDF file specification
                        PdfEmbeddedFileSpecification fileSpecification = new PdfEmbeddedFileSpecification(filename, embeddedFile);

                        // add file to PDF document
                        string name = Document.EmbeddedFiles.Add(fileSpecification);
                        _embeddedFiles.Add(fileSpecification, name);
                        
                        // add embedded file to data grid
                        AddRow(fileSpecification);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                Title = FormTitle;
                controlButtonsDockPanel.IsEnabled = true;
            }
        }

        #endregion

    }
}
