using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;

using WpfDemosCommonCode.Imaging;
using WpfDemosCommonCode.Imaging.Codecs;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A form that allows to display PDF resources and select PDF resource.
    /// </summary>
    public partial class PdfResourcesViewerWindow : Window
    {

        #region Fields

        /// <summary>
        /// The source PDF document.
        /// </summary>
        PdfDocument _sourceDocument = null;

        /// <summary>
        /// Dictionary: PDF document => list of PDF resources, which are opened in this form.
        /// </summary>
        static Dictionary<PdfDocument, List<PdfResource>> _documentToNewResources =
            new Dictionary<PdfDocument, List<PdfResource>>();

        /// <summary>
        /// The ToolStripMenuItem, which is selected in "Preview" menu.
        /// </summary>
        MenuItem _previewSelectedMenuItem = null;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes the <see cref="PdfResourcesViewerWindow"/> class.
        /// </summary>
        static PdfResourcesViewerWindow()
        {
            PdfDocumentController.DocumentClosed +=
                new EventHandler<PdfDocumentEventArgs>(PdfDocumentController_DocumentClosed);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfResourcesViewerWindow"/> class.
        /// </summary>
        /// <param name="document">The source PDF document.</param>
        public PdfResourcesViewerWindow(PdfDocument document)
            : this(document, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfResourcesViewerWindow"/> class.
        /// </summary>
        /// <param name="document">The source PDF document.</param>
        /// <param name="canAddResources">Determines that
        /// the resources can be added to the source PDF document.</param>
        public PdfResourcesViewerWindow(PdfDocument document, bool canAddResources)
            : this(canAddResources, document)
        {
            if (document == null)
                throw new ArgumentNullException();

            DocumentResourceViewer.SelectedObject = document;
            UpdateViewMenu();

            if (_documentToNewResources.ContainsKey(document))
                DocumentResourceViewer.AdditionalResources = _documentToNewResources[document];

            SelectFirstItem();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfResourcesViewerWindow"/> class.
        /// </summary>
        /// <param name="treeNode">The tree node, which should be viewed.</param>
        public PdfResourcesViewerWindow(PdfTreeNodeBase treeNode)
            : this(false, GetDocument(treeNode, "treeNode"))
        {
            DocumentResourceViewer.SelectedObject = treeNode;
            UpdateViewMenu();
            SelectFirstItem();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfResourcesViewerWindow"/> class.
        /// </summary>
        /// <param name="document">A PDF document.</param>
        /// <param name="treeNodes">The PDF tree nodes, which should be viewed.</param>
        public PdfResourcesViewerWindow(
            PdfDocument document,
            IEnumerable<PdfTreeNodeBase> treeNodes)
            : this(false, document)
        {
            DocumentResourceViewer.SelectedObject = treeNodes;
            UpdateViewMenu();
            SelectFirstItem();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfResourcesViewerWindow"/> class.
        /// </summary>
        /// <param name="document">A PDF document.</param>
        /// <param name="treeNodes">The PDF tree nodes, which should be viewed.</param>
        public PdfResourcesViewerWindow(
            PdfDocument document, 
            params PdfTreeNodeBase[] treeNodes)
            : this(document, (IEnumerable<PdfTreeNodeBase>)treeNodes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfResourcesViewerWindow"/> class.
        /// </summary>
        /// <param name="canAddResources">Determines that
        /// the resources can be added to the source PDF document.</param>
        /// <param name="document">The source PDF document.</param>
        private PdfResourcesViewerWindow(bool canAddResources, PdfDocument document)
        {
            InitializeComponent();

            if (document == null)
                throw new ArgumentException("sourceDocument");

            FileStream fileStream = document.SourceStream as FileStream;
            if (fileStream != null)
            {
                string fileName = Path.GetFileName(fileStream.Name);
                Title = string.Format("Resources of {0}", fileName);
            }

            CanAddResources = canAddResources;
            _sourceDocument = document;

            hierarchicalMenuItem.Tag = PdfResourceTreeViewType.Hierarchical;
            linearMenuItem.Tag = PdfResourceTreeViewType.Linear;

            switch (DocumentResourceViewer.TreeType)
            {
                case PdfResourceTreeViewType.Hierarchical:
                    _previewSelectedMenuItem = hierarchicalMenuItem;
                    break;

                case PdfResourceTreeViewType.Linear:
                    _previewSelectedMenuItem = linearMenuItem;
                    break;
            }
            _previewSelectedMenuItem.IsChecked = true;

            UpdateUI();
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the resource viewer should
        /// show the image resources of PDF document.
        /// </summary>
        /// <value>
        /// Default value is <b>true</b>.
        /// </value>
        public bool ShowImageResources
        {
            get
            {
                return DocumentResourceViewer.ShowImageResources;
            }
            set
            {
                DocumentResourceViewer.ShowImageResources = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the resource viewer should
        /// show the form resources of PDF document.
        /// </summary>
        /// <value>
        /// Default value is <b>true</b>.
        /// </value>
        public bool ShowFormResources
        {
            get
            {
                return DocumentResourceViewer.ShowFormResources;
            }
            set
            {
                DocumentResourceViewer.ShowFormResources = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the resources can be added
        /// to the source PDF document.
        /// </summary>
        /// <value>
        /// <b>true</b> - the resources can be added to the source PDF document;<br />
        /// <b>false</b> - the resources can NOT be added to the source PDF document.
        /// </value>
        public bool CanAddResources
        {
            get
            {
                return addFromDocumentMenuItem.Visibility == Visibility.Visible;
            }
            set
            {
                Visibility visibility;
                if (value)
                    visibility = Visibility.Visible;
                else
                    visibility = Visibility.Collapsed;

                addFromDocumentMenuItem.Visibility = visibility;
                addFromDocumentSeparator.Visibility = visibility;

                createResourceFromSelectedImageMenuItem.Visibility = visibility;
                createResourceFromImageMenuItem.Visibility = visibility;
                createResourceFromSelectedPageMenuItem.Visibility = visibility;
                createResourcesSeparator.Visibility = visibility;
            }
        }

        PdfResource _selectedResource = null;
        /// <summary>
        /// Gets the selected PDF resource.
        /// </summary>
        public PdfResource SelectedResource
        {
            get
            {
                return _selectedResource;
            }
        }

        bool _propertyValueChanged = false;
        /// <summary>
        /// Gets a value indicating whether the resource property is changed.
        /// </summary>
        /// <value>
        /// <b>true</b> - at least one property of one of displayed resources is changed;
        /// <b>false</b> - properties of all displayed resources are NOT changed.<br />
        /// Default value is <b>false</b>.
        /// </value>
        public bool PropertyValueChanged
        {
            get
            {
                return _propertyValueChanged;
            }
        }

        #endregion



        #region Methods

        #region PROTECTED

        /// <summary>
        /// Form is closing.
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            DocumentResourceViewer.SelectedItemChanged -= pdfDocumentResourceViewer_SelectedItemChanged;

            base.OnClosing(e);
        }

        #endregion


        #region INTERNAL

        /// <summary>
        /// Removes the additional resource from PDF document.
        /// </summary>
        /// <param name="resource">The additional PDF resource.</param>
        internal static void RemoveAdditionalResource(PdfResource resource)
        {
            PdfDocument document = resource.Document;
            List<PdfResource> resources = null;
            if (_documentToNewResources.TryGetValue(document, out resources))
                resources.Remove(resource);
        }

        #endregion


        #region PRIVATE, STATIC

        /// <summary>
        /// Returns PDF document associated with PDF tree node.
        /// </summary>
        /// <param name="pdfTreeNode">The PDF tree node.</param>
        /// <param name="exceptionMessage">The exception message.</param>
        private static PdfDocument GetDocument(PdfTreeNodeBase pdfTreeNode, string exceptionMessage)
        {
            if (pdfTreeNode == null)
                throw new ArgumentNullException(exceptionMessage);

            return pdfTreeNode.Document;
        }

        /// <summary>
        /// PDF document is closed.
        /// </summary>
        private static void PdfDocumentController_DocumentClosed(
            object sender,
            PdfDocumentEventArgs e)
        {
            _documentToNewResources.Remove(e.Document);
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            bool isResourceSelected = SelectedResource != null;
            bool isImageResourceSelected = SelectedResource is PdfImageResource;

            saveAsBinaryMenuItem.IsEnabled = isResourceSelected;
            saveAsImageMenuItem.IsEnabled = isImageResourceSelected;

            createResourceFromSelectedPageMenuItem.IsEnabled =
                DocumentResourceViewer.SelectedPage != null;
            createResourceFromSelectedImageMenuItem.IsEnabled =
                DocumentResourceViewer.SelectedResource is PdfImageResource;
        }

        /// <summary>
        /// Updates the view menu.
        /// </summary>
        private void UpdateViewMenu()
        {
            bool isEnabled = false;
            foreach (TreeViewItem item in DocumentResourceViewer.Items)
            {
                if (item.Items.Count > 0)
                {
                    isEnabled = true;
                    break;
                }
            }

            viewMenuItem.IsEnabled = isEnabled;
        }

        /// <summary>
        /// Selects the first resource in this form.
        /// </summary>
        private void SelectFirstItem()
        {
            if (DocumentResourceViewer.Items.Count > 0)
            {
                TreeViewItem item = DocumentResourceViewer.Items[0] as TreeViewItem;
                if (item != null)
                {
                    item.IsSelected = true;
                    DocumentResourceViewer.Focus();
                }
            }
        }

        /// <summary>
        /// Changes the selected PDF resource.
        /// </summary>
        private void pdfDocumentResourceViewer_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            PdfResource resource = DocumentResourceViewer.SelectedResource;

            _selectedResource = resource;
            pdfResourceViewerControl.Resource = resource;
            propertyGrid.SelectedObject = resource;

            UpdateUI();
        }

        /// <summary>
        /// Button "Save As Binary..." is clicked.
        /// </summary>
        private void saveAsBinaryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "bin";
            saveFileDialog.Filter = "Binary Files (*.bin)|*.bin";

            if (saveFileDialog.ShowDialog() == true)
            {
                byte[] bytes = SelectedResource.GetBytes();
                File.WriteAllBytes(saveFileDialog.FileName, bytes);
            }
        }

        /// <summary>
        /// Button "Save As Image..." is clicked.
        /// </summary>
        private void saveAsImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfImageResource resourceStream = (PdfImageResource)SelectedResource;

            using (VintasoftImage image = resourceStream.GetImage())
            {
                SaveImageFileWindow.SaveImageToFile(image, PluginsEncoderFactory.Default);
            }
        }

        /// <summary>
        /// Button "Default Compression Params..." is clicked.
        /// </summary>
        private void defaultCompressionParamsButton_Click(object sender, RoutedEventArgs e)
        {
            PropertyGridWindow window =
                new PropertyGridWindow(PdfCompressionSettings.DefaultSettings, "Compression Default Params");
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = this;
            window.ShowDialog();
        }

        /// <summary>
        /// Loads resources from the specified PDF document and
        /// adds selected PDF resources to this form.
        /// </summary>
        private void addFromDocumentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Pdf files|*.pdf";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (Stream stream = File.Open(openFileDialog.FileName, FileMode.Open))
                    {
                        PdfDocument document = PdfDocumentController.OpenDocument(stream);
                        try
                        {
                            PdfResourcesViewerWindow dialog = new PdfResourcesViewerWindow(document);
                            dialog.Owner = this;
                            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                            dialog.ShowImageResources = ShowImageResources;
                            dialog.ShowFormResources = ShowFormResources;
                            dialog.CanAddResources = true;

                            if (dialog.ShowDialog() == true)
                            {
                                CopyAndAddResourceToSourceDocument(dialog.SelectedResource);
                            }
                        }
                        finally
                        {
                            PdfDocumentController.CloseDocument(document);
                            document.Dispose();
                        }
                    }
                }
                catch (Exception exc)
                {
                    DemosTools.ShowErrorMessage(exc);
                }
            }
        }

        /// <summary>
        /// Creates a PDF resource from the selected PDF page.
        /// </summary>
        private void createResourceFromSelectedPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfPage page = DocumentResourceViewer.SelectedPage;
            PdfFormXObjectResource form = new PdfFormXObjectResource(_sourceDocument, page);
            CopyAndAddResourceToSourceDocument(form);
        }

        /// <summary>
        /// Creates a PDF resource from the selected image-resource.
        /// </summary>
        private void createResourceFromSelectedImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PdfImageResource image = (PdfImageResource)DocumentResourceViewer.SelectedResource;
            PdfFormXObjectResource form = new PdfFormXObjectResource(_sourceDocument, image);
            CopyAndAddResourceToSourceDocument(form);
        }

        /// <summary>
        /// Creates a PDF resource from th selected image.
        /// </summary>
        private void createResourceFromImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openImageFileDialog = new OpenFileDialog();
            CodecsFileFilters.SetFilters(openImageFileDialog);
            if (openImageFileDialog.ShowDialog() == true)
            {
                try
                {
                    // select image from file
                    VintasoftImage selectedImage = SelectImageWindow.SelectImageFromFile(openImageFileDialog.FileName);

                    if (selectedImage != null)
                    {
                        PdfPage page =
                            PdfDocumentController.GetPageAssociatedWithImage(selectedImage);

                        PdfResource resource = null;
                        if (page != null)
                        {
                            resource = new PdfFormXObjectResource(_sourceDocument, page);
                        }
                        else
                        {
                            PdfImageResource imageResource = new PdfImageResource(_sourceDocument,
                                selectedImage, PdfCompression.Auto);

                            if (ShowFormResources && !ShowImageResources)
                                resource = new PdfFormXObjectResource(_sourceDocument, imageResource);
                            else
                                resource = imageResource;
                        }

                        CopyAndAddResourceToSourceDocument(resource);
                    }
                }
                catch (Exception exc)
                {
                    DemosTools.ShowErrorMessage(exc);
                }
            }
        }

        /// <summary>
        /// Copies and adds the resource to the source PDF document.
        /// </summary>
        /// <param name="resource">The PDF resource.</param>
        private void CopyAndAddResourceToSourceDocument(PdfResource resource)
        {
            if (resource == null)
                return;

            if (resource.Document != _sourceDocument)
            {
                if (resource is PdfFormXObjectResource)
                {
                    PdfFormXObjectResource formResource =
                        (PdfFormXObjectResource)resource;
                    resource = formResource.CreateCopy(_sourceDocument);
                }
                else if (resource is PdfImageResource)
                {
                    PdfImageResource imageResource =
                        (PdfImageResource)resource;
                    resource = imageResource.CreateCopy(_sourceDocument);
                }
            }

            List<PdfResource> resources = null;
            if (!_documentToNewResources.TryGetValue(resource.Document, out resources))
            {
                resources = new List<PdfResource>();
                _documentToNewResources.Add(resource.Document, resources);
            }
            resources.Add(resource);

            if (DocumentResourceViewer.AdditionalResources == resources)
                DocumentResourceViewer.UpdateTreeView();
            else
                DocumentResourceViewer.AdditionalResources = resources;

            DocumentResourceViewer.SelectedResource = resource;
        }

        /// <summary>
        /// Tree view type is changed.
        /// </summary>
        private void treeViewTypeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;

            menuItem.IsChecked ^= true;
            _previewSelectedMenuItem.IsChecked ^= true;
            _previewSelectedMenuItem = menuItem;

            DocumentResourceViewer.TreeType = (PdfResourceTreeViewType)menuItem.Tag;
            SelectFirstItem();
        }

        /// <summary>
        /// Resource property is changed.
        /// </summary>
        private void propertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            pdfResourceViewerControl.ReloadResource();
            _propertyValueChanged = true;
        }

        /// <summary>
        /// "Ok" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// "Cancel" button is clicked.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

        #endregion

    }
}
