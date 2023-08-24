using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Wpf.UI;
using Vintasoft.Imaging.Pdf.Tree.OptionalContent;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view and change settings of optional content (layers) of PDF document.
    /// </summary>
    public partial class OptionalContentSettingsWindow : Window
    {

        #region Fields

        /// <summary>
        /// An image viewer.
        /// </summary>
        WpfImageViewer _viewer;

        /// <summary>
        /// A PDF document.
        /// </summary>
        PdfDocument _document;

        /// <summary>
        /// A list of optional content configurations of PDF document.
        /// </summary>
        List<PdfOptionalContentConfiguration> _configurations = new List<PdfOptionalContentConfiguration>();

        /// <summary>
        /// Determines that all layers must be shown.
        /// </summary>
        bool _showAllLayers = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalContentSettingsWindow"/> class.
        /// </summary>
        public OptionalContentSettingsWindow()
        {
            InitializeComponent();
            _showAllLayers = (bool)showAllLayersCheckBox.IsChecked;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalContentSettingsWindow"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="viewer">The viewer.</param>
        public OptionalContentSettingsWindow(PdfDocument document, WpfImageViewer viewer)
            : this()
        {
            // intialize PDF document
            _document = document;
            // initialize image viewer
            _viewer = viewer;

            // if default optional content configuration of PDF document if not empty
            if (_document.OptionalContentProperties.DefaultConfiguration != null)
            {
                // get name of default configuration
                string defaultName = string.Format("{0}(Default)", _document.OptionalContentProperties.DefaultConfiguration.Name);
                // add configuration to the cofiguration combo box
                AddConfiguration(defaultName, _document.OptionalContentProperties.DefaultConfiguration);
            }
            // get list of optional content configurations
            IList<PdfOptionalContentConfiguration> configurations = _document.OptionalContentProperties.Configurations;
            // if list of optional content configuration is not empty
            if (configurations != null)
            {
                // for each configuration
                for (int i = 0; i < configurations.Count; i++)
                {
                    // add configuration to the cofiguration combo box
                    AddConfiguration(configurations[i].Name, configurations[i]);
                }
            }
            // select configuration of current optional content
            configurationsComboBox.SelectedIndex = _configurations.IndexOf(_document.OptionalContentConfiguration);

        }

        #endregion



        #region Methods

        /// <summary>
        /// Returns a value that indicates that auto states of optional content is empty.
        /// </summary>
        /// <param name="configuration">A configuration of optional content.</param>
        /// <returns>
        /// <b>True</b> - if auto states is empty;
        /// <b>False</b> - is auto states is not empty.
        /// </returns>
        private bool GetIsAutoStateEmpty(PdfOptionalContentConfiguration configuration)
        {
            // indicates that auto states of optional content are empty
            bool isAutoStatesEmpty = true;
            // if auto states are not empty
            if (configuration.AutoStates != null)
            {
                // the message
                string message = "Optional content configuration has auto state. To change layers order, auto states should be deleted.\n" +
                    "Delete auto states?\nPress 'Yes' to delete auto states.";
                // if auto states must be deleted
                if (MessageBox.Show(message, "Info", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // delete auto states
                    configuration.AutoStates = null;
                    // indicate that auto states are deleted
                    isAutoStatesEmpty = true;
                }
                else
                {
                    // indicate that auto states are not deleted
                    isAutoStatesEmpty = false;
                }
            }
            return isAutoStatesEmpty;
        }

        /// <summary>
        /// Adds the optional content configuration to the cofiguration combo box.
        /// </summary>
        /// <param name="name">A name of configuration.</param>
        /// <param name="configuration">A configuration.</param>
        private void AddConfiguration(string name, PdfOptionalContentConfiguration configuration)
        {
            // if name of configuration is empty
            if (name == "")
            {
                // set predefined name for configuration
                name = "(no name)";
            }
            // add configuration to the cofiguration combo box
            configurationsComboBox.Items.Add(name);
            // add configuration to the list of optional content configurations
            _configurations.Add(configuration);
        }

        /// <summary>
        /// Updates configuration of PDF document according to the selected configuration in combo box.
        /// </summary>
        private void configurationsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCurrentConfiguration();

            // set chosen configuration
            _document.OptionalContentConfiguration = _configurations[configurationsComboBox.SelectedIndex];

            // if auto states are empty
            if (GetIsAutoStateEmpty(_document.OptionalContentConfiguration))
            {
                // enable list box
                ocGroupsCheckedListBox.IsEnabled = true;
            }
            // if auto states are not empty
            else
            {
                // disable list box
                ocGroupsCheckedListBox.IsEnabled = false;
            }

            LoadGroupsList();
        }

        /// <summary>
        /// Updates current configuration and loads new group list according to
        /// "Show All Layers" check state.
        /// </summary>
        private void showAllLayersCheckBox_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentConfiguration();
            _showAllLayers = (bool)showAllLayersCheckBox.IsChecked;
            LoadGroupsList();
        }

        /// <summary>
        /// Loads list of optional content groups of PDF document.
        /// </summary>
        private void LoadGroupsList()
        {
            // clear list box
            ocGroupsCheckedListBox.Items.Clear();

            // get a new optional content configuration
            PdfOptionalContentConfiguration configuration = _document.OptionalContentConfiguration;
            // get a new list of optional content groups 
            IList<PdfOptionalContentGroup> groups = GetOptionalContentGroupList(configuration);

            // if list of optional content groups is not empty
            if (groups != null)
            {
                // for each group in list
                foreach (PdfOptionalContentGroup group in groups)
                {
                    CheckBox checkBox = new CheckBox();
                    checkBox.Content = group.Name;
                    checkBox.IsChecked = configuration.GetGroupVisibility(group);
                    // add optional content group to the list box
                    ocGroupsCheckedListBox.Items.Add(checkBox);
                }
            }
        }

        /// <summary>
        /// Returns an optional content group list.
        /// </summary>
        /// <param name="configuration">A configuration.</param>
        /// <returns>An optional content group list.</returns>
        private IList<PdfOptionalContentGroup> GetOptionalContentGroupList(PdfOptionalContentConfiguration configuration)
        {
            IList<PdfOptionalContentGroup> groups = null;

            // if all layers must be shown
            if (_showAllLayers)
            {
                // get a new list of optional content groups
                groups = _document.OptionalContentProperties.OptionalContentGroups;
            }
            // if presentation order is not empty
            else if (configuration.PresentationOrder != null)
            {
                // if list of optional content groups is empty
                if (groups == null)
                {
                    // create new list
                    groups = new List<PdfOptionalContentGroup>();
                }

                if (configuration.PresentationOrder.OptionalContentGroup != null)
                    groups.Add(configuration.PresentationOrder.OptionalContentGroup);

                // if list of presentation orders is not empty 
                if (configuration.PresentationOrder.Items != null && configuration.PresentationOrder.Items.Length > 0)
                {
                    // load list of optional content groups from presentation order
                    LoadGroupListFromPresentationOrder(groups, configuration.PresentationOrder);
                }
            }
            return groups;
        }

        /// <summary>
        /// Loads list of optional content groups from presentation order.
        /// </summary>
        /// <param name="groups">List of optional content groups.</param>
        /// <param name="presentationOrder">Presentation order.</param>
        private void LoadGroupListFromPresentationOrder(
            IList<PdfOptionalContentGroup> groups,
            PdfOptionalContentPresentationOrder presentationOrder)
        {
            // for each sub presentation order
            foreach (PdfOptionalContentPresentationOrder subPresentationOrder in presentationOrder.Items)
            {
                // if optional content group is not empty
                if (subPresentationOrder.OptionalContentGroup != null)
                {
                    // add group to group list
                    groups.Add(subPresentationOrder.OptionalContentGroup);
                }

                // if list of sub presentation orders is not empty
                if (subPresentationOrder.Items != null)
                {
                    // load optional content gropus from sub presentation order
                    LoadGroupListFromPresentationOrder(groups, subPresentationOrder);
                }
            }
        }

        /// <summary>
        /// Updates current optional content configuration of PDF document.
        /// </summary>
        private void UpdateCurrentConfiguration()
        {
            // if list box is not empty
            if (ocGroupsCheckedListBox.Items.Count > 0)
            {
                // get current optional content configuration
                PdfOptionalContentConfiguration configuration = _document.OptionalContentConfiguration;
                // get current list of optional content groups 
                IList<PdfOptionalContentGroup> groups = GetOptionalContentGroupList(configuration);

                // for each group in list
                for (int i = 0; i < groups.Count; i++)
                {
                    CheckBox checkBox = (CheckBox)ocGroupsCheckedListBox.Items[i];
                    // update group visibility
                    configuration.SetGroupVisibility(groups[i], checkBox.IsChecked.Value);
                }
            }
        }

        /// <summary>
        /// Updates current optional content configuration of PDF document
        /// and set dialog result <b>true</b>.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentConfiguration();
            DialogResult = true;
        }

        #endregion

    }
}
