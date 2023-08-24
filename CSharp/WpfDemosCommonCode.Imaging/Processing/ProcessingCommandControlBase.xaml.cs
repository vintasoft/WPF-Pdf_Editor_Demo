using System;
using System.Windows;
using System.Windows.Controls;

using Vintasoft.Imaging.Processing;
using Vintasoft.Imaging.Utils;
using System.ComponentModel;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A form that allows to view the processing commands.
    /// </summary>
    public abstract partial class ProcessingCommandControlBase : UserControl
    {

        #region Fields

        /// <summary>
        /// The parent window.
        /// </summary>
        Window _parentWindow = null;

        /// <summary>
        /// The result of processing command.
        /// </summary>
        ProcessingResult _processingCommandResult = null;

        #endregion



        #region Constructors

        /// <summary>
        /// Prevents a default instance of
        /// the <see cref="ProcessingCommandControlBase"/> class from being created.
        /// </summary>
        /// <param name="processingCommands">The processing commands.</param>
        protected ProcessingCommandControlBase()
        {
            InitializeComponent();

            using (ProcessingState processingState = new ProcessingState())
            {
                // get the default values 
                StorePredicateResults = processingState.StorePredicateResults;
                ThrowTriggerActivatedException = processingState.ThrowTriggerActivatedException;
            }
        }

        #endregion



        #region Properties

        IProcessingCommandInfo[] _processingCommands = null;
        /// <summary>
        /// Gets or sets the processing commands.
        /// </summary>
        [Browsable(false)]
        public IProcessingCommandInfo[] ProcessingCommands
        {
            get
            {
                return _processingCommands;
            }
            set
            {
                if (_processingCommands != value)
                {
                    _processingCommands = value;

                    processingCommandViewer.ProcessingCommands = _processingCommands;

                    if (_processingCommands.Length > 0)
                        SelectedProcessingCommand = _processingCommands[0];
                    else
                        SelectedProcessingCommand = null;
                }

                UpdateUI();
            }
        }

        /// <summary>
        /// Gets or sets the selected processing command.
        /// </summary>
        [Browsable(false)]
        public IProcessingCommandInfo SelectedProcessingCommand
        {
            get
            {
                return processingCommandViewer.SelectedProcessingCommand;
            }
            set
            {
                processingCommandViewer.SelectedProcessingCommand = value;
            }
        }

        bool _storePredicateResults = false;
        /// <summary>
        /// Gets or sets a value indicating whether the processing results 
        /// must store predicate results of triggers, analyzer result comparers and etc.
        /// </summary>
        public bool StorePredicateResults
        {
            get
            {
                return _storePredicateResults;
            }
            set
            {
                _storePredicateResults = value;
            }
        }

        bool _throwTriggerActivatedException = false;
        /// <summary>
        /// Gets or sets a value indicating whether exception must be thrown if 
        /// an important trigger is activated.
        /// </summary>
        public bool ThrowTriggerActivatedException
        {
            get
            {
                return _throwTriggerActivatedException;
            }
            set
            {
                _throwTriggerActivatedException = value;
            }
        }

        bool _updateCommandViewerAfterPropertyChanged = false;
        /// <summary>
        /// Gets or sets a value indicating whether command viewer must
        /// be updated after selected command property is changed.
        /// </summary>
        /// <value>
        /// <b>true</b> if command viewer must be updated after selected command property is changed; otherwise, <b>false</b>.
        /// </value>
        public bool UpdateCommandViewerAfterPropertyChanged
        {
            get
            {
                return _updateCommandViewerAfterPropertyChanged;
            }
            set
            {
                _updateCommandViewerAfterPropertyChanged = value;
            }
        }

        #endregion



        #region Methods

        #region PROTECTED

        /// <summary>
        /// The visual parent is changed.
        /// </summary>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            Window parentWindow = Window.GetWindow(this.Parent);
            if (parentWindow != null)
                parentWindow.Closed += new EventHandler(parentWindow_Closed);

            if (_parentWindow != null)
                _parentWindow.Closed -= parentWindow_Closed;
            _parentWindow = parentWindow;
        }

        /// <summary>
        /// Determines whether the specified command can be executed.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>
        /// <b>true</b> if the specified command can be executed;
        /// <b>false</b> if the specified command can NOT be executed.
        /// </returns>
        protected abstract bool CanExecute(IProcessingCommandInfo command);

        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="processingState">The state of processing.</param>
        /// <returns>The command processing result.</returns>
        protected abstract ProcessingResult Execute(
            IProcessingCommandInfo command,
            ProcessingState processingState);

        /// <summary>
        /// Processes the results.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>
        /// The changed result.
        /// </returns>
        protected virtual ProcessingResult ProcessResult(ProcessingResult result)
        {
            return result;
        }

        /// <summary>
        /// Refreshes the results on control.
        /// </summary>
        protected void RefreshResults()
        {
            if (_processingCommandResult == null)
                return;

            ProcessingResult result = ProcessResult(_processingCommandResult);
            ShowResults(result);
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Updates the user interface of this form.
        /// </summary>
        private void UpdateUI()
        {
            IProcessingCommandInfo selectedCommand = SelectedProcessingCommand;
            bool isCommandSelected = selectedCommand != null;

            executeButton.IsEnabled = isCommandSelected && CanExecute(selectedCommand);

            string executeText = "Execute";
            if (selectedCommand != null)
            {
                Type commandType = selectedCommand.GetType();

                if (ProcessingDemosTools.IsNameEqual(commandType, "Analyzer") ||
                    ProcessingDemosTools.IsNameEqual(commandType, "Verifier"))
                    executeText = "Analyze";
            }
            executeButton.Content = executeText;
        }

        /// <summary>
        /// Updates the processing command viewer.
        /// </summary>
        private void PropertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            if (UpdateCommandViewerAfterPropertyChanged)
                processingCommandViewer.UpdateProcessingCommandsTree();
        }

        /// <summary>
        /// The state of "View Processing Tree Structure" checkbox is changed.
        /// </summary>
        private void viewProcessingTreeStructureCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (viewProcessingTreeStructureCheckBox.IsChecked.Value == true)
                processingCommandViewer.ViewProcessingTreeStructure = true;
            else
                processingCommandViewer.ViewProcessingTreeStructure = false;
        }

        /// <summary>
        /// Shows properties of command before command is selected.
        /// </summary>
        private void processingCommandViewer_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UpdateUI();

            IProcessingCommandInfo command = SelectedProcessingCommand;
            propertyGrid.SelectedObject = command;

            string text = string.Empty;
            if (command != null)
                text = ProcessingDemosTools.GetReadableTypeName(command.GetType());
            propertyGridGroupBox.Header = text;

            if (SelectedProcessingCommandChanged != null)
                SelectedProcessingCommandChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// "Execute" button is clicked.
        /// </summary>
        private void executeButton_Click(object sender, RoutedEventArgs e)
        {
            ShowResults(null);

            string caption = SelectedProcessingCommand.ToString();
            ActionProgressWindow progressWindow =
                new ActionProgressWindow(ExecuteSelectedProcessingCommand, 3, caption);
            progressWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            progressWindow.CloseAfterComplete = true;
            progressWindow.RunAndShowDialog(_parentWindow);

            ProcessingResult result = _processingCommandResult;
            if (result != null)
                result = ProcessResult(result);
            ShowResults(result);

            if (showResultsAfretExecuteCheckBox.IsChecked.Value == true)
                mainTabControl.SelectedItem = resultTabPage;
        }

        /// <summary>
        /// Shows the results of processing command.
        /// </summary>
        /// <param name="result">The result.</param>
        private void ShowResults(ProcessingResult result)
        {
            string caption = string.Empty;
            if (result != null)
                caption = SelectedProcessingCommand.ToString();
            resultGroupBox.Header = caption;

            ProcessingResult prevousResult = processingResultViewer.ProcessingResult;

            processingResultViewer.ProcessingResult = result;

            if (prevousResult != null &&
                prevousResult != _processingCommandResult)
                prevousResult.Dispose();
        }

        /// <summary>
        /// Executes the selected processing command.
        /// </summary>
        /// <param name="progressController">The progress controller.</param>
        private void ExecuteSelectedProcessingCommand(IActionProgressController progressController)
        {
            IProcessingCommandInfo selectedProcessingCommandInfo = SelectedProcessingCommand;
            if (_processingCommandResult != null)
            {
                _processingCommandResult.Dispose();
                _processingCommandResult = null;
            }

            using (ProcessingState processingState = new ProcessingState(progressController))
            {
                processingState.StorePredicateResults = StorePredicateResults;
                processingState.ThrowTriggerActivatedException = ThrowTriggerActivatedException;
                _processingCommandResult = Execute(selectedProcessingCommandInfo, processingState);
            }
        }

        /// <summary>
        /// Selected page is changed.
        /// </summary>
        private void mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainTabControl.SelectedItem == processingTabPage)
                processingCommandViewer.Focus();
        }


        /// <summary>
        /// The parent window is closed.
        /// </summary>
        void parentWindow_Closed(object sender, EventArgs e)
        {
            ProcessingResult result = processingResultViewer.ProcessingResult;
            if (result != _processingCommandResult)
                result.Dispose();

            if (_processingCommandResult != null)
                _processingCommandResult.Dispose();
        }

        #endregion

        #endregion



        #region Events

        /// <summary>
        /// Occurs when the selected processing command is changed.
        /// </summary>
        public event EventHandler SelectedProcessingCommandChanged;

        #endregion

    }
}
