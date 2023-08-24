using System;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Processing;

using WpfDemosCommonCode.Imaging;


namespace WpfDemosCommonCode.Pdf
{
    /// <summary>
    /// A window that allows to view the PDF document processing commands.
    /// </summary>
    public partial class DocumentProcessingCommandWindow : Window
    {

        #region Fields

        /// <summary>
        /// The preview selected menu item.
        /// </summary>
        MenuItem _prevMenuItem = null;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentProcessingCommandWindow"/> class.
        /// </summary>
        /// <param name="processingTarget">The processing target.</param>
        /// <param name="processingCommands">The processing commands.</param>
        public DocumentProcessingCommandWindow(
            PdfDocument processingTarget,
            params IProcessingCommand<PdfDocument>[] processingCommands)
        {
            InitializeComponent();

            detailedMenuItem.Tag = ProcessingResultTreeType.Detailed;
            byPageMenuItem.Tag = ProcessingResultTreeType.ByPage;
            UpdateProcessingResultViewType(detailedMenuItem);

            documentProcessingCommandControl.ProcessingTarget = processingTarget;
            documentProcessingCommandControl.ProcessingCommands = processingCommands;

            decreaseMemoryUsageMenuItem.IsChecked = !documentProcessingCommandControl.StorePredicateResults;
            fastModeMenuItem.IsChecked = documentProcessingCommandControl.ThrowTriggerActivatedException;
        }

        #endregion



        #region Porperties

        /// <summary>
        /// Gets or sets a value indicating whether command viewer must
        /// be updated after selected command property changed.
        /// </summary>
        /// <value>
        /// <b>true</b> if command viewer must be updated after selected command property changed; otherwise, <b>false</b>.
        /// </value>
        public bool UpdateCommandViewerAfterPropertyChanged
        {
            get
            {
                return documentProcessingCommandControl.UpdateCommandViewerAfterPropertyChanged;
            }
            set
            {
                documentProcessingCommandControl.UpdateCommandViewerAfterPropertyChanged = value;
            }
        }

        #endregion



        #region Methods

        #region PUBLIC, STATIC

        /// <summary>
        /// Builds the menu.
        /// </summary>
        /// <param name="rootMenu">The root menu.</param>
        /// <param name="rootNamespace">The root namespace.</param>
        /// <param name="targetDelegate">The target delegate.</param>
        /// <param name="processingCommands">The processing commands.</param>
        public static void BuildMenu(
            MenuItem rootMenu,
            string rootNamespace,
            ProcessingCommandWindow<PdfDocument>.GetProcessingTargetDelegate targetDelegate,
            params IProcessingCommand<PdfDocument>[] processingCommands)
        {
            ProcessingCommandWindow<PdfDocument>.BuildMenu(rootMenu, rootNamespace, targetDelegate,
                CreateProcessingCommandsWindow, processingCommands);
        }

        /// <summary>
        /// Executes the processing of PDF document using the specified processing command.
        /// </summary>
        /// <param name="processingTarget">The target of processing.</param>
        /// <param name="command">The processing command.</param>
        public static void ExecuteDocumentProcessing(
            PdfDocument processingTarget,
            IProcessingCommand<PdfDocument> command)
        {
            ExecuteDocumentProcessing(processingTarget, command, true);
        }


        /// <summary>
        /// Executes the processing of PDF document using the specified processing command.
        /// </summary>
        /// <param name="processingTarget">The target of processing.</param>
        /// <param name="command">The processing command.</param>
        /// <param name="showDialog">Indicates whether need show processing dialog.</param>
        public static void ExecuteDocumentProcessing(
            PdfDocument processingTarget,
            IProcessingCommand<PdfDocument> command,
            bool showDialog)
        {
            ExecuteDocumentProcessing(processingTarget, command, showDialog, false);
        }

        /// <summary>
        /// Executes the processing of PDF document using the specified processing command.
        /// </summary>
        /// <param name="processingTarget">The target of processing.</param>
        /// <param name="command">The processing command.</param>
        /// <param name="showDialog">Indicates whether need show processing dialog.</param>
        /// <param name="updateCommandViewerAfterPropertyChanged">
        /// Indicates whether the processing command viewer must be updated when property is changed.
        /// </param>
        public static void ExecuteDocumentProcessing(
            PdfDocument processingTarget,
            IProcessingCommand<PdfDocument> command,
            bool showDialog,
            bool updateCommandViewerAfterPropertyChanged)
        {
            if (showDialog)
            {
                DocumentProcessingCommandWindow window = new DocumentProcessingCommandWindow(processingTarget, command);
                window.Owner = Application.Current.MainWindow;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.UpdateCommandViewerAfterPropertyChanged = updateCommandViewerAfterPropertyChanged;
                window.ShowDialog();
            }
            else
            {
                ProcessingDemosTools.ExecuteProcessing(processingTarget, command);
            }
        }

        #endregion


        #region PRIVATE, STATIC

        /// <summary>
        /// Creates the processing commands window.
        /// </summary>
        /// <param name="processingTarget">The processing target.</param>
        /// <param name="processingCommands">The processing commands.</param>
        private static Window CreateProcessingCommandsWindow(
            PdfDocument processingTarget,
            IProcessingCommand<PdfDocument>[] processingCommands)
        {
            return new DocumentProcessingCommandWindow(processingTarget, processingCommands);
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Changes the view type of processing result tree.
        /// </summary>
        private void ProcessingResultViewToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            UpdateProcessingResultViewType((MenuItem)sender);
        }

        /// <summary>
        /// Updates the type of the processing result view.
        /// </summary>
        /// <param name="menuItem">The menu item.</param>
        private void UpdateProcessingResultViewType(MenuItem menuItem)
        {
            // if view type is not changed
            if (_prevMenuItem == menuItem)
                return;

            if (_prevMenuItem != null)
                // uncheck preview menu item
                _prevMenuItem.IsChecked = false;

            // check current menu item
            menuItem.IsChecked = true;
            // change view type
            documentProcessingCommandControl.ViewType = (ProcessingResultTreeType)menuItem.Tag;
            _prevMenuItem = menuItem;
        }

        /// <summary>
        /// Property <see cref="StorePredicateResults.StorePredicateResults"/> 
        /// of <see cref="StorePredicateResults"/> is changed.
        /// </summary>
        private void decreaseMemoryUsageToolStripMenuItem_CheckedChanged(object sender, RoutedEventArgs e)
        {
            documentProcessingCommandControl.StorePredicateResults = !decreaseMemoryUsageMenuItem.IsChecked;
        }

        /// <summary>
        /// Property <see cref="StorePredicateResults.ThrowTriggerActivatedException"/> 
        /// of <see cref="StorePredicateResults"/> is changed.
        /// </summary>
        private void fastModeToolStripMenuItem_CheckedChanged(object sender, RoutedEventArgs e)
        {
            documentProcessingCommandControl.ThrowTriggerActivatedException = fastModeMenuItem.IsChecked;
        }

        #endregion

        #endregion

    }
}
