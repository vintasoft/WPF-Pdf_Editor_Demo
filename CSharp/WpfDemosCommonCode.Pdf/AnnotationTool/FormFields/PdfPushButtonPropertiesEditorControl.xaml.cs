using System;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit properties of the <see cref="PdfInteractiveFormPushButtonField"/>.
    /// </summary>
    public partial class PdfPushButtonPropertiesEditorControl : UserControl, IPdfInteractiveFormPropertiesEditor
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfPushButtonPropertiesEditorControl"/> class.
        /// </summary>
        public PdfPushButtonPropertiesEditorControl()
        {
            InitializeComponent();

            foreach (PdfAnnotationHighlightingMode item in Enum.GetValues(typeof(PdfAnnotationHighlightingMode)))
                highlightingModeComboBox.Items.Add(item);

            foreach (PdfAnnotationCaptionIconRelation item in Enum.GetValues(typeof(PdfAnnotationCaptionIconRelation)))
                captionIconRelationComboBox.Items.Add(item);
        }

        #endregion



        #region Properties

        PdfInteractiveFormPushButtonField _field = null;
        /// <summary>
        /// Gets or sets the <see cref="PdfInteractiveFormPushButtonField"/>.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public PdfInteractiveFormPushButtonField Field
        {
            get
            {
                return _field;
            }
            set
            {
                _field = value;

                mainTabControl.IsEnabled = Field != null && Field.Annotation != null;
                if (mainTabControl.IsEnabled && AppearanceCharacteristics == null)
                    Field.Annotation.AppearanceCharacteristics = new PdfAnnotationAppearanceCharacteristics(Field.Document);

                UpdateFieldInfo();
            }
        }

        /// <summary>
        /// Gets or sets the PDF interactive form field.
        /// </summary>
        PdfInteractiveFormField IPdfInteractiveFormPropertiesEditor.Field
        {
            get
            {
                return Field;
            }
            set
            {
                Field = value as PdfInteractiveFormPushButtonField;
            }
        }

        /// <summary>
        /// Gets the appearance characteristics of annotation.
        /// </summary>
        private PdfAnnotationAppearanceCharacteristics AppearanceCharacteristics
        {
            get
            {
                return _field.Annotation.AppearanceCharacteristics;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Updates the field information.
        /// </summary>
        public void UpdateFieldInfo()
        {
            if (Field != null)
                UpdateMainTabContol();
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Index in main tab control is changed.
        /// </summary>
        private void mainTabControl_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMainTabContol();
        }

        /// <summary>
        /// Updates the main tab contol.
        /// </summary>
        private void UpdateMainTabContol()
        {
            if (mainTabControl.SelectedItem == valueTabPage)
            {
                highlightingModeComboBox.SelectedItem = Field.Annotation.HighlightingMode;
                captionIconRelationComboBox.SelectedItem = AppearanceCharacteristics.ButtonCaptionIconRelation;

                UpdateButtonStateTabControl();
            }
            else if (mainTabControl.SelectedItem == activateActionTabPage)
            {
                if (pdfActionEditorControl.Document == null)
                    pdfActionEditorControl.Document = Field.Document;
                pdfActionEditorControl.Action = Field.Annotation.ActivateAction;
            }
        }

        /// <summary>
        /// Activate action is changed.
        /// </summary>
        private void pdfActionEditorControl_ActionChanged(object sender, EventArgs e)
        {
            Field.Annotation.ActivateAction = pdfActionEditorControl.Action;
        }

        /// <summary>
        /// Highlighting mode is changed.
        /// </summary>
        private void highlightingModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PdfAnnotationHighlightingMode highlightingMode =
                (PdfAnnotationHighlightingMode)highlightingModeComboBox.SelectedItem;
            if (Field.Annotation.HighlightingMode != highlightingMode)
            {
                Field.Annotation.HighlightingMode = highlightingMode;

                if (highlightingMode == PdfAnnotationHighlightingMode.Push)
                {
                    if (!buttonStateTabControl.Items.Contains(rolloverStateTabPage))
                        buttonStateTabControl.Items.Add(rolloverStateTabPage);

                    if (!buttonStateTabControl.Items.Contains(downStateTabPage))
                        buttonStateTabControl.Items.Add(downStateTabPage);
                }
                else
                {
                    buttonStateTabControl.Items.Remove(rolloverStateTabPage);
                    buttonStateTabControl.Items.Remove(downStateTabPage);
                }

                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// Position of caption and icon is changed.
        /// </summary>
        private void captionIconRelationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PdfAnnotationCaptionIconRelation captionIconRelation =
                (PdfAnnotationCaptionIconRelation)captionIconRelationComboBox.SelectedItem;
            PdfAnnotationAppearanceCharacteristics appearanceCharacteristics =
                Field.Annotation.AppearanceCharacteristics;

            if (appearanceCharacteristics.ButtonCaptionIconRelation != captionIconRelation)
            {
                appearanceCharacteristics.ButtonCaptionIconRelation = captionIconRelation;

                normalCaptionTextBox.IsEnabled = true;
                normalIconChangeButton.IsEnabled = true;

                bool isIconEnabled = true;
                bool isCaptionEnabled = true;

                if (captionIconRelation == PdfAnnotationCaptionIconRelation.NoCaption)
                    isCaptionEnabled = false;
                else if (captionIconRelation == PdfAnnotationCaptionIconRelation.NoIcon)
                    isIconEnabled = false;

                normalCaptionTextBox.IsEnabled = isCaptionEnabled;
                rolloverCaptionTextBox.IsEnabled = isCaptionEnabled;
                downCaptionTextBox.IsEnabled = isCaptionEnabled;

                normalIconChangeButton.IsEnabled = isIconEnabled;
                rolloverIconChangeButton.IsEnabled = isIconEnabled;
                downIconChangeButton.IsEnabled = isIconEnabled;

                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// Index of button state is changed.
        /// </summary>
        private void buttonStateTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonStateTabControl();
        }

        /// <summary>
        /// Updates the button state tab control.
        /// </summary>
        private void UpdateButtonStateTabControl()
        {
            if (_field == null)
                return;

            TabItem selectedTab = (TabItem)buttonStateTabControl.SelectedItem;

            if (selectedTab == normalStateTabPage)
            {
                normalIconPdfResourceViewerControl.Resource = AppearanceCharacteristics.ButtonNormalIcon;
                normalCaptionTextBox.Text = AppearanceCharacteristics.ButtonNormalCaption;
            }
            else if (selectedTab == rolloverStateTabPage)
            {
                rolloverIconPdfResourceViewerControl.Resource = AppearanceCharacteristics.ButtonRolloverIcon;
                rolloverCaptionTextBox.Text = AppearanceCharacteristics.ButtonRolloverCaption;
            }
            else if (selectedTab == downStateTabPage)
            {
                downIconPdfResourceViewerControl.Resource = AppearanceCharacteristics.ButtonDownIcon;
                downCaptionTextBox.Text = AppearanceCharacteristics.ButtonDownCaption;
            }
        }

        /// <summary>
        /// Creates PdfFormXObjectResource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        private bool GetFormXObjectResource(out PdfFormXObjectResource resource)
        {
            resource = null;

            PdfResourcesViewerWindow window = new PdfResourcesViewerWindow(Field.Document, true);
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = Window.GetWindow(this);

            if (window.ShowDialog() == true)
            {
                if (window.SelectedResource is PdfFormXObjectResource)
                    resource = (PdfFormXObjectResource)window.SelectedResource;
                else if (window.SelectedResource is PdfImageResource)
                {
                    PdfImageResource imageResource = (PdfImageResource)window.SelectedResource;
                    resource = new PdfFormXObjectResource(Field.Document, imageResource);
                }
            }

            return resource != null;
        }

        /// <summary>
        /// Raises the PropertyValueChanged event.
        /// </summary>
        private void OnPropertyValueChanged()
        {
            if (PropertyValueChanged != null)
                PropertyValueChanged(this, EventArgs.Empty);
        }


        #region Normal State

        /// <summary>
        /// Caption of normal state is changed.
        /// </summary>
        private void normalCaptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AppearanceCharacteristics.ButtonNormalCaption != normalCaptionTextBox.Text)
            {
                AppearanceCharacteristics.ButtonNormalCaption = normalCaptionTextBox.Text;
                OnPropertyValueChanged();
            }
        }

        /// <summary>
        /// Icon of normal state is changed.
        /// </summary>
        private void normalIconChangeButton_Click(object sender, RoutedEventArgs e)
        {
            PdfFormXObjectResource resource = null;
            if (GetFormXObjectResource(out resource))
            {
                AppearanceCharacteristics.ButtonNormalIcon = resource;
                normalIconPdfResourceViewerControl.Resource = resource;
                OnPropertyValueChanged();
            }
        }

        #endregion


        #region Rollover State

        /// <summary>
        /// Caption of rollover state is changed.
        /// </summary>
        private void rolloverCaptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AppearanceCharacteristics.ButtonRolloverCaption = rolloverCaptionTextBox.Text;
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Icon of rollover state is changed.
        /// </summary>
        private void rolloverIconChangeButton_Click(object sender, RoutedEventArgs e)
        {
            PdfFormXObjectResource resource = null;
            if (GetFormXObjectResource(out resource))
            {
                AppearanceCharacteristics.ButtonRolloverIcon = resource;
                rolloverIconPdfResourceViewerControl.Resource = resource;
                OnPropertyValueChanged();
            }
        }

        #endregion


        #region Down State

        /// <summary>
        /// Caption of down state is changed.
        /// </summary>
        private void downCaptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AppearanceCharacteristics.ButtonDownCaption = downCaptionTextBox.Text;
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Icon of down state is changed.
        /// </summary>
        private void downIconChangeButton_Click(object sender, RoutedEventArgs e)
        {
            PdfFormXObjectResource resource = null;
            if (GetFormXObjectResource(out resource))
            {
                AppearanceCharacteristics.ButtonDownIcon = resource;
                downIconPdfResourceViewerControl.Resource = resource;
                OnPropertyValueChanged();
            }
        }

        #endregion

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
