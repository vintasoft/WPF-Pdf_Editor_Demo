using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WpfDemosCommonCode
{
    /// <summary>
    /// Represents an editor form of item set.
    /// </summary>
    public partial class ItemSetEditorWindow : Window
    {

        #region Fields

        /// <summary>
        /// Cached item names.
        /// </summary>
        string[] _itemNames;

        /// <summary>
        /// Indicates when selected item is changing.
        /// </summary>
        bool _selectedItemChanging = false;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsSetEditorWindow"/> class.
        /// </summary>
        public ItemSetEditorWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsSetEditorWindow"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public ItemSetEditorWindow(ItemSet items)
            : this()
        {
            Items = items;
        }

        #endregion



        #region Properties

        ItemSet _items = null;
        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public ItemSet Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
                grid1.IsEnabled = _items != null;
            }
        }

        #endregion



        #region Methods

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            UpdateUI();
        }

        /// <summary>
        /// Changes selected item in property grid.
        /// </summary>
        private void itemListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_selectedItemChanging)
            {
                _selectedItemChanging = true;
                int index = itemListBox.SelectedIndex;
                if (index >= 0)
                {
                    itemPropertyGrid.SelectedObject = _items.GetItem(_itemNames[index]);
                    if (nameTextBox.Text != _itemNames[index])
                        nameTextBox.Text = _itemNames[index];
                }
                else
                {
                    itemPropertyGrid.SelectedObject = null;
                    nameTextBox.Text = "";
                }
                _selectedItemChanging = false;
            }
        }

        /// <summary>
        /// Closes editor.
        /// </summary>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Updates the User Interface.
        /// </summary>
        private void UpdateUI()
        {
            ICollection<string> itemNames = _items.GetItemNames();
            _itemNames = new string[itemNames.Count];
            itemNames.CopyTo(_itemNames, 0);
            int selectedIndex = itemListBox.SelectedIndex;
            itemListBox.BeginInit();
            itemListBox.Items.Clear();
            foreach (string name in _itemNames)
                itemListBox.Items.Add(name);
            if (selectedIndex >= 0)
                itemListBox.SelectedIndex = Math.Min(_itemNames.Length - 1, selectedIndex);
            itemListBox.EndInit();
        }

        /// <summary>
        /// Deletes selected item.
        /// </summary>
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = itemListBox.SelectedIndex;
            if (index >= 0)
            {
                _items.RemoveItem(_itemNames[index]);
                UpdateUI();
            }
        }

        /// <summary>
        /// Adds new item.
        /// </summary>
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            string name = _items.AddNewItem();
            if (name != null)
            {
                UpdateUI();
                itemListBox.SelectedIndex = Array.IndexOf(_itemNames, name);
            }
        }

        /// <summary>
        /// Renames the item.
        /// </summary>
        private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_selectedItemChanging)
            {
                int index = itemListBox.SelectedIndex;
                if (index >= 0 && _itemNames[index] != nameTextBox.Text)
                {
                    try
                    {
                        _items.RenameItem(_itemNames[index], nameTextBox.Text);
                        _itemNames[index] = nameTextBox.Text;
                        _selectedItemChanging = true;
                        itemListBox.Items[index] = _itemNames[index];
                    }
                    catch (Exception ex)
                    {
                        DemosTools.ShowErrorMessage(ex);
                    }
                    _selectedItemChanging = false;
                }
            }
        }

        #endregion

    }
}
