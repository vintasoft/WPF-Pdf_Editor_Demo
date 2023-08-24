using System.Collections.Generic;

using Vintasoft.Imaging.Processing;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A control that allows to view the processing commands.
    /// </summary>
    public class ProcessingCommandControl<TTarget> : ProcessingCommandControlBase
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingCommandControl{TTarget}"/> class.
        /// </summary>
        public ProcessingCommandControl()
            : base()
        {
        }

        #endregion



        #region Properties

        TTarget _processingTarget = default(TTarget);
        /// <summary>
        /// Gets or sets the processing target.
        /// </summary>
        public TTarget ProcessingTarget
        {
            get
            {
                return _processingTarget;
            }
            set
            {
                _processingTarget = value;
            }
        }

        #endregion



        #region Methods

        #region PROTECTED

        /// <summary>
        /// Determines whether the specified command can be executed.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>
        /// <b>true</b> if the specified command can be executed;
        /// <b>false</b> if the specified command can NOT be executed.
        /// </returns>
        protected override bool CanExecute(IProcessingCommandInfo command)
        {
            if (_processingTarget != null && GetCommand(command) != null)
                return true;

            return false;
        }

        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="processingState">The state of processing.</param>
        /// <returns>The command processing result.</returns>
        protected override ProcessingResult Execute(
            IProcessingCommandInfo command,
            ProcessingState processingState)
        {
            IProcessingCommand<TTarget> selectedProcessingCommand = null;
            if (System.Threading.Thread.CurrentThread == Dispatcher.Thread)
            {
                selectedProcessingCommand = GetCommand(command);
            }
            else
            {
                selectedProcessingCommand = 
                    (IProcessingCommand<TTarget>)Dispatcher.Invoke(new GetCommandDelegate(GetCommand), command);
            }

            ProcessingResult result = null;
            if (selectedProcessingCommand != null)
            {
                try
                {
                    result = selectedProcessingCommand.Execute(_processingTarget, processingState);
                }
                catch (TriggerActivationException ex)
                {
                    result = ex.ActivationResult;
                }
            }

            return result;
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <param name="command">Information about the processing command.</param>
        /// <returns>The processing command.</returns>
        private IProcessingCommand<TTarget> GetCommand(IProcessingCommandInfo commandInfo)
        {
            IProcessingCommand<TTarget> result = commandInfo as IProcessingCommand<TTarget>;
            if (result == null)
            {
                ProcessingTreeViewItem node = processingCommandViewer.GetNodeFromProcessingCommand(commandInfo);
                if (node != null)
                {
                    ProcessingCommandTree<TTarget> processingCommandTree = null;
                    do
                    {
                        processingCommandTree =
                            processingCommandViewer.GetProcessingCommandFromNode(node) as ProcessingCommandTree<TTarget>;
                        node = node.Parent as ProcessingTreeViewItem;
                    }
                    while (node != null && processingCommandTree == null);

                    if (processingCommandTree != null)
                    {
                        try
                        {
                            List<IProcessingCommand<TTarget>> processingTree =
                                processingCommandTree.BuildProcessingTree(new IProcessingCommandInfo[] { commandInfo });

                            if (processingTree != null && processingTree.Count > 0)
                            {
                                if (processingTree.Count == 1)
                                {
                                    return processingTree[0];
                                }
                                else
                                {
                                    CompositeProcessingCommand<TTarget> compositeCommand =
                                        new CompositeProcessingCommand<TTarget>(processingTree);
                                    return compositeCommand;
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #endregion



        #region Delegates

        private delegate IProcessingCommand<TTarget> GetCommandDelegate(IProcessingCommandInfo commandInfo);

        #endregion

    }
}
