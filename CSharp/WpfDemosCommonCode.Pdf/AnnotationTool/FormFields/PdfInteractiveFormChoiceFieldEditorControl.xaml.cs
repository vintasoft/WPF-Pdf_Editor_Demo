using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf.Tree.InteractiveForms;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A control that allows to view and edit the <see cref="PdfInteractiveFormChoiceField"/>.
    /// </summary>
    [System.ComponentModel.DefaultEvent("PropertyValueChanged")]
    public partial class PdfInteractiveFormChoiceFieldEditorControl : UserControl, IPdfInteractiveFormPropertiesEditor
    {

        #region Constants

        /// <summary>
        /// The array separator.
        /// </summary>
        const char ARRAY_SEPARATOR = ';';

        #endregion



        #region Fields

        /// <summary>
        /// Determines that the item is moving.
        /// </summary>
        bool _isItemMoving = false;

        /// <summary>
        /// Determines that the items are updating.
        /// </summary>
        bool _isItemsUpdating = false;

        /// <summary>
        /// Determines that field with displayed value must to be copied to a field with exported value.
        /// </summary>
        bool _copyDisplayedValue = true;

        /// <summary>
        /// Determines that the item is changing.
        /// </summary>
        bool _isDisplayedValueChanging = false;

        /// <summary>
        /// The updating ComboBox.
        /// </summary>
        ComboBox _updatingComboBox = null;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfInteractiveFormChoiceFieldEditorControl"/> class.
        /// </summary>
        public PdfInteractiveFormChoiceFieldEditorControl()
        {
            InitializeComponent();

            foreach (TextQuaddingType quaddingType in Enum.GetValues(typeof(TextQuaddingType)))
                textQuaddingComboBox.Items.Add(quaddingType);
        }

        #endregion



        #region Properties

        PdfInteractiveFormChoiceField _field = null;
        /// <summary>
        /// Gets or sets the form choice fields items.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public PdfInteractiveFormChoiceField Field
        {
            get
            {
                return _field;
            }
            set
            {
                if (_field != null)
                    _field.Items.Changed -= new CollectionChangeEventHandler<PdfInteractiveFormChoiceFieldItem>(Items_Changed);

                _field = value;

                mainGrid.IsEnabled = _field != null;
                displayedValueTextBox.Text = string.Empty;

                if (_field == null)
                {
                    itemsListBox.Items.Clear();
                    valueComboBox.Text = string.Empty;
                    defaultValueComboBox.Text = string.Empty;
                }
                else
                {
                    UpdateItemsListBox();

                    UpdateFieldInfo();

                    _field.Items.Changed += new CollectionChangeEventHandler<PdfInteractiveFormChoiceFieldItem>(Items_Changed);
                }

                UpdateUI();
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
                Field = value as PdfInteractiveFormChoiceField;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC

        /// <summary>
        /// Updates value and default value of the <see cref="PdfInteractiveFormComboBoxField"/>.
        /// </summary>
        public void UpdateFieldInfo()
        {
            bool editable = true;
            if (_field is PdfInteractiveFormListBoxField)
            {
                if (!((PdfInteractiveFormListBoxField)_field).IsMultiSelect)
                    editable = false;
            }

            valueComboBox.IsEditable = editable;
            object fieldValue = _field.FieldValue;
            UpdateItemsComboBox(valueComboBox, _field);
            _field.FieldValue = fieldValue;
            UpdateTextComboBox(valueComboBox, _field.FieldValue);

            defaultValueComboBox.IsEditable = editable;
            object fieldDefaultValue = _field.FieldDefaultValue;
            UpdateItemsComboBox(defaultValueComboBox, _field);
            _field.FieldDefaultValue = fieldDefaultValue;
            UpdateTextComboBox(defaultValueComboBox, _field.FieldDefaultValue);

            textQuaddingComboBox.SelectedItem = _field.TextQuadding;
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            bool isItemSelected = itemsListBox.SelectedIndex != -1;
            bool isFirstItemSelected = itemsListBox.SelectedIndex == 0;
            bool isLastItemSelected = itemsListBox.SelectedIndex == itemsListBox.Items.Count - 1;
            bool canAddItem = false;
            if (!string.IsNullOrEmpty(exportedValueTextBox.Text) &&
                !string.IsNullOrEmpty(displayedValueTextBox.Text))
                canAddItem = FindItem(_field, exportedValueTextBox.Text) == null;
            else
                canAddItem = false;

            addButton.IsEnabled = canAddItem;
            moveUpButton.IsEnabled = isItemSelected && !isFirstItemSelected;
            moveDownButton.IsEnabled = isItemSelected && !isLastItemSelected;
            deleteButton.IsEnabled = isItemSelected;
        }

        /// <summary>
        /// Changes justification of text.
        /// </summary>
        private void textQuaddingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextQuaddingType textQuadding = (TextQuaddingType)textQuaddingComboBox.SelectedItem;

            if (_field.TextQuadding != textQuadding)
            {
                _field.TextQuadding = textQuadding;
                OnPropertyValueChanged();
            }
        }


        #region Value

        /// <summary>
        /// Field value is changed.
        /// </summary>
        private void valueComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isItemMoving)
                return;

            if (_field is PdfInteractiveFormListBoxField)
            {
                string selectedText = string.Empty;
                if (valueComboBox.SelectedItem != null)
                    selectedText = valueComboBox.SelectedItem.ToString();
                UpdateListBoxFieldValue((PdfInteractiveFormListBoxField)_field, selectedText);
            }
            else
                _field.FieldValue = valueComboBox.SelectedItem;
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Field value is changed.
        /// </summary>
        private void valueComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isItemMoving)
                return;

            if (_field is PdfInteractiveFormListBoxField)
                UpdateListBoxFieldValue((PdfInteractiveFormListBoxField)_field, valueComboBox.Text);
            else
                _field.FieldValue = valueComboBox.Text;
            OnPropertyValueChanged();
        }

        /// <summary>
        /// Updates value of the ListBox.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        private void UpdateListBoxFieldValue(PdfInteractiveFormListBoxField field, string value)
        {
            string[] result = null;
            try
            {
                result = CreateArrayFromString(value);
                result = RemoveInvalidItems(result, field.Items);
            }
            catch
            {
                return;
            }
            field.FieldValue = result;
        }

        #endregion


        #region Default Value

        /// <summary>
        /// The default value of field is changed.
        /// </summary>
        private void defaultValueComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isItemMoving)
                return;

            _field.FieldDefaultValue = defaultValueComboBox.SelectedItem;
            OnPropertyValueChanged();
        }

        /// <summary>
        /// The default value of field is changed.
        /// </summary>
        private void defaultValueComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_field is PdfInteractiveFormListBoxField)
            {
                string[] result = null;
                try
                {
                    result = CreateArrayFromString(defaultValueComboBox.Text);
                    result = RemoveInvalidItems(result, _field.Items);
                }
                catch
                {
                    return;
                }
                _field.FieldDefaultValue = result;
            }
            else
                _field.FieldDefaultValue = defaultValueComboBox.Text;
            OnPropertyValueChanged();
        }

        #endregion


        #region Items

        /// <summary>
        /// Selected item is changed.
        /// </summary>
        private void itemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = itemsListBox.SelectedIndex;

            string displayedValue = string.Empty;
            string exportedValue = string.Empty;

            if (selectedIndex != -1)
            {
                PdfInteractiveFormChoiceFieldItem item = _field.Items[selectedIndex];

                displayedValue = item.DisplayedValue;
                exportedValue = displayedValue;

                PdfInteractiveFormChoiceFieldExtendedItem extendedItem =
                    item as PdfInteractiveFormChoiceFieldExtendedItem;
                if (extendedItem != null)
                    exportedValue = extendedItem.ExportedValue;

                displayedValueTextBox.Text = displayedValue;
                exportedValueTextBox.Text = exportedValue;

            }

            _copyDisplayedValue = displayedValue == exportedValue;
            UpdateUI();
        }

        /// <summary>
        /// Items are changed.
        /// </summary>
        void Items_Changed(object sender, CollectionChangeEventArgs<PdfInteractiveFormChoiceFieldItem> e)
        {
            if (!_isItemMoving && !_isItemsUpdating)
            {
                _isItemsUpdating = true;
                UpdateItemsListBox();

                UpdateItemsComboBox(valueComboBox, _field);
                UpdateTextComboBox(valueComboBox, _field.FieldValue);

                UpdateItemsComboBox(defaultValueComboBox, _field);
                UpdateTextComboBox(defaultValueComboBox, _field.FieldDefaultValue);
                _isItemsUpdating = false;
            }
        }

        /// <summary>
        /// The displayed value of item is changed.
        /// </summary>
        private void displayedValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _isDisplayedValueChanging = true;
            try
            {
                if (_copyDisplayedValue)
                    exportedValueTextBox.Text = displayedValueTextBox.Text;

                UpdateUI();
            }
            finally
            {
                _isDisplayedValueChanging = false;
            }
        }

        /// <summary>
        /// The exported value of item is changed.
        /// </summary>
        private void exportedValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isDisplayedValueChanging)
                return;

            _copyDisplayedValue = displayedValueTextBox.Text == exportedValueTextBox.Text;

            UpdateUI();
        }

        /// <summary>
        /// Adds item to the list of choice fields of interactive form.
        /// </summary>
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string displayedValue = displayedValueTextBox.Text;
                string exportedValue = exportedValueTextBox.Text;

                PdfInteractiveFormChoiceFieldItem item = null;
                if (displayedValue != exportedValue)
                    item = new PdfInteractiveFormChoiceFieldExtendedItem(_field.Document, exportedValue, displayedValue);
                else
                    item = new PdfInteractiveFormChoiceFieldItem(_field.Document, displayedValue);

                _field.Items.Add(item);
                itemsListBox.SelectedIndex = _field.Items.Count - 1;
                OnPropertyValueChanged();

                displayedValueTextBox.Focus();
                displayedValueTextBox.SelectAll();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// The selected item is removed.
        /// </summary>
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = itemsListBox.SelectedIndex;

            int selectedIndex = index;
            if (selectedIndex == itemsListBox.Items.Count - 1)
                selectedIndex -= 1;

            if (_field is PdfInteractiveFormListBoxField)
            {
                PdfInteractiveFormListBoxField listBoxField = (PdfInteractiveFormListBoxField)_field;

                if (listBoxField.SelectedItemIndexes != null)
                {
                    List<int> selectedItemIndexes = new List<int>(listBoxField.SelectedItemIndexes);
                    if (selectedItemIndexes.Contains(index))
                        selectedItemIndexes.Remove(index);

                    for (int i = 0; i < selectedItemIndexes.Count; i++)
                        if (selectedItemIndexes[i] > index)
                            selectedItemIndexes[i] -= 1;

                    listBoxField.SelectedItemIndexes = selectedItemIndexes.ToArray();
                }
                else
                {
                    string selectedValue = itemsListBox.Items[index].ToString();
                    if (selectedValue.Equals(listBoxField.SelectedItem))
                        listBoxField.SelectedItem = string.Empty;
                    if (selectedValue.Equals(listBoxField.FieldValue))
                        listBoxField.FieldValue = string.Empty;
                    if (selectedValue.Equals(listBoxField.FieldDefaultValue))
                        listBoxField.FieldDefaultValue = string.Empty;
                }
            }

            _field.Items.RemoveAt(index);

            displayedValueTextBox.Text = string.Empty;
            itemsListBox.SelectedIndex = selectedIndex;

            UpdateUI();

            OnPropertyValueChanged();
        }

        /// <summary>
        /// The selected item is moved up.
        /// </summary>
        private void moveUpButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSelectedItem(true);

            OnPropertyValueChanged();
        }

        /// <summary>
        /// The selected item is moved down.
        /// </summary>
        private void moveDownButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSelectedItem(false);

            OnPropertyValueChanged();
        }

        /// <summary>
        /// Finds the specified displayed value in item collection.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="displayedValue">The displayed value of item.</param>
        /// <returns>
        /// Reference to the item if item is found in the collection;
        /// otherwise, <b>null</b>.
        /// </returns>
        private PdfInteractiveFormChoiceFieldItem FindItem(
            PdfInteractiveFormChoiceField field,
            string exportedValue)
        {
            foreach (PdfInteractiveFormChoiceFieldItem item in _field.Items)
            {
                if (item is PdfInteractiveFormChoiceFieldExtendedItem)
                {
                    PdfInteractiveFormChoiceFieldExtendedItem extendedItem =
                        (PdfInteractiveFormChoiceFieldExtendedItem)item;
                    if (extendedItem.ExportedValue == exportedValue)
                        return item;
                }
                else if (item.DisplayedValue == exportedValue)
                    return item;
            }
            return null;
        }

        /// <summary>
        /// Updates the items in list box.
        /// </summary>
        private void UpdateItemsListBox()
        {
            itemsListBox.BeginInit();
            itemsListBox.Items.Clear();

            foreach (PdfInteractiveFormChoiceFieldItem item in _field.Items)
                itemsListBox.Items.Add(item.DisplayedValue);

            itemsListBox.EndInit();
        }

        /// <summary>
        /// Moves the selected item.
        /// </summary>
        /// <param name="moveUp">Indicates that the selected item must be moved up.</param>
        private void MoveSelectedItem(bool moveUp)
        {
            _isItemMoving = true;
            try
            {
                int index = itemsListBox.SelectedIndex;
                PdfInteractiveFormChoiceFieldItem item = _field.Items[index];
                int newIndex = index;
                if (moveUp)
                {
                    if (index == 0)
                        return;

                    newIndex = index - 1;
                }
                else
                {
                    if (index == _field.Items.Count - 1)
                        return;

                    newIndex = index + 1;
                }

                _field.Items.Remove(item);
                _field.Items.Insert(newIndex, item);

                if (_field is PdfInteractiveFormListBoxField)
                {
                    PdfInteractiveFormListBoxField listBoxField = (PdfInteractiveFormListBoxField)_field;

                    int[] selectedItemIndexes = listBoxField.SelectedItemIndexes;
                    if (selectedItemIndexes != null)
                    {
                        bool selectedItemIndexesChanged = false;
                        for (int i = 0; i < selectedItemIndexes.Length; i++)
                        {
                            if (selectedItemIndexes[i] == index)
                            {
                                selectedItemIndexes[i] = newIndex;
                                selectedItemIndexesChanged = true;
                            }
                            else if (selectedItemIndexes[i] == newIndex)
                            {
                                selectedItemIndexes[i] = index;
                                selectedItemIndexesChanged = true;
                            }
                        }

                        if (selectedItemIndexesChanged)
                            listBoxField.SelectedItemIndexes = selectedItemIndexes;
                    }
                }

                UpdateItemsListBox();

                UpdateItemsComboBox(valueComboBox, _field);
                UpdateTextComboBox(valueComboBox, _field.FieldValue);

                UpdateItemsComboBox(defaultValueComboBox, _field);
                UpdateTextComboBox(defaultValueComboBox, _field.FieldDefaultValue);

                itemsListBox.SelectedIndex = newIndex;
                UpdateUI();
            }
            finally
            {
                _isItemMoving = false;
            }
        }

        #endregion


        #region Common

        /// <summary>
        /// Updates the items of combo box.
        /// </summary>
        /// <param name="comboBox">The combo box.</param>
        /// <param name="field">The field.</param>
        private void UpdateItemsComboBox(ComboBox comboBox,
            PdfInteractiveFormChoiceField field)
        {
            if (_updatingComboBox == comboBox)
                return;

            _updatingComboBox = comboBox;
            comboBox.BeginInit();

            comboBox.Items.Clear();

            if ((field is PdfInteractiveFormListBoxField) &&
                ((PdfInteractiveFormListBoxField)_field).IsMultiSelect)
            {
                int count = Math.Min(2, field.Items.Count);

                string result = string.Empty;

                for (int i = 0; i < count; i++)
                    result += string.Format("\"{0}\"{1} ", field.Items[i].DisplayedValue, ARRAY_SEPARATOR);

                if (!string.IsNullOrEmpty(result))
                {
                    result = result.Trim();
                    result = result.TrimEnd(ARRAY_SEPARATOR);
                    comboBox.Items.Add(result);
                }
            }
            else
            {
                foreach (PdfInteractiveFormChoiceFieldItem item in field.Items)
                    comboBox.Items.Add(item.DisplayedValue);
            }

            comboBox.EndInit();
            _updatingComboBox = null;
        }

        /// <summary>
        /// Updates the text of combo box.
        /// </summary>
        /// <param name="comboBox">The combo box.</param>
        /// <param name="selectedItem">The selected item.</param>
        private void UpdateTextComboBox(ComboBox comboBox,
            object selectedItem)
        {
            string text = string.Empty;

            if (selectedItem != null)
            {
                if (_field is PdfInteractiveFormListBoxField)
                {
                    string[] selectedItemArray = selectedItem as string[];
                    bool isMultiselect = ((PdfInteractiveFormListBoxField)_field).IsMultiSelect;
                    if (selectedItemArray != null)
                    {
                        if (isMultiselect)
                            text = CreateStringFromArray(selectedItemArray);
                        else if (selectedItemArray.Length > 0)
                            text = selectedItemArray[0];
                    }
                    else
                    {
                        text = selectedItem.ToString();

                        if (!isMultiselect)
                        {
                            bool contains = false;
                            foreach (object item in comboBox.Items)
                            {
                                if (item.ToString() == text)
                                {
                                    contains = true;
                                    break;
                                }
                            }

                            if (!contains)
                                text = string.Empty;
                        }
                    }
                }
                else
                    text = selectedItem.ToString();
            }

            if (comboBox.IsEditable)
                comboBox.Text = text;
            else
            {
                comboBox.Text = string.Empty;
                foreach (object item in comboBox.Items)
                {
                    if (item.ToString() == text)
                    {
                        comboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Removes invalid items.
        /// </summary>
        /// <param name="sourceArray">An item array from which invalid values must be removed.</param>
        /// <param name="itemList">A list with "good" items.</param>
        private string[] RemoveInvalidItems(
            string[] items,
            PdfInteractiveFormChoiceFieldItemList itemList)
        {
            List<string> result = new List<string>();

            foreach (string sourceItem in items)
            {
                if (result.Contains(sourceItem))
                    continue;

                foreach (PdfInteractiveFormChoiceFieldItem item in itemList)
                {
                    if (sourceItem == item.DisplayedValue)
                    {
                        result.Add(sourceItem);
                        break;
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Creates the array from string.
        /// </summary>
        /// <param name="sourceStr">The source string.</param>
        private string[] CreateArrayFromString(string sourceStr)
        {
            if (string.IsNullOrEmpty(sourceStr))
                return new string[0];

            string[] splitedSourceStr = sourceStr.Split(
                new string[] { string.Format("{0} ", ARRAY_SEPARATOR) }, StringSplitOptions.None);

            for (int i = 0; i < splitedSourceStr.Length; i++)
            {
                string currentStr = splitedSourceStr[i];

                int startIndex = currentStr.IndexOf('\"') + 1;
                int endIndex = currentStr.LastIndexOf('\"');

                if (endIndex != -1)
                    splitedSourceStr[i] = currentStr.Substring(startIndex, endIndex - startIndex);
            }

            return splitedSourceStr;
        }

        /// <summary>
        /// Creates the string from array.
        /// </summary>
        /// <param name="sourceArray">The source array.</param>
        private string CreateStringFromArray(params string[] sourceArray)
        {
            string result = string.Empty;

            if (sourceArray != null && sourceArray.Length > 0)
            {
                foreach (string sourceItem in sourceArray)
                    result += string.Format("\"{0}\"{1} ", sourceItem, ARRAY_SEPARATOR);

                result = result.TrimEnd(' ');
                result = result.TrimEnd(ARRAY_SEPARATOR);
            }

            return result;
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

        #endregion



        #region Events

        /// <summary>
        /// Occurs when value of property is changed.
        /// </summary>
        public event EventHandler PropertyValueChanged;

        #endregion

    }
}
