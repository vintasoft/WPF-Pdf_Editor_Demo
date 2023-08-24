using System;
using System.Collections.Generic;

using Vintasoft.Imaging.Pdf.Tree.Annotations;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// Controller of PDF annotation appearance generators.
    /// </summary>
    public class PdfAnnotationAppearanceGeneratorController
    {

        #region Fields

        /// <summary>
        /// Dictionary: object => PDF annotation appearance generator.
        /// </summary>
        Dictionary<object, PdfAnnotationAppearanceGenerator> _appearanceGenerators =
            new Dictionary<object, PdfAnnotationAppearanceGenerator>();

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of
        /// the <see cref="PdfAnnotationAppearanceGeneratorController"/> class.
        /// </summary>
        public PdfAnnotationAppearanceGeneratorController()
        {
        }

        #endregion



        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="PdfAnnotationAppearanceGenerator"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="PdfAnnotationAppearanceGenerator"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>The <see cref="PdfAnnotationAppearanceGenerator"/>.</returns>
        public PdfAnnotationAppearanceGenerator this[object key]
        {
            get
            {
                return GetAppearanceGenerator(key);
            }
            set
            {
                SetAppearanceGenerator(key, value);
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Determines whether the controller contains the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><b>True</b> if controller contains the specified key;
        /// otherwise, <b>false</b>.</returns>
        public bool ContainsKey(object key)
        {
            return _appearanceGenerators.ContainsKey(key);
        }

        /// <summary>
        /// Tries to get the value for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><b>True</b> if value is received;
        /// otherwise, <b>false</b>.</returns>
        public bool TryGetValue(object key, out PdfAnnotationAppearanceGenerator value)
        {
            value = GetAppearanceGenerator(key);
            return value != null;
        }

        /// <summary>
        /// Returns the PDF annotation appearance generator for the specified PDF annotation view.
        /// </summary>
        /// <param name="view">The annotation view.</param>
        /// <returns>The PDF annotation appearance generator</returns>
        /// <exception cref="ArgumentNullException">Thrown if
        /// <i>key</i> is <b>null</b>.</exception>
        public PdfAnnotationAppearanceGenerator GetAppearanceGenerator(object key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (!_appearanceGenerators.ContainsKey(key))
                return null;
            return _appearanceGenerators[key];
        }

        /// <summary>
        /// Sets the PDF annotation appearance generator for the specified PDF annotation view.
        /// </summary>
        /// <param name="view">The PDF annotation view.</param>
        /// <param name="appearanceGenerator">The PDF annotation appearance generator.</param>
        /// <exception cref="ArgumentNullException">Thrown if
        /// <i>key</i> is <b>null</b>.</exception>
        public void SetAppearanceGenerator(
            object key,
            PdfAnnotationAppearanceGenerator appearanceGenerator)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            _appearanceGenerators[key] = appearanceGenerator;
        }

        #endregion

    }
}
