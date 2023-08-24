using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.Patterns;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A tree view that allows to view resources of PDF document.
    /// </summary>
    public class PdfDocumentResourceViewer : TreeView
    {

        #region Nested class

        /// <summary>
        /// A tree view item, which is necessary for correct sorting of tree items.
        /// </summary>
        private class VintasoftTreeViewItem : TreeViewItem
        {

            int _order = -1;
            /// <summary>
            /// Gets the order.
            /// </summary>
            public int Order
            {
                get
                {
                    if (_order == -1)
                    {
                        if (this.Tag is PdfPage)
                        {
                            PdfPage page = (PdfPage)this.Tag;
                            _order = page.Document.Pages.IndexOf(page);
                        }
                        else if (this.Tag is PdfResource)
                        {
                            PdfResource resource = (PdfResource)this.Tag;
                            _order = resource.ObjectNumber;
                        }
                    }

                    return _order;
                }
            }



            /// <summary>
            /// Property is changed.
            /// </summary>
            protected override void OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs e)
            {
                base.OnPropertyChanged(e);

                if (e.Property == TreeViewItem.TagProperty)
                    _order = -1;
            }

        }

        #endregion



        #region Constants

        /// <summary>
        /// Name of the private node.
        /// </summary>
        const string PRIVATE_NODE_NAME = "PRIVATE";

        /// <summary>
        /// Name of the resource node.
        /// </summary>
        const string RESOURCES_NODE_NAME = "Resources";

        /// <summary>
        /// Name of the annotation node.
        /// </summary>
        const string ANNOTATIONS_NODE_NAME = "Annotations";

        #endregion



        #region Fields

        /// <summary>
        /// Indicates that the resource can be removed from additional resources.
        /// </summary>
        bool _removeAdditionalResources = true;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDocumentResourceViewer"/> class.
        /// </summary>
        public PdfDocumentResourceViewer()
        {
            Items.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending));
        }

        #endregion



        #region Properties

        #region PUBLIC

        bool _showFormResources = true;
        /// <summary>
        /// Gets or sets a value indicating whether the resource viewer should
        /// show the image resources of PDF document.
        /// </summary>
        [Browsable(true)]
        [Description("Indicates that the resource viewer should show the image resources of PDF document")]
        [DefaultValue(true)]
        public bool ShowFormResources
        {
            get
            {
                return _showFormResources;
            }
            set
            {
                if (_showFormResources != value)
                {
                    _showFormResources = value;

                    UpdateTreeView();
                }
            }
        }

        bool _showImageResources = true;
        /// <summary>
        /// Gets or sets a value indicating whether the resource viewer should
        /// show the form resources of PDF document.
        /// </summary>
        [Browsable(true)]
        [Description("Indicates that the resource viewer should show the form resources of PDF document.")]
        [DefaultValue(true)]
        public bool ShowImageResources
        {
            get
            {
                return _showImageResources;
            }
            set
            {
                if (_showImageResources != value)
                {
                    _showImageResources = value;

                    UpdateTreeView();
                }
            }
        }

        object _selectedObject = null;
        /// <summary>
        /// Gets or sets the object, which should be viewed.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.<br />
        /// <br />
        /// Supported value types: <see cref="PdfDocument"/>, <see cref="PdfPage"/>,
        /// <see cref="PdfImageResource"/>, <see cref="PdfFormXObjectResource"/>.
        /// </value>
        [Browsable(false)]
        public object SelectedObject
        {
            get
            {
                return _selectedObject;
            }
            set
            {
                if (_selectedObject != value)
                {
                    _selectedObject = value;

                    UpdateTreeView();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected PDF resource.
        /// </summary>
        [Browsable(false)]
        public PdfResource SelectedResource
        {
            get
            {
                if (SelectedNode == null)
                    return null;

                if (SelectedNode.Tag is PdfResource)
                    return (PdfResource)SelectedNode.Tag;
                else
                    return null;
            }
            set
            {
                if (SelectedResource != value)
                    SelectedNode = FindNode(Items, value);
            }
        }

        /// <summary>
        /// Gets the selected PDF page.
        /// </summary>
        [Browsable(false)]
        public PdfPage SelectedPage
        {
            get
            {
                if (SelectedNode == null)
                    return null;

                if (SelectedNode.Tag is PdfPage)
                    return (PdfPage)SelectedNode.Tag;
                else
                    return null;
            }
        }

        PdfResourceTreeViewType _treeType = PdfResourceTreeViewType.Hierarchical;
        /// <summary>
        /// Gets or sets a value that determines how the PDF resource tree must be shown.
        /// </summary>
        /// <value>
        /// Default value is <b>PdfResourceTreeType.Hierarchical</b>.
        /// </value>
        [Browsable(true)]
        [Description("A value that determines how the PDF resource tree must be shown.")]
        [DefaultValue(PdfResourceTreeViewType.Hierarchical)]
        public PdfResourceTreeViewType TreeType
        {
            get
            {
                return _treeType;
            }
            set
            {
                if (_treeType != value)
                {
                    _treeType = value;

                    UpdateTreeView();
                }
            }
        }

        List<PdfResource> _additionalResources = null;
        /// <summary>
        /// Gets or sets the additional resources.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        [Browsable(false)]
        public List<PdfResource> AdditionalResources
        {
            get
            {
                return _additionalResources;
            }
            set
            {
                if (_additionalResources != value)
                {
                    _additionalResources = value;

                    UpdateTreeView();
                }
            }
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Gets the selected node.
        /// </summary>
        /// <value>
        /// The selected node.
        /// </value>
        private TreeViewItem SelectedNode
        {
            get
            {
                return SelectedItem as TreeViewItem;
            }
            set
            {
                if (value != null)
                    value.IsSelected = true;
            }
        }

        #endregion

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Updates the tree view.
        /// </summary>
        public void UpdateTreeView()
        {
            BeginInit();
            try
            {
                if (TreeType == PdfResourceTreeViewType.Linear)
                    VirtualizingStackPanel.SetIsVirtualizing(this, true);

                Items.Clear();

                Add(Items, _selectedObject);

                _removeAdditionalResources = false;
                Add(Items, _additionalResources);
                _removeAdditionalResources = true;
            }
            finally
            {
                EndInit();
            }
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Adds the object to the specified collection.
        /// </summary>
        /// <param name="rootCollection">The root collection.</param>
        /// <param name="obj">The object.</param>
        private void Add(ItemCollection rootCollection, object obj)
        {
            // if root tree node contains the current object
            if (TreeType == PdfResourceTreeViewType.Linear &&
                FindNode(rootCollection, obj as PdfResource) != null)
                return;

            if (obj is PdfDocument)
            {
                PdfDocument document = obj as PdfDocument;
                int pageIndex = 1;
                foreach (PdfPage page in document.Pages)
                {
                    ItemCollection pageNodeCollection = rootCollection;
                    TreeViewItem pageNode = null;
                    if (TreeType == PdfResourceTreeViewType.Hierarchical)
                    {
                        string nodeName = string.Format("Page {0}", pageIndex);
                        pageNode = AddTreeViewItem(rootCollection, nodeName, page);
                        pageNodeCollection = pageNode.Items;
                        pageIndex++;
                    }

                    AddPageResources(pageNodeCollection, page);

                    if (pageNode != null &&
                        pageNode.Items.Count == 0 &&
                        ShowImageResources &&
                        !ShowFormResources)
                    {
                        rootCollection.Remove(pageNode);
                    }
                }
            }
            else if (obj is PdfPage)
            {
                AddPageResources(rootCollection, (PdfPage)obj);
            }
            else if (obj is PdfImageResource)
            {
                PdfImageResource imageResource = (PdfImageResource)obj;
                if (ShowImageResources)
                {
                    TreeViewItem node = AddResourceTreeNode(rootCollection, imageResource);

                    if (imageResource.SoftMask != null)
                        AddPrivateTreeNode(node, imageResource.SoftMask);

                    if (imageResource.StencilMask != null)
                        AddPrivateTreeNode(node, imageResource.StencilMask);
                }
            }
            else if (obj is PdfFormXObjectResource)
            {
                PdfFormXObjectResource formXObjectResource = (PdfFormXObjectResource)obj;

                if (ShowFormResources)
                {
                    TreeViewItem node = AddResourceTreeNode(rootCollection, formXObjectResource);

                    AddPrivateTreeNode(node, formXObjectResource.Resources);
                }
                else
                {
                    Add(rootCollection, formXObjectResource.Resources);
                }
            }
            else if (obj is PdfAnnotation)
            {
                PdfAnnotation annotationResource = (PdfAnnotation)obj;

                if (annotationResource.Appearances != null)
                {
                    PdfFormXObjectResource[] appearances = annotationResource.Appearances.GetAllAppearances();
                    Add(rootCollection, appearances);
                }
            }
            else if (obj is PdfResources)
            {
                PdfResources resources = (PdfResources)obj;
                if (resources.XObjectResources != null &&
                    resources.XObjectResources.Count > 0)
                {
                    Add(rootCollection, resources.XObjectResources.Values);
                }

                if (resources.Patterns != null &&
                    resources.Patterns.Count > 0)
                {
                    Add(rootCollection, resources.Patterns.Values);
                }
            }
            else if (obj is TilingPattern)
            {
                TilingPattern tilingPattern = (TilingPattern)obj;
                Add(rootCollection, tilingPattern.Resources);
            }
            else if (obj is IEnumerable)
            {
                IEnumerable array = (IEnumerable)obj;
                foreach (object item in array)
                    Add(rootCollection, item);
            }
        }

        /// <summary>
        /// Adds the private tree node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="resource">The resource.</param>
        private void AddPrivateTreeNode(TreeViewItem node, object resource)
        {
            if (TreeType == PdfResourceTreeViewType.Hierarchical)
            {
                TreeViewItem privateNode = FindNode(node.Items, PRIVATE_NODE_NAME);
                if (privateNode == null)
                {
                    privateNode = AddTreeViewItem(node.Items, PRIVATE_NODE_NAME, null);
                }

                if (privateNode.Tag == null)
                {
                    privateNode.Tag = resource;
                }
                else
                {
                    object tag = privateNode.Tag;
                    if (tag is Array)
                    {
                        Array array = (Array)tag;

                        object[] newArray = new object[array.Length + 1];
                        Array.Copy(array, newArray, array.Length);
                        newArray[newArray.Length - 1] = resource;
                        privateNode.Tag = newArray;
                    }
                    else
                    {
                        privateNode.Tag = new object[] { tag, resource };
                    }
                }
            }
            else if (TreeType == PdfResourceTreeViewType.Linear)
            {
                ItemCollection nodeCollection = Items;
                if (node.Parent is TreeViewItem)
                    nodeCollection = ((TreeViewItem)node.Parent).Items;
                Add(nodeCollection, resource);
            }
            else
                throw new InvalidOperationException();
        }

        /// <summary>
        /// Adds the resource tree node.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="resource">The resource.</param>
        private TreeViewItem AddResourceTreeNode(ItemCollection collection, PdfResource resource)
        {
            // get resource name
            string nodeName = GetResourceName(resource);
            // find resource
            TreeViewItem node = FindNode(collection, nodeName);
            // if resource is not found
            if (node == null)
            {
                // add tree node to collection
                node = AddTreeViewItem(collection, nodeName, resource);
            }

            if (_removeAdditionalResources)
                PdfResourcesViewerWindow.RemoveAdditionalResource(resource);

            return node;
        }

        /// <summary>
        /// Adds all resources of PDF page to the tree node collection.
        /// </summary>
        /// <param name="rootCollection">The root tree node collection.</param>
        /// <param name="page">A PDF page.</param>
        private void AddPageResources(ItemCollection rootCollection, PdfPage page)
        {
            ItemCollection resourcesNodeCollection = rootCollection;
            TreeViewItem resourcesNode = null;
            if (TreeType == PdfResourceTreeViewType.Hierarchical)
            {
                string resourcesNodeName = RESOURCES_NODE_NAME;
                resourcesNode = FindNode(rootCollection, resourcesNodeName);
                if (resourcesNode == null)
                    resourcesNode = AddTreeViewItem(rootCollection, resourcesNodeName, null);
                resourcesNodeCollection = resourcesNode.Items;
            }
            Add(resourcesNodeCollection, page.Resources);
            if (resourcesNodeCollection.Count == 0 && resourcesNode != null)
                rootCollection.Remove(resourcesNode);

            PdfAnnotationList annotationList = page.Annotations;
            if (ShowFormResources && annotationList != null && annotationList.Count > 0)
            {
                if (TreeType == PdfResourceTreeViewType.Hierarchical)
                {
                    string annotationsNodeName = ANNOTATIONS_NODE_NAME;
                    TreeViewItem annotationsNode = FindNode(rootCollection, annotationsNodeName);
                    if (annotationsNode == null)
                        annotationsNode = AddTreeViewItem(rootCollection, annotationsNodeName, null);
                    ItemCollection annotationsNodeCollection = annotationsNode.Items;

                    foreach (PdfAnnotation annotation in annotationList)
                    {
                        if (annotation.Appearances != null)
                        {
                            PdfFormXObjectResource[] appearances = annotation.Appearances.GetAllAppearances();
                            if (appearances.Length != 0)
                            {
                                string annotationName = PdfDemosTools.GetAnnotationName(annotation);
                                string nodeName = GetResourceName(annotation.GetType(), annotation.ObjectNumber);
                                if (!string.IsNullOrEmpty(annotationName))
                                    nodeName += string.Format(" {0}", annotationName);
                                TreeViewItem node = AddTreeViewItem(annotationsNodeCollection, nodeName, annotation);

                                AddPrivateTreeNode(node, appearances);
                            }
                        }
                    }

                    if (annotationsNodeCollection.Count == 0)
                        rootCollection.Remove(annotationsNode);
                }
                else
                {
                    foreach (PdfAnnotation annotation in annotationList)
                        Add(rootCollection, annotation);
                }
            }
        }

        /// <summary>
        /// Returns the resource name.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns>The resource name.</returns>
        private string GetResourceName(PdfResource resource)
        {
            return GetResourceName(resource.GetType(), resource.ObjectNumber);
        }

        /// <summary>
        /// Returns the resource name.
        /// </summary>
        /// <param name="type">The type of resource.</param>
        /// <param name="objectNumber">The number of resource.</param>
        private string GetResourceName(Type type, int objectNumber)
        {
            string resourceName;
            if (objectNumber > 0)
            {
                resourceName = string.Format("{0}: {1}",
                   type.Name, objectNumber.ToString());
            }
            else
            {
                resourceName = string.Format("{0}: Inline", type.Name);
            }

            if (resourceName.StartsWith("Pdf", StringComparison.InvariantCultureIgnoreCase))
                resourceName = resourceName.Substring(3);

            return resourceName;
        }

        /// <summary>
        /// Finds the node with the specified text in a tree node collection.
        /// </summary>
        /// <param name="nodes">A tree node collection, where node must be searched.</param>
        /// <param name="text">A text, which nust present in a node.</param>
        private TreeViewItem FindNode(ItemCollection nodes, string text)
        {
            foreach (object node in nodes)
            {
                TreeViewItem item = node as TreeViewItem;
                if (item != null && item.Header.ToString() == text)
                    return item;
            }
            return null;
        }

        /// <summary>
        /// Finds the node, which contains the resource.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="resource">The resource.</param>
        private TreeViewItem FindNode(ItemCollection items, PdfResource resource)
        {
            if (resource == null)
                return null;

            foreach (object item in items)
            {
                TreeViewItem node = item as TreeViewItem;

                if (node == null)
                    continue;

                if (node.Tag == resource)
                    return node;

                TreeViewItem result = FindNode(node.Items, resource);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="header">The header of item.</param>
        /// <param name="tag">The tag of item.</param>
        private TreeViewItem AddTreeViewItem(ItemCollection collection, string header, object tag)
        {
            TreeViewItem item = new VintasoftTreeViewItem();
            item.Header = header;
            item.Tag = tag;

            collection.Add(item);

            if (header == PRIVATE_NODE_NAME)
            {
                TreeViewItem parentItem = (TreeViewItem)item.Parent;
                parentItem.Expanded += new System.Windows.RoutedEventHandler(item_Expanded);
            }
            return item;
        }

        /// <summary>
        /// Builds new level of tree before node expands.
        /// </summary>
        private void item_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            TreeViewItem root = (TreeViewItem)sender;

            if (root.Items.Count == 1)
            {
                TreeViewItem item = root.Items[0] as TreeViewItem;

                if (item != null && item.Header.ToString() == PRIVATE_NODE_NAME)
                {
                    object tag = item.Tag;
                    root.Items.Clear();

                    Add(root.Items, tag);
                }
            }
            root.Expanded -= item_Expanded;
        }

        #endregion

        #endregion

    }
}
