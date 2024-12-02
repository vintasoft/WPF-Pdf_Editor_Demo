using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Drawing.GraphicsFigures;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Pdf.Tree.Annotations;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms.AppearanceGenerators;
using Vintasoft.Imaging.Pdf.Wpf.UI.Annotations;
using Vintasoft.Imaging.Wpf.UI;

using WpfDemosCommonCode.Imaging;
using WpfDemosCommonCode.Pdf.Security;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A panel that allows to build new PDF interactive form field and add the field to a PDF page.
    /// </summary>
    public partial class PdfInteractiveFormFieldBuilderControl : UserControl
    {

        #region Fields

        /// <summary>
        /// Dictionary: the tool strip item => the interactive form field type.
        /// </summary>
        Dictionary<FrameworkElement, InteractiveFormFieldType> _frameworkElementToInteractiveFormFieldType =
            new Dictionary<FrameworkElement, InteractiveFormFieldType>();

        /// <summary>
        /// Dictionary: the interactive form field type =>  the tool strip item.
        /// </summary>
        Dictionary<InteractiveFormFieldType, FrameworkElement> _interactiveFormFieldTypeToFrameworkElement =
            new Dictionary<InteractiveFormFieldType, FrameworkElement>();

        /// <summary>
        /// The interactive form field button, which is currently selected in the control.
        /// </summary>
        Button _selectedInteractiveFormFieldButton;

        /// <summary>
        /// The type of last built interactive form field.
        /// </summary>
        InteractiveFormFieldType _lastBuiltInteractiveFormFieldType = InteractiveFormFieldType.Unknown;

        /// <summary>
        /// The parent of the last built interactive form field.
        /// </summary>
        PdfInteractiveFormField _lastBuiltInteractiveFormFieldParent = null;

        /// <summary>
        /// Indicates that the focused index of image viewer is changing.
        /// </summary>
        bool _isFocusedIndexChanging = false;

        /// <summary>
        /// Indicates that the interaction mode is changing.
        /// </summary>
        bool _isInteractionModeChanging = false;

        /// <summary>
        /// Indicates that the annotation building must be continued after changing focus in viewer.
        /// </summary>
        bool _needContinueBuildAnnotationsAfterFocusedIndexChanged = false;

        /// <summary>
        /// The previous value of
        /// <see cref="Vintasoft.Imaging.Pdf.UI.Annotations.PdfAnnotationTool.AllowMultipleSelection"/>
        /// </summary>
        bool _allowMultipleSelectionPreviousValue = false;

        /// <summary>
        /// The mouse observer of visual tool.
        /// </summary>
        VisualToolMouseObserver _visualToolMouseObserver = new VisualToolMouseObserver();

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of
        /// the <see cref="PdfInteractiveFormFieldBuilderControl"/> class.
        /// </summary>
        public PdfInteractiveFormFieldBuilderControl()
        {
            AddImageResource("TextFieldImage", "TextField");
            AddImageResource("CheckBoxImage", "CheckBoxField");
            AddImageResource("ButtonImage", "ButtonField");
            AddImageResource("ListBoxImage", "ListBoxField");
            AddImageResource("ComboBoxImage", "ComboBoxField");
            AddImageResource("RadioButtonImage", "RadioButtonField");
            AddImageResource("BarcodeImage", "BarcodeField");
            AddImageResource("DigitalSignatureImage", "SignatureField");

            InitializeComponent();

            InitializeInteractionFormFieldButtons();

            AnnotationAppearanceGeneratorController = new PdfAnnotationAppearanceGeneratorController();

            textFieldNoBorderMenuItem.IsChecked = true;
            checkBoxStandardMenuItem.IsChecked = true;
            buttonBorder3DMenuItem.IsChecked = true;
            listBoxNoBorderMenuItem.IsChecked = true;
            comboBoxNoBorderMenuItem.IsChecked = true;
            radioButtonStandardMenuItem.IsChecked = true;
            barcodePdf417MenuItem.IsChecked = true;
            signatureDefaultMenuItem.IsChecked = true;
        }

        #endregion



        #region Properties

        WpfPdfAnnotationTool _annotationTool;
        /// <summary>
        /// Gets or sets the PDF annotation tool.
        /// </summary>
        [DefaultValue((object)null)]
        public WpfPdfAnnotationTool AnnotationTool
        {
            get
            {
                return _annotationTool;
            }
            set
            {
                if (_annotationTool != value)
                {
                    if (value != null && value.ImageViewer != null)
                        throw new InvalidOperationException("Annotation tool should be deactivated.");

                    if (_annotationTool != null)
                        UnsubscribeFromPdfAnnotationToolEvents(_annotationTool);
                    _annotationTool = value;
                    _visualToolMouseObserver.VisualTool = value;
                    if (_annotationTool != null)
                        SubscribeToPdfAnnotationToolEvents(_annotationTool);

                    UpdateUI();
                }
            }
        }

        /// <summary>
        /// Gets the current PDF document.
        /// </summary>
        public PdfDocument CurrentDocument
        {
            get
            {
                if (AnnotationTool == null)
                    return null;
                if (AnnotationTool.FocusedPage == null)
                    return null;
                return AnnotationTool.FocusedPage.Document;
            }
        }

        /// <summary>
        /// Gets or sets the signature appearance graphics figure.
        /// </summary>
        [DefaultValue((object)null)]
        [Browsable(false)]
        public SignatureAppearanceGraphicsFigure SignatureAppearance
        {
            get
            {
                SignatureAppearanceGraphicsFigure figure =
                    ((SignatureAppearanceGenerator)_annotationAppearanceGeneratorController[signatureDefaultMenuItem]).SignatureAppearance;
                return figure;
            }
            set
            {
                ((SignatureAppearanceGenerator)_annotationAppearanceGeneratorController[signatureDefaultMenuItem]).SignatureAppearance = value;
            }
        }

        PdfAnnotationAppearanceGeneratorController _annotationAppearanceGeneratorController = null;
        /// <summary>
        /// Gets or sets the annotation appearance generator controller.
        /// </summary>
        [Browsable(false)]
        public PdfAnnotationAppearanceGeneratorController AnnotationAppearanceGeneratorController
        {
            get
            {
                return _annotationAppearanceGeneratorController;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                if (_annotationAppearanceGeneratorController == value)
                    return;

                _annotationAppearanceGeneratorController = value;
                InitAnnotationAppearanceGeneratorController(_annotationAppearanceGeneratorController);
            }
        }

        bool _addRadioButtonToFocusedGroup = true;
        /// <summary>
        /// Gets or sets a value indicating whether a radio button must be add to
        /// the focused radio buttons group.
        /// </summary>
        public bool AddRadioButtonToFocusedGroup
        {
            get
            {
                return _addRadioButtonToFocusedGroup;
            }
            set
            {
                _addRadioButtonToFocusedGroup = value;
            }
        }

        bool _needBuildFormFieldsContinuously = false;
        /// <summary>
        /// Gets a value indicating whether the interactive form fields must be built continuously.
        /// </summary>
        public bool NeedBuildFormFieldsContinuously
        {
            get
            {
                return _needBuildFormFieldsContinuously;
            }
            set
            {
                _needBuildFormFieldsContinuously = value;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Updates the user interface of this control.
        /// </summary>
        public void UpdateUI()
        {
            mainMenu.IsEnabled =
                _annotationTool != null &&
                _annotationTool.ImageViewer != null &&
                _annotationTool.ImageViewer.Image != null &&
                PdfDocumentController.GetPageAssociatedWithImage(_annotationTool.ImageViewer.Image) != null;
        }

        /// <summary>
        /// Shows the signature verify/sing dialog.
        /// </summary>
        /// <param name="signatureView">The signature view.</param>
        public void ShowSignatureDialog(WpfPdfSignatureWidgetAnnotationView signatureView)
        {
            // if signature is empty
            if (signatureView.Field.SignatureInfo == null)
            {
                // sign document

                SignatureAppearanceGenerator generator = signatureView.Annotation.AppearanceGenerator as SignatureAppearanceGenerator;
                SignatureAppearanceGraphicsFigure signatureAppearanceGraphicsFigure;
                if (generator == null)
                    signatureAppearanceGraphicsFigure = SignatureAppearance;
                else
                    signatureAppearanceGraphicsFigure = generator.SignatureAppearance;

                // check pages of PDF document
                if (PdfDemosTools.CheckAllPagesFromDocument(this.AnnotationTool.ImageViewer.Images, signatureView.Field.Document))
                {
                    if (CreateSignatureFieldWithAppearance.ShowDialog(signatureView.Field, signatureAppearanceGraphicsFigure))
                    {
                        signatureView.Invalidate();
                    }
                }
            }
            // if signature is NOT empty
            else
            {
                // verify signature

                DocumentSignaturesWindow window = new DocumentSignaturesWindow(signatureView.Field.Document);
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Owner = Window.GetWindow(this);
                window.SelectedSignatureField = (PdfInteractiveFormSignatureField)signatureView.Field;
                window.ShowDialog();
            }
        }

        /// <summary>
        /// Adds and builds the interactive form field.
        /// </summary>
        /// <param name="interactiveFormFieldType">The interactive form field type.</param>
        public void AddAndBuildInteractiveFormField(InteractiveFormFieldType interactiveFormFieldType)
        {
            AddAndBuildInteractiveFormField(interactiveFormFieldType, null);
        }

        /// <summary>
        /// Adds and builds the interactive form field.
        /// </summary>
        /// <param name="interactiveFormFieldType">The interactive form field type.</param>
        /// <param name="parentInteractiveFormField">The parent field.</param>
        public void AddAndBuildInteractiveFormField(
            InteractiveFormFieldType interactiveFormFieldType,
            PdfInteractiveFormField parentInteractiveFormField)
        {
            try
            {
                SetSelectedInteractiveFormFieldButton(interactiveFormFieldType);

                _lastBuiltInteractiveFormFieldType = interactiveFormFieldType;
                _lastBuiltInteractiveFormFieldParent = parentInteractiveFormField;

                if (interactiveFormFieldType != InteractiveFormFieldType.Unknown)
                {
                    if (AnnotationTool.InteractionMode != WpfPdfAnnotationInteractionMode.Edit)
                    {
                        _isInteractionModeChanging = true;
                        AnnotationTool.InteractionMode = WpfPdfAnnotationInteractionMode.Edit;
                        _isInteractionModeChanging = false;
                    }

                    switch (interactiveFormFieldType)
                    {
                        case InteractiveFormFieldType.TextField:
                            AddAndBuildTextField(parentInteractiveFormField);
                            break;

                        case InteractiveFormFieldType.CheckBox:
                            AddAndBuildCheckBoxField(parentInteractiveFormField);
                            break;

                        case InteractiveFormFieldType.PushButton:
                            AddAndBuildButtonField(parentInteractiveFormField);
                            break;

                        case InteractiveFormFieldType.ListBox:
                            AddAndBuildListBoxField(parentInteractiveFormField);
                            break;

                        case InteractiveFormFieldType.ComboBox:
                            AddAndBuildComboBoxField(parentInteractiveFormField);
                            break;

                        case InteractiveFormFieldType.RadioButton:
                            AddAndBuildRadioButtonField(parentInteractiveFormField);
                            break;

                        case InteractiveFormFieldType.BarcodeField:
                            AddAndBuildBarcodeField(parentInteractiveFormField);
                            break;

                        case InteractiveFormFieldType.VintasoftBarcodeField:
                            AddAndBuildVintasoftBarcodeField(parentInteractiveFormField);
                            break;

                        case InteractiveFormFieldType.SignatureField:
                            AddAndBuildSignatureField(parentInteractiveFormField);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                DemosTools.ShowErrorMessage(e.Message);
            }
        }

        #endregion


        #region PRIVATE

        #region Image viewer

        /// <summary>
        /// Subscribes to the image viewer events.
        /// </summary>
        /// <param name="imageViewer">The image viewer.</param>
        private void SubscribeToImageViewerEvents(WpfImageViewer imageViewer)
        {
            imageViewer.FocusedIndexChanged += new PropertyChangedEventHandler<int>(imageViewer_FocusedIndexChanged);
            imageViewer.VisualToolChanging += new PropertyChangingEventHandler<Vintasoft.Imaging.Wpf.UI.VisualTools.WpfVisualTool>(imageViewer_VisualToolChanging);
        }

        /// <summary>
        /// Unsubscribes from the image viewer events.
        /// </summary>
        /// <param name="imageViewer">The image viewer.</param>
        private void UnsubscribeFromImageViewerEvents(WpfImageViewer imageViewer)
        {
            imageViewer.FocusedIndexChanged -= new PropertyChangedEventHandler<int>(imageViewer_FocusedIndexChanged);
            imageViewer.VisualToolChanging -= new PropertyChangingEventHandler<Vintasoft.Imaging.Wpf.UI.VisualTools.WpfVisualTool>(imageViewer_VisualToolChanging);
        }

        /// <summary>
        /// Index, of focused image in viewer, is changing.
        /// </summary>
        private void imageViewer_FocusedIndexChanging(object sender, Vintasoft.Imaging.PropertyChangedEventArgs<int> e)
        {
            _isFocusedIndexChanging = true;

            if (AnnotationTool != null && NeedBuildFormFieldsContinuously)
            {
                // if focused annotation view is not empty
                if (AnnotationTool.FocusedAnnotationView != null &&
                    IsPdfWidgetAnnotation(AnnotationTool.FocusedAnnotationView))
                {
                    _needContinueBuildAnnotationsAfterFocusedIndexChanged = true;

                    // cancel building annotation
                    AnnotationTool.CancelBuilding();
                }
            }
        }

        /// <summary>
        /// Index, of focused image in viewer, is changed.
        /// </summary>
        private void imageViewer_FocusedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            _isFocusedIndexChanging = false;

            UpdateUI();

            if (_needContinueBuildAnnotationsAfterFocusedIndexChanged &&
                AnnotationTool.ImageViewer.FocusedIndex != -1)
            {
                AddAndBuildInteractiveFormField(
                    _lastBuiltInteractiveFormFieldType,
                    _lastBuiltInteractiveFormFieldParent);
            }

            _needContinueBuildAnnotationsAfterFocusedIndexChanged = false;
        }

        /// <summary>
        /// Visual tool in image viewer is changing.
        /// </summary>
        private void imageViewer_VisualToolChanging(
            object sender,
            Vintasoft.Imaging.PropertyChangingEventArgs<Vintasoft.Imaging.Wpf.UI.VisualTools.WpfVisualTool> e)
        {
            if (AnnotationTool != null)
                AnnotationTool.CancelBuilding();
        }

        #endregion


        #region PDF annotation tool

        /// <summary>
        /// Subscribes to the PDF annotation tool events.
        /// </summary>
        /// <param name="annotationTool">The PDF annotation tool.</param>
        private void SubscribeToPdfAnnotationToolEvents(WpfPdfAnnotationTool annotationTool)
        {
            annotationTool.Activating += new EventHandler(pdfAnnotationTool_Activating);
            annotationTool.Activated += new EventHandler(pdfAnnotationTool_Activated);
            annotationTool.Deactivating += new EventHandler(pdfAnnotationTool_Deactivating);
            annotationTool.Deactivated += new EventHandler(pdfAnnotationTool_Deactivated);
            annotationTool.BuildingFinished += new EventHandler<WpfPdfAnnotationViewEventArgs>(pdfAnnotationTool_BuildingFinished);
            annotationTool.BuildingCanceled += new EventHandler<WpfPdfAnnotationViewEventArgs>(pdfAnnotationTool_BuildingCanceled);
        }

        /// <summary>
        /// Unsubscribes from the PDF annotation tool events.
        /// </summary>
        /// <param name="annotationTool">The PDF annotation tool.</param>
        private void UnsubscribeFromPdfAnnotationToolEvents(WpfPdfAnnotationTool annotationTool)
        {
            annotationTool.Activating -= new EventHandler(pdfAnnotationTool_Activating);
            annotationTool.Activated -= new EventHandler(pdfAnnotationTool_Activated);
            annotationTool.Deactivating -= new EventHandler(pdfAnnotationTool_Deactivating);
            annotationTool.Deactivated -= new EventHandler(pdfAnnotationTool_Deactivated);
            annotationTool.BuildingFinished -= new EventHandler<WpfPdfAnnotationViewEventArgs>(pdfAnnotationTool_BuildingFinished);
            annotationTool.BuildingCanceled -= new EventHandler<WpfPdfAnnotationViewEventArgs>(pdfAnnotationTool_BuildingCanceled);
        }

        /// <summary>
        /// PDF annotation tool is activating.
        /// </summary>
        private void pdfAnnotationTool_Activating(object sender, EventArgs e)
        {
            AnnotationTool.ImageViewer.FocusedIndexChanging += new Vintasoft.Imaging.PropertyChangedEventHandler<int>(imageViewer_FocusedIndexChanging);
        }

        /// <summary>
        /// PDF annotation tool is activated.
        /// </summary>
        private void pdfAnnotationTool_Activated(object sender, EventArgs e)
        {
            SubscribeToImageViewerEvents(AnnotationTool.ImageViewer);

            UpdateUI();
        }

        /// <summary>
        /// PDF annotation tool is deactivating.
        /// </summary>
        private void pdfAnnotationTool_Deactivating(object sender, EventArgs e)
        {
            AnnotationTool.ImageViewer.FocusedIndexChanging -= new Vintasoft.Imaging.PropertyChangedEventHandler<int>(imageViewer_FocusedIndexChanging);
        }

        /// <summary>
        /// PDF annotation tool is deactivated.
        /// </summary>
        private void pdfAnnotationTool_Deactivated(object sender, EventArgs e)
        {
            UnsubscribeFromImageViewerEvents(AnnotationTool.ImageViewer);

            mainMenu.IsEnabled = false;
        }

        /// <summary>
        /// PDF annotation building is finished.
        /// </summary>
        private void pdfAnnotationTool_BuildingFinished(object sender, WpfPdfAnnotationViewEventArgs e)
        {
            if (!AnnotationTool.AnnotationCollection.Contains(e.AnnotationView.Annotation))
                return;

            if (e.AnnotationView is WpfPdfSignatureWidgetAnnotationView)
            {
                ShowSignatureDialog((WpfPdfSignatureWidgetAnnotationView)e.AnnotationView);
            }
            else if (e.AnnotationView is WpfPdfPushButtonWidgetAnnotationView)
            {
                PdfAction activateAction = CreatePdfActionWindow.CreateAction(e.AnnotationView.Annotation.Document, Window.GetWindow(this));
                if (activateAction != null)
                {
                    if (PdfActionsEditorTool.EditAction(activateAction, AnnotationTool.ImageViewer.Images, Window.GetWindow(this)))
                        e.AnnotationView.Annotation.ActivateAction = activateAction;
                }
            }

            if (IsPdfWidgetAnnotation(e.AnnotationView))
            {
                if (NeedBuildFormFieldsContinuously)
                {
                    AddAndBuildInteractiveFormField(
                        _lastBuiltInteractiveFormFieldType,
                        _lastBuiltInteractiveFormFieldParent);
                }
                else
                {
                    EndBuilding();
                }
            }
        }

        /// <summary>
        /// PDF annotation building is canceled.
        /// </summary>
        private void pdfAnnotationTool_BuildingCanceled(object sender, WpfPdfAnnotationViewEventArgs e)
        {
            if (!_isFocusedIndexChanging &&
                !_isInteractionModeChanging)
                EndBuilding();
        }

        #endregion


        #region Interactive form field buttons

        /// <summary>
        /// Initializes the interactive form field buttons.
        /// </summary>
        private void InitializeInteractionFormFieldButtons()
        {
            InitializeInteractiveFormFieldButton(textFieldButton, "TextFieldImage", InteractiveFormFieldType.TextField);
            InitializeInteractiveFormFieldButton(textFieldNoBorderMenuItem, null, InteractiveFormFieldType.TextField);
            InitializeInteractiveFormFieldButton(textFieldSingleBorderMenuItem, null, InteractiveFormFieldType.TextField);
            InitializeInteractiveFormFieldButton(textField3DBorderMenuItem, null, InteractiveFormFieldType.TextField);


            InitializeInteractiveFormFieldButton(checkBoxButton, "CheckBoxImage", InteractiveFormFieldType.CheckBox);
            InitializeInteractiveFormFieldButton(checkBoxStandardMenuItem, null, InteractiveFormFieldType.CheckBox);
            InitializeInteractiveFormFieldButton(checkBoxSimpleMenuItem, null, InteractiveFormFieldType.CheckBox);
            InitializeInteractiveFormFieldButton(checkBoxSymbolVMenuItem, null, InteractiveFormFieldType.CheckBox);
            InitializeInteractiveFormFieldButton(checkBoxSymbolXMenuItem, null, InteractiveFormFieldType.CheckBox);


            InitializeInteractiveFormFieldButton(buttonButton, "ButtonImage", InteractiveFormFieldType.PushButton);
            InitializeInteractiveFormFieldButton(buttonFlatBorderMenuItem, null, InteractiveFormFieldType.PushButton);
            InitializeInteractiveFormFieldButton(buttonBorder3DMenuItem, null, InteractiveFormFieldType.PushButton);


            InitializeInteractiveFormFieldButton(listBoxButton, "ListBoxImage", InteractiveFormFieldType.ListBox);
            InitializeInteractiveFormFieldButton(listBoxNoBorderMenuItem, null, InteractiveFormFieldType.ListBox);


            InitializeInteractiveFormFieldButton(comboBoxButton, "ComboBoxImage", InteractiveFormFieldType.ComboBox);
            InitializeInteractiveFormFieldButton(comboBoxNoBorderMenuItem, null, InteractiveFormFieldType.ComboBox);
            InitializeInteractiveFormFieldButton(comboBoxSingleBorderMenuItem, null, InteractiveFormFieldType.ComboBox);
            InitializeInteractiveFormFieldButton(comboBox3dBorderMenuItem, null, InteractiveFormFieldType.ComboBox);


            InitializeInteractiveFormFieldButton(radioButtonButton, "RadioButtonImage", InteractiveFormFieldType.RadioButton);
            InitializeInteractiveFormFieldButton(radioButtonStandardMenuItem, null, InteractiveFormFieldType.RadioButton);
            InitializeInteractiveFormFieldButton(radioButtonSymbolMenuItem, null, InteractiveFormFieldType.RadioButton);


            InitializeInteractiveFormFieldButton(barcodeButton, "BarcodeImage", InteractiveFormFieldType.BarcodeField);
            InitializeInteractiveFormFieldButton(barcodePdf417MenuItem, null, InteractiveFormFieldType.BarcodeField);
            InitializeInteractiveFormFieldButton(barcodeDataMatrixMenuItem, null, InteractiveFormFieldType.BarcodeField);
            InitializeInteractiveFormFieldButton(barcodeQrCodeMenuItem, null, InteractiveFormFieldType.BarcodeField);
            InitializeInteractiveFormFieldButton(vintasoftBarcodeMenuItem, null, InteractiveFormFieldType.BarcodeField);


            InitializeInteractiveFormFieldButton(signatureButton, "DigitalSignatureImage", InteractiveFormFieldType.SignatureField);
            InitializeInteractiveFormFieldButton(signatureDefaultMenuItem, null, InteractiveFormFieldType.SignatureField);
        }

        /// <summary>
        /// Initialize the interactive form field button.
        /// </summary>
        /// <param name="interactiveFormFieldButton">A button,
        /// which must be clicked for starting of interactive form field building.</param>
        /// <param name="interactiveFormFieldImageResourceName">Name of the resource,
        /// which stores image for interactive form field button.</param>
        /// <param name="interactiveFormFieldType">The interactive form field type,
        /// which must be built when button is clicked.</param>
        private void InitializeInteractiveFormFieldButton(
            FrameworkElement interactiveFormFieldButton,
            string interactiveFormFieldImageResourceName,
            InteractiveFormFieldType interactiveFormFieldType)
        {
            MenuItem menuItem = interactiveFormFieldButton as MenuItem;
            if (menuItem != null && !string.IsNullOrEmpty(interactiveFormFieldImageResourceName))
            {
                StackPanel panel = (StackPanel)menuItem.Header;
                Button button = (Button)panel.Children[0];
                button.Tag = Resources[interactiveFormFieldImageResourceName];
            }

            _frameworkElementToInteractiveFormFieldType[interactiveFormFieldButton] = interactiveFormFieldType;
            _interactiveFormFieldTypeToFrameworkElement[interactiveFormFieldType] = interactiveFormFieldButton;
        }


        /// <summary>
        /// "Build interaction form field" menu item is clicked.
        /// </summary>
        private void addAndBuildInteractiveFormFieldMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;

            if (element is Button && element.Parent is StackPanel)
            {
                while (!(element is MenuItem))
                    element = (FrameworkElement)element.Parent;
            }

            // get new building interactive form field type
            InteractiveFormFieldType interactiveFormFieldType =
                _frameworkElementToInteractiveFormFieldType[element];

            if (sender is Button)
            {
                Button checkedButton = (Button)sender;

                if (checkedButton.BorderBrush == DemosTools.SELECTED_CONTROL_BRUSH)
                {
                    interactiveFormFieldType = InteractiveFormFieldType.Unknown;
                }
            }
            else
            {
                MenuItem menuItem = sender as MenuItem;

                if (menuItem != null)
                    menuItem.IsChecked = true;
            }

            // cancel current building
            _annotationTool.CancelBuilding();

            // add and build annotation
            AddAndBuildInteractiveFormField(interactiveFormFieldType);

            if (element is MenuItem)
                ((MenuItem)element).IsSubmenuOpen = false;
            element.Focus();
            AnnotationTool.ImageViewer.Focus();
        }

        /// <summary>
        /// Check state, in element of appearance menu, is changed.
        /// </summary>
        private void appearanceMenuItem_CheckedChanged(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            if (item.IsChecked)
            {
                _annotationAppearanceGeneratorController[item.Parent] = _annotationAppearanceGeneratorController[item];
                foreach (Object ownerItem in ((MenuItem)item.Parent).Items)
                {
                    if (ownerItem is MenuItem && ownerItem != item)
                        ((MenuItem)ownerItem).IsChecked = false;
                }
            }
        }

        /// <summary>
        /// "Properties" menu of appearance generator is selected.
        /// </summary>
        private void fieldAppearanceGeneratorPropertiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            PdfAnnotationAppearanceGenerator appearanceGenerator = _annotationAppearanceGeneratorController[item.Parent];
            PropertyGridWindow properties = new PropertyGridWindow(appearanceGenerator, "Apperanace Generator Properties");
            properties.ShowDialog();

            if (_lastBuiltInteractiveFormFieldType != InteractiveFormFieldType.Unknown)
            {
                InteractiveFormFieldType interactiveFormFieldType = _lastBuiltInteractiveFormFieldType;
                PdfInteractiveFormField interactiveFormFieldParent = _lastBuiltInteractiveFormFieldParent;

                AnnotationTool.CancelBuilding();
                AddAndBuildInteractiveFormField(interactiveFormFieldType, interactiveFormFieldParent);
            }
        }

        /// <summary>
        /// Sets the selected interactive form field button.
        /// </summary>
        /// <param name="interactiveFormFieldType">The type of selected interactive form field.</param>
        private void SetSelectedInteractiveFormFieldButton(InteractiveFormFieldType interactiveFormFieldType)
        {
            Button checkedButton = null;

            if (interactiveFormFieldType != InteractiveFormFieldType.Unknown)
            {
                // get the button
                FrameworkElement element =
                    _interactiveFormFieldTypeToFrameworkElement[interactiveFormFieldType];

                // get button
                checkedButton = GetButton(element);
            }

            // if button is selected
            if (checkedButton == _selectedInteractiveFormFieldButton)
                return;

            // uncheck current button
            if (_selectedInteractiveFormFieldButton != null)
            {
                _selectedInteractiveFormFieldButton.BorderBrush = DemosTools.UNSELECTED_CONTROL_BRUSH;

                AnnotationTool.AllowMultipleSelection = _allowMultipleSelectionPreviousValue;
            }

            // save reference to current button
            _selectedInteractiveFormFieldButton = checkedButton;

            // check specified button
            if (checkedButton != null)
            {
                checkedButton.BorderBrush = DemosTools.SELECTED_CONTROL_BRUSH;

                _allowMultipleSelectionPreviousValue = AnnotationTool.AllowMultipleSelection;
                AnnotationTool.AllowMultipleSelection = false;
            }
        }

        /// <summary>
        /// Return the <see cref="CheckedToolStripSplitButton"/>.
        /// </summary>
        /// <param name="element">The element.</param>
        private Button GetButton(FrameworkElement element)
        {
            if (element == null)
                return null;

            MenuItem menuItem = element as MenuItem;

            if (menuItem != null)
            {
                if (menuItem.Header is StackPanel)
                {
                    StackPanel stackPanel = (StackPanel)menuItem.Header;

                    foreach (UIElement uiElement in stackPanel.Children)
                    {
                        if (uiElement is Button)
                            return (Button)uiElement;
                    }
                }
            }

            if (menuItem.Parent is FrameworkElement)
            {
                Button button = GetButton((FrameworkElement)menuItem.Parent);
                if (button != null)
                    return button;
            }

            return null;
        }

        /// <summary>
        /// Ends the annotation building.
        /// </summary>
        private void EndBuilding()
        {
            _lastBuiltInteractiveFormFieldType = InteractiveFormFieldType.Unknown;
            _lastBuiltInteractiveFormFieldParent = null;
            SetSelectedInteractiveFormFieldButton(InteractiveFormFieldType.Unknown);

            int count = AnnotationTool.AnnotationViewCollection.Count;
            if (NeedBuildFormFieldsContinuously && count > 0)
            {
                WpfPdfAnnotationView view = AnnotationTool.AnnotationViewCollection[count - 1];

                AnnotationTool.FocusedAnnotationView = view;
            }
        }


        #region Add And Build...

        /// <summary>
        /// Adds and builds a text field.
        /// </summary>
        /// <param name="parentField">The parent field.</param>
        private void AddAndBuildTextField(PdfInteractiveFormField parentField)
        {
            PdfInteractiveFormTextField field = new PdfInteractiveFormTextField(
                CurrentDocument,
                GetFieldName(parentField, "TextField{0}"),
                GetNewFieldRectangle(textFieldButton));

            AddAndBuildTerminalField(field, GetAppearanceGenerator(textFieldButton), parentField);
        }

        /// <summary>
        /// Adds and builds a check box field.
        /// </summary>
        /// <param name="parentField">The parent field.</param>
        private void AddAndBuildCheckBoxField(PdfInteractiveFormField parentField)
        {
            PdfInteractiveFormCheckBoxField field = new PdfInteractiveFormCheckBoxField(
                CurrentDocument,
                GetFieldName(parentField, "CheckBox{0}"),
                GetNewFieldRectangle(checkBoxButton));

            // set value and default value
            field.Value = "Off";
            field.DefaultValue = field.Value;

            AddAndBuildTerminalField(field, GetAppearanceGenerator(checkBoxButton), parentField);
        }

        /// <summary>
        /// Adds and builds a button field.
        /// </summary>
        /// <param name="parentField">The parent field.</param>
        private void AddAndBuildButtonField(PdfInteractiveFormField parentField)
        {
            PdfInteractiveFormPushButtonField field = new PdfInteractiveFormPushButtonField(
                CurrentDocument,
                GetFieldName(parentField, "Button{0}"),
                GetNewFieldRectangle(buttonButton));

            AddAndBuildTerminalField(field, GetAppearanceGenerator(buttonButton), parentField);
        }

        /// <summary>
        /// Adds and builds a list box field.
        /// </summary>
        /// <param name="parentField">The parent field.</param>
        private void AddAndBuildListBoxField(PdfInteractiveFormField parentField)
        {
            PdfInteractiveFormListBoxField field = new PdfInteractiveFormListBoxField(
                CurrentDocument,
                GetFieldName(parentField, "ListBox{0}"),
                GetNewFieldRectangle(listBoxButton));

            AddAndBuildTerminalField(field, GetAppearanceGenerator(listBoxButton), parentField);
        }

        /// <summary>
        /// Adds and builds a combo box field.
        /// </summary>
        /// <param name="parentField">The parent field.</param>
        private void AddAndBuildComboBoxField(PdfInteractiveFormField parentField)
        {
            PdfInteractiveFormComboBoxField field = new PdfInteractiveFormComboBoxField(
                CurrentDocument,
                GetFieldName(parentField, "ComboBox{0}"),
                GetNewFieldRectangle(comboBoxButton));
            field.IsEdit = true;

            AddAndBuildTerminalField(field, GetAppearanceGenerator(comboBoxButton), parentField);
        }

        /// <summary>
        /// Adds and builds a radio button field.
        /// </summary>
        /// <param name="parentField">The parent field.</param>
        private void AddAndBuildRadioButtonField(PdfInteractiveFormField parentField)
        {
            PdfDocument document = CurrentDocument;
            PdfInteractiveFormRadioButtonGroupField parentRadioGroup = null;
            if (AddRadioButtonToFocusedGroup)
                parentRadioGroup = parentField as PdfInteractiveFormRadioButtonGroupField;

            if (parentRadioGroup == null)
            {
                WpfPdfAnnotationView view = AnnotationTool.FocusedAnnotationView;
                if (AddRadioButtonToFocusedGroup && view != null && view.Figure is PdfWidgetAnnotationGraphicsFigure)
                {
                    PdfInteractiveFormRadioButtonField radioButtonField =
                        ((PdfWidgetAnnotationGraphicsFigure)view.Figure).Field as PdfInteractiveFormRadioButtonField;
                    if (radioButtonField != null)
                    {
                        parentRadioGroup = radioButtonField.Parent as PdfInteractiveFormRadioButtonGroupField;
                        parentField = parentRadioGroup;
                    }
                }

                if (parentRadioGroup == null)
                {
                    parentRadioGroup = new PdfInteractiveFormRadioButtonGroupField(
                       CurrentDocument, GetFieldName(parentField, "RadioGroup{0}"));
                    if (parentField != null)
                    {
                        parentField.Kids.Add(parentRadioGroup);
                    }
                    else
                    {
                        if (document.InteractiveForm == null)
                            document.InteractiveForm = new PdfDocumentInteractiveForm(document);
                        document.InteractiveForm.Fields.Add(parentRadioGroup);
                    }
                    parentField = parentRadioGroup;
                }
            }

            PdfInteractiveFormRadioButtonField field = new PdfInteractiveFormRadioButtonField(
                document,
                GetNewFieldRectangle(radioButtonButton));

            field.ButtonValue = GetFreeName(parentRadioGroup.GetButtonValues(), "Value{0}");

            AddAndBuildTerminalField(field, GetAppearanceGenerator(radioButtonButton), parentField);
        }

        /// <summary>
        /// Adds and builds a barcode field.
        /// </summary>
        /// <param name="parentField">The parent field.</param>
        private void AddAndBuildBarcodeField(PdfInteractiveFormField parentField)
        {
            if (GetAppearanceGenerator(barcodeButton) is PdfVintasoftBarcodeFieldAppearanceGenerator)
            {
                AddAndBuildVintasoftBarcodeField(parentField);
                return;
            }

            PdfBarcodeFieldAppearanceGenerator generator = (PdfBarcodeFieldAppearanceGenerator)GetAppearanceGenerator(barcodeButton);

            PdfInteractiveFormBarcodeField field = new PdfInteractiveFormBarcodeField(
                CurrentDocument,
                GetFieldName(parentField, "Barcode{0}"),
                generator.BarcodeSymbology,
                GetNewFieldRectangle(barcodeButton));
            field.TextValue = "Barcode value";

            AddAndBuildTerminalField(field, generator, parentField);
        }

        /// <summary>
        /// Adds and builds a Vintasoft barcode field.
        /// </summary>
        /// <param name="parentField">The parent field.</param>
        private void AddAndBuildVintasoftBarcodeField(PdfInteractiveFormField parentField)
        {
            PdfVintasoftBarcodeFieldAppearanceGenerator generator = (PdfVintasoftBarcodeFieldAppearanceGenerator)GetAppearanceGenerator(vintasoftBarcodeMenuItem);

            PdfInteractiveFormVintasoftBarcodeField field = new PdfInteractiveFormVintasoftBarcodeField(
                CurrentDocument,
                GetFieldName(parentField, "Barcode{0}"),
                generator.BarcodeSymbology,
                GetNewFieldRectangle(barcodeButton));
            field.TextValue = "123456789";

            AddAndBuildTerminalField(field, generator, parentField);
        }

        /// <summary>
        /// Adds and builds a signature field.
        /// </summary>
        /// <param name="parentField">The parent field.</param>
        private void AddAndBuildSignatureField(PdfInteractiveFormField parentField)
        {
            SignatureAppearanceGenerator generator = (SignatureAppearanceGenerator)GetAppearanceGenerator(signatureButton);
            generator.SignatureAppearance.Text = "Empty Signatrue Field";

            PdfInteractiveFormSignatureField field = new PdfInteractiveFormSignatureField(
                CurrentDocument,
                GetFieldName(parentField, "Signature{0}"),
                GetNewFieldRectangle(signatureButton));

            AddAndBuildTerminalField(field, generator, parentField);
        }


        /// <summary>
        /// Adds and builds a terminal field.
        /// </summary>
        /// <param name="field">The PDF interactive form field.</param>
        /// <param name="appearanceGenerator">The PDF annotation appearance generator.</param>
        /// <param name="parentField">The parent PDF interactive form field.</param>
        private void AddAndBuildTerminalField(
            PdfInteractiveFormField field,
            PdfAnnotationAppearanceGenerator appearanceGenerator,
            PdfInteractiveFormField parentField)
        {
            // update field appearance
            if (appearanceGenerator != null)
                field.Annotation.AppearanceGenerator = appearanceGenerator.Clone();
            field.Annotation.UpdateAppearance();

            // add and build the widget annotation of form field
            AnnotationTool.AddAndBuildFormField(field, parentField);
        }

        #endregion

        #endregion


        /// <summary>
        /// Initializes the controller of PDF annotation appearance generators.
        /// </summary>
        /// <param name="controller">The controller.</param>
        private void InitAnnotationAppearanceGeneratorController(
            PdfAnnotationAppearanceGeneratorController controller)
        {
            if (ImagingEnvironment.IsInDesignMode)
                return;

            textFieldNoBorderMenuItem.IsChecked = false;
            checkBoxStandardMenuItem.IsChecked = false;
            buttonBorder3DMenuItem.IsChecked = false;
            listBoxNoBorderMenuItem.IsChecked = false;
            comboBoxNoBorderMenuItem.IsChecked = false;
            radioButtonStandardMenuItem.IsChecked = false;
            barcodePdf417MenuItem.IsChecked = false;
            signatureDefaultMenuItem.IsChecked = false;

            // Text Field -> No Border            
            controller.SetAppearanceGenerator(textFieldNoBorderMenuItem,
                new PdfTextBoxFieldAppearanceGenerator());

            // Text Field -> Single Border
            controller.SetAppearanceGenerator(textFieldSingleBorderMenuItem,
                new PdfTextBoxSingleBorderFieldAppearanceGenerator());

            // Text Field -> 3D Border
            controller.SetAppearanceGenerator(textField3DBorderMenuItem,
                new PdfTextBoxBorder3DFieldAppearanceGenerator());

            // Check Box -> Satandard
            controller.SetAppearanceGenerator(checkBoxStandardMenuItem,
                new PdfCheckBoxFieldDynamicAppearanceGenerator(13));

            // Check Box -> Simple
            controller.SetAppearanceGenerator(checkBoxSimpleMenuItem,
                new PdfCheckBoxSimpleFieldAppearanceGenerator(13, 13));

            // Check Box -> Symbol "V"
            controller.SetAppearanceGenerator(checkBoxSymbolVMenuItem,
                new PdfCheckBoxSymbolFieldAppearanceGenerator(13, "4", ""));

            // Check Box -> Symbol "X"
            controller.SetAppearanceGenerator(checkBoxSymbolXMenuItem,
                new PdfCheckBoxSymbolFieldAppearanceGenerator(13, "8", ""));

            // Button -> Flat Border
            controller.SetAppearanceGenerator(buttonFlatBorderMenuItem,
                new PdfButtonFlatBorderFieldAppearanceGenerator());

            // Button -> 3D Border
            controller.SetAppearanceGenerator(buttonBorder3DMenuItem,
                new PdfButton3DBorderFieldAppearanceGenerator());

            // List Box -> No Border
            controller.SetAppearanceGenerator(listBoxNoBorderMenuItem,
                new PdfListBoxFieldAppearanceGenerator());

            // Combo Box -> No Border
            controller.SetAppearanceGenerator(comboBoxNoBorderMenuItem,
                new PdfComboBoxFieldAppearanceGenerator());

            // Combo Box  -> Single Border
            controller.SetAppearanceGenerator(comboBoxSingleBorderMenuItem,
                new PdfComboBoxSingleBorderFieldAppearanceGenerator());

            // Combo Box  -> 3D Border
            controller.SetAppearanceGenerator(comboBox3dBorderMenuItem,
                new PdfComboBox3DFieldAppearanceGenerator());

            // Radio Button -> Standard
            controller.SetAppearanceGenerator(radioButtonStandardMenuItem,
                new PdfRadioButtonFieldDynamicAppearanceGenerator(13));

            // Radio Button -> Symbol
            controller.SetAppearanceGenerator(radioButtonSymbolMenuItem,
                new PdfRadioButtonSymbolFieldAppearanceGenerator(13, "l", "m"));

            // Barcode -> PDF417
            controller.SetAppearanceGenerator(barcodePdf417MenuItem,
                new PdfPDF417BarcodeFieldAppearanceGenerator());

            // Barcode -> DataMatrix
            controller.SetAppearanceGenerator(barcodeDataMatrixMenuItem,
                new PdfDataMatrixBarcodeFieldAppearanceGenerator());

            // Barcode -> QR Code
            controller.SetAppearanceGenerator(barcodeQrCodeMenuItem,
                new PdfQrBarcodeFieldAppearanceGenerator());

            // Barcode -> Vintasoft Barcode
            controller.SetAppearanceGenerator(vintasoftBarcodeMenuItem,
                new PdfVintasoftBarcodeFieldAppearanceGenerator());

            // Digital Signature -> Default
            SignatureAppearanceGenerator signatureAppearanceGenerator = new SignatureAppearanceGenerator();
            controller.SetAppearanceGenerator(signatureDefaultMenuItem,
                signatureAppearanceGenerator);
            SignatureAppearance = new SignatureAppearanceGraphicsFigure();

            textFieldNoBorderMenuItem.IsChecked = true;
            checkBoxStandardMenuItem.IsChecked = true;
            buttonBorder3DMenuItem.IsChecked = true;
            listBoxNoBorderMenuItem.IsChecked = true;
            comboBoxNoBorderMenuItem.IsChecked = true;
            radioButtonStandardMenuItem.IsChecked = true;
            barcodePdf417MenuItem.IsChecked = true;
            signatureDefaultMenuItem.IsChecked = true;
        }

        /// <summary>
        /// Determines whether the specified view is widget annotation.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns>
        /// <b>true</b> if the annotation view is widget annotation; otherwise, <b>false</b>.
        /// </returns>
        private bool IsPdfWidgetAnnotation(WpfPdfAnnotationView view)
        {
            if (view is WpfPdfWidgetAnnotationView)
                return true;

            return false;
        }

        /// <summary>
        /// Returns the name of new field.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <returns>The name of new field.</returns>
        private string GetFieldName(PdfInteractiveFormField parent, string nameFormat)
        {
            string format = nameFormat;
            if (parent != null)
                format = string.Format("{0}.{1}", parent.FullyQualifiedName, nameFormat);
            PdfDocument document = CurrentDocument;
            if (document.InteractiveForm == null)
                return string.Format(nameFormat, 1);
            int i = 1;
            while (document.InteractiveForm.FindField(string.Format(format, i)) != null)
                i++;
            return string.Format(nameFormat, i);
        }

        /// <summary>
        /// Returns a rectangle for new field.
        /// </summary>
        /// <param name="appearanceGeneratorKey">The appearance generator key (button).</param>
        /// <returns>A rectangle for new field.</returns>
        private RectangleF GetNewFieldRectangle(object appearanceGeneratorKey)
        {
            if (_annotationAppearanceGeneratorController.ContainsKey(appearanceGeneratorKey))
            {
                RectangleF rect = _annotationAppearanceGeneratorController[appearanceGeneratorKey].GetDefaultAnnotationRectangle();
                return PdfAnnotationsTools.GetNewAnnotationRectangle(AnnotationTool, _visualToolMouseObserver, rect.Width, rect.Height);
            }
            return PdfAnnotationsTools.GetNewAnnotationRectangle(AnnotationTool, _visualToolMouseObserver, 20, 20);
        }

        /// <summary>
        /// Returns the PDF annotation appearance generator for
        /// the specified appearance generator key (button).
        /// </summary>
        /// <param name="appearanceGeneratorKey">The appearance generator key (button).</param>
        /// <returns>The PDF annotation appearance generator.</returns>
        private PdfAnnotationAppearanceGenerator GetAppearanceGenerator(object appearanceGeneratorKey)
        {
            PdfAnnotationAppearanceGenerator result = null;
            if (_annotationAppearanceGeneratorController.TryGetValue(appearanceGeneratorKey, out result))
                return result;
            return null;
        }

        /// <summary>
        /// Returns the free field name.
        /// </summary>
        /// <param name="names">An array with existing name.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <returns>The free field name.</returns>
        private string GetFreeName(string[] names, string nameFormat)
        {
            int i = 1;
            if (names != null && names.Length > 0)
            {
                while (Array.IndexOf(names, string.Format(nameFormat, i)) >= 0)
                    i++;
            }
            return string.Format(nameFormat, i);
        }

        /// <summary>
        /// Adds the image to the resources.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="fileName">Name of the file.</param>
        private void AddImageResource(string resourceName, string fileName)
        {
            if (ImagingEnvironment.IsInDesignMode)
                return;

            string resourceNameFormatString = "WpfDemosCommonCode.Pdf.AnnotationTool.FormFields.Resources.{0}.png";
            using (VintasoftImage image = DemosResourcesManager.GetResourceAsImage(string.Format(resourceNameFormatString, fileName)))
            {
                Resources[resourceName] = Vintasoft.Imaging.Wpf.VintasoftImageConverter.ToBitmapSource(image);
            }
        }

        #endregion

        #endregion

    }
}
