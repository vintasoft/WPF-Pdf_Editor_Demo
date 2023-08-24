using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf.Tree;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to specify parameters of page, which should be added to a PDF document.
    /// </summary>
    public partial class AddEmptyPageWindow : Window
    {

        #region Fields

        bool _updateValues = false;

        #endregion



        #region Constructor

        public AddEmptyPageWindow(SizeF initialPageSizeInUserUnits, UnitOfMeasure initialUnits)
        {
            InitializeComponent();
            _pageSizeInUserUnits = initialPageSizeInUserUnits;
            foreach (object value in Enum.GetValues(typeof(UnitOfMeasure)))
                unitsComboBox.Items.Add(value);
            unitsComboBox.SelectedItem = initialUnits;

            foreach (object value in Enum.GetValues(typeof(PaperSizeKind)))
                paperKindComboBox.Items.Add(value);
            paperKindComboBox.Items.Remove(PaperSizeKind.Custom);
            paperKindComboBox.SelectedItem = PaperSizeKind.A4;

            standardSizeRadioButton.IsChecked = true;
        }

        #endregion



        #region Properties

        SizeF _pageSizeInUserUnits;
        public SizeF PageSizeInUserUnits
        {
            get
            {
                return _pageSizeInUserUnits;
            }
        }

        public UnitOfMeasure Units
        {
            get
            {
                return (UnitOfMeasure)unitsComboBox.SelectedItem;
            }
        }

        PaperSizeKind _paperKind = PaperSizeKind.Custom;
        public PaperSizeKind PaperKind
        {
            get
            {
                return _paperKind;
            }
        }

        public bool Rotated
        {
            get
            {
                return rotatedCheckBox.IsChecked.Value == true;
            }
        }

        #endregion



        #region Methods

        private void UpdateValues()
        {
            _updateValues = true;
            widthTextBox.Text = FloatToString(PdfPage.ConvertFromUserUnitsToUnitOfMeasure(PageSizeInUserUnits.Width, Units));
            heightTextBox.Text = FloatToString(PdfPage.ConvertFromUserUnitsToUnitOfMeasure(PageSizeInUserUnits.Height, Units));
            _updateValues = false;
        }

        static string FloatToString(float value)
        {
            return value.ToString("f2", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Handles the Click event of OkButton object.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (customSizeRadioButton.IsChecked.Value == true)
                _paperKind = PaperSizeKind.Custom;
            else
                _paperKind = (PaperSizeKind)paperKindComboBox.SelectedItem;

            DialogResult = true;
        }

        /// <summary>
        /// Handles the Click event of CancelButton object.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Handles the SelectionChanged event of UnitsComboBox object.
        /// </summary>
        private void unitsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateValues();
        }

        /// <summary>
        /// Handles the TextChanged event of HeightTextBox object.
        /// </summary>
        private void heightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_updateValues)
                return;
            try
            {
                _pageSizeInUserUnits.Height = PdfPage.ConvertFromUnitOfMeasureToUserUnits(float.Parse(heightTextBox.Text, CultureInfo.InvariantCulture), Units);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error");
            }
        }

        /// <summary>
        /// Handles the TextChanged event of WidthTextBox object.
        /// </summary>
        private void widthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_updateValues)
                return;
            try
            {
                _pageSizeInUserUnits.Width = PdfPage.ConvertFromUnitOfMeasureToUserUnits(float.Parse(widthTextBox.Text, CultureInfo.InvariantCulture), Units);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error");
            }
        }

        /// <summary>
        /// Handles the Checked event of CustomSizeRadioButton object.
        /// </summary>
        private void customSizeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            groupBox1.IsEnabled = customSizeRadioButton.IsChecked.Value == true;
            groupBox2.IsEnabled = groupBox1.IsEnabled == false;
        }

        /// <summary>
        /// Handles the Checked event of StandardSizeRadioButton object.
        /// </summary>
        private void standardSizeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            groupBox2.IsEnabled = standardSizeRadioButton.IsChecked.Value == true;
            groupBox1.IsEnabled = groupBox2.IsEnabled == false;
        }

        /// <summary>
        /// Handles the SelectionChanged event of PaperKindComboBox object.
        /// </summary>
        private void paperKindComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (standardSizeRadioButton.IsChecked.Value == true)
            {
                ImageSize size = ImageSize.FromPaperKind((PaperSizeKind)paperKindComboBox.SelectedItem, ImagingEnvironment.ScreenResolution);
                float width = (float)PdfPage.ConvertFromUnitOfMeasureToUserUnits((float)size.WidthInInch, UnitOfMeasure.Inches);
                width = PdfPage.ConvertFromUserUnitsToUnitOfMeasure(width, (UnitOfMeasure)unitsComboBox.SelectedItem);
                float height = (float)PdfPage.ConvertFromUnitOfMeasureToUserUnits((float)size.HeightInInch, UnitOfMeasure.Inches);
                height = PdfPage.ConvertFromUserUnitsToUnitOfMeasure(height, (UnitOfMeasure)unitsComboBox.SelectedItem);
                widthTextBox.Text = width.ToString(CultureInfo.InvariantCulture);
                heightTextBox.Text = height.ToString(CultureInfo.InvariantCulture);
            }
        }

        #endregion

    }
}
