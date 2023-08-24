using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.JavaScriptApi;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Wpf;

using WpfDemosCommonCode.Pdf.JavaScript;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A tree view that allows to view and edit bookmarks of PDF document.
    /// </summary>
    public class BookmarkTreeViewEditor : BookmarkTreeView
    {

        #region Constants

        const double DELTA_FOR_INSERTION_IN_THE_SAME_LEVEL = 0.3;

        #endregion



        #region Fields

        /// <summary>
        /// The context menu.
        /// </summary>
        private ContextMenu _contextMenu;

        /// <summary>
        /// The "Add..." menu item of context menu.
        /// </summary>
        private MenuItem _addMenuItem;

        /// <summary>
        /// The "Edit..." menu item of context menu.
        /// </summary>
        private MenuItem _editMenuItem;

        /// <summary>
        /// The "Delete" menu item of context menu.
        /// </summary>
        private MenuItem _deleteOutlineNodeMenuItem;

        /// <summary>
        /// The "Move Up" menu item of context menu.
        /// </summary>
        private MenuItem _moveUpMenuItem;

        /// <summary>
        /// The "Move Down" menu item of context menu.
        /// </summary>
        private MenuItem _moveDownMenuItem;

        /// <summary>
        /// The dragging item.
        /// </summary>
        TreeViewItem _draggingItem = null;

        /// <summary>
        /// Previously changed tree view item.
        /// </summary>
        TreeViewItem _prevChangedTreeViewItem = null;

        #endregion



        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BookmarkTreeViewEditor"/> class.
        /// </summary>
        public BookmarkTreeViewEditor()
        {
            InitializeComponent();

            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(BookmarkTreeView_DragEnter);
            this.DragLeave += new DragEventHandler(BookmarkTreeView_DragLeave);
            this.Drop += new DragEventHandler(BookmarkTreeView_Drop);
            this.DragOver += new DragEventHandler(BookmarkTreeView_DragOver);
        }

        #endregion



        #region Properties  

        bool _canEditBookmarks = false;
        /// <summary>
        /// Gets or sets a value indicating whether bookmarks can be edited.
        /// </summary>
        [DefaultValue(false)]
        public bool CanEditBookmarks
        {
            get
            {
                return _canEditBookmarks;
            }
            set
            {
                _canEditBookmarks = value;
                if (_canEditBookmarks)
                    ContextMenu = _contextMenu;
                else
                    ContextMenu = null;
            }
        }

        #endregion



        #region Designer

        private void InitializeComponent()
        {
            _addMenuItem = new MenuItem();
            _editMenuItem = new MenuItem();
            _deleteOutlineNodeMenuItem = new MenuItem();
            _moveUpMenuItem = new MenuItem();
            _moveDownMenuItem = new MenuItem();

            _contextMenu = new ContextMenu();
            _contextMenu.Name = "outlineMenuStrip";
            _contextMenu.Opened += new RoutedEventHandler(contextMenu_Opened);
            _contextMenu.Items.Add(_addMenuItem);
            _contextMenu.Items.Add(_editMenuItem);
            _contextMenu.Items.Add(_deleteOutlineNodeMenuItem);
            _contextMenu.Items.Add(_moveUpMenuItem);
            _contextMenu.Items.Add(_moveDownMenuItem);

            // addToolStripMenuItem
            _addMenuItem.Name = "addMenuItem";
            _addMenuItem.Header = "Add...";
            _addMenuItem.Click += new RoutedEventHandler(addMenuItem_Click);

            // editMenuItem
            _editMenuItem.Name = "editMenuItem";
            _editMenuItem.Header = "Edit...";
            _editMenuItem.Click += new RoutedEventHandler(editMenuItem_Click);

            // deleteOutlineNodeMenuItem
            _deleteOutlineNodeMenuItem.Name = "deleteOutlineNodeMenuItem";
            _deleteOutlineNodeMenuItem.Header = "Delete";
            _deleteOutlineNodeMenuItem.Click += new RoutedEventHandler(deleteOutlineNodeMenuItem_Click);

            // moveUpMenuItem
            _moveUpMenuItem.Name = "moveUpMenuItem";

            RoutedCommand moveUpCommand = new RoutedCommand();
            CommandBinding moveUpCommandBinding = new CommandBinding();
            moveUpCommandBinding.Executed += new ExecutedRoutedEventHandler(moveUpMenuItem_Click);
            moveUpCommandBinding.CanExecute += new CanExecuteRoutedEventHandler(CommandBinding_CanExecute);
            moveUpCommandBinding.Command = moveUpCommand;
            moveUpCommand.InputGestures.Add(new KeyGesture(Key.Up, ModifierKeys.Control));
            this.CommandBindings.Add(moveUpCommandBinding);
            _moveUpMenuItem.Header = "Move Up";
            _moveUpMenuItem.Click += new RoutedEventHandler(moveUpMenuItem_Click);

            // moveDownMenuItem
            _moveDownMenuItem.Name = "moveDownMenuItem";
            RoutedCommand moveDownCommand = new RoutedCommand();
            CommandBinding moveDownCommandBinding = new CommandBinding();
            moveDownCommandBinding.Executed += new ExecutedRoutedEventHandler(moveDownMenuItem_Click);
            moveDownCommandBinding.CanExecute += new CanExecuteRoutedEventHandler(CommandBinding_CanExecute);
            moveDownCommandBinding.Command = moveDownCommand;
            moveDownCommand.InputGestures.Add(new KeyGesture(Key.Down, ModifierKeys.Control));
            this.CommandBindings.Add(moveDownCommandBinding);
            _moveDownMenuItem.Header = "Move Down";
            _moveDownMenuItem.Click += new RoutedEventHandler(moveDownMenuItem_Click);

            this.ContextMenu = _contextMenu;
        }

        #endregion



        #region Methods      

        #region PUBLIC

        /// <summary>
        /// Adds the bookmark to the PDF document.
        /// </summary>
        /// <param name="pageIndex">The zero-based index of PDF page.</param>
        public void AddBookmark(int pageIndex)
        {
            // create new PDF bookmark
            PdfBookmark outline = new PdfBookmark(Document);
            // create edit bookmark dialog
            EditBookmarkNodeWindow dialog = new EditBookmarkNodeWindow(Viewer, pageIndex, outline, true);
            // if dialog result is true
            if (dialog.ShowDialog().Value)
            {
                PdfBookmarkCollection bookmarks = null;
                ItemCollection nodes = null;
                // if there is selected bookmark
                // and bookmark must be added not to the root
                if (SelectedItem != null && !dialog.AddToRoot)
                {
                    // get child bookmarks
                    TreeViewItem selectedItem = SelectedItem as TreeViewItem;
                    bookmarks = ((PdfBookmark)selectedItem.Tag).ChildBookmarks;
                    nodes = selectedItem.Items;
                }
                else
                {
                    // if document has bookmarks
                    if (Document.Bookmarks == null)
                    {
                        // create new bookmark collection
                        Document.Bookmarks = new PdfBookmarkCollection(Document);
                    }

                    // add new bookmark
                    bookmarks = Document.Bookmarks;
                    nodes = Items;
                }

                bookmarks.Add(dialog.Bookmark);
                if (SelectedItem != null)
                    (SelectedItem as TreeViewItem).IsExpanded = true;
                AddBookmark(nodes, outline).IsSelected = true;

                Document.DocumentViewMode = PdfDocumentViewMode.UseOutlines;
            }
        }

        /// <summary>
        /// Deletes the selected bookmark.
        /// </summary>
        public void DeleteSelectedBookmark()
        {
            if (CanEditBookmarks)
            {
                if (SelectedItem != null)
                {
                    PdfBookmark current = (PdfBookmark)(SelectedItem as TreeViewItem).Tag;
                    current.Remove();
                    SearchItemCollection(base.Items, SelectedItem as TreeViewItem).Remove(SelectedItem);
                }
            }
        }

        /// <summary>
        /// Edits the selected bookmark.
        /// </summary>
        public void EditSelectedBookmark()
        {
            if (CanEditBookmarks)
            {
                if (SelectedItem != null)
                {
                    TreeViewItem selectedItem = SelectedItem as TreeViewItem;
                    PdfBookmark outline = (PdfBookmark)selectedItem.Tag;

                    int pageIndex = 0;
                    if (Viewer != null && Viewer.FocusedIndex >= 0)
                        pageIndex = Viewer.FocusedIndex;
                    if (outline.Destination != null)
                        pageIndex = Document.Pages.IndexOf(outline.Destination.Page);

                    EditBookmarkNodeWindow dialog = new EditBookmarkNodeWindow(Viewer, pageIndex, outline, false);
                    if (dialog.ShowDialog().Value)
                    {
                        PdfBookmark bookmark = dialog.Bookmark;
                        selectedItem.Header = bookmark.Title;
                        selectedItem.Tag = bookmark;
                        selectedItem.FontFamily = base.FontFamily;
                        selectedItem.FontSize = base.FontSize;
                        selectedItem.FontStyle = FontStyles.Normal;
                        selectedItem.FontWeight = FontWeights.Normal;
                        if (bookmark.Flags != PdfBookmarkFlags.None)
                        {
                            if ((bookmark.Flags & PdfBookmarkFlags.Bold) != 0)
                                selectedItem.FontWeight = FontWeights.Bold;
                            if ((bookmark.Flags & PdfBookmarkFlags.Italic) != 0)
                                selectedItem.FontStyle = FontStyles.Italic;
                        }
                        selectedItem.Foreground = new SolidColorBrush(WpfObjectConverter.CreateWindowsColor(bookmark.Color));

                        selectedItem.IsSelected = false;
                        selectedItem.IsSelected = true;
                    }
                }
            }
        }

        #endregion


        #region PROTECTED

        /// <summary>
        /// Creates the <see cref="TreeViewItem" /> for <see cref="PdfBookmark" />.
        /// </summary>
        /// <param name="bookmark">The bookmark.</param>
        /// <returns>
        /// The <see cref="TreeViewItem" />.
        /// </returns>
        protected override TreeViewItem CreateTreeViewItem(PdfBookmark bookmark)
        {
            TreeViewItem treeViewItem = base.CreateTreeViewItem(bookmark);

            treeViewItem.ContextMenu = _contextMenu;
            treeViewItem.MouseMove += new MouseEventHandler(treeItem_MouseMove);

            return treeViewItem;
        }

        /// <summary>
        /// Sets the current page index to the bookmark destination page index.
        /// </summary>
        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            base.OnSelectedItemChanged(e);

            // if viewer is not empty
            if (Viewer != null)
            {
                // if PDF document is empty OR selected item is empty
                if (Document == null || SelectedItem == null)
                    return;

                // if PDF document is not empty
                // and PDF action executor is not empty
                if (Document != null && ActionExecutor != null)
                {
                    // get selected bookmark
                    PdfBookmark bookmark = (PdfBookmark)(SelectedItem as TreeViewItem).Tag;

                    // create JavaScript event
                    PdfJsEvent jsEvent = null;
                    if (PdfJavaScriptManager.JsApp != null)
                        jsEvent = PdfJsEvent.CreateUndefinedEventObject(PdfJavaScriptManager.JsApp.GetDoc(bookmark.Document));
                    PdfTriggerEventArgs args = new PdfTriggerEventArgs(null, jsEvent);

                    // if bookmark action is not empty
                    if (bookmark.Action != null)
                    {
                        // execute action
                        ActionExecutor.ExecuteActionSequence(bookmark.Action, args);
                    }
                    // if bookmark destination is not empty
                    else if (bookmark.Destination != null)
                    {
                        // go to the destination of the bookmark
                        ActionExecutor.ExecuteActionSequence(new PdfGotoAction(bookmark.Destination, false), args);
                    }
                }
            }
        }

        /// <summary>
        /// Edits selected bookmark.
        /// </summary>
        protected override void OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            EditSelectedBookmark();
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Moves bookmark at delta positions.
        /// </summary>
        private void MoveBookmark(TreeViewItem node, int delta)
        {
            ItemCollection parentNodes = SearchItemCollection(base.Items, node);
            PdfBookmark outline = (PdfBookmark)node.Tag;
            PdfBookmarkCollection parentOutlines = outline.ParentBookmarks;

            int currentIndex = parentOutlines.IndexOf(outline);
            int newIndex = currentIndex + delta;
            if (newIndex < 0)
                newIndex = 0;
            if (newIndex >= parentOutlines.Count)
                newIndex = parentOutlines.Count - 1;
            if (newIndex == currentIndex)
                return;

            parentOutlines.RemoveAt(currentIndex);
            parentOutlines.Insert(newIndex, outline);

            (SelectedItem as TreeViewItem).IsSelected = false;

            parentNodes.RemoveAt(currentIndex);
            parentNodes.Insert(newIndex, node);

            node.IsSelected = true;
        }

        /// <summary>
        /// Adds new bookmark.
        /// </summary>
        private void addMenuItem_Click(object sender, RoutedEventArgs e)
        {
            int pageIndex = 0;
            if (Viewer != null && Viewer.FocusedIndex >= 0)
                pageIndex = Viewer.FocusedIndex;
            AddBookmark(pageIndex);
        }

        /// <summary>
        /// Deletes bookmark.
        /// </summary>
        private void deleteOutlineNodeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedBookmark();
        }

        /// <summary>
        /// Edits selected bookmark.
        /// </summary>
        private void editMenuItem_Click(object sender, RoutedEventArgs e)
        {
            EditSelectedBookmark();
        }

        /// <summary>
        /// Moves down selected bookmark.
        /// </summary>
        private void moveDownMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem != null)
                MoveBookmark(SelectedItem as TreeViewItem, 1);
        }

        /// <summary>
        /// Moves up selected bookmark.
        /// </summary>
        private void moveUpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem != null)
                MoveBookmark(SelectedItem as TreeViewItem, -1);
        }

        /// <summary>
        /// Context menu is opened.
        /// </summary>
        private void contextMenu_Opened(object sender, RoutedEventArgs e)
        {
            bool canEdit = SelectedItem != null;
            _moveDownMenuItem.IsEnabled = canEdit;
            _moveUpMenuItem.IsEnabled = canEdit;
            _editMenuItem.IsEnabled = canEdit;
            _deleteOutlineNodeMenuItem.IsEnabled = canEdit;
        }

        /// <summary>
        /// Determines whether command can be executed.
        /// </summary>
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedItem != null;
        }

        /// <summary>
        /// Searches item collection which contains search item.
        /// </summary>
        /// <param name="itemCollection">An item collection.</param>
        /// <param name="searchItem">A search item.</param>
        /// <returns>The item collection which contains search item.</returns>
        private ItemCollection SearchItemCollection(ItemCollection itemCollection, TreeViewItem searchItem)
        {
            if (itemCollection.IndexOf(searchItem) == -1)
            {
                foreach (object item in itemCollection)
                {
                    TreeViewItem treeItem = (item as TreeViewItem);
                    ItemCollection searchedItemCollection = null;
                    if (treeItem.Items != null && treeItem.Items.Count > 0)
                    {
                        if ((searchedItemCollection = SearchItemCollection(treeItem.Items, searchItem)) != null)
                            return searchedItemCollection;
                    }
                }
                return null;
            }
            else
                return itemCollection;
        }


        #region Drag & drop

        /// <summary>
        /// Handles the MouseMove event of the treeItem control.
        /// </summary>
        private void treeItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (_draggingItem == null && e.LeftButton == MouseButtonState.Pressed)
            {
                _draggingItem = (TreeViewItem)sender;
                DragDrop.DoDragDrop(this, _draggingItem, DragDropEffects.Move);
                _draggingItem = null;
                if (_prevChangedTreeViewItem != null)
                {
                    _prevChangedTreeViewItem.BorderThickness = new Thickness();
                    _prevChangedTreeViewItem = null;
                }
            }
        }

        /// <summary>
        /// Handles the DragEnter event of the BookmarkTreeView control.
        /// </summary>
        private void BookmarkTreeView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeViewItem)))
                e.Effects = DragDropEffects.Move;
        }

        /// <summary>
        /// Handles the DragLeave event of the BookmarkTreeView control.
        /// </summary>
        private void BookmarkTreeView_DragLeave(object sender, DragEventArgs e)
        {
            _draggingItem = null;
        }

        /// <summary>
        /// Determines that <i>parentNode</i> contains <i>childNode</i>.
        /// </summary>
        /// <param name="source">The parent node.</param>
        /// <param name="destNode">The child node.</param>
        /// <returns>
        /// <b>true</b> - <i>parentNode</i> contains <i>childNode</i>;
        /// <b>false</b> - <i>parentNode</i> does NOT contain <i>childNode</i>.
        /// </returns>
        private bool NodeIsEmbedded(TreeViewItem parentNode, TreeViewItem childNode)
        {
            if (parentNode == childNode)
                return true;

            if (parentNode == null || childNode == null)
                return false;

            foreach (TreeViewItem node in parentNode.Items)
            {
                if (childNode == node || NodeIsEmbedded(node, childNode))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Finds the TreeView item.
        /// </summary>
        /// <param name="p">The point.</param>
        private TreeViewItem FindTreeViewItem(Point p)
        {
            return FindTreeViewItem(Items, p);
        }

        /// <summary>
        /// Finds the TreeView item.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="p">The point.</param>
        private TreeViewItem FindTreeViewItem(ItemCollection items, Point p)
        {
            foreach (TreeViewItem item in items)
            {
                Point pt = TranslatePoint(p, item);
                if (pt.Y >= 0 && pt.Y <= 16)
                    return item;
                else if (item.IsExpanded)
                {
                    TreeViewItem result = FindTreeViewItem(item.Items, p);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        /// <summary>
        /// Handles the DragOver event of the BookmarkTreeView control.
        /// </summary>
        private void BookmarkTreeView_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.AllowedEffects;
            if (e.Data.GetDataPresent(typeof(TreeViewItem)))
            {
                TreeView treeView = (TreeView)sender;
                // get reference to the dragging TreeNode
                TreeViewItem sourceNode = (TreeViewItem)e.Data.GetData(typeof(TreeViewItem));
                // get reference to the destination TreeNode
                Point pt = e.GetPosition(treeView);
                double delta = 16 * DELTA_FOR_INSERTION_IN_THE_SAME_LEVEL;
                // get tree node, which is located before the node under mouse cursor
                TreeViewItem destNode1 = FindTreeViewItem(new Point(pt.X, pt.Y - delta));
                // get tree node, which is located after the node under mouse cursor
                TreeViewItem destNode2 = FindTreeViewItem(new Point(pt.X, pt.Y + delta));

                if (destNode1 == destNode2 && destNode1 != null)
                {
                    if (NodeIsEmbedded(sourceNode, destNode1))
                        e.Effects = DragDropEffects.None;

                    if (_prevChangedTreeViewItem != null)
                    {
                        _prevChangedTreeViewItem.BorderThickness = new Thickness();
                        _prevChangedTreeViewItem = null;
                    }
                }
                else
                {
                    // if dragging is NOT possible
                    if (NodeIsEmbedded(sourceNode, destNode1) ||
                        NodeIsEmbedded(sourceNode, destNode2))
                    {
                        // disable drag drop
                        e.Effects = DragDropEffects.None;
                    }
                    else
                    {
                        if (_prevChangedTreeViewItem != null)
                        {
                            _prevChangedTreeViewItem.BorderThickness = new Thickness();
                            _prevChangedTreeViewItem = null;
                        }

                        if (destNode1 != null)
                        {
                            destNode1.BorderThickness = new Thickness(0, 0, 0, 2);
                            _prevChangedTreeViewItem = destNode1;
                        }
                        else if (destNode2 != null)
                        {
                            destNode2.BorderThickness = new Thickness(0, 2, 0, 0);
                            _prevChangedTreeViewItem = destNode2;
                        }
                        else
                        {
                            TreeViewItem lastItem = (TreeViewItem)treeView.Items[treeView.Items.Count - 1];
                            lastItem.BorderThickness = new Thickness(0, 0, 0, 2);
                            _prevChangedTreeViewItem = lastItem;
                        }
                    }
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the Drop event of the BookmarkTreeView control.
        /// </summary>
        private void BookmarkTreeView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeViewItem)))
            {
                // get tree view
                TreeView treeView = (TreeView)sender;
                // begin update of BookmarkTreeView
                treeView.BeginInit();
                try
                {
                    // get reference to the dragging TreeNode
                    TreeViewItem sourceNode = (TreeViewItem)e.Data.GetData(typeof(TreeViewItem));

                    // get reference to the destination TreeNode
                    Point pt = e.GetPosition(treeView);
                    // calculate inaccuracy of position of mouse
                    double delta = 16 * DELTA_FOR_INSERTION_IN_THE_SAME_LEVEL;
                    // get tree node, which is located before the node under mouse cursor
                    TreeViewItem destNode1 = FindTreeViewItem(new Point(pt.X, pt.Y - delta));
                    // get tree node, which is located after the node under mouse cursor
                    TreeViewItem destNode2 = FindTreeViewItem(new Point(pt.X, pt.Y + delta));

                    // update the bookmark tree in TreeView

                    if (sourceNode == destNode1 ||
                        NodeIsEmbedded(sourceNode, destNode1) ||
                        NodeIsEmbedded(sourceNode, destNode2))
                        return;

                    if (sourceNode.Parent is TreeView)
                        ((TreeView)sourceNode.Parent).Items.Remove(sourceNode);
                    else if (sourceNode.Parent is TreeViewItem)
                        ((TreeViewItem)sourceNode.Parent).Items.Remove(sourceNode);
                    else
                        throw new NotImplementedException();

                    // get reference to the dragging bookmark
                    PdfBookmark sourceBookmark = (PdfBookmark)sourceNode.Tag;
                    PdfBookmark destBookmark = null;

                    // if dragging node must be added as child of root node
                    if (destNode1 == null && destNode2 == null)
                    {
                        // remove source bookmark
                        sourceBookmark.Remove();
                        // add source bookmark to root of PDF document bookmarks
                        sourceBookmark.Document.Bookmarks.Add(sourceBookmark);
                        // add node to root of BookmarkTreeView
                        treeView.Items.Add(sourceNode);
                    }
                    // if dragging node must be inserted between nodes
                    else if (destNode1 != destNode2)
                    {
                        // parent node
                        ItemsControl parent = null;
                        // if tree node, which is located before the node under mouse cursor, exists
                        if (destNode1 != null)
                            // get parent of tree view node
                            parent = (ItemsControl)destNode1.Parent;
                        else
                            // get parent of tree view node
                            parent = (ItemsControl)destNode2.Parent;

                        // bookmarks of parent bookmark
                        PdfBookmarkCollection parentBookmark = null;
                        if (parent is TreeView)
                            parentBookmark = sourceBookmark.Document.Bookmarks;
                        else if (destNode1 != null)
                            parentBookmark = ((PdfBookmark)destNode1.Tag).ParentBookmarks;
                        else
                            parentBookmark = ((PdfBookmark)destNode2.Tag).ParentBookmarks;

                        // position of bookmark/node to insert
                        int index = 0;
                        // if tree node, which is located before the node under mouse cursor, exists
                        if (destNode1 != null)
                            // get index
                            index = parent.Items.IndexOf(destNode1) + 1;
                        else
                        {
                            // get index
                            index = parent.Items.IndexOf(destNode2) - 1;
                            if (index == -1)
                                index = 0;
                        }

                        // if insert to end
                        if (index == parent.Items.Count)
                        {
                            // add node
                            parent.Items.Add(sourceNode);
                            // move bookmark
                            sourceBookmark.Move(parentBookmark);
                        }
                        else
                        {
                            // insert node
                            parent.Items.Insert(index, sourceNode);
                            // remove bookmark
                            sourceBookmark.Remove();
                            // insert bookmark
                            parentBookmark.Insert(index, sourceBookmark);
                        }

                        // if parent exist
                        if (parent != null && parent is TreeViewItem)
                            // expand parent
                            ((TreeViewItem)parent).IsExpanded = true;
                    }
                    else
                    {
                        // add node to destination node
                        destNode1.Items.Add(sourceNode);
                        // expand destination node
                        destNode1.IsExpanded = true;

                        // gete reference to the destination bookmark
                        destBookmark = (PdfBookmark)destNode1.Tag;
                        // add the dragging bookmark as a child of destination bookmark
                        sourceBookmark.Move(destBookmark.ChildBookmarks);
                    }
                }
                finally
                {
                    // end update of BookmarkTreeView
                    treeView.EndInit();
                }
            }
        }

        #endregion

        #endregion

        #endregion

    }
}
