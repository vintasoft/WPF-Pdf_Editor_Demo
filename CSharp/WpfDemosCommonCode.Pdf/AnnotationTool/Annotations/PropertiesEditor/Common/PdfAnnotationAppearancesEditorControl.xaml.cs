using System;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Annotations;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit properties of
    /// the <see cref="PdfAnnotationAppearances"/>.
    /// </summary>
    public partial class PdfAnnotationAppearancesEditorControl : UserControl
    {

        #region Nested Enum

        /// <summary>
        /// Determines available types of appearances.
        /// </summary>
        private enum AppearanceType
        {
            /// <summary>
            /// The normal type.
            /// </summary>
            Normal,

            /// <summary>
            /// The rollover type.
            /// </summary>
            Rollover,

            /// <summary>
            /// Down type.
            /// </summary>
            Down
        }

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of 
        /// the <see cref="PdfAnnotationAppearancesEditorControl"/> class.
        /// </summary>
        public PdfAnnotationAppearancesEditorControl()
        {
            InitializeComponent();

            foreach (AppearanceType type in Enum.GetValues(typeof(AppearanceType)))
                appearanceTypeComboBox.Items.Add(type);
            appearanceTypeComboBox.SelectedItem = AppearanceType.Normal;

            Annotation = null;
        }

        #endregion



        #region Properties

        PdfAnnotation _annotation = null;
        /// <summary>
        /// Gets or sets the annotation.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public PdfAnnotation Annotation
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

                    if (_annotation != null && _annotation.Appearances == null)
                        _annotation.Appearances = new PdfAnnotationAppearances(_annotation.Document);

                    UpdateAppearance();
                }
            }
        }

        /// <summary>
        /// Gets or sets the appearances.
        /// </summary>
        private PdfAnnotationAppearances Appearances
        {
            get
            {
                if (_annotation == null)
                    return null;

                return _annotation.Appearances;
            }
        }

        /// <summary>
        /// Gets or sets the selected appearance.
        /// </summary>
        private PdfFormXObjectResource SelectedAppearance
        {
            get
            {
                if (Appearances == null || appearanceStateNameComboBox.SelectedItem == null)
                    return null;

                PdfFormXObjectResourceDictionary resourceDictionary = null;
                AppearanceType appearanceType = (AppearanceType)appearanceTypeComboBox.SelectedItem;
                switch (appearanceType)
                {
                    case AppearanceType.Normal:
                        resourceDictionary = Appearances.NormalStates;
                        break;

                    case AppearanceType.Rollover:
                        resourceDictionary = Appearances.RolloverStates;
                        break;

                    case AppearanceType.Down:
                        resourceDictionary = Appearances.DownStates;
                        break;
                }

                string appearanceStateName = appearanceStateNameComboBox.SelectedItem.ToString();
                if (resourceDictionary == null || !resourceDictionary.Keys.Contains(appearanceStateName))
                    return null;
                else
                    return resourceDictionary[appearanceStateName];
            }
            set
            {
                if (Appearances == null || appearanceStateNameComboBox.SelectedItem == null)
                    return;

                PdfFormXObjectResourceDictionary resourceDictionary = null;
                AppearanceType appearanceType = (AppearanceType)appearanceTypeComboBox.SelectedItem;
                switch (appearanceType)
                {
                    case AppearanceType.Normal:
                        if (Appearances.NormalStates == null && value != null)
                            Appearances.NormalStates = new PdfFormXObjectResourceDictionary(Appearances.Document);
                        resourceDictionary = Appearances.NormalStates;
                        break;

                    case AppearanceType.Rollover:
                        if (Appearances.RolloverStates == null && value != null)
                            Appearances.RolloverStates = new PdfFormXObjectResourceDictionary(Appearances.Document);
                        resourceDictionary = Appearances.RolloverStates;
                        break;

                    case AppearanceType.Down:
                        if (Appearances.DownStates == null && value != null)
                            Appearances.DownStates = new PdfFormXObjectResourceDictionary(Appearances.Document);
                        resourceDictionary = Appearances.DownStates;
                        break;
                }

                if (resourceDictionary == null)
                    return;

                string appearanceStateName = appearanceStateNameComboBox.SelectedItem.ToString();
                resourceDictionary[appearanceStateName] = value;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the appearance.
        /// </summary>
        public void UpdateAppearance()
        {
            if (Appearances == null)
                return;

            string[] appearanceStateNames = new string[Appearances.NormalStates.Keys.Count];
            Appearances.NormalStates.Keys.CopyTo(appearanceStateNames, 0);

            string selectedItem = string.Empty;
            if (appearanceStateNameComboBox.SelectedItem != null)
                selectedItem = appearanceStateNameComboBox.SelectedItem.ToString();

            appearanceStateNameComboBox.BeginInit();
            appearanceStateNameComboBox.Items.Clear();
            foreach (string appearanceStateName in appearanceStateNames)
                appearanceStateNameComboBox.Items.Add(appearanceStateName);
            if (string.IsNullOrEmpty(selectedItem))
            {
                if (appearanceStateNames.Length > 0)
                    appearanceStateNameComboBox.SelectedItem = appearanceStateNames[0];
            }
            else
                appearanceStateNameComboBox.SelectedItem = selectedItem;
            appearanceStateNameComboBox.EndInit();

            pdfResourceViewerControl.Resource = SelectedAppearance;
        }

        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            AppearanceType appearanceType = (AppearanceType)appearanceTypeComboBox.SelectedItem;
            removeAppearanceButton.IsEnabled = appearanceType != AppearanceType.Normal && SelectedAppearance != null;
        }

        /// <summary>
        /// The name of appearance is changed.
        /// </summary>
        private void appearanceStateNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pdfResourceViewerControl.Resource = SelectedAppearance;

            UpdateUI();
        }

        /// <summary>
        /// Type of appearance is changed.
        /// </summary>
        private void appearanceTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pdfResourceViewerControl.Resource = SelectedAppearance;

            UpdateUI();
        }

        /// <summary>
        /// Changes the view of appearance.
        /// </summary>
        private void changeAppearanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (Annotation.AppearanceGenerator != null)
            {
                string message = "The annotation appearance is generated using the appearance generator." +
                    " Do you want to disable the appearance generator?";
                if (MessageBox.Show(message, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    Annotation.AppearanceGenerator = null;
                else
                    return;
            }

            PdfResourcesViewerWindow window = new PdfResourcesViewerWindow(Appearances.Document, true);
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = Window.GetWindow(this);

            if (window.ShowDialog() == true)
            {
                PdfFormXObjectResource resource = null;
                if (window.SelectedResource is PdfFormXObjectResource)
                    resource = (PdfFormXObjectResource)window.SelectedResource;
                else if (window.SelectedResource is PdfImageResource)
                {
                    PdfImageResource imageResource = (PdfImageResource)window.SelectedResource;
                    resource = new PdfFormXObjectResource(Annotation.Document, imageResource);
                }

                SelectedAppearance = resource;
                pdfResourceViewerControl.Resource = resource;

                UpdateUI();

                OnAppearanceChanged();
            }
        }

        /// <summary>
        /// Removes the view of appearance.
        /// </summary>
        private void removeAppearanceButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedAppearance = null;
            pdfResourceViewerControl.Resource = null;

            UpdateUI();

            OnAppearanceChanged();
        }

        /// <summary>
        /// Raises the AppearanceChanged event.
        /// </summary>
        private void OnAppearanceChanged()
        {
            if (AppearanceChanged != null)
                AppearanceChanged(this, EventArgs.Empty);
        }

        #endregion



        #region Events

        /// <summary>
        /// Occurs when appearance is changed.
        /// </summary>
        public event EventHandler AppearanceChanged;

        #endregion

    }
}
