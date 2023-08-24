using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;

using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Wpf;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit border properties of the <see cref="PdfAnnotation"/>.
    /// </summary>
    public partial class PdfAnnotationBorderStyleControl : UserControl
    {

        #region Constants

        /// <summary>
        /// The dash pattern none.
        /// </summary>
        const string DASH_PATTERN_NONE = "None";

        /// <summary>
        /// The name of custom dash pattern.
        /// </summary>
        string _customDashPatternName = string.Empty;

        #endregion



        #region Fields

        /// <summary>
        /// Ditionary that contains available dash pattern styles.
        /// </summary>
        Dictionary<string, float[]> _dashPatternStyles = new Dictionary<string, float[]>();

        /// <summary>
        /// Determines that the user interface is updating.
        /// </summary>
        bool _isUpdatingUI = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfAnnotationBorderStyleControl"/> class.
        /// </summary>
        public PdfAnnotationBorderStyleControl()
        {
            InitializeComponent();

            foreach (PdfAnnotationBorderStyleType style in Enum.GetValues(typeof(PdfAnnotationBorderStyleType)))
                styleTypeComboBox.Items.Add(style);

            // add default styles of pattern
            _dashPatternStyles.Add("Dash", new float[] { 3, 1 });
            _dashPatternStyles.Add("Dash Dot", new float[] { 3, 1, 1, 1 });
            _dashPatternStyles.Add("Dash Dot Dot", new float[] { 3, 1, 1, 1, 1, 1 });
            _dashPatternStyles.Add("Dot", new float[] { 1, 1 });
            _dashPatternStyles.Add(DASH_PATTERN_NONE, null);

            foreach (string name in _dashPatternStyles.Keys)
                dashPatternComboBox.Items.Add(name);

            dashPatternComboBox.SelectedItem = DASH_PATTERN_NONE;
        }

        #endregion



        #region Properties

        #region PUBLIC

        PdfAnnotation _annotation = null;
        /// <summary>
        /// Gets or sets the annotation.
        /// </summary>
        public virtual PdfAnnotation Annotation
        {
            get
            {
                return _annotation;
            }
            set
            {
                if (_annotation != value)
                {
                    _annotation = value;
                    mainGrid.IsEnabled = _annotation != null;

                    UpdateBorderInfo();
                }
            }
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Gets or sets the border color.
        /// </summary>
        protected virtual Color BorderColor
        {
            get
            {
                if (_annotation is PdfFreeTextAnnotation)
                    return WpfObjectConverter.CreateWindowsColor(((PdfFreeTextAnnotation)_annotation).TextColor);
                else
                    return WpfObjectConverter.CreateWindowsColor(_annotation.Color);
            }
            set
            {
                if (_annotation is PdfFreeTextAnnotation)
                    ((PdfFreeTextAnnotation)_annotation).TextColor = WpfObjectConverter.CreateDrawingColor(value);
                else
                    _annotation.Color = WpfObjectConverter.CreateDrawingColor(value);
            }
        }

        /// <summary>
        /// Gets or sets the type of the border style.
        /// </summary>
        protected virtual PdfAnnotationBorderStyleType BorderStyleType
        {
            get
            {
                return _annotation.BorderStyleType;
            }
            set
            {
                _annotation.BorderStyleType = value;
            }
        }

        /// <summary>
        /// Gets or sets the border width.
        /// </summary>
        protected virtual float BorderWidth
        {
            get
            {
                return _annotation.BorderWidth;
            }
            set
            {
                _annotation.BorderWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets the border dash array.
        /// </summary>
        protected virtual float[] BorderDashArray
        {
            get
            {
                return _annotation.BorderDashArray;
            }
            set
            {
                _annotation.BorderDashArray = value;
            }
        }

        #endregion

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Updates the information about border.
        /// </summary>
        public void UpdateBorderInfo()
        {
            if (_isUpdatingUI)
                return;

            _isUpdatingUI = true;
            try
            {
                if (!string.IsNullOrEmpty(_customDashPatternName))
                {
                    _dashPatternStyles.Remove(_customDashPatternName);
                    dashPatternComboBox.Items.Remove(_customDashPatternName);
                    _customDashPatternName = string.Empty;
                }
                dashPatternComboBox.SelectedItem = DASH_PATTERN_NONE;
                styleTypeComboBox.SelectedItem = PdfAnnotationBorderStyleType.Default;

                if (Annotation != null)
                {
                    widthNumericUpDown.Value = (int)BorderWidth;
                    colorPanelControl.Color = BorderColor;
                    styleTypeComboBox.SelectedItem = BorderStyleType;

                    float[] dashArray = BorderDashArray;
                    string dashPatternStyleName = GetDashPatternStyleName(dashArray, BorderWidth);
                    if (string.IsNullOrEmpty(dashPatternStyleName))
                    {
                        dashPatternStyleName = string.Empty;
                        string floatFormat = "0.#";
                        int length = Math.Min(14, dashArray.Length);
                        for (int i = 0; i < length - 1; i++)
                            dashPatternStyleName += string.Format("{0}, ",
                                dashArray[i].ToString(floatFormat, CultureInfo.InvariantCulture));
                        dashPatternStyleName += string.Format("{0}",
                            dashArray[length - 1].ToString(floatFormat, CultureInfo.InvariantCulture));
                        if (dashArray.Length > length)
                            dashPatternStyleName += " ...";
                        _customDashPatternName = dashPatternStyleName;
                        dashPatternComboBox.Items.Add(_customDashPatternName);
                        _dashPatternStyles.Add(_customDashPatternName, dashArray);
                    }
                    dashPatternComboBox.SelectedItem = dashPatternStyleName;
                }
            }
            finally
            {
                _isUpdatingUI = false;
            }
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Border style is changed.
        /// </summary>
        private void styleTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PdfAnnotationBorderStyleType style = (PdfAnnotationBorderStyleType)styleTypeComboBox.SelectedItem;
            dashPatternComboBox.IsEnabled = (style == PdfAnnotationBorderStyleType.Dashed);

            if (_isUpdatingUI)
                return;

            BorderStyleType = style;
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Border width is changed.
        /// </summary>
        private void widthNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUI)
                return;

            BorderWidth = (float)widthNumericUpDown.Value;

            UpdateDashArray();
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Border color is changed.
        /// </summary>
        private void colorPanelControl_ColorChanged(object sender, EventArgs e)
        {
            if (_isUpdatingUI)
                return;

            BorderColor = colorPanelControl.Color;
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Dash pattern of border is changed.
        /// </summary>
        private void dashPatternComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Annotation == null || _isUpdatingUI)
                return;

            UpdateDashArray();
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Updates the border dash array.
        /// </summary>
        private void UpdateDashArray()
        {
            string dashPatternName = DASH_PATTERN_NONE;
            if (dashPatternComboBox.SelectedItem != null)
                dashPatternName = dashPatternComboBox.SelectedItem.ToString();

            float[] dashPattern = _dashPatternStyles[dashPatternName];
            if (dashPattern != null)
                dashPattern = (float[])dashPattern.Clone();

            if (dashPatternName != DASH_PATTERN_NONE &&
                dashPatternName != _customDashPatternName)
            {
                float borderWidth = BorderWidth;
                for (int i = 0; i < dashPattern.Length; i++)
                    dashPattern[i] *= borderWidth;
            }

            BorderDashArray = dashPattern;
        }

        /// <summary>
        /// Returns the name of dash pattern.
        /// </summary>
        /// <param name="dashPattern">The dash pattern.</param>
        /// <param name="borderWidth">The border width</param>
        /// <returns>The name of dash pattern.</returns>
        private string GetDashPatternStyleName(float[] dashPattern, float borderWidth)
        {
            if (dashPattern != null && dashPattern.Length == 0)
                dashPattern = null;

            if (dashPattern != null && borderWidth != 1)
            {
                dashPattern = (float[])dashPattern.Clone();
                for (int i = 0; i < dashPattern.Length; i++)
                    dashPattern[i] /= borderWidth;
            }

            foreach (string name in _dashPatternStyles.Keys)
            {
                float[] currentDashPattern = _dashPatternStyles[name];

                if ((dashPattern == null && currentDashPattern != null) ||
                    (dashPattern != null && currentDashPattern == null))
                    continue;

                if (dashPattern == null && currentDashPattern == null)
                    return name;

                if (currentDashPattern.Length == dashPattern.Length)
                {
                    bool equal = true;
                    for (int i = 0; equal && i < dashPattern.Length; i++)
                        if (dashPattern[i] != currentDashPattern[i])
                            equal = false;

                    if (equal)
                        return name;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Raises the PropertyValueChanged event.
        /// </summary>
        private void OnPropertyValueChanged()
        {
            if (PropertyValueChanged != null)
                PropertyValueChanged(this, EventArgs.Empty);
        }

        #endregion

        #endregion



        #region Events

        /// <summary>
        /// Occurs when value of property is changed.
        /// </summary>
        public event EventHandler PropertyValueChanged;

        #endregion

    }
}
