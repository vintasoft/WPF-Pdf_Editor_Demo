using System.Windows.Controls;
using System.Windows.Media;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A view of the processing item.
    /// </summary>
    public class ProcessingTreeViewItem : TreeViewItem
    {

        #region Fields

        /// <summary>
        /// The image.
        /// </summary>
        Image _image = new Image();

        /// <summary>
        /// The text block.
        /// </summary>
        TextBlock _textBlock = new TextBlock();

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingTreeViewItem"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        public ProcessingTreeViewItem(string text)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;

            _image.Source = null;
            _image.Stretch = Stretch.None;
            _image.Width = 16;
            _image.Height = 16;
            _image.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            _image.Margin = new System.Windows.Thickness(0, 0, 3, 0);
            stackPanel.Children.Add(_image);

            _textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            _textBlock.TextWrapping = System.Windows.TextWrapping.NoWrap;
            stackPanel.Children.Add(_textBlock);

            Header = stackPanel;

            Text = text;
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text
        {
            get
            {
                return _textBlock.Text;
            }
            set
            {
                string text = value;
                text = text.Replace('\n', ' ');
                text = text.Replace('\r', ' ');

                _textBlock.Text = text;
            }
        }

        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public ImageSource Image
        {
            get
            {
                return _image.Source;
            }
            set
            {
                if (Image != value)
                {
                    _image.Source = value;

                    if (value != null)
                        _image.Visibility = System.Windows.Visibility.Visible;
                    else
                        _image.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        #endregion

    }
}
