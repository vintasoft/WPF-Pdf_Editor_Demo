using System;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Security;

namespace WpfDemosCommonCode.Pdf.Security
{
    /// <summary>
    /// A window that allows to edit the security properties of PDF document.
    /// </summary>
    public partial class SecuritySettingsWindow : Window
    {

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SecuritySettingsWindow"/> class.
        /// </summary>
        /// <param name="currentEncryptionSettings">Current encryption settings.</param>
        public SecuritySettingsWindow(EncryptionSystem currentEncryptionSettings)
        {
            InitializeComponent();

            // add Adobe Acrobat compatibility modes to the combo box
            compatibilityModeComboBox.Items.Add(AdobeAcrobatCompatibilityMode.Acrobat4);
            compatibilityModeComboBox.Items.Add(AdobeAcrobatCompatibilityMode.Acrobat5);
            compatibilityModeComboBox.Items.Add(AdobeAcrobatCompatibilityMode.Acrobat6);
            compatibilityModeComboBox.Items.Add(AdobeAcrobatCompatibilityMode.Acrobat7);
            compatibilityModeComboBox.Items.Add(AdobeAcrobatCompatibilityMode.Acrobat8);
            compatibilityModeComboBox.Items.Add(AdobeAcrobatCompatibilityMode.Acrobat9);
            compatibilityModeComboBox.Items.Add(AdobeAcrobatCompatibilityMode.AcrobatX);

            UpdateUI(currentEncryptionSettings);
            _newEncryptionSettings = currentEncryptionSettings;
        }

        #endregion



        #region Properties

        EncryptionSystem _newEncryptionSettings;
        /// <summary>
        /// The encryption settings.
        /// </summary>
        public EncryptionSystem NewEncryptionSettings
        {
            get
            {
                return _newEncryptionSettings;
            }
        }

        /// <summary>
        /// The user password.
        /// </summary>
        private string UserPassword
        {
            get
            {
                if (showPasswordsCheckBox.IsChecked.Value == true)
                    return userPasswordTextBox.Text;
                else
                    return userPasswordPasswordBox.Password;
            }
        }

        /// <summary>
        /// The owner password.
        /// </summary>
        private string OwnerPassword
        {
            get
            {
                if (showPasswordsCheckBox.IsChecked.Value == true)
                    return ownerPasswordTextBox.Text;
                else
                    return ownerPasswordPasswordBox.Password;
            }
        }

        #endregion



        #region Methods


        /// <summary>
        /// Updates the user interface.
        /// </summary>
        /// <param name="currentEncryptionSettings">Current encryption settings.</param>
        private void UpdateUI(EncryptionSystem currentEncryptionSettings)
        {
            if (currentEncryptionSettings == null)
            {
                dontChangeRadioButton.Content = "Don't Change (No Security)";
                securityMethodLabel.Content = "";
                compatibilityModeComboBox.SelectedIndex = -1;
            }
            else
            {
                dontChangeRadioButton.Content = string.Format("Don't Change ({0})", currentEncryptionSettings);
                compatibilityModeComboBox.SelectedItem = currentEncryptionSettings.CompatibilityMode;
                ShowSecurityMethodByCompatibility(currentEncryptionSettings.CompatibilityMode);
            }
            userPasswordPasswordBox.Clear();
            ownerPasswordPasswordBox.Clear();
            userPasswordTextBox.Clear();
            ownerPasswordTextBox.Clear();

            if (currentEncryptionSettings != null)
            {
                // get user access permissions
                UserAccessPermissions permissions = currentEncryptionSettings.UserPermissions;
                // show user access permissions
                if ((permissions & UserAccessPermissions.AssembleDocument) == 0)
                    assembleDocumentCheckBox.IsChecked = false;
                else
                    assembleDocumentCheckBox.IsChecked = true;

                if ((permissions & UserAccessPermissions.ExtractTextAndGraphics) == 0)
                    extractTextAndGraphicsCheckBox.IsChecked = false;
                else
                    extractTextAndGraphicsCheckBox.IsChecked = true;

                if ((permissions & UserAccessPermissions.ExtractTextAndGraphicsForAccessibility) == 0)
                    extractTextAndGraphicsForAccessibilityCheckBox.IsChecked = false;
                else
                    extractTextAndGraphicsForAccessibilityCheckBox.IsChecked = true;

                if ((permissions & UserAccessPermissions.FillInteractiveFormFields) == 0)
                    fillInteractiveFormFieldsCheckBox.IsChecked = false;
                else
                    fillInteractiveFormFieldsCheckBox.IsChecked = true;

                if ((permissions & UserAccessPermissions.ModifyAnnotations) == 0)
                    modifyAnnotationsCheckBox.IsChecked = false;
                else
                    modifyAnnotationsCheckBox.IsChecked = true;

                if ((permissions & UserAccessPermissions.ModifyContents) == 0)
                    modifyContentsCheckBox.IsChecked = false;
                else
                    modifyContentsCheckBox.IsChecked = true;

                if ((permissions & UserAccessPermissions.PrintDocumentInLowResolution) == 0)
                    printInLowQualityCheckBox.IsChecked = false;
                else
                    printInLowQualityCheckBox.IsChecked = true;

                if ((permissions & UserAccessPermissions.PrintDocumentInHighResolution) == 0)
                    printInHighQualityCheckBox.IsChecked = false;
                else
                    printInHighQualityCheckBox.IsChecked = true;
            }
        }

