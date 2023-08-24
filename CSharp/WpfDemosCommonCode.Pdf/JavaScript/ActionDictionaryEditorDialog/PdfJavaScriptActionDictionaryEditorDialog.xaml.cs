using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf.Tree;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Form that allows to edit the JavaScript of PDF document.
    /// </summary>
    public partial class PdfJavaScriptActionDictionaryEditorDialog : Window
    {

        #region Fields

        /// <summary>
        /// The dictionary (script name => script code) that
        /// contains information about new scripts.
        /// </summary>
        Dictionary<string, string> _javaScriptNameToJavaScriptCode =
            new Dictionary<string, string>();

        /// <summary>
        /// The dictionary (script object => script code) that
        /// contains information about existing scripts.
        /// </summary>
        Dictionary<PdfJavaScriptAction, string> _javaScriptActionToJavaScriptName =
            new Dictionary<PdfJavaScriptAction, string>();

        /// <summary>
        /// The dictionary (script object => script code) that
        /// contains information about modified existing scripts.
        /// </summary>
        Dictionary<PdfJavaScriptAction, string> _javaScriptActionToJavaScriptCode =
            new Dictionary<PdfJavaScriptAction, string>();

        /// <summary>
        /// The dictionary that contains JavaScript actions of PDF document.
        /// </summary>
        PdfJavaScriptActionDictionary _javaScriptActionDictionary = null;

        /// <summary>
        /// The name of selected JavaScript action.
        /// </summary>
        string _selectedJavaScriptActionName = null;

        /// <summary>
        /// Indicates that list box is changing.
        /// </summary>
        bool _isListBoxChanging = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of
        /// the <see cref="PdfJavaScriptActionDictionaryEditorDialog"/> class.
        /// </summary>
        /// <param name="javaScriptActionDictionary">The dictionary that contains
        /// JavaScript actions of PDF document.</param>
        public PdfJavaScriptActionDictionaryEditorDialog(
            PdfJavaScriptActionDictionary javaScriptActionDictionary)
        {
            if (javaScriptActionDictionary == null)
                throw new ArgumentNullException();

            InitializeComponent();

            _javaScriptActionDictionary = javaScriptActionDictionary;

            foreach (string key in javaScriptActionDictionary.Keys)
                _javaScriptActionToJavaScriptName.Add(javaScriptActionDictionary[key], key);

            // add all scripts to the list box

            javaScripActionsListBox.BeginInit();
            foreach (string key in _javaScriptActionDictionary.Keys)
                javaScripActionsListBox.Items.Add(key);
            javaScripActionsListBox.SelectedIndex = -1;
            javaScripActionsListBox.EndInit();

            wordWrapCheckBox.IsChecked = javaScriptTextBox.TextWrapping != TextWrapping.NoWrap;

            UpdateUI();
        }

        #endregion



        #region Methods

        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            bool isSelectedJavaScript = javaScripActionsListBox.SelectedIndex != -1;

            renameButton.IsEnabled = isSelectedJavaScript;
            removeButton.IsEnabled = isSelectedJavaScript;

            javaScriptTextBox.IsEnabled = isSelectedJavaScript;
        }

        /// <summary>
        /// JavaScript code is changed.
        /// </summary>
        private void javaScripActionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // if list box is changing
            if (_isListBoxChanging)
                return;

            string selectedItem = javaScripActionsListBox.SelectedItem as string;

            if (selectedItem == _selectedJavaScriptActionName)
                return;

            SaveSelectedJavaScript();

            _selectedJavaScriptActionName = selectedItem;

            // update text box
            javaScriptTextBox.Text = GetJavaScriptCode(_selectedJavaScriptActionName);
            // update UI
            UpdateUI();
        }

        /// <summary>
        /// Saves the selected JavaScript code.
        /// </summary>
        private void SaveSelectedJavaScript()
        {
            if (string.IsNullOrEmpty(_selectedJavaScriptActionName))
                return;

            string javaScriptCode = javaScriptTextBox.Text;
            string previousJavaScriptCode = GetJavaScriptCode(_selectedJavaScriptActionName);

            if (!string.Equals(previousJavaScriptCode, javaScriptCode, StringComparison.InvariantCulture))
            {
                PdfJavaScriptAction action =
                    GetActionByName(_javaScriptActionToJavaScriptName, _selectedJavaScriptActionName);
                if (action != null)
                    _javaScriptActionToJavaScriptCode[action] = javaScriptCode;
                else
                    _javaScriptNameToJavaScriptCode[_selectedJavaScriptActionName] = javaScriptCode;
            }
        }

        /// <summary>
        /// Returns the JavaScript code.
        /// </summary>
        /// <param name="name">The name.</param>
        private string GetJavaScriptCode(string name)
        {
            string code = string.Empty;

            if (!string.IsNullOrEmpty(name) &&
                !_javaScriptNameToJavaScriptCode.TryGetValue(name, out code))
            {
                PdfJavaScriptAction action = GetActionByName(_javaScriptActionToJavaScriptName, name);
                if (!_javaScriptActionToJavaScriptCode.TryGetValue(action, out code))
                    code = PreprocessJavaScriptCode(action.JavaScript);
            }

            return code;
        }

        /// <summary>
        /// Returns the names of all JavaScripts.
        /// </summary>
        private string[] GetJavaScriptActionNames()
        {
            int count = _javaScriptActionToJavaScriptName.Count +
                _javaScriptNameToJavaScriptCode.Count;
            string[] names = new string[count];

            _javaScriptActionToJavaScriptName.Values.CopyTo(names, 0);
            _javaScriptNameToJavaScriptCode.Keys.CopyTo(names, _javaScriptActionToJavaScriptName.Count);

            return names;
        }

        /// <summary>
        /// "OK" button is clicked.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSelectedJavaScript();

            string[] keys = new string[_javaScriptActionDictionary.Count];
            _javaScriptActionDictionary.Keys.CopyTo(keys, 0);
            foreach (string key in keys)
            {
                PdfJavaScriptAction action = _javaScriptActionDictionary[key];

                string javaScriptCode = string.Empty;
                if (_javaScriptActionToJavaScriptCode.TryGetValue(action, out javaScriptCode))
                    action.JavaScript = javaScriptCode;

                string newKey = string.Empty;
                if (_javaScriptActionToJavaScriptName.TryGetValue(action, out newKey))
                {
                    if (!string.Equals(newKey, key, StringComparison.InvariantCulture))
                    {
                        _javaScriptActionDictionary.Remove(key);
                        _javaScriptActionDictionary.Add(newKey, action);
                    }
                }
                else
                {
                    _javaScriptActionDictionary.Remove(key);
                }
            }

            foreach (string key in _javaScriptNameToJavaScriptCode.Keys)
            {
                PdfJavaScriptAction action = new PdfJavaScriptAction(_javaScriptActionDictionary.Document);
                action.JavaScript = _javaScriptNameToJavaScriptCode[key];

                _javaScriptActionDictionary.Add(key, action);
            }

            DialogResult = true;
        }

        /// <summary>
        /// Adds JavaScript action to PDF document.
        /// </summary>
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            string title = "JavaScript Action Name";
            string message = "Enter name of JavaScript action.";

            string[] javaScriptNames = GetJavaScriptActionNames();
            // create dialog
            PdfJavaScriptActionDictionaryRenameActionDialog dialog =
                new PdfJavaScriptActionDictionaryRenameActionDialog(title, message, javaScriptNames);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = this;
            dialog.ActionName = string.Empty;

            // show dialog
            if (dialog.ShowDialog() == true)
            {
                string actionName = dialog.ActionName;
                _javaScriptNameToJavaScriptCode.Add(actionName, string.Empty);
                // add action name to the list box
                javaScripActionsListBox.Items.Add(actionName);
                // set action name as focused in list box
                javaScripActionsListBox.SelectedItem = actionName;
                javaScriptTextBox.Focus();
            }
        }

        /// <summary>
        /// Renames the JavaScript action in PDF document.
        /// </summary>
        private void renameButton_Click(object sender, RoutedEventArgs e)
        {
            string title = "JavaScript Action Name";
            string message = "Enter name of JavaScript action.";

            string[] javaScriptNames = GetJavaScriptActionNames();
            // create dialog
            PdfJavaScriptActionDictionaryRenameActionDialog dialog =
               new PdfJavaScriptActionDictionaryRenameActionDialog(title, message, javaScriptNames);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = this;
            dialog.ActionName = _selectedJavaScriptActionName;

            // show dialog
            if (dialog.ShowDialog() == true)
            {
                string actionName = dialog.ActionName;

                PdfJavaScriptAction action = GetActionByName(
                    _javaScriptActionToJavaScriptName,
                    _selectedJavaScriptActionName);
                if (action != null)
                {
                    _javaScriptActionToJavaScriptName[action] = actionName;
                }
                else
                {
                    string javaScript = _javaScriptNameToJavaScriptCode[_selectedJavaScriptActionName];
                    _javaScriptNameToJavaScriptCode.Remove(_selectedJavaScriptActionName);
                    _javaScriptNameToJavaScriptCode.Add(actionName, javaScript);
                }
                _selectedJavaScriptActionName = actionName;

                _isListBoxChanging = true;
                javaScripActionsListBox.BeginInit();
                javaScripActionsListBox.Items[javaScripActionsListBox.SelectedIndex] = actionName;
                javaScripActionsListBox.SelectedItem = actionName;
                javaScripActionsListBox.EndInit();
                _isListBoxChanging = false;
            }
        }

        /// <summary>
        /// Removes the JavaScript action from PDF document.
        /// </summary>
        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            // get name of selected action
            string selectedJavaScriptName = javaScripActionsListBox.SelectedItem.ToString();

            PdfJavaScriptAction action = GetActionByName(_javaScriptActionToJavaScriptName, selectedJavaScriptName);
            if (action != null)
            {
                _javaScriptActionToJavaScriptName.Remove(action);
                _javaScriptActionToJavaScriptCode.Remove(action);
            }
            else
            {
                _javaScriptNameToJavaScriptCode.Remove(selectedJavaScriptName);
            }
            // remove action from listbox
            _selectedJavaScriptActionName = string.Empty;
            int selectedItemIndex = javaScripActionsListBox.SelectedIndex;
            javaScripActionsListBox.BeginInit();
            javaScripActionsListBox.Items.Remove(selectedJavaScriptName);
            if (selectedItemIndex >= javaScripActionsListBox.Items.Count)
                selectedItemIndex--;
            javaScripActionsListBox.EndInit();
            javaScripActionsListBox.SelectedIndex = selectedItemIndex;
            // update UI
            UpdateUI();
        }

        /// <summary>
        /// Word wrap check box is changed.
        /// </summary>
        private void wordWrapCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (wordWrapCheckBox.IsChecked.Value == true)
                javaScriptTextBox.TextWrapping = TextWrapping.Wrap;
            else
                javaScriptTextBox.TextWrapping = TextWrapping.NoWrap;
        }

        /// <summary>
        /// Preprocesses the JavaScript code.
        /// </summary>
        /// <param name="jsCode">The JavaScript code.</param>
        /// <returns>Preprocessed JavaScript code.</returns>
        private string PreprocessJavaScriptCode(string jsCode)
        {
            jsCode = jsCode.Replace("\r\n", "\n");
            jsCode = jsCode.Replace("\r", "\n");
            jsCode = jsCode.Replace("\n", "\r\n");
            return jsCode;
        }

        /// <summary>
        /// "Cancel" button is clicked.
        /// </summary>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Returns the action by the action name.
        /// </summary>
        /// <param name="dict">The dictionary.</param>
        /// <param name="name">The name.</param>
        /// <returns>The action.</returns>
        private PdfJavaScriptAction GetActionByName(
            Dictionary<PdfJavaScriptAction, string> dict,
            string name)
        {
            foreach (PdfJavaScriptAction action in dict.Keys)
            {
                if (dict[action] == name)
                    return action;
            }

            return null;
        }

        #endregion

    }
}
