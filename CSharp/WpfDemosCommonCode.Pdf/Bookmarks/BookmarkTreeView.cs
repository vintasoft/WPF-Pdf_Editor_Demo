using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Wpf;
using Vintasoft.Imaging.Wpf.UI;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A tree view that allows to view bookmarks of PDF document.
    /// </summary>
    public class BookmarkTreeView : TreeView
    {

        #region Fields

        /// <summary>
        /// Indicates that tree view item is selected.
        /// </summary>
        bool _isTreeViewItemSelected = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BookmarkTreeView"/> class.
        /// </summary>
        public BookmarkTreeView()
        {
        }

        #endregion



        #region Properties

        PdfActionExecutorBase _actionExecutor;
        /// <summary>
        /// Gets or sets the action executor.
        /// </summary>
        public PdfActionExecutorBase ActionExecutor
        {
            get
            {
                return _actionExecutor;
            }
            set
            {
                _actionExecutor = value;
            }
        }

        PdfDocument _document;
        /// <summary>
        /// Gets or sets the source PDF document.
        /// </summary>
        public PdfDocument Document
        {
            get
            {
                return _document;
            }
            set
            {
                if (_document != value)
                {
                    _document = value;
                    ShowBookmarks();
                }
            }
        }

        WpfImageViewerBase _viewer;
        /// <summary>
        /// Gets or sets the image viewer.
        /// </summary>
        public WpfImageViewerBase Viewer
        {
            get
            {
                return _viewer;
            }
            set
            {
                _viewer = value;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Shows the bookmarks.
        /// </summary>
        public void ShowBookmarks()
        {
            // clear the current bookmarks tree
            Items.Clear();

            // if document is not empty
            if (_document != null)
            {
                lock (_document)
                {
                    // add the document bookmarks to the bookmarks tree
                    AddBookmarks(Items, _document.Bookmarks);
                }
            }
        }

        #endregion


        #region PROTECTED

        /// <summary>
        /// Creates the <see cref="TreeViewItem"/> for <see cref="PdfBookmark"/>.
        /// </summary>
        /// <param name="bookmark">The bookmark.</param>
        /// <returns>
        /// The <see cref="TreeViewItem"/>.
        /// </returns>
        protected virtual TreeViewItem CreateTreeViewItem(PdfBookmark bookmark)
        {
            // create new tree node
            TreeViewItem treeViewItem = new TreeViewItem();

            // set bookmark title
            treeViewItem.Header = bookmark.Title.Replace("\r", " ").Replace("\n", " ");

            treeViewItem.MouseRightButtonDown += new MouseButtonEventHandler(BookmarkTreeView_MouseRightButtonDown);
            treeViewItem.MouseRightButtonUp += new MouseButtonEventHandler(BookmarkTreeView_MouseRightButtonUp);

            return treeViewItem;
        }

        /// <summary>
        /// Adds bookmark to the specified tree node collection.
        /// </summary>
        /// <param name="destination">A tree node collection.</param>
        /// <param name="bookmark">A PDF bookmark.</param>
        /// <returns>A tree node.</returns>
        protected TreeViewItem AddBookmark(ItemCollection destination, PdfBookmark bookmark)
        {
            // create new tree node
            TreeViewItem treeItem = CreateTreeViewItem(bookmark);
            // set tag as bookmark
            treeItem.Tag = bookmark;
            // if tree node collection is not empty
            if (destination != null)
            {
                // add bookmark to the collection
                destination.Add(treeItem);
            }
            // set font
            treeItem.FontFamily = base.FontFamily;
            treeItem.FontSize = base.FontSize;
            treeItem.FontStyle = FontStyles.Normal;
            treeItem.FontWeight = FontWeights.Normal;
            treeItem.BorderBrush = Brushes.Black;
            // if bookmark has flags
            if (bookmark.Flags != PdfBookmarkFlags.None)
            {
                // set new font
                if ((bookmark.Flags & PdfBookmarkFlags.Bold) != 0)
                    treeItem.FontWeight = FontWeights.Bold;
                if ((bookmark.Flags & PdfBookmarkFlags.Italic) != 0)
                    treeItem.FontStyle = FontStyles.Italic;
            }
            if (bookmark.ChildBookmarks != null)
                AddBookmarks(treeItem.Items, bookmark.ChildBookmarks);

            // set bookmark color
            treeItem.Foreground = new SolidColorBrush(WpfObjectConverter.CreateWindowsColor(bookmark.Color));

            return treeItem;
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Right mouse button is down.
        /// </summary>
        private void BookmarkTreeView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isTreeViewItemSelected = false;
        }

        /// <summary>
        /// Sets selection to the bookmark.
        /// </summary>
        private void BookmarkTreeView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isTreeViewItemSelected)
            {
                (sender as TreeViewItem).IsSelected = true;
                _isTreeViewItemSelected = true;
            }
        }

        /// <summary>
        /// Adds bookmarks to the specified tree node collection.
        /// </summary>
        /// <param name="destination">An item collection.</param>
        /// <param name="source">A collection of bookmarks.</param>
        private void AddBookmarks(ItemCollection destination, PdfBookmarkCollection source)
        {
            // if bookmark collection is not empty
            if (source != null)
            {
                // get the bookmark count
                int count = source.Count;
                // for each bookmark
                for (int i = 0; i < count; i++)
                {
                    // add bookmark to the tree node collection
                    AddBookmark(destination, source[i]);
                }
            }
        }

        #endregion

        #endregion

    }
}
