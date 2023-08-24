using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view and edit PDF goto action.
    /// </summary>
    public partial class PdfGotoActionEditorWindow : Window
    {

        #region Fields

        /// <summary>
        /// The PDF goto action.
        /// </summary>
        PdfGotoAction _action;

        /// <summary>
        /// The image collection, which is associated with PDF document.
        /// </summary>
        ImageCollection _imageCollection = null;

        #endregion



        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="PdfGotoActionEditorWindow"/> class
        /// from being created.
        /// </summary>
        private PdfGotoActionEditorWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PdfGotoActionEditorWindow"/> class.
        /// </summary>
        /// <param name="action">The PDF goto action.</param>
        public PdfGotoActionEditorWindow(PdfGotoAction action)
            : this(action, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfGotoActionEditorForm"/> class.
        /// </summary>
        /// <param name="action">The PDF goto action.</param>
        /// <param name="imageCollection">The image collection, which is associated with PDF document.</param>
        public PdfGotoActionEditorWindow(PdfGotoAction action, ImageCollection imageCollection)
            : this()
        {
            _imageCollection = imageCollection;
            _action = action;

            PdfDocument document = action.Document;

            // add destination types
            positionComboBox.Items.Add(PdfDestinationType.XYZoom);
            positionComboBox.Items.Add(PdfDestinationType.Fit);
            positionComboBox.Items.Add(PdfDestinationType.FitHorizontal);
            positionComboBox.Items.Add(PdfDestinationType.FitVertical);
            positionComboBox.Items.Add(PdfDestinationType.FitRectangle);
            positionComboBox.SelectedItem = PdfDestinationType.Fit;


            // update count of PDF pages

            pageNumberNumericUpDown.Minimum = 1;
            pageNumberNumericUpDown.Value = 1;
            // if image collection oes NOT exist
            if (_imageCollection == null)
                // get page count from PDF document
                pageNumberNumericUpDown.Maximum = document.Pages.Count;
            else
                // get page count from image collection
                pageNumberNumericUpDown.Maximum = _imageCollection.Count;

            // if action has destination
            if (_action.Destination != null)
            {
                // get the destination type
                PdfDestinationType type = _action.Destination.DestinationType;
                positionComboBox.SelectedItem = type;
                // get index of PDF page associated with goto action
                int pdfPageIndex = document.Pages.IndexOf(_action.Destination.Page);
                // if image collection does NOT exist
                if (_imageCollection == null)
                    pageNumberNumericUpDown.Value = pdfPageIndex + 1;
                else
                {
                    // find image, which is associated with PDF page

                    for (int i = 0; i < _imageCollection.Count; i++)
                    {
                        VintasoftImage image = _imageCollection[i];
                        if (image.SourceInfo.PageIndex == pdfPageIndex)
                        {
                            pageNumberNumericUpDown.Value = i + 1;
                            break;
                        }
                    }
                }

                switch (type)
                {
                    case PdfDestinationType.XYZoom:
                        PdfDestinationXYZ destinationXyz = (PdfDestinationXYZ)_action.Destination;
                        // get destination location
                        PointF location = destinationXyz.Location;
                        // update values of text boxes
                        if (location.X == float.MinValue)
                        {
                            destinationXTextBox.Text = "0";
                            destinationXCheckBox.IsChecked = false;
                        }
                        else
                        {
                            destinationXTextBox.Text = location.X.ToString(CultureInfo.InvariantCulture);
                            destinationXCheckBox.IsChecked = true;
                        }
                        if (location.Y == float.MinValue)
                        {
                            destinationYTextBox.Text = "0";
                            destinationYCheckBox.IsChecked = false;
                        }
                        else
                        {
                            destinationYTextBox.Text = location.Y.ToString(CultureInfo.InvariantCulture);
                            destinationYCheckBox.IsChecked = true;
                        }
                        // update value of numeric up down
                        if (destinationXyz.Zoom <= 0)
                        {
                            destinationZoomNumericUpDown.Value = 100;
                            destinationZoomCheckBox.IsChecked = false;
                        }
                        else
                        {
                            destinationZoomNumericUpDown.Value = (int)Math.Round(destinationXyz.Zoom * 100.0);
                            destinationZoomCheckBox.IsChecked = true;
                        }
                        break;

                    case PdfDestinationType.FitRectangle:
                        PdfDestinationFitRectangle destinationRectangle = (PdfDestinationFitRectangle)_action.Destination;
                        float rectX = destinationRectangle.Left;
                        float rectY = destinationRectangle.Bottom;
                        float rectWidth = destinationRectangle.Right - destinationRectangle.Left;
                        float rectHeight = destinationRectangle.Top - destinationRectangle.Bottom;
                        // update values of text boxes
                        destinationFitRectangleXTextBox.Text = rectX.ToString(CultureInfo.InvariantCulture);
                        destinationFitRectangleYTextBox.Text = rectY.ToString(CultureInfo.InvariantCulture);
                        destinationFitRectangleWidthTextBox.Text = rectWidth.ToString(CultureInfo.InvariantCulture);
                        destinationFitRectangleHeightTextBox.Text = rectHeight.ToString(CultureInfo.InvariantCulture);
                        break;
                }
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// "Ok" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            // get PDF document
            PdfDocument document = _action.Document;
            // get PDF page
            PdfPage page = GetSelectedPage();

            // create destination of action

            switch ((PdfDestinationType)positionComboBox.SelectedItem)
            {
                case PdfDestinationType.Fit:
                    _action.Destination = new PdfDestinationFit(document, page);
                    break;

                case PdfDestinationType.FitHorizontal:
                    _action.Destination = new PdfDestinationFitHorizontal(document, page);
                    break;

                case PdfDestinationType.FitVertical:
                    _action.Destination = new PdfDestinationFitVertical(document, page);
                    break;

                case PdfDestinationType.XYZoom:
                    float x;
                    if (destinationXCheckBox.IsChecked.Value == true)
                    {
                        if (!float.TryParse(destinationXTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out x))
                        {
                            DemosTools.ShowErrorMessage("X coordinate is not a valid float number.");
                            destinationXTextBox.SelectAll();
                            destinationXTextBox.Focus();
                            return;
                        }
                    }
                    else
                    {
                        x = float.MinValue;
                    }
                    float y;
                    if (destinationYCheckBox.IsChecked.Value == true)
                    {
                        if (!float.TryParse(destinationYTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out y))
                        {
                            DemosTools.ShowErrorMessage("Y coordinate is not a valid float number.");
                            destinationYTextBox.SelectAll();
                            destinationYTextBox.Focus();
                            return;
                        }
                    }
                    else
                    {
                        y = float.MinValue;
                    }

                    float zoom;
                    if (destinationZoomCheckBox.IsChecked.Value == true)
                    {
                        zoom = (float)destinationZoomNumericUpDown.Value / 100f;
                    }
                    else
                    {
                        zoom = float.MinValue;
                    }

                    PointF location = new PointF(x, y);
                    _action.Destination = new PdfDestinationXYZ(document, page, location, zoom);
                    break;

                case PdfDestinationType.FitRectangle:
                    float left;
                    if (!float.TryParse(destinationFitRectangleXTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out left))
                    {
                        DemosTools.ShowErrorMessage("X coordinate is not a valid float number.");
                        destinationFitRectangleXTextBox.SelectAll();
                        destinationFitRectangleXTextBox.Focus();
                        return;
                    }

                    float bottom;
                    if (!float.TryParse(destinationFitRectangleYTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out bottom))
                    {
                        DemosTools.ShowErrorMessage("Y coordinate is not a valid float number.");
                        destinationFitRectangleYTextBox.SelectAll();
                        destinationFitRectangleYTextBox.Focus();
                        return;
                    }

                    float width;
                    if (!float.TryParse(destinationFitRectangleWidthTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out width))
                    {
                        DemosTools.ShowErrorMessage("Width is not a valid float number.");
                        destinationFitRectangleWidthTextBox.SelectAll();
                        destinationFitRectangleWidthTextBox.Focus();
                        return;
                    }

                    float height;
                    if (!float.TryParse(destinationFitRectangleHeightTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out height))
                    {
                        DemosTools.ShowErrorMessage("Height is not a valid float number.");
                        destinationFitRectangleHeightTextBox.SelectAll();
                        destinationFitRectangleHeightTextBox.Focus();
                        return;
                    }

                    if (width <= 0)
                    {
                        DemosTools.ShowErrorMessage("Width must be positive.");
                        destinationFitRectangleWidthTextBox.SelectAll();
                        destinationFitRectangleWidthTextBox.Focus();
                        return;
                    }

                    if (height <= 0)
                    {
                        DemosTools.ShowErrorMessage("Height must be positive.");
                        destinationFitRectangleHeightTextBox.SelectAll();
                        destinationFitRectangleHeightTextBox.Focus();
                        return;
                    }

                    PdfDestinationFitRectangle destination = new PdfDestinationFitRectangle(document, page);
                    destination.Left = left;
                    destination.Bottom = bottom;
                    destination.Right = left + width;
                    destination.Top = bottom + height;
                    _action.Destination = destination;
                    break;
            }

            DialogResult = true;
        }

        /// <summary>
        /// "Cancel" button is clicked.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Destination position type is changed.
        /// </summary>
        private void positionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // get destination type
            PdfDestinationType type = (PdfDestinationType)positionComboBox.SelectedItem;

            // hide panels
            destinationXyzPanel.Visibility = Visibility.Hidden;
            destinationFitRectanglePanel.Visibility = Visibility.Hidden;

            // if type is XYZoom
            if (type == PdfDestinationType.XYZoom)
                // show XYZoom panel
                destinationXyzPanel.Visibility = Visibility.Visible;
            // if type is FitRectangle
            else if (type == PdfDestinationType.FitRectangle)
                // show FitRectangle panel
                destinationFitRectanglePanel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the Click event of the destinationXCheckBox control.
        /// </summary>
        private void destinationXCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (destinationXCheckBox.IsChecked.Value == true)
                destinationXTextBox.IsEnabled = true;
            else
                destinationXTextBox.IsEnabled = false;
        }

        /// <summary>
        /// Handles the Click event of the destinationYCheckBox control.
        /// </summary>
        private void destinationYCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (destinationYCheckBox.IsChecked.Value == true)
                destinationYTextBox.IsEnabled = true;
            else
                destinationYCheckBox.IsChecked = false;
        }

        /// <summary>
        /// Handles the Click event of the destinationZoomCheckBox control.
        /// </summary>
        private void destinationZoomCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (destinationZoomCheckBox.IsChecked.Value == true)
                destinationZoomNumericUpDown.IsEnabled = true;
            else
                destinationZoomNumericUpDown.IsEnabled = false;
        }

        /// <summary>
        /// Returns the selected PDF page.
        /// </summary>
        /// <returns>The selected PDF page.</returns>
        private PdfPage GetSelectedPage()
        {
            // get document
            PdfDocument document = _action.Document;
            // get index of selected page
            int imageIndex = (int)pageNumberNumericUpDown.Value - 1;
            PdfPage page = null;
            // if image collection exists
            if (_imageCollection != null)
            {
                // get image
                VintasoftImage image = _imageCollection[imageIndex];
                // get PDF page associated with image
                page = document.Pages[image.SourceInfo.PageIndex];
            }
            else
                // get page
                page = document.Pages[imageIndex];
            return page;
        }

        #endregion

    }
}
