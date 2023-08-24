using System;
using System.Collections.Generic;

using Vintasoft.Imaging.Pdf.Tree;

namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Represents a set of name-value pairs.
    /// </summary>
    public class PdfNamedDictionaryItemSet<T> : ItemSet
         where T : PdfTreeNodeBase
    {

        #region Fields

        /// <summary>
        /// The delegate for adding new item to a dictionary.
        /// </summary>
        AddNewItemDelegate _addNewItem;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfNamedDictionaryItemSet{T}"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary that should be managed.</param>
        /// <param name="addNewItem">The delegate for adding new item to a dictionary.</param>
        public PdfNamedDictionaryItemSet(PdfNamedDictionary<T> dictionary, AddNewItemDelegate addNewItem)
        {
            _dictionary = dictionary;
            _addNewItem = addNewItem;
        }

        #endregion



        #region Properties

        PdfNamedDictionary<T> _dictionary;
        /// <summary>
        /// Gets the dictionary that should be managed.
        /// </summary>
        protected PdfNamedDictionary<T> Dictionary
        {
            get
            {
                return _dictionary;
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Gets the collection that contains names of the items.
        /// </summary>
        /// <returns>
        /// Collection that contains item names.
        /// </returns>
        public override ICollection<string> GetItemNames()
        {
            return _dictionary.Keys;
        }

        /// <summary>
        /// Gets the item by name.
        /// </summary>
        /// <param name="name">The item name.</param>
        /// <returns>
        /// The item.
        /// </returns>
        public override object GetItem(string name)
        {
            return _dictionary[name];
        }

        /// <summary>
        /// Adds the new item.
        /// </summary>
        /// <returns>
        /// Name of new item.
        /// </returns>
        public override void RenameItem(string oldName, string newName)
        {
            if (_dictionary.ContainsKey(newName))
                throw new ArgumentException(string.Format("Key with name '{0}' already exsists in dictionary.", newName));
            _dictionary.Add(newName, _dictionary[oldName]);
            _dictionary.Remove(oldName);
        }

        /// <summary>
        /// Renames the item.
        /// </summary>
        /// <param name="oldName">The old name.</param>
        /// <param name="newName">The new name.</param>
        public override string AddNewItem()
        {
            return _addNewItem(Dictionary);
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="name">The item name.</param>
        public override void RemoveItem(string name)
        {
            _dictionary.Remove(name);
        }

        #endregion



        #region Delegates

        /// <summary>
        /// Delegate that adds new item to a dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary where new item must be added.</param>
        /// <returns>Name of added item.</returns>
        public delegate string AddNewItemDelegate(PdfNamedDictionary<T> dictionary);

        #endregion

    }
}