        /// <summary>
        /// Print in low quality check box check state is changed.
        /// </summary>
        private void printInLowQualityCheckBox_Click(object sender, RoutedEventArgs e)
        {
            printInHighQualityCheckBox.IsEnabled = printInLowQualityCheckBox.IsChecked.Value == true;
            if (!printInHighQualityCheckBox.IsEnabled)
                printInHighQualityCheckBox.IsChecked = false;
        }

        /// <summary>
        /// "OK" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (noSecurityRadioButton.IsChecked.Value == true)
                {
                    _newEncryptionSettings = null;
                }
                else if (passwordProtectionRadioButton.IsChecked.Value == true)
                {

                    // if both password boxes are empty
                    if (OwnerPassword == "" && UserPassword == "")
                    {
                        DemosTools.ShowErrorMessage("Please, enter the owner and/or user password!");
                        if (ownerPasswordTextBox.Visibility == Visibility.Collapsed)
                            ownerPasswordPasswordBox.Focus();
                        else
                            ownerPasswordTextBox.Focus();
                        return;
                    }

                    UserAccessPermissions accessPermissions = UserAccessPermissions.None;
                    if (printInLowQualityCheckBox.IsChecked.Value == true)
                        accessPermissions |= UserAccessPermissions.PrintDocumentInLowResolution;
                    if (printInHighQualityCheckBox.IsChecked.Value == true)
                        accessPermissions |= UserAccessPermissions.PrintDocumentInHighResolution;
                    if (extractTextAndGraphicsCheckBox.IsChecked.Value == true)
                        accessPermissions |= UserAccessPermissions.ExtractTextAndGraphics;
                    if (extractTextAndGraphicsForAccessibilityCheckBox.IsChecked.Value == true)
                        accessPermissions |= UserAccessPermissions.ExtractTextAndGraphicsForAccessibility;
                    if (modifyContentsCheckBox.IsChecked.Value == true)
                        accessPermissions |= UserAccessPermissions.ModifyContents;
                    if (modifyAnnotationsCheckBox.IsChecked.Value == true)
                        accessPermissions |= UserAccessPermissions.ModifyAnnotations;
                    if (fillInteractiveFormFieldsCheckBox.IsChecked.Value == true)
                        accessPermissions |= UserAccessPermissions.FillInteractiveFormFields;
                    if (assembleDocumentCheckBox.IsChecked.Value == true)
                        accessPermissions |= UserAccessPermissions.AssembleDocument;

                    // get the Adobe Acrobat compatibility mode
                    AdobeAcrobatCompatibilityMode compatibilityMode = (AdobeAcrobatCompatibilityMode)compatibilityModeComboBox.SelectedItem;

                    // create new encryption system
                    _newEncryptionSettings = new EncryptionSystem(
                        compatibilityMode,
                        UserPassword,
                        OwnerPassword,
                        accessPermissions);
                }
                DialogResult = true;
            }
            catch (Exception ex)
            {
                DemosTools.ShowErrorMessage(ex);
            }
        }

        /// <summary>
        /// "Cancel" button is clicked.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Show password check box check state is changed.
        /// </summary>
        private void showPasswordsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool showPasswords = showPasswordsCheckBox.IsChecked.Value == true;

            userPasswordPasswordBox.Visibility = showPasswords ? Visibility.Collapsed : Visibility.Visible;
            ownerPasswordPasswordBox.Visibility = userPasswordPasswordBox.Visibility;

            userPasswordTextBox.Visibility = showPasswords ? Visibility.Visible : Visibility.Collapsed;
            ownerPasswordTextBox.Visibility = userPasswordTextBox.Visibility;

            if (showPasswords)
            {
                userPasswordTextBox.Text = userPasswordPasswordBox.Password;
                ownerPasswordTextBox.Text = ownerPasswordPasswordBox.Password;
            }
            else
            {
                userPasswordPasswordBox.Password = userPasswordTextBox.Text;
                ownerPasswordPasswordBox.Password = ownerPasswordTextBox.Text;
            }
        }

        /// <summary>
        /// Shows encryption algorithm according to the selected Adobe Acrobat compatibility mode.
        /// </summary>
        private void compatibilityModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AdobeAcrobatCompatibilityMode compatibilityMode;

            if (compatibilityModeComboBox.SelectedItem == null)
                compatibilityMode = AdobeAcrobatCompatibilityMode.Unspecified;
            else
                compatibilityMode = (AdobeAcrobatCompatibilityMode)compatibilityModeComboBox.SelectedItem;

            ShowSecurityMethodByCompatibility(compatibilityMode);
        }


        /// <summary>
        /// Shows the security method.
        /// </summary>
        /// <param name="compatibility">Adobe Acrobat compatibility mode.</param>
        private void ShowSecurityMethodByCompatibility(AdobeAcrobatCompatibilityMode compatibility)
        {
            switch (compatibility)
            {
                case AdobeAcrobatCompatibilityMode.Acrobat4:
                    securityMethodLabel.Content = "40-bit RC4 (PDF 1.1)";
                    break;
                case AdobeAcrobatCompatibilityMode.Acrobat5:
                case AdobeAcrobatCompatibilityMode.Acrobat6:
                    securityMethodLabel.Content = "128-bit RC4 (PDF 1.4)";
                    break;
                case AdobeAcrobatCompatibilityMode.Acrobat7:
                case AdobeAcrobatCompatibilityMode.Acrobat8:
                    securityMethodLabel.Content = "128-bit AES (PDF 1.6)";
                    break;
                case AdobeAcrobatCompatibilityMode.Acrobat9:
                    securityMethodLabel.Content = "256-bit AES (PDF 1.7)";
                    break;
                case AdobeAcrobatCompatibilityMode.AcrobatX:
                    securityMethodLabel.Content = "256-bit AES (PDF 2.0)";
                    break;
                default:
                    securityMethodLabel.Content = "";
                    break;
            }
        }

        /// <summary>
        /// "Don't changed" radio button is clicked.
        /// </summary>
        private void dontChangeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (dontChangeRadioButton.IsChecked.Value == true)
            {
                securitySettingsGroupBox.IsEnabled = false;
            }
            UpdateUI(NewEncryptionSettings);
        }

        /// <summary>
        /// "No Security" radio button is clicked.
        /// </summary>
        private void noSecurityRadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (noSecurityRadioButton.IsChecked.Value == true)
            {
                securitySettingsGroupBox.IsEnabled = false;
            }
            UpdateUI(NewEncryptionSettings);
        }

        /// <summary>
        /// "Password protection" radio button is clicked.
        /// </summary>
        private void passwordProtectionRadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (passwordProtectionRadioButton.IsChecked.Value == true)
            {
                securitySettingsGroupBox.IsEnabled = true;

                if (compatibilityModeComboBox.SelectedIndex == -1)
                    compatibilityModeComboBox.SelectedIndex = 0;
            }
        }

        #endregion

    }
}
