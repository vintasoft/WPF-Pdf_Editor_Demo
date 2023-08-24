using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

using Microsoft.Win32;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Codecs;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.FileAttachments;

using WpfDemosCommonCode.Imaging;
using WpfDemosCommonCode.Imaging.Codecs;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Form that allows to view and edit PDF attachments (portfolio).
    /// </summary>
    public partial class AttachmentsEditorWindow : Window
    {

        #region Fields

        /// <summary>
        /// The PDF document.
        /// </summary>
        PdfDocument _document;

        /// <summary>
        /// The action controller.
        /// </summary>
        StatusBarActionController _actionController;

        /// <summary>
        /// Indicates when UI is updating.
        /// </summary>
        bool _updatingUI = false;

        public static RoutedCommand _createNewFolderCommand = new RoutedCommand();
        public static RoutedCommand _saveSelectedFilesToCommand = new RoutedCommand();
        public static RoutedCommand _closeCommand = new RoutedCommand();
        public static RoutedCommand _levelUpCommand = new RoutedCommand();
        public static RoutedCommand _moveToRootCommand = new RoutedCommand();
        public static RoutedCommand _deleteSelectedCommand = new RoutedCommand();
        public static RoutedCommand _selectAllCommand = new RoutedCommand();

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentsEditorWindow"/> class.
        /// </summary>
        public AttachmentsEditorWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentsEditorWindow"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public AttachmentsEditorWindow(PdfDocument document)
            : this()
        {
            _actionController = new StatusBarActionController(statusBar1, actionStatusLabel, actionProgressBar, this);

            if (document.Attachments == null)
            {
                document.CreateAttachments(true);
                document.Attachments.View = AttachmentCollectionViewMode.TileMode;
            }

            attachmentViewer.Document = document;
            viewModeComboBox.Items.Add(ViewMode.LargeIcon);
            viewModeComboBox.Items.Add(ViewMode.Details);
            if (document.Attachments.View == AttachmentCollectionViewMode.DetailsMode)
                viewModeComboBox.SelectedItem = ViewMode.Details;
            else
                viewModeComboBox.SelectedItem = ViewMode.LargeIcon;

            initialViewModeComboBox.Items.Add(AttachmentCollectionViewMode.TileMode);
            initialViewModeComboBox.Items.Add(AttachmentCollectionViewMode.DetailsMode);
            initialViewModeComboBox.Items.Add(AttachmentCollectionViewMode.Hidden);
            initialViewModeComboBox.Items.Add(AttachmentCollectionViewMode.Custom);
            initialViewModeComboBox.SelectedItem = document.Attachments.View;

            fileCompressionComboBox.Items.Add(PdfCompression.None);
            fileCompressionComboBox.Items.Add(PdfCompression.Zip);
            fileCompressionComboBox.Items.Add(PdfCompression.Lzw);
            fileCompressionComboBox.SelectedItem = PdfCompression.None;

            encodeFilesImmediatelyMenuItem.IsChecked = true;

            _document = document;
            UpdateUI();
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this editor is read only.
        /// </summary>
        /// <value>
        /// <b>true</b> - editor does NOT allow to edit attachments (portfolio);
        /// <b>false</b> - editor allows to edit attachments (portfolio).
        /// </value>
        [DefaultValue(false)]
        public bool IsReadOnly
        {
            get
            {
                return addFilesButton.Visibility != Visibility.Visible;
            }
            set
            {
                Visibility isEditor = (value ? Visibility.Collapsed : Visibility.Visible);
                addFilesButton.Visibility = isEditor;
                addFilesMenuItem.Visibility = isEditor;
                createNewFolderButton.Visibility = isEditor;
                createNewFolderMenuItem.Visibility = isEditor;
                addExistingFolderButton.Visibility = isEditor;
                addExistingFolderMenuItem.Visibility = isEditor;
                compressionOfNewFilesMenuItem.Visibility = isEditor;
                initialViewModeMenuItem.Visibility = isEditor;
                sortMenuItem.Visibility = isEditor;
                colorsMenuItem.Visibility = isEditor;
                resetColorsMenuItem.Visibility = isEditor;
                setThumbnailForAllFoldersMenuItem.Visibility = isEditor;
                setThumbnailForSelectedItemsMenuItem.Visibility = isEditor;
                generateThumbnailsForAllFilesMenuItem.Visibility = isEditor;
                generateThumbnailsForlSelectedFilesMenuItem.Visibility = isEditor;
                editMenuItem.Visibility = isEditor;
                schemaMenuItem.Visibility = isEditor;
                separator5.Visibility = isEditor;
                separator7.Visibility = isEditor;
                separator9.Visibility = isEditor;
                itemsPropertyGrid.Enabled = !value;
            }
        }

        #endregion



        #region Methods

        #region 'File' menu

        /// <summary>
        /// Adds files to current folder in portfolio.
        /// </summary>
        private void addFilesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Multiselect = true;
            openFile.FileName = "";
            openFile.Filter = "All Files (*.*)|*.*";
            openFile.FilterIndex = 0;
            if (openFile.ShowDialog().Value)
            {
                List<PdfEmbeddedFileSpecification> addedFiles = new List<PdfEmbeddedFileSpecification>();
                _actionController.StartAction("Add files", openFile.FileNames.Length);
                foreach (string filename in openFile.FileNames)
                {
                    _actionController.NextSubAction(Path.GetFileName(filename));
                    try
                    {
                        addedFiles.Add(attachmentViewer.AddFile(filename, (PdfCompression)fileCompressionComboBox.SelectedItem));
                    }
                    catch (OverflowException ex)
                    {
                        DemosTools.ShowErrorMessage(string.Format("{0}: {1}.\nDisable 'Encode Files Immediately' option in 'File' menu.", Path.GetFileName(filename), ex.Message));
                    }
                    catch (Exception ex)
                    {
                        DemosTools.ShowErrorMessage(string.Format("{0}: {1}", Path.GetFileName(filename), ex.Message));
                    }
                }
                _actionController.EndAction();
                attachmentViewer.SelectedFiles = addedFiles.ToArray();
                attachmentViewer.SelectedFolders = null;
            }
        }

        /// <summary>
        /// Handles the Click event of EncodeFilesImmediatelyMenuItem object.
        /// </summary>
        private void encodeFilesImmediatelyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            encodeFilesImmediatelyMenuItem.IsChecked = !encodeFilesImmediatelyMenuItem.IsChecked;
            attachmentViewer.EncodeFileImmediately = encodeFilesImmediatelyMenuItem.IsChecked;
        }

        /// <summary>
        /// Adds new folder to current folder in portfolio.
        /// </summary>
        private void createNewFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            attachmentViewer.AddNewFolder("NewFolder");
        }

        /// <summary>
        /// Adds path to current folder in portfolio.
        /// </summary>
        private void addExistingFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openFolder = new System.Windows.Forms.FolderBrowserDialog();
            if (openFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _actionController.StartAction("Add path");
                attachmentViewer.AddPath(openFolder.SelectedPath, (PdfCompression)fileCompressionComboBox.SelectedItem, _actionController);
                _actionController.EndAction();
            }
        }

        /// <summary>
        /// Saves selected files and folders to disk.
        /// </summary>
        private void saveSelectedFilesToMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openFolder = new System.Windows.Forms.FolderBrowserDialog();
            if (openFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _actionController.StartAction("Save selected items");
                bool result = attachmentViewer.SaveSelectionTo(openFolder.SelectedPath, _actionController);
                _actionController.EndAction();
                if (result)
                    MessageBox.Show("Item(s) saved successfully.");
                else
                    MessageBox.Show("Item(s) does not saved.");
            }
        }

        /// <summary>
        /// Closes this form.
        /// </summary>
        private void closeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion


        #region 'View' menu

        /// <summary>
        /// Changes view mode of attachment viewer.
        /// </summary>
        private void viewModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            attachmentViewer.BeginInit();
            attachmentViewer.ViewMode = (ViewMode)viewModeComboBox.SelectedItem;
            ColumnsAutoResize();
            attachmentViewer.EndInit();
        }

        /// <summary>
        /// Sets attachment viewer view mode to large icons mode.
        /// </summary>
        private void iconViewModeButton_Click(object sender, RoutedEventArgs e)
        {
            viewModeComboBox.SelectedItem = ViewMode.LargeIcon;
            UpdateUI();
        }

        /// <summary>
        /// Sets attachment viewer view mode to details (table) mode.
        /// </summary>
        private void detailViewModeButton_Click(object sender, RoutedEventArgs e)
        {
            viewModeComboBox.SelectedItem = ViewMode.Details;
            UpdateUI();
        }

        /// <summary>
        /// Changes initial view mode of attachments.
        /// </summary>
        private void initialViewModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_document != null)
                _document.Attachments.View = (AttachmentCollectionViewMode)initialViewModeComboBox.SelectedItem;
        }

        /// <summary>
        /// Changes field name that is used as sort item in attachments.
        /// </summary>
        private void sortFieldNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_updatingUI && e.AddedItems != null && e.RemovedItems != null)
            {
                string text = string.Empty;
                if (e.AddedItems.Count >= 1)
                    text = e.AddedItems[0].ToString();
                if (_document.Attachments.Sort == null)
                    _document.Attachments.Sort = new PdfAttachmentCollectionSort(_document);
                if (string.IsNullOrEmpty(text))
                    _document.Attachments.Sort.FieldNames = null;
                else
                    _document.Attachments.Sort.FieldNames = new string[] { text };
                attachmentViewer.UpdateCurrentFolder();
            }
        }

        /// <summary>
        /// Changes sort ascending order.
        /// </summary>
        private void ascendingOrderMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            if (!_updatingUI)
            {
                if (_document.Attachments.Sort == null)
                    _document.Attachments.Sort = new PdfAttachmentCollectionSort(_document);
                _document.Attachments.Sort.AscendingOrders = new bool[] { ascendingOrderMenuItem.IsChecked };
                attachmentViewer.UpdateCurrentFolder();
            }
        }

        /// <summary>
        /// Removes sort information.
        /// </summary>
        private void removeSortInformationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to remove sort information, which specifies the order in which items in the attachment collection should be sorted in the user interface?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _document.Attachments.Sort = null;
                UpdateUI();
            }
        }

        /// <summary>
        /// Navigates to up level (parent folder).
        /// </summary>
        private void levelUpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            attachmentViewer.CurrentFolder = attachmentViewer.CurrentFolder.Parent;
        }

        /// <summary>
        /// Navigates to root folder.
        /// </summary>
        private void moveToRootMenuItem_Click(object sender, RoutedEventArgs e)
        {
            attachmentViewer.CurrentFolder = attachmentViewer.RootFolder;
        }

        /// <summary>
        /// Shows a form that allows to edit colors of attachments (portfolio).
        /// </summary>
        private void colorsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_document.Attachments.Colors == null)
            {
                if (MessageBox.Show("Colors are not specified. Do you want to create information about colors?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    _document.Attachments.Colors = new PdfPresentationColors(_document);
            }
            if (_document.Attachments.Colors != null)
            {
                PropertyGridWindow form = new PropertyGridWindow(_document.Attachments.Colors, "Portfolio Colors");
                form.ShowDialog();
                attachmentViewer.UpdateColors();
            }
        }

        /// <summary>
        /// Deletes information about colors of attachments.
        /// </summary>
        private void resetColorsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to delete information about portfolio colors?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _document.Attachments.Colors = null;
                attachmentViewer.UpdateColors();
                UpdateUI();
            }
        }

        /// <summary>
        /// Generates thumbnails for all embedded files.
        /// </summary>
        private void generateThumbnailsForAllFilesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to generate thumbnails for all files?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _actionController.StartAction("Generate thumbnails");
                if (_document.Attachments.RootFolder != null)
                {
                    string[] folderFullNames = _document.Attachments.GetFolderFullNames();
                    foreach (string folderFullName in folderFullNames)
                        GenerateThumbnails(_document.Attachments.GetFiles(folderFullName));
                }
                else
                {
                    GenerateThumbnails(_document.Attachments.GetFiles(""));
                }
                _actionController.EndAction();
                attachmentViewer.UpdateFileView(attachmentViewer.CurrentFolder.Files);
            }
        }

        /// <summary>
        /// Generates thumbnails for selected files and all files in selected folders.
        /// </summary>
        private void generateThumbnailsForlSelectedFilesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfAttachmentFolder[] folders = attachmentViewer.SelectedFolders;
            PdfEmbeddedFileSpecification[] files = attachmentViewer.SelectedFiles;

            if (files.Length == 0 && folders.Length == 0)
                return;

            if (files.Length > 0 && folders.Length > 0)
            {
                if (MessageBox.Show("Do you want to generate thumbnails for selected files and files in all selected folders?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }
            else if (files.Length > 0)
            {
                if (MessageBox.Show("Do you want to generate thumbnails for selected files?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }
            else if (folders.Length > 0)
            {
                if (MessageBox.Show("Do you want to generate thumbnails for files in all selected folders?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }

            _actionController.StartAction("Generate thumbnails");
            GenerateThumbnails(files);
            foreach (PdfAttachmentFolder folder in folders)
            {
                string folderFullName = _document.Attachments.GetFolderFullName(folder);
                GenerateThumbnails(_document.Attachments.GetFiles(folderFullName));
                string[] subFolderFullNames = _document.Attachments.GetSubFolderFullNames(folderFullName, false);
                foreach (string subFolderFullName in subFolderFullNames)
                    GenerateThumbnails(_document.Attachments.GetFiles(subFolderFullName));
            }
            _actionController.EndAction();

            attachmentViewer.UpdateFileView(files);
        }

        /// <summary>
        /// Sets thumbnail for all folders.
        /// </summary>
        private void setThumbnailForAllFoldersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_document.Attachments.RootFolder != null)
                {
                    OpenFileDialog openImageFile = new OpenFileDialog();
                    openImageFile.Title = "Open thumbnail image";
                    CodecsFileFilters.SetFilters(openImageFile);
                    if (openImageFile.ShowDialog().Value)
                    {
                        _actionController.StartAction("Set thumbnail");
                        using (VintasoftImage image = new VintasoftImage(openImageFile.FileName))
                        {
                            PdfImageResource thumbnailResource = CreateThumbnailImageResource(image);
                            SetThumbnailRecursive(_document.Attachments.RootFolder, thumbnailResource);
                        }
                        _actionController.EndAction();
                        attachmentViewer.UpdateFolderView(attachmentViewer.CurrentFolder.Folders);
                    }
                }
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// Sets thumbnails for all selected items.
        /// </summary>
        private void setThumbnailForSelectedItemsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfAttachmentFolder[] folders = attachmentViewer.SelectedFolders;
            PdfEmbeddedFileSpecification[] files = attachmentViewer.SelectedFiles;
            if (folders.Length > 0 || files.Length > 0)
            {
                OpenFileDialog openImageFile = new OpenFileDialog();
                openImageFile.Title = "Open thumbnail image";
                CodecsFileFilters.SetFilters(openImageFile);
                if (openImageFile.ShowDialog().Value)
                {
                    _actionController.StartAction("Set thumbnail");
                    using (VintasoftImage image = new VintasoftImage(openImageFile.FileName))
                    {
                        PdfImageResource thumbnailResource = CreateThumbnailImageResource(image);
                        foreach (PdfAttachmentFolder folder in folders)
                            folder.Thumbnail = thumbnailResource;
                        foreach (PdfEmbeddedFileSpecification file in files)
                            file.Thumbnail = thumbnailResource;
                        attachmentViewer.UpdateFolderView(folders);
                        attachmentViewer.UpdateFileView(files);
                    }
                    _actionController.EndAction();
                }
            }
        }

        #endregion


        #region 'Edit' menu

        /// <summary>
        /// Deletes selected files and folders in current folder.
        /// </summary>
        private void deleteSelectedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            attachmentViewer.DeleteSelectedItems();
        }

        /// <summary>
        /// Selects all files and folders in current folder.
        /// </summary>
        private void selectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            attachmentViewer.SelectedFiles = attachmentViewer.FilesInCurrentFolder;
            attachmentViewer.SelectedFolders = attachmentViewer.FoldersInCurrentFolder;
            attachmentViewer.Focus();
        }

        #endregion


        #region 'Schema' menu

        /// <summary>
        /// Shows attachments schema editor.
        /// </summary>
        private void schemaEditorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_document.Attachments.Schema == null)
            {
                if (MessageBox.Show("Attachments does not have schema. Do you want to create schema?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
                _document.Attachments.Schema = new PdfAttachmentCollectionSchema(_document);
            }
            PdfNamedDictionaryItemSet<PdfAttachmentCollectionSchemaField> itemSet =
                    new PdfNamedDictionaryItemSet<PdfAttachmentCollectionSchemaField>(_document.Attachments.Schema, AddNewAttachmentsSchemaField);
            ItemSetEditorWindow editorForm = new ItemSetEditorWindow(itemSet);
            editorForm.Owner = this;
            editorForm.Title = "Attachments Schema Editor";
            editorForm.ShowDialog();

            attachmentViewer.BeginInit();
            try
            {
                attachmentViewer.UpdateSchema();
                attachmentViewer.UpdateFileView(attachmentViewer.FilesInCurrentFolder);
            }
            finally
            {
                attachmentViewer.EndInit();
            }
        }

        /// <summary>
        /// Shows data fields editor of selected embedded file.
        /// </summary>
        private void selectedFileDataFieldsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (attachmentViewer.SelectedFiles.Length == 1)
            {
                PdfEmbeddedFileSpecification selectedFile = attachmentViewer.SelectedFiles[0];
                if (selectedFile.DataFields == null)
                {
                    if (MessageBox.Show(string.Format("File '{0}' does not have data fields. Do you want to create data fields?", Path.GetFileName(selectedFile.Filename)), "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        return;
                    selectedFile.DataFields = new PdfAttachmentDataFieldCollection(_document);
                }
                PdfNamedDictionaryItemSet<PdfAttachmentDataField> itemSet =
                    new PdfNamedDictionaryItemSet<PdfAttachmentDataField>(selectedFile.DataFields, AddNewAttachmentDataField);
                ItemSetEditorWindow editorForm = new ItemSetEditorWindow(itemSet);
                editorForm.Owner = this;
                editorForm.Title = "Embedded File Data Fields Editor";
                editorForm.ShowDialog();

                if (string.IsNullOrEmpty(sortFieldNameComboBox.Text))
                    attachmentViewer.UpdateFileView(selectedFile);
                else
                    attachmentViewer.UpdateCurrentFolder();
            }
            else if (attachmentViewer.SelectedFolders.Length == 1)
            {
                PdfAttachmentFolder selectedFolder = attachmentViewer.SelectedFolders[0];
                if (selectedFolder.DataFields == null)
                {
                    if (MessageBox.Show(string.Format("Folder '{0}' does not have data fields. Do you want to create data fields?", selectedFolder.Name), "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        return;
                    selectedFolder.DataFields = new PdfAttachmentDataFieldCollection(_document);
                }
                PdfNamedDictionaryItemSet<PdfAttachmentDataField> itemSet =
                    new PdfNamedDictionaryItemSet<PdfAttachmentDataField>(selectedFolder.DataFields, AddNewAttachmentDataField);
                ItemSetEditorWindow editorForm = new ItemSetEditorWindow(itemSet);
                editorForm.Owner = this;
                editorForm.Title = "Attachment Folder Data Fields Editor";
                editorForm.ShowDialog();

                if (string.IsNullOrEmpty(sortFieldNameComboBox.Text))
                    attachmentViewer.UpdateFolderView(selectedFolder);
                else
                    attachmentViewer.UpdateCurrentFolder();
            }
        }

        #endregion


        #region Attachment viewer

        /// <summary>
        /// Executes action when item was activated.
        /// </summary>
        private void attachmentViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (attachmentViewer.SelectedItems == null)
                return;
            if (attachmentViewer.SelectedItems.Count == 0)
                return;

            PdfAttachmentFolder[] selectedFolders = attachmentViewer.SelectedFolders;
            // if folder activated
            if (selectedFolders.Length > 0)
            {
                // change current folder
                attachmentViewer.CurrentFolder = selectedFolders[0];
                return;
            }

            PdfEmbeddedFileSpecification[] files = attachmentViewer.SelectedFiles;
            // if file activated
            if (files.Length > 0)
            {
                // open file
                try
                {
                    PdfEmbeddedFileSpecification fileSpec = files[0];
                    if (fileSpec.EmbeddedFile != null)
                    {
                        MessageBoxResult result = MessageBox.Show(string.Format("Open file '{0}' using the default program, or save file?\n\nPress 'Yes' to open file using the default program.\nPress 'No' to save file to disk.", fileSpec.Filename), "", MessageBoxButton.YesNoCancel);
                        if (result == MessageBoxResult.Cancel)
                            return;
                        if (result == MessageBoxResult.Yes)
                        {
                            string path = String.Empty;
                            string[] args = Environment.GetCommandLineArgs();
                            if (args != null && args.Length > 0)
                                path = System.IO.Path.GetDirectoryName(args[0]);
                            string filename = System.IO.Path.Combine(path, fileSpec.Filename);
                            fileSpec.EmbeddedFile.Save(filename);

                            ProcessStartInfo processInfo = new ProcessStartInfo(filename);
                            processInfo.UseShellExecute = true;
                            Process process = Process.Start(processInfo);
                            if (process != null)
                            {
                                try
                                {
                                    process.WaitForExit();
                                    File.Delete(filename);
                                }
                                catch
                                {
                                }
                            }
                        }
                        else
                        {
                            SaveFileDialog saveDialog = new SaveFileDialog();
                            saveDialog.FileName = fileSpec.Filename;
                            if (saveDialog.ShowDialog().Value)
                                fileSpec.EmbeddedFile.Save(saveDialog.FileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }
        }

        /// <summary>
        /// Updates UI when current folder was changed.
        /// </summary>
        private void attachmentViewer_CurrentFolderChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Shows properties of selected item in property grid.
        /// </summary>
        private void attachmentViewer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (attachmentViewer.SelectedItems == null)
                return;

            if (attachmentViewer.SelectedItems.Count == 1)
            {
                PdfEmbeddedFileSpecification[] files = attachmentViewer.SelectedFiles;
                if (files.Length > 0)
                    itemsPropertyGrid.SelectedObject = files[0];
                else
                {
                    PdfAttachmentFolder[] folders = attachmentViewer.SelectedFolders;
                    if (folders.Length > 0)
                        itemsPropertyGrid.SelectedObject = folders[0];
                }
            }
            else
            {
                itemsPropertyGrid.SelectedObject = null;
            }
            UpdateUI();
        }

        #endregion


        #region Items property grid

        /// <summary>
        /// Updates view of selected item after item property was changed.
        /// </summary>
        private void itemsPropertyGrid_PropertyValueChanged(object sender, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            if (itemsPropertyGrid.SelectedObject != null)
            {
                if (itemsPropertyGrid.SelectedObject is PdfAttachmentFolder)
                    attachmentViewer.UpdateFolderView((PdfAttachmentFolder)itemsPropertyGrid.SelectedObject);
                else
                    attachmentViewer.UpdateFileView((PdfEmbeddedFileSpecification)itemsPropertyGrid.SelectedObject);
            }
        }

        #endregion

        /// <summary>
        /// Updates the User Interface.
        /// </summary>
        private void UpdateUI()
        {
            _updatingUI = true;
            try
            {
                if (_document != null)
                {
                    bool canLevelUp = attachmentViewer.CurrentFolder != null && attachmentViewer.CurrentFolder.Parent != null;
                    bool canMoveToRoot = attachmentViewer.RootFolder != attachmentViewer.CurrentFolder;
                    levelUpButton.IsEnabled = canLevelUp;
                    levelUpMenuItem.IsEnabled = canLevelUp;
                    moveToRootMenuItem.IsEnabled = canMoveToRoot;
                    moveToRootButton.IsEnabled = canMoveToRoot;

                    detailViewModeButton.IsEnabled = attachmentViewer.ViewMode != ViewMode.Details;
                    iconViewModeButton.IsEnabled = attachmentViewer.ViewMode != ViewMode.LargeIcon;

                    bool itemsSelected = attachmentViewer.SelectedItems.Count > 0;
                    saveSelectedFilesToMenuItem.IsEnabled = itemsSelected;
                    saveSelectedItemsButton.IsEnabled = itemsSelected;
                    deleteSelectedButton.IsEnabled = itemsSelected;
                    deleteSelectedMenuItem.IsEnabled = itemsSelected;

                    saveSelectedFilesToMenuItem.IsEnabled = attachmentViewer.SelectedItems.Count > 0;

                    selectedFileDataFieldsMenuItem.IsEnabled = attachmentViewer.SelectedItems.Count == 1;

                    resetColorsMenuItem.IsEnabled = _document.Attachments.Colors != null;

                    removeSortInformationMenuItem.IsEnabled = _document.Attachments.Sort != null;

                    generateThumbnailsForlSelectedFilesMenuItem.IsEnabled = attachmentViewer.SelectedFiles.Length > 0;
                    setThumbnailForSelectedItemsMenuItem.IsEnabled = attachmentViewer.SelectedItems.Count > 0;

                    sortFieldNameComboBox.Items.Clear();
                    if (_document.Attachments.Schema != null)
                    {
                        foreach (string fieldName in _document.Attachments.Schema.Keys)
                            sortFieldNameComboBox.Items.Add(fieldName);
                    }
                    sortFieldNameComboBox.SelectedValue = "";
                    ascendingOrderMenuItem.IsChecked = true;
                    if (_document.Attachments.Sort != null)
                    {
                        string[] sortFieldNames = _document.Attachments.Sort.FieldNames;
                        if (sortFieldNames != null && sortFieldNames.Length > 0)
                            sortFieldNameComboBox.Text = sortFieldNames[0];
                        bool[] ascendingOrders = _document.Attachments.Sort.AscendingOrders;
                        if (ascendingOrders != null && ascendingOrders.Length > 0)
                            ascendingOrderMenuItem.IsChecked = ascendingOrders[0];
                    }
                }
            }
            finally
            {
                _updatingUI = false;
            }
        }

        /// <summary>
        /// Resizes columns automatically.
        /// </summary>
        private void ColumnsAutoResize()
        {
            if (attachmentViewer.ViewMode == ViewMode.Details)
            {
                GridView gridView = attachmentViewer.View as GridView;
                for (int i = 0; i < gridView.Columns.Count; i++)
                {
                    GridViewColumn columnHeader = gridView.Columns[i];
                    if (columnHeader.Width < 100)
                        columnHeader.Width = 100;
                }
            }
        }

        /// <summary>
        /// Sets the thumbnail for specified folder (recursive).
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="thumbnailResource">The thumbnail resource.</param>
        private void SetThumbnailRecursive(PdfAttachmentFolder folder, PdfImageResource thumbnailResource)
        {
            folder.Thumbnail = thumbnailResource;
            PdfAttachmentFolder[] subFolders = folder.Folders;
            if (subFolders != null)
            {
                foreach (PdfAttachmentFolder subFolder in subFolders)
                    SetThumbnailRecursive(subFolder, thumbnailResource);
            }
        }

        /// <summary>
        /// Creates the thumbnail of image resource.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>Thumbnail of image resource.</returns>
        private PdfImageResource CreateThumbnailImageResource(VintasoftImage image)
        {
            using (VintasoftImage thumbnail = image.Thumbnail.GetThumbnailImage(100, 100))
            {
                PdfCompressionSettings compressionSettings = new PdfCompressionSettings();
                compressionSettings.JpegQuality = 90;
                return new PdfImageResource(_document, thumbnail, PdfCompression.Jpeg, compressionSettings);
            }
        }

        /// <summary>
        /// Generates the thumbnails for specified embedded files.
        /// </summary>
        /// <param name="fileSpecifications">The file specifications.</param>
        private void GenerateThumbnails(params PdfEmbeddedFileSpecification[] fileSpecifications)
        {
            if (fileSpecifications != null)
            {
                foreach (PdfEmbeddedFileSpecification fileSpecification in fileSpecifications)
                {
                    _actionController.NextSubAction(fileSpecification.Filename);
                    try
                    {
                        if (fileSpecification.EmbeddedFile != null)
                        {
                            Codec codec = AvailableCodecs.GetCodecByExtension(Path.GetExtension(fileSpecification.Filename));
                            if (codec != null && codec.CanCreateDecoder)
                            {
                                using (Stream imageDataStream = fileSpecification.EmbeddedFile.GetAsStream())
                                {
                                    try
                                    {
                                        using (VintasoftImage image = new VintasoftImage(imageDataStream, false))
                                            fileSpecification.Thumbnail = CreateThumbnailImageResource(image);
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DemosTools.ShowErrorMessage(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the new attachments shema field.
        /// </summary>
        /// <param name="dictionary">The dictionary to which attachments schema field must be added.</param>
        /// <returns>Name of shema field.</returns>
        private string AddNewAttachmentsSchemaField(PdfNamedDictionary<PdfAttachmentCollectionSchemaField> dictionary)
        {
            string name = null;
            PdfAttachmentCollectionSchemaField field = PdfAttachmentSchemaFieldFactoryWindow.CreateSchemaField(_document, out name, this);
            if (field != null)
            {
                field.Order = _document.Attachments.Schema.GetMaxOrder() + 1;
                try
                {
                    dictionary.Add(name, field);
                    return name;
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Adds the new attachment data field.
        /// </summary>
        /// <param name="dictionary">The dictionary to which attachment data field must be added.</param>
        /// <returns>Field name.</returns>
        private string AddNewAttachmentDataField(PdfNamedDictionary<PdfAttachmentDataField> dictionary)
        {
            string name = null;
            PdfAttachmentDataField field = PdfAttachmentDataFieldFactoryWindow.CreateDataField(_document, out name, this);
            if (field != null)
            {
                try
                {
                    dictionary.Add(name, field);
                    return name;
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }
            return null;
        }

        #region Hot keys

        /// <summary>
        /// Handles the CanExecute event of CreateNewFolderCommandBinding object.
        /// </summary>
        private void createNewFolderCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = createNewFolderMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of SaveSelectedFilesToCommandBinding object.
        /// </summary>
        private void saveSelectedFilesToCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = saveSelectedFilesToMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of CloseCommandBinding object.
        /// </summary>
        private void closeCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = closeMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of LevelUpCommandBinding object.
        /// </summary>
        private void levelUpCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = levelUpMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of MoveToRootCommandBinding object.
        /// </summary>
        private void moveToRootCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = moveToRootMenuItem.IsEnabled;
        }

        /// <summary>
        /// Handles the CanExecute event of DeleteSelectedCommandBinding object.
        /// </summary>
        private void deleteSelectedCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = deleteSelectedMenuItem.IsEnabled && !windowsFormsHost1.IsFocused;
            e.ContinueRouting = !e.CanExecute;
        }

        /// <summary>
        /// Handles the CanExecute event of SelectAllCommandBinding object.
        /// </summary>
        private void selectAllCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = selectAllMenuItem.IsEnabled;
        }

        #endregion

        #endregion

    }
}
