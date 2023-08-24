using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree.OptionalContent;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to remove the optional content from PDF document.
    /// </summary>
    public partial class RemoveOptionalContentWindow : Window
    {

        #region Fields

        /// <summary>
        /// A PDF document.
        /// </summary>
        PdfDocument _document;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveOptionalContentWindow"/> class.
        /// </summary>
        public RemoveOptionalContentWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveOptionalContentWindow"/> class.
        /// </summary>
        /// <param name="document">The PDF document.</param>
        public RemoveOptionalContentWindow(PdfDocument document)
            : this()
        {
            // intialize PDF document
            _document = document;

            // get a new list of optional content groups 
            IList<PdfOptionalContentGroup> groups = document.OptionalContentProperties.OptionalContentGroups;

            // if list of optional content groups is not empty
            if (groups != null)
            {
                // for each group in list
                foreach (PdfOptionalContentGroup group in groups)
                {
                    CheckBox checkBox = new CheckBox();
                    checkBox.Content = group.Name;
                    // add optional content group to the list box
                    ocGroupsCheckedListBox.Items.Add(checkBox);
                }
            }
        }

        #endregion



        #region Properties

        List<PdfOptionalContentGroup> _selectedOptionalGroups = new List<PdfOptionalContentGroup>();
        /// <summary>
        /// A list of optional content, which should be removed from PDF document.
        /// </summary>
        public List<PdfOptionalContentGroup> SelectedOptionalGroups
        {
            get
            {
                return _selectedOptionalGroups;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// "OK" button is clicked.
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            for (int index = 0; index < ocGroupsCheckedListBox.Items.Count; index++)
            {
                CheckBox ocGroupCheckBox = (CheckBox)ocGroupsCheckedListBox.Items[index];

                if (ocGroupCheckBox.IsChecked.Value == true)
                    _selectedOptionalGroups.Add(_document.OptionalContentProperties.OptionalContentGroups[index]);
            }

            DialogResult = true;
        }

        /// <summary>
        /// "Cancel" button is clicked.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

    }
}
