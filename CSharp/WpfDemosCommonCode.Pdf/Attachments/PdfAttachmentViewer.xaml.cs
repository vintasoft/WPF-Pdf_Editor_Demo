using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Vintasoft.Imaging;
using Vintasoft.Imaging.ImageProcessing;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.FileAttachments;
using Vintasoft.Imaging.Wpf;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Represents the PDF attachment viewer.
    /// </summary>
    public partial class PdfAttachmentViewer : ListView
    {

        #region Nested class

        /// <summary>
        /// Represents a list view sub item.
        /// </summary>
        public class ListViewSubItem
        {

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ListViewSubItem"/> class.
            /// </summary>
            public ListViewSubItem()
                : this(string.Empty)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ListViewSubItem"/> class.
            /// </summary>
            /// <param name="text">The text.</param>
            public ListViewSubItem(string text)
            {
                Text = text;
            }

            #endregion



            #region Properties

            string _text;
            /// <summary>
            /// Gets or sets the text.
            /// </summary>
            public string Text
            {
                get
                {
                    return _text;
                }
                set
                {
                    _text = value;
                }
            }

            Brush _backColor = null;
            /// <summary>
            /// Gets or sets the color of the background.
            /// </summary>
            public Brush BackColor
            {
                get
                {
                    return _backColor;
                }
                set
                {
                    _backColor = value;
                }
            }

            Brush _foreColor = null;
            /// <summary>
            /// Gets or sets the color of the foreground.
            /// </summary>
            public Brush ForeColor
            {
                get
                {
                    return _foreColor;
                }
                set
                {
                    _foreColor = value;
                }
            }

            #endregion

        }

        /// <summary>
        /// Represents a list view row data.
        /// </summary>
        public class ListViewRowData
        {

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ListViewRowData"/> class.
            /// </summary>
            public ListViewRowData()
            {
                ImageIndex = -1;
                Text = string.Empty;
            }

            #endregion



            #region Properties

            string _text;
            /// <summary>
            /// Gets or sets the text of data.
            /// </summary>
            public string Text
            {
                get
                {
                    return _text;
                }
                set
                {
                    _text = value;
                }
            }

            private List<ListViewSubItem> _subItems = new List<ListViewSubItem>();
            /// <summary>
            /// Gets the sub items.
            /// </summary>
            public List<ListViewSubItem> SubItems
            {
                get
                {
                    return _subItems;
                }
            }

            /// <summary>
            /// Gets the small image.
            /// </summary>
            public BitmapSource SmallImage
            {
                get
                {
                    if (ImageIndex == -1)
                        return null;
                    else
                        return _smallImageList[ImageIndex];
                }
            }

            /// <summary>
            /// Gets the large image.
            /// </summary>
            public BitmapSource LargeImage
            {
                get
                {
                    if (ImageIndex == -1)
                        return null;
                    else
                        return _largeImageList[ImageIndex];
                }
            }

            int _imageIndex;
            /// <summary>
            /// Gets or sets the index of the image.
            /// </summary>
            public int ImageIndex
            {
                get
                {
                    return _imageIndex;
                }
                set
                {
                    _imageIndex = value;
                }
            }

            Brush _backColor = null;
            /// <summary>
            /// Gets or sets the color of the background.
            /// </summary>
            public Brush BackColor
            {
                get
                {
                    return _backColor;
                }
                set
                {
                    _backColor = value;
                }
            }

            Brush _foreColor = null;
            /// <summary>
            /// Gets or sets the color of the foreground.
            /// </summary>
            public Brush ForeColor
            {
                get
                {
                    return _foreColor;
                }
                set
                {
                    _foreColor = value;
                }
            }

            #endregion

        }

        #endregion



        #region Fields

        /// <summary>
        /// The large images of items of list view.
        /// </summary>
        public static List<BitmapSource> _largeImageList = new List<BitmapSource>();

        /// <summary>
        /// The size of large images.
        /// </summary>
        static Size _largeImageSize = new Size(100, 100);

        /// <summary>
        /// The small images of items of list view.
        /// </summary>
        public static List<BitmapSource> _smallImageList = new List<BitmapSource>();

        /// <summary>
        /// The size of small images.
        /// </summary>
        static Size _smallImageSize = new Size(40, 40);

        /// <summary>
        /// The names of grid view columns.
        /// </summary>
        Dictionary<GridViewColumn, string> _columnNames = new Dictionary<GridViewColumn, string>();

        /// <summary>
        /// A view mode that displays data items in columns for a ListView control.
        /// </summary>
        GridView _gridView;

        /// <summary>
        /// The count of standard icons.
        /// </summary>
        const int StandardIconCount = 4;

        /// <summary>
        /// The dictionary that provides a mapping from list items to PDF attachment folders.
        /// </summary>
        Dictionary<ListViewRowData, PdfAttachmentFolder> _listItemToFolder = new Dictionary<ListViewRowData, PdfAttachmentFolder>();

        /// <summary>
        /// The dictionary that provides a mapping from PDF attachment folders to list items.
        /// </summary>
        Dictionary<PdfAttachmentFolder, ListViewRowData> _folderToListItem = new Dictionary<PdfAttachmentFolder, ListViewRowData>();

        /// <summary>
        /// The dictionary that provides a mapping from list items to PDF embedded files.
        /// </summary>
        Dictionary<ListViewRowData, PdfEmbeddedFileSpecification> _listItemToFile = new Dictionary<ListViewRowData, PdfEmbeddedFileSpecification>();

        /// <summary>
        /// The dictionary that provides a mapping from PDF embedded files to list items.
        /// </summary>
        Dictionary<PdfEmbeddedFileSpecification, ListViewRowData> _fileToListItem = new Dictionary<PdfEmbeddedFileSpecification, ListViewRowData>();

        /// <summary>
        /// Determines that schema from PDF document must be used for
        /// displaying properties of embedded files.
        /// </summary>
        bool _useSchema;

        #endregion



        #region Constructors

        public PdfAttachmentViewer()
        {
            InitializeComponent();

            // add standard icons
            using (VintasoftImage image = DemosResourcesManager.GetResourceAsImage("Icon_EmptyFolder_100x100.png"))
                AddImage(_largeImageList, image);
            using (VintasoftImage image = DemosResourcesManager.GetResourceAsImage("Icon_Folder_100x100.png"))
                AddImage(_largeImageList, image);
            using (VintasoftImage image = DemosResourcesManager.GetResourceAsImage("Icon_PDF_100x100.png"))
                AddImage(_largeImageList, image);
            using (VintasoftImage image = DemosResourcesManager.GetResourceAsImage("Icon_File_100x100.png"))
                AddImage(_largeImageList, image);

            // add standard icons
            using (VintasoftImage image = DemosResourcesManager.GetResourceAsImage("Icon_EmptyFolder_40x40.png"))
                AddImage(_smallImageList, image);
            using (VintasoftImage image = DemosResourcesManager.GetResourceAsImage("Icon_Folder_40x40.png"))
                AddImage(_smallImageList, image);
            using (VintasoftImage image = DemosResourcesManager.GetResourceAsImage("Icon_PDF_40x40.png"))
                AddImage(_smallImageList, image);
            using (VintasoftImage image = DemosResourcesManager.GetResourceAsImage("Icon_File_40x40.png"))
                AddImage(_smallImageList, image);

            this.MouseLeftButtonUp += new MouseButtonEventHandler(PdfAttachmentViewer_MouseLeftButtonUp);
            _gridView = (GridView)Resources["GridView"];

            ViewMode = ViewMode.Details;
        }

        #endregion



        #region Properties

        bool _encodeFileImmediately = true;
        /// <summary>
        /// Gets or sets a value indicating whether added file must be encoded immediately and stored in memory.
        /// </summary>
        /// <value>
        /// <b>True</b> - added file must be encoded immediately and stored in memory, file stream will be closed after file encoding;<br />
        /// <b>false</b> - added file must be encoded when document is saved or packed, file stream will be closed after document saving or packing.<br />
        /// Default value is <b>true</b>.
        /// </value>
        [Browsable(false)]
        public bool EncodeFileImmediately
        {
            get
            {
                return _encodeFileImmediately;
            }
            set
            {
                _encodeFileImmediately = value;
            }
        }

        /// <summary>
        /// Gets an array that contains files in current folder.
        /// </summary>
        [Browsable(false)]
        public PdfEmbeddedFileSpecification[] FilesInCurrentFolder
        {
            get
            {
                if (CurrentFolder == null)
                    return _document.Attachments.GetFiles("");
                return CurrentFolder.Files;
            }
        }

        /// <summary>
        /// Gets an array that contains sub folders of current folder.
        /// </summary>
        [Browsable(false)]
        public PdfAttachmentFolder[] FoldersInCurrentFolder
        {
            get
            {
                if (CurrentFolder == null)
                    return null;
                return CurrentFolder.Folders;
            }
        }

        PdfDocument _document;
        /// <summary>
        /// Gets or sets the PDF document.
        /// </summary>
        [Browsable(false)]
        public PdfDocument Document
        {
            get
            {
                return _document;
            }
            set
            {
                _document = value;
                ResetUI();
            }
        }

        /// <summary>
        /// Gets the root folder of attachment collection.
        /// </summary>
        [Browsable(false)]
        public PdfAttachmentFolder RootFolder
        {
            get
            {
                if (_document == null)
                    return null;
                if (_document.Attachments == null)
                    return null;
                return _document.Attachments.RootFolder;
            }
        }

        PdfAttachmentFolder _currentFolder;
        /// <summary>
        /// Gets or sets the current folder of attachment collection.
        /// </summary>
        [Browsable(false)]
        public PdfAttachmentFolder CurrentFolder
        {
            get
            {
                return _currentFolder;
            }
            set
            {
                _currentFolder = value;
                UpdateCurrentFolder();
            }
        }

        /// <summary>
        /// Gets or sets the selected folders.
        /// </summary>
        [Browsable(false)]
        public PdfAttachmentFolder[] SelectedFolders
        {
            get
            {
                List<PdfAttachmentFolder> folders = new List<PdfAttachmentFolder>();
                for (int i = 0; i < SelectedItems.Count; i++)
                {
                    if (_listItemToFolder.ContainsKey((ListViewRowData)SelectedItems[i]))
                        folders.Add(_listItemToFolder[(ListViewRowData)SelectedItems[i]]);
                }
                return folders.ToArray();
            }
            set
            {
                int i = 0;
                while (i < SelectedItems.Count)
                {
                    if (_listItemToFolder.ContainsKey((ListViewRowData)SelectedItems[i]))
                        SelectedItems.Remove((ListViewRowData)SelectedItems[i]);
                    else
                        i++;
                }
                if (value != null && value.Length > 0)
                {
                    foreach (ListViewRowData item in _listItemToFolder.Keys)
                    {
                        if (Array.IndexOf(value, _listItemToFolder[item]) >= 0)
                            SelectedItems.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected embedded files.
        /// </summary>
        [Browsable(false)]
        public PdfEmbeddedFileSpecification[] SelectedFiles
        {
            get
            {
                List<PdfEmbeddedFileSpecification> files = new List<PdfEmbeddedFileSpecification>();
                for (int i = 0; i < SelectedItems.Count; i++)
                {
                    if (_listItemToFile.ContainsKey((ListViewRowData)SelectedItems[i]))
                        files.Add(_listItemToFile[(ListViewRowData)SelectedItems[i]]);
                }
                return files.ToArray();
            }
            set
            {
                int i = 0;
                while (i < SelectedItems.Count)
                {
                    if (_listItemToFile.ContainsKey((ListViewRowData)SelectedItems[i]))
                        SelectedItems.Remove((ListViewRowData)SelectedItems[i]);
                    else
                        i++;
                }
                if (value != null && value.Length > 0)
                {
                    foreach (ListViewRowData item in _listItemToFile.Keys)
                    {
                        if (Array.IndexOf(value, _listItemToFile[item]) >= 0)
                            SelectedItems.Add(item);
                    }
                }
            }
        }

        private ViewMode _viewMode;
        /// <summary>
        /// Gets or sets the view mode.
        /// </summary>
        /// <value>
        /// Default value is <b>ViewMode.Details</b>.
        /// </value>
        public ViewMode ViewMode
        {
            get
            {
                return _viewMode;
            }
            set
            {
                if (_viewMode != value)
                {
                    _viewMode = value;

                    if (value == ViewMode.LargeIcon)
                        Style = (Style)Resources["LargeIconStyle"];
                    else
                        Style = (Style)Resources["GridStyle"];
                }
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Updates the current folder view.
        /// </summary>
        public void UpdateCurrentFolder()
        {
            if (_document == null)
                return;

            BeginInit();
            try
            {
                _folderToListItem.Clear();
                _fileToListItem.Clear();
                _listItemToFile.Clear();
                _listItemToFolder.Clear();
                PdfAttachmentFolder[] folders = null;
                PdfEmbeddedFileSpecification[] files = null;
                GetFilesAndFolders(_currentFolder, out folders, out files);
                Items.Clear();
                while (_smallImageList.Count > StandardIconCount)
                {
                    _smallImageList.RemoveAt(StandardIconCount);
                }
                while (_largeImageList.Count > StandardIconCount)
                {
                    _largeImageList.RemoveAt(StandardIconCount);
                }
                List<ListViewRowData> items = new List<ListViewRowData>();
                if (folders != null)
                {
                    foreach (PdfAttachmentFolder folder in folders)
                        items.Add(CreateItem(folder));
                }
                if (files != null)
                {
                    foreach (PdfEmbeddedFileSpecification file in files)
                        items.Add(CreateItem(file));
                }
                if (_document.Attachments.Sort != null && _document.Attachments.Schema != null)
                {
                    string[] sortFieldNames = _document.Attachments.Sort.FieldNames;
                    if (sortFieldNames != null && sortFieldNames.Length > 0)
                    {
                        string sortFieldName = sortFieldNames[0];
                        string[] sortKeys = new string[items.Count];
                        for (int i = 0; i < items.Count; i++)
                        {
                            if (_listItemToFolder.ContainsKey(items[i]))
                                sortKeys[i] = Document.Attachments.Schema.GetDataAsString(sortFieldName, _listItemToFolder[items[i]]);
                            else
                                sortKeys[i] = Document.Attachments.Schema.GetDataAsString(sortFieldName, _listItemToFile[items[i]]);
                        }
                        ListViewRowData[] itemsArray = items.ToArray();
                        Array.Sort(sortKeys, itemsArray);
                        items.Clear();
                        bool[] ascendingOrders = _document.Attachments.Sort.AscendingOrders;
                        if (ascendingOrders != null && ascendingOrders.Length > 0)
                            if (!ascendingOrders[0])
                                Array.Reverse(itemsArray);
                        items.AddRange(itemsArray);
                    }
                }
                foreach (ListViewRowData item in items)
                    Items.Add(item);
            }
            finally
            {
                EndInit();
            }

            OnCurrentFolderChanged(new EventArgs());
        }

        /// <summary>
        /// Updates the colors of attachment viewer.
        /// </summary>
        public void UpdateColors()
        {
            System.Drawing.Color backColor = System.Drawing.SystemColors.Window;
            System.Drawing.Color foreColor = System.Drawing.SystemColors.WindowText;
            PdfPresentationColors colors = _document.Attachments.Colors;
            if (colors != null)
            {
                if (!colors.Background.IsEmpty)
                    backColor = colors.Background;
                if (!colors.PrimaryText.IsEmpty)
                    foreColor = colors.PrimaryText;
            }
            Background = new SolidColorBrush(WpfObjectConverter.CreateWindowsColor(backColor));
            Foreground = new SolidColorBrush(WpfObjectConverter.CreateWindowsColor(foreColor));

            foreach (object obj in Items)
            {
                ListViewRowData item = (ListViewRowData)obj;
                SetProperties(item);
                foreach (ListViewSubItem subItem in item.SubItems)
                    SetProperties(subItem);
            }

            Items.Refresh();
        }

        /// <summary>
        /// Adds the new folder to the current folder.
        /// </summary>
        /// <param name="name">The new foleder name.</param>
        /// <returns>The newly created folder.</returns>
        public PdfAttachmentFolder AddNewFolder(string name)
        {
            if (RootFolder == null)
            {
                _document.Attachments.RootFolder = new PdfAttachmentFolder(_document);
                _document.Attachments.RootFolder.CreationDate = DateTime.Now;
                CurrentFolder = RootFolder;
            }
            PdfAttachmentFolder newFolder = new PdfAttachmentFolder(CurrentFolder, GetFreeName(name));
            newFolder.CreationDate = DateTime.Now;
            CurrentFolder.ModificationDate = newFolder.CreationDate;
            UpdateCurrentFolder();
            SelectedFiles = null;
            SelectedFolders = new PdfAttachmentFolder[] { newFolder };
            return newFolder;
        }

        /// <summary>
        /// Adds the file to the current folder.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="compression">The file compression.</param>
        /// <returns>The newly created embedded file.</returns>
        public PdfEmbeddedFileSpecification AddFile(string filename, PdfCompression compression)
        {
            if (CurrentFolder == null)
            {
                _document.Attachments.RootFolder = new PdfAttachmentFolder(_document);
                RootFolder.CreationDate = DateTime.Now;
                CurrentFolder = RootFolder;
            }
            PdfEmbeddedFileSpecification result = AddFile(CurrentFolder, filename, compression);
            CurrentFolder.ModificationDate = DateTime.Now;
            UpdateCurrentFolder();
            return result;
        }

        /// <summary>
        /// Adds the path to the current folder.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="compression">The compression.</param>
        /// <param name="actionController">The action controller.</param>
        /// <returns>The newly created attachment folder.</returns>
        public PdfAttachmentFolder AddPath(
            string path,
            PdfCompression compression,
            StatusBarActionController actionController)
        {
            PdfAttachmentFolder result = null;
            if (CurrentFolder == null)
            {
                _document.Attachments.RootFolder = new PdfAttachmentFolder(_document);
                RootFolder.CreationDate = DateTime.Now;
                CurrentFolder = RootFolder;
            }
            try
            {
                result = AddPathRecursive(CurrentFolder, path, compression, actionController);
                CurrentFolder.ModificationDate = DateTime.Now;
                UpdateCurrentFolder();
                SelectedFiles = null;
                SelectedFolders = new PdfAttachmentFolder[] { result };
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(string.Format("{0}: {1}", path, ex.Message));
            }
            return result;
        }

        /// <summary>
        /// Deletes the selected items (files and folders).
        /// </summary>
        public void DeleteSelectedItems()
        {
            PdfEmbeddedFileSpecification[] files = SelectedFiles;
            if (files != null && files.Length > 0)
            {
                if (CurrentFolder == null)
                {
                    for (int i = 0; i < files.Length; i++)
                        _document.Attachments.DeleteFile("", files[i].Filename);
                }
                else
                {
                    for (int i = 0; i < files.Length; i++)
                        CurrentFolder.DeleteFile(files[i]);
                }
            }
            PdfAttachmentFolder[] folders = SelectedFolders;
            if (folders != null && folders.Length > 0)
            {
                for (int i = 0; i < folders.Length; i++)
                    CurrentFolder.DeleteFolder(folders[i]);
            }
            UpdateCurrentFolder();
        }

        /// <summary>
        /// Saves the selection (files and folders) to the specified path.
        /// </summary>
        /// <param name="path">The path to save.</param>
        /// <param name="actionController">The action controller.</param>
        /// <returns><b>true</b> if all files and folers saved; otherwise <b>false</b>.</returns>
        public bool SaveSelectionTo(string path, StatusBarActionController actionController)
        {
            PdfEmbeddedFileSpecification[] files = SelectedFiles;
            if (files != null)
            {
                foreach (PdfEmbeddedFileSpecification file in files)
                {
                    if (actionController != null)
                        actionController.NextSubAction(Path.GetFileName(file.Filename));
                    if (!SaveFile(file, Path.Combine(path, file.Filename)))
                        return false;
                }
            }
            PdfAttachmentFolder[] folders = SelectedFolders;
            if (folders != null)
            {
                foreach (PdfAttachmentFolder folder in folders)
                {
                    if (!SaveFolderRecursive(folder, path, actionController))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Updates view of specified folders.
        /// </summary>
        /// <param name="folders">The folders.</param>
        public void UpdateFolderView(params PdfAttachmentFolder[] folders)
        {
            if (folders != null && folders.Length > 0)
            {
                foreach (PdfAttachmentFolder folder in folders)
                {
                    if (_folderToListItem.ContainsKey(folder))
                        SetFolderItemProperties(_folderToListItem[folder], folder);
                }
                Items.Refresh();
            }
        }

        /// <summary>
        /// Updates view of specified files.
        /// </summary>
        /// <param name="fileSpecifications">The file specifications.</param>
        public void UpdateFileView(params PdfEmbeddedFileSpecification[] fileSpecifications)
        {
            if (fileSpecifications != null && fileSpecifications.Length > 0)
            {
                foreach (PdfEmbeddedFileSpecification fileSpecification in fileSpecifications)
                {
                    if (_fileToListItem.ContainsKey(fileSpecification))
                        SetFileItemProperties(_fileToListItem[fileSpecification], fileSpecification);
                }
                Items.Refresh();
            }
        }

        /// <summary>
        /// Updates the attachments schema.
        /// </summary>
        public void UpdateSchema()
        {
            Dictionary<object, double> columnsWidth = new Dictionary<object, double>();
            foreach (GridViewColumn column in _gridView.Columns)
                columnsWidth[column.Header] = column.ActualWidth;

            ClearColumns();
            // file name / folder name
            GridViewColumn nameColumn = new GridViewColumn();
            nameColumn.Header = "Name";
            nameColumn.CellTemplate = FindResource("GridNameView") as DataTemplate;
            _gridView.Columns.Add(nameColumn);

            if (_document.Attachments.Schema != null)
            {
                List<string> columnDisplayedNameList = new List<string>();
                List<string> columnNameList = new List<string>();
                List<int> columnOrderList = new List<int>();
                foreach (string schemaFieldName in _document.Attachments.Schema.Keys)
                {
                    PdfAttachmentCollectionSchemaField schemaField = _document.Attachments.Schema[schemaFieldName];
                    if (schemaField.IsVisible)
                    {
                        columnOrderList.Add(_document.Attachments.Schema[schemaFieldName].Order);
                        columnNameList.Add(schemaFieldName);
                        if (schemaField.DisplayedName != "")
                        {
                            columnDisplayedNameList.Add(schemaField.DisplayedName);
                        }
                        else
                        {
                            switch (schemaField.DataType)
                            {
                                case AttachmentCollectionSchemaFieldDataType.CompressedSize:
                                    columnDisplayedNameList.Add("Compressed Size");
                                    break;
                                case AttachmentCollectionSchemaFieldDataType.UncompressedSize:
                                    columnDisplayedNameList.Add("Uncompressed Size");
                                    break;
                                case AttachmentCollectionSchemaFieldDataType.ModificationDate:
                                    columnDisplayedNameList.Add("Modification Date");
                                    break;
                                case AttachmentCollectionSchemaFieldDataType.CreationDate:
                                    columnDisplayedNameList.Add("Creation Date");
                                    break;
                                case AttachmentCollectionSchemaFieldDataType.Filename:
                                    columnDisplayedNameList.Add("File name");
                                    break;
                                case AttachmentCollectionSchemaFieldDataType.FileDescription:
                                    columnDisplayedNameList.Add("Description");
                                    break;
                                default:
                                    columnDisplayedNameList.Add(schemaFieldName);
                                    break;
                            }
                        }
                    }
                }
                string[] columnDisplayedNames = columnDisplayedNameList.ToArray();
                string[] columnNames = columnNameList.ToArray();
                int[] columnOrders = columnOrderList.ToArray();
                Array.Sort(columnOrders, columnDisplayedNames);
                columnOrders = columnOrderList.ToArray();
                Array.Sort(columnOrders, columnNames);
                for (int i = 0; i < columnOrders.Length; i++)
                {
                    AddColumn(columnNames[i], columnDisplayedNames[i]);
                }
            }

            _useSchema = _gridView.Columns.Count > 1;
            if (_useSchema)
            {
            }
            else
            {
                AddColumn("Compressed Size");
                AddColumn("Uncompressed Size");
                AddColumn("Modified date");
            }

            for (int i = 0; i < _gridView.Columns.Count; i++)
                if (columnsWidth.ContainsKey(_gridView.Columns[i].Header))
                    _gridView.Columns[i].Width = columnsWidth[_gridView.Columns[i].Header];
                else
                    _gridView.Columns[i].Width = 100;
        }

        #endregion


        #region PROTECTED

        /// <summary>
        /// Raises the <see cref="E:CurrentFolderChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnCurrentFolderChanged(EventArgs e)
        {
            if (CurrentFolderChanged != null)
                CurrentFolderChanged(this, e);
        }

        #endregion


        #region PRIVATE

        private GridViewColumn AddColumn(string text)
        {
            return AddColumn(text, text);
        }

        private GridViewColumn AddColumn(string name, string text)
        {
            GridViewColumn column = new GridViewColumn();
            column.Header = text;
            Binding binding = new Binding(string.Format("SubItems[{0}].Text", _gridView.Columns.Count - 1));
            column.DisplayMemberBinding = binding;
            _columnNames.Add(column, name);
            _gridView.Columns.Add(column);
            return column;
        }

        private void ClearColumns()
        {
            _columnNames.Clear();
            _gridView.Columns.Clear();
        }

        /// <summary>
        /// Creates a list item for attachment folder.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns><see cref="ListViewItem"/> instance.</returns>
        private ListViewRowData CreateItem(PdfAttachmentFolder folder)
        {
            ListViewRowData item = new ListViewRowData();
            _listItemToFolder[item] = folder;
            _folderToListItem[folder] = item;
            SetFolderItemProperties(item, folder);
            return item;
        }

        /// <summary>
        /// Creates a list item for embedded file.
        /// </summary>
        /// <param name="file">The embedded file.</param>
        /// <returns><see cref="ListViewItem"/> instance.</returns>
        private ListViewRowData CreateItem(PdfEmbeddedFileSpecification file)
        {
            ListViewRowData item = new ListViewRowData();
            SetFileItemProperties(item, file);
            _listItemToFile[item] = file;
            _fileToListItem[file] = item;
            return item;
        }

        /// <summary>
        /// Sets the file item properties.
        /// </summary>
        /// <param name="item">The list item.</param>
        /// <param name="file">The embedded file.</param>
        private void SetFileItemProperties(ListViewRowData item, PdfEmbeddedFileSpecification file)
        {
            // set thumbnail

            if (file.Thumbnail != null)
            {
                if (item.ImageIndex > StandardIconCount)
                {
                    SetImage(_largeImageList, item.ImageIndex, file.Thumbnail);
                    SetImage(_smallImageList, item.ImageIndex, file.Thumbnail);
                }
                else
                {
                    int newIndex = _largeImageList.Count;
                    SetImage(_largeImageList, newIndex, file.Thumbnail);
                    SetImage(_smallImageList, newIndex, file.Thumbnail);
                    item.ImageIndex = newIndex;
                }
            }
            else
            {
                if (System.IO.Path.GetExtension(file.Filename).ToUpperInvariant() == ".PDF")
                    item.ImageIndex = 2;
                else
                    item.ImageIndex = 3;
            }


            // set properties

            item.SubItems.Clear();
            item.Text = file.Filename;
            // if schema from PDF must be used
            if (_useSchema)
            {
                // add field values of schema
                for (int i = 1; i < _gridView.Columns.Count; i++)
                {
                    string schemaFieldName = _columnNames[_gridView.Columns[i]];
                    PdfAttachmentCollectionSchemaField schemaField = _document.Attachments.Schema[schemaFieldName];
                    if (schemaField.IsVisible)
                    {
                        if (schemaField.DataType == AttachmentCollectionSchemaFieldDataType.Date ||
                            schemaField.DataType == AttachmentCollectionSchemaFieldDataType.Number ||
                            schemaField.DataType == AttachmentCollectionSchemaFieldDataType.String)
                        {
                            string fieldDataAsString = _document.Attachments.Schema.GetDataAsString(schemaFieldName, file);
                            PdfAttachmentDataField dataField = _document.Attachments.Schema.GetDataField(schemaFieldName, file);
                            if (dataField != null && dataField.Prefix != null)
                                fieldDataAsString = dataField.Prefix + fieldDataAsString;
                            item.SubItems.Add(new ListViewSubItem(fieldDataAsString));
                            SetProperties(item.SubItems[item.SubItems.Count - 1]);
                        }
                        else
                        {
                            AddStandardFieldValue(item, schemaField.DataType, file);
                        }
                    }
                }
            }
            else
            {
                // Compressed Size
                AddStandardFieldValue(item, AttachmentCollectionSchemaFieldDataType.CompressedSize, file);
                // Uncompressed Size
                AddStandardFieldValue(item, AttachmentCollectionSchemaFieldDataType.UncompressedSize, file);
                // Modified date
                AddStandardFieldValue(item, AttachmentCollectionSchemaFieldDataType.ModificationDate, file);
            }

            SetProperties(item);
        }

        /// <summary>
        /// Sets the folder item properties.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="folder">The folder.</param>
        private void SetFolderItemProperties(ListViewRowData item, PdfAttachmentFolder folder)
        {
            if (folder.Thumbnail != null)
            {
                if (item.ImageIndex > StandardIconCount)
                {
                    SetImage(_largeImageList, item.ImageIndex, folder.Thumbnail);
                    SetImage(_smallImageList, item.ImageIndex, folder.Thumbnail);
                }
                else
                {
                    int newIndex = _largeImageList.Count;
                    SetImage(_largeImageList, newIndex, folder.Thumbnail);
                    SetImage(_smallImageList, newIndex, folder.Thumbnail);
                    item.ImageIndex = newIndex;
                }
            }
            else
            {
                if (folder.IsContainsFiles || folder.IsContainsFolders)
                    item.ImageIndex = 1;
                else
                    item.ImageIndex = 0;
            }

            // set properties

            item.SubItems.Clear();
            item.Text = folder.Name;
            // if schema from PDF must  be used
            if (_useSchema)
            {
                // add filled values of schema
                for (int i = 1; i < _gridView.Columns.Count; i++)
                {
                    string schemaFieldName = _columnNames[_gridView.Columns[i]];
                    PdfAttachmentCollectionSchemaField schemaField = _document.Attachments.Schema[schemaFieldName];
                    if (schemaField.IsVisible)
                    {
                        if (schemaField.DataType == AttachmentCollectionSchemaFieldDataType.Date ||
                            schemaField.DataType == AttachmentCollectionSchemaFieldDataType.Number ||
                            schemaField.DataType == AttachmentCollectionSchemaFieldDataType.String)
                        {
                            string fieldDataAsString = _document.Attachments.Schema.GetDataAsString(schemaFieldName, folder);
                            PdfAttachmentDataField dataField = _document.Attachments.Schema.GetDataField(schemaFieldName, folder);
                            if (dataField != null && dataField.Prefix != null)
                                fieldDataAsString = dataField.Prefix + fieldDataAsString;
                            item.SubItems.Add(new ListViewSubItem(fieldDataAsString));
                            SetProperties(item.SubItems[item.SubItems.Count - 1]);
                        }
                        else
                        {
                            AddStandardFieldValue(item, schemaField.DataType, folder);
                        }
                    }
                }
            }
            else
            {
                // Compressed Size
                item.SubItems.Add(new ListViewSubItem(""));
                // Uncompressed Size
                item.SubItems.Add(new ListViewSubItem(""));
                // Modified date
                AddStandardFieldValue(item, AttachmentCollectionSchemaFieldDataType.ModificationDate, folder);
            }

            SetProperties(item);
        }

        /// <summary>
        /// Sets the properties of list item.
        /// </summary>
        /// <param name="item">The item.</param>
        private void SetProperties(ListViewRowData item)
        {
            System.Drawing.Color backColor = System.Drawing.Color.Transparent;
            if (_document.Attachments.Colors != null)
            {
                backColor = _document.Attachments.Colors.CardBackground;
            }
            item.BackColor = new SolidColorBrush(WpfObjectConverter.CreateWindowsColor(backColor));
        }

        /// <summary>
        /// Sets the properties of list sub item (data field).
        /// </summary>
        /// <param name="item">The sub item.</param>
        private void SetProperties(ListViewSubItem item)
        {
            System.Drawing.Color backColor = System.Drawing.Color.Transparent;
            System.Drawing.Color foreColor = System.Drawing.SystemColors.WindowText;
            if (_document.Attachments.Colors != null)
            {
                backColor = _document.Attachments.Colors.CardBackground;
                foreColor = _document.Attachments.Colors.SecondaryText;
            }

            item.BackColor = new SolidColorBrush(WpfObjectConverter.CreateWindowsColor(backColor));
            item.ForeColor = new SolidColorBrush(WpfObjectConverter.CreateWindowsColor(foreColor));
        }


        /// <summary>
        /// Adds the standard field value to list item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="fileSpecification">The file specification.</param>
        private void AddStandardFieldValue(ListViewRowData item, AttachmentCollectionSchemaFieldDataType dataType, PdfEmbeddedFileSpecification fileSpecification)
        {
            string textValue = "N/A";
            switch (dataType)
            {
                case AttachmentCollectionSchemaFieldDataType.CompressedSize:
                    if (fileSpecification.EmbeddedFile != null)
                        textValue = FileSizeToString(fileSpecification.EmbeddedFile.Length);
                    break;
                case AttachmentCollectionSchemaFieldDataType.CreationDate:
                    if (fileSpecification.EmbeddedFile != null)
                        textValue = ToString(fileSpecification.EmbeddedFile.CreationDate);
                    break;
                case AttachmentCollectionSchemaFieldDataType.ModificationDate:
                    if (fileSpecification.EmbeddedFile != null)
                        textValue = ToString(fileSpecification.EmbeddedFile.ModifyDate);
                    break;
                case AttachmentCollectionSchemaFieldDataType.FileDescription:
                    textValue = fileSpecification.Description;
                    break;
                case AttachmentCollectionSchemaFieldDataType.Filename:
                    textValue = fileSpecification.Filename;
                    break;
                case AttachmentCollectionSchemaFieldDataType.UncompressedSize:
                    if (fileSpecification.EmbeddedFile != null)
                        textValue = FileSizeToString(fileSpecification.EmbeddedFile.UncompressedLength);
                    break;
            }
            item.SubItems.Add(new ListViewSubItem(textValue));
            SetProperties(item.SubItems[item.SubItems.Count - 1]);
        }

        /// <summary>
        /// Adds the standard field value to list item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="fileSpecification">The file specification.</param>
        private void AddStandardFieldValue(
            ListViewRowData item,
            AttachmentCollectionSchemaFieldDataType dataType,
            PdfAttachmentFolder folder)
        {
            string textValue = "N/A";
            switch (dataType)
            {
                case AttachmentCollectionSchemaFieldDataType.CreationDate:
                    textValue = ToString(folder.CreationDate);
                    break;
                case AttachmentCollectionSchemaFieldDataType.ModificationDate:
                    textValue = ToString(folder.ModificationDate);
                    break;
                case AttachmentCollectionSchemaFieldDataType.FileDescription:
                    textValue = folder.Description;
                    break;
                case AttachmentCollectionSchemaFieldDataType.Filename:
                    textValue = folder.Name;
                    break;
            }
            item.SubItems.Add(new ListViewSubItem(textValue));
            SetProperties(item.SubItems[item.SubItems.Count - 1]);
        }

        /// <summary>
        /// Converts file size to a string.
        /// </summary>
        /// <param name="sizeInBytes">The size in bytes.</param>
        /// <returns>String with file size representation.</returns>
        private static string FileSizeToString(long sizeInBytes)
        {
            if (sizeInBytes < 1024)
                return string.Format("{0} bytes", sizeInBytes);
            if (sizeInBytes < 1024 * 1024)
                return string.Format("{0:f1} KB", sizeInBytes / 1024f);
            return string.Format("{0:f1} MB", sizeInBytes / (1024f * 1024f));
        }

        /// <summary>
        /// Converts date time structure to a string.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>String with DateTime representation.</returns>
        private static string ToString(DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue)
                return "N/A";
            return dateTime.ToString();
        }

        /// <summary>
        /// Adds the image to the specified image list.
        /// </summary>
        /// <param name="imageList">The image list.</param>
        /// <param name="sourceImage">The source image.</param>
        private void AddImage(List<BitmapSource> imageList, VintasoftImage sourceImage)
        {
            SetImage(imageList, imageList.Count, sourceImage);
        }

        /// <summary>
        /// Sets the image with specified index in image list.
        /// </summary>
        /// <param name="imageList">The image list.</param>
        /// <param name="imageIndex">Index of the image.</param>
        /// <param name="sourceImage">The source image.</param>
        private void SetImage(List<BitmapSource> imageList, int imageIndex, VintasoftImage sourceImage)
        {
            Size size = _largeImageSize;
            if (imageList == _smallImageList)
                size = _smallImageSize;

            using (VintasoftImage resultImage = new VintasoftImage(
                (int)size.Width, (int)size.Height, Vintasoft.Imaging.PixelFormat.Bgra32))
            {
                ClearImageCommand clearImage = new ClearImageCommand(System.Drawing.Color.Transparent);
                clearImage.ExecuteInPlace(resultImage);
                System.Drawing.Graphics g = resultImage.OpenGraphics();
                float scale = Math.Min(
                    resultImage.Width / (float)sourceImage.Width,
                    resultImage.Height / (float)sourceImage.Height);
                float width = sourceImage.Width * scale;
                float height = sourceImage.Height * scale;
                float x = (resultImage.Width - width) / 2;
                float y = (resultImage.Height - height) / 2;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                sourceImage.Draw(g, new System.Drawing.RectangleF(x, y, width, height));
                resultImage.CloseGraphics();

                BitmapSource bitmapSource = VintasoftImageConverter.ToBitmapSource(resultImage);

                if (imageIndex == imageList.Count)
                {
                    imageList.Add(bitmapSource);
                }
                else
                {
                    imageList[imageIndex] = bitmapSource;
                }
            }
        }

        /// <summary>
        /// Sets the image resource with specified index in image list.
        /// </summary>
        /// <param name="imageList">The image list.</param>
        /// <param name="imageIndex">Index of the image.</param>
        /// <param name="imageResource">The image resource.</param>
        private void SetImage(
            List<BitmapSource> imageList,
            int imageIndex,
            PdfImageResource imageResource)
        {
            using (VintasoftImage sourceImage = imageResource.GetImage())
                SetImage(imageList, imageIndex, sourceImage);
        }

        /// <summary>
        /// Gets the files and sub folders of specified folder.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="subFolders">The sub folders.</param>
        /// <param name="files">The files.</param>
        private void GetFilesAndFolders(
            PdfAttachmentFolder folder,
            out PdfAttachmentFolder[] subFolders,
            out PdfEmbeddedFileSpecification[] files)
        {
            if (folder == null && _document.Attachments.RootFolder == null)
            {
                subFolders = null;
                files = _document.Attachments.GetFiles("");
                return;
            }
            if (folder == null)
                folder = _document.Attachments.RootFolder;

            subFolders = folder.Folders;
            files = folder.Files;
        }


        /// <summary>
        /// Resets the User Interface.
        /// </summary>
        private void ResetUI()
        {
            if (_document == null)
            {
                ClearColumns();
                Items.Clear();
                IsEnabled = false;
            }
            else
            {
                IsEnabled = true;

                if (_document.Attachments != null)
                {
                    UpdateSchema();
                    UpdateColors();
                    CurrentFolder = _document.Attachments.RootFolder;
                }
                else
                {
                    ClearColumns();
                    Items.Clear();
                }
            }
        }

        /// <summary>
        /// Gets a free name of in current folder.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Free name.</returns>
        private string GetFreeName(string name)
        {
            List<string> names = new List<string>();
            names.AddRange(CurrentFolder.GetFilenames());
            names.AddRange(CurrentFolder.GetFolderNames());
            if (!names.Contains(name))
                return name;
            int i = 1;
            while (i < int.MaxValue)
            {
                string newName = string.Format("{0}{1}", name, i);
                if (!names.Contains(newName))
                    return newName;
                i++;
            }
            return null;
        }

        /// <summary>
        /// Adds the path (all files and sub folders) to the specified folder.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="path">The path.</param>
        /// <param name="compression">The compression.</param>
        /// <param name="actionController">The action controller.</param>
        /// <returns>Added folder.</returns>
        private PdfAttachmentFolder AddPathRecursive(
            PdfAttachmentFolder folder,
            string path,
            PdfCompression compression,
            StatusBarActionController actionController)
        {
            // add folder
            PdfAttachmentFolder subFolder = folder.AddFolder(Path.GetFileName(path));
            subFolder.CreationDate = DateTime.Now;

            // add files
            string[] files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                foreach (string filename in files)
                {
                    try
                    {
                        if ((File.GetAttributes(filename) & FileAttributes.Hidden) == 0)
                        {
                            if (actionController != null)
                                actionController.NextSubAction(Path.GetFileName(filename));

                            PdfEmbeddedFileSpecification file = AddFile(subFolder, filename, compression);
                            file.EmbeddedFile.CreationDate = DateTime.Now;
                        }
                    }
                    catch (Exception ex)
                    {
                        DemosTools.ShowErrorMessage(string.Format("{0}: {1}", filename, ex.Message));
                    }
                }
                subFolder.ModificationDate = DateTime.Now;
            }

            // add sub folders
            string[] paths = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            foreach (string subPath in paths)
            {
                try
                {
                    if ((File.GetAttributes(subPath) & FileAttributes.Hidden) == 0)
                        AddPathRecursive(subFolder, subPath, compression, actionController);
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(string.Format("{0}: {1}", subFolder, ex.Message));
                }
            }

            return subFolder;
        }

        /// <summary>
        /// Saves the file to the specified file path.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="filename">The filename.</param>
        /// <returns><b>true</b> if file saved; otherwise <b>false</b>.</returns>
        private bool SaveFile(PdfEmbeddedFileSpecification file, string filename)
        {
            try
            {
                if (file.EmbeddedFile != null)
                {
                    if (File.Exists(filename))
                    {
                        MessageBoxResult dialogResult = MessageBox.Show(string.Format("File '{0}' already exists, override it?", filename), "", MessageBoxButton.YesNoCancel);
                        if (dialogResult == MessageBoxResult.Cancel)
                            return false;
                        if (dialogResult == MessageBoxResult.No)
                            return true;
                    }
                    file.EmbeddedFile.Save(filename);
                }
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Saves the folder (all sub folders and embedded files) to the specifed path.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="path">The path.</param>
        /// <param name="actionController">The action controller.</param>
        /// <returns><b>true</b> if folder saved; otherwise <b>false</b>.</returns>
        private bool SaveFolderRecursive(PdfAttachmentFolder folder, string path, StatusBarActionController actionController)
        {
            // create path
            try
            {
                path = Path.Combine(path, folder.Name);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
                return false;
            }

            // save embedded files
            PdfEmbeddedFileSpecification[] files = folder.Files;
            if (files != null)
            {
                foreach (PdfEmbeddedFileSpecification file in files)
                {
                    if (actionController != null)
                        actionController.NextSubAction(Path.GetFileName(file.Filename));
                    if (!SaveFile(file, Path.Combine(path, file.Filename)))
                        return false;
                }
            }

            // save sub folders
            PdfAttachmentFolder[] subFolders = folder.Folders;
            if (subFolders != null)
            {
                foreach (PdfAttachmentFolder subFolder in subFolders)
                {
                    if (!SaveFolderRecursive(subFolder, path, actionController))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Handles the MouseLeftButtonUp event of PdfAttachmentViewer object.
        /// </summary>
        void PdfAttachmentViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer)
            {
                this.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Adds the file to to specified folder.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="compression">The file compression.</param>
        /// <returns>A new instance of <see cref="PdfEmbeddedFileSpecification"/>.</returns>
        private PdfEmbeddedFileSpecification AddFile(PdfAttachmentFolder folder, string filename, PdfCompression compression)
        {
            folder.EncodeFileImmediately = EncodeFileImmediately;
            return folder.AddFile(filename, compression);
        }

        #endregion

        #endregion



        #region Events

        /// <summary>
        /// Occurs when current folder is changed.
        /// </summary>
        public event EventHandler CurrentFolderChanged;

        #endregion

    }
}
