using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Vintasoft.Imaging.Processing;
using Vintasoft.Imaging.Utils;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A form that allows to view the processing commands.
    /// </summary>
    public class ProcessingCommandWindow<TTarget> : Window
    {

        #region Nested Class

        /// <summary>
        /// Contains the parameters for a preview form of processing command.
        /// </summary>
        private class ProcessingCommandFormParameters
        {

            #region Fields

            /// <summary>
            /// The delegate that allows to get the target of processing command.
            /// </summary>
            GetProcessingTargetDelegate _getProcessingTargetDelegate = null;

            /// <summary>
            /// The delegate that allows to create a preview form and
            /// execute the processing command.
            /// </summary>
            CreateProcessingCommandsFormDelegate _createProcessingCommandsFormDelegate = null;

            #endregion



            #region Constructors

            /// <summary>
            /// Initializes a new instance of
            /// the <see cref="ProcessingCommandFormParameters"/> class.
            /// </summary>
            /// <param name="getTargetDelegate">
            /// The delegate that allows to get the target of processing command.
            /// </param>
            /// <param name="createProcessingCommandsFormDelegate">
            /// The delegate that allows to create a preview form and
            /// execute the processing command.
            /// </param>
            /// <param name="processingCommand">The processing command.</param>
            internal ProcessingCommandFormParameters(
                GetProcessingTargetDelegate getTargetDelegate,
                CreateProcessingCommandsFormDelegate createProcessingCommandsFormDelegate,
                IProcessingCommand<TTarget> processingCommand)
            {
                _getProcessingTargetDelegate = getTargetDelegate;
                _createProcessingCommandsFormDelegate = createProcessingCommandsFormDelegate;
                _processingCommand = processingCommand;
            }

            #endregion



            #region Properties

            IProcessingCommand<TTarget> _processingCommand = null;
            /// <summary>
            /// Gets the processing command.
            /// </summary>
            internal IProcessingCommand<TTarget> ProcessingCommand
            {
                get
                {
                    return _processingCommand;
                }
            }

            #endregion



            #region Methods

            /// <summary>
            /// Shows the processing command window.
            /// </summary>
            internal void ShowProcessingCommandWindow()
            {
                // get target
                TTarget target = _getProcessingTargetDelegate();
                // if target is empty
                if (target == null)
                    return;

                if (!ProcessingCommand.CanModifyTarget &&
                    !(ProcessingCommand is ProcessingProfile<TTarget>))
                {
                    ProcessingDemosTools.ExecuteProcessing<TTarget>(target, ProcessingCommand);
                }
                else
                {
                    // create window for view processing command
                    Window window = _createProcessingCommandsFormDelegate(target, new IProcessingCommand<TTarget>[] { ProcessingCommand });
                    window.Owner = Application.Current.MainWindow;
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.ShowDialog();
                }
            }

            #endregion

        }

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingCommandForm{TTarget}"/> class.
        /// </summary>
        /// <param name="processingTarget">The processing target.</param>
        /// <param name="processingCommands">The processing commands.</param>
        public ProcessingCommandWindow(
            TTarget processingTarget,
            params IProcessingCommand<TTarget>[] processingCommands)
            : base()
        {
            Title = string.Format("{0} processing", ProcessingDemosTools.GetReadableTypeName(typeof(TTarget)));

            if (processingTarget == null)
                throw new ArgumentNullException("processingTarget");

            ProcessingCommandControl<TTarget> processingCommandControl =
                new ProcessingCommandControl<TTarget>();

            this.Content = processingCommandControl;

            this.MinWidth = processingCommandControl.MinWidth + 50;
            this.MinHeight = processingCommandControl.MinHeight + 50;
            this.Width = 500;
            this.Height = 600;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            processingCommandControl.ProcessingTarget = processingTarget;
            processingCommandControl.ProcessingCommands = processingCommands;
        }

        #endregion



        #region Methods

        #region PUBLIC, STATIC

        /// <summary>
        /// Executes the processing of PDF document using the specified processing command.
        /// </summary>
        /// <param name="processingTarget">The target of processing.</param>
        /// <param name="command">The processing command.</param>
        /// <param name="showDialog">Indicates whether need show processing dialog.</param>
        public static void ExecuteProcessing(
            TTarget processingTarget,
            IProcessingCommand<TTarget> command,
            bool showDialog)
        {
            if (showDialog)
            {
                ProcessingCommandWindow<TTarget> window =
                    new ProcessingCommandWindow<TTarget>(processingTarget, command);
                window.Owner = Application.Current.MainWindow;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.ShowDialog();
            }
            else
            {
                ProcessingDemosTools.ExecuteProcessing(processingTarget, command);
            }
        }    

        /// <summary>
        /// Gets the full names of processing commands,
        /// processed the full names of processing commands
        /// by excluding the specified root namespace from fullnames of processing commands,
        /// creates the hierarchical submenus based on processed names of processing commands.
        /// </summary>
        /// <param name="rootMenu">The root menu, where new submenus must be added.</param>
        /// <param name="rootNamespace">The root namespace, which must be excluded from
        /// the full names of processing commands.</param>
        /// <param name="getProcessingTargetDelegate">The delegate that allows to get
        /// the target for the processing commands.</param>
        /// <param name="processingCommands">The processing commands
        /// for which submenus must be created.</param>
        public static void BuildMenu(
            MenuItem rootMenu,
            string rootNamespace,
            GetProcessingTargetDelegate getProcessingTargetDelegate,
            params IProcessingCommand<TTarget>[] processingCommands)
        {
            // create the submenus for processing commands,
            // submeny will open a standard dialog for executing the processing command
            BuildMenu(rootMenu, rootNamespace, getProcessingTargetDelegate,
                CreateProcessingCommandsWindow, processingCommands);
        }

        /// <summary>
        /// Gets the full names of processing commands,
        /// processed the full names of processing commands
        /// by excluding the specified root namespace from fullnames of processing commands,
        /// creates the hierarchical submenus based on processed names of processing commands.
        /// </summary>
        /// <param name="rootMenu">The root menu, where new submenus must be added.</param>
        /// <param name="rootNamespace">The root namespace, which must be excluded from
        /// the full names of processing commands.</param>
        /// <param name="getProcessingTargetDelegate">The delegate that allows to get
        /// the target for the processing commands.</param>
        /// <param name="createProcessingFormDelegate">The delegate that allows to create
        /// a preview dialog for processing command.</param>
        /// <param name="processingCommands">The processing commands
        /// for which submenus must be created.</param>
        public static void BuildMenu(
            MenuItem rootMenu,
            string rootNamespace,
            GetProcessingTargetDelegate getProcessingTargetDelegate,
            CreateProcessingCommandsFormDelegate createProcessingFormDelegate,
            params IProcessingCommand<TTarget>[] processingCommands)
        {
            // if processing commands are not specified
            if (processingCommands == null)
                return;

            // for each processing command
            foreach (IProcessingCommand<TTarget> processingCommand in processingCommands)
            {
                // if processing command is empty
                if (processingCommand == null)
                    continue;

                // get type of processing command
                Type processingCommandType = processingCommand.GetType();
                // get namespace of processing command
                string commandNamespace = processingCommandType.Namespace;
                // if namespace of processing is not empty
                if (!String.IsNullOrEmpty(rootNamespace))
                {
                    // exclude the root namespace from the full name of processing command

                    // if root namespace equals the namespace of processing command
                    if (commandNamespace == rootNamespace)
                        commandNamespace = string.Empty;
                    // if current namespace contains root namespace
                    else if (commandNamespace.StartsWith(rootNamespace))
                        // remove root namespace
                        commandNamespace = commandNamespace.Substring(rootNamespace.Length + 1);
                }

                // get the names of submenus for processing command

                Queue<string> commandNamespacesQueue = new Queue<string>();
                string[] commandNamespaces =
                    commandNamespace.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (commandNamespaces.Length != 0)
                {
                    for (int i = 0; i < commandNamespaces.Length; i++)
                        commandNamespacesQueue.Enqueue(commandNamespaces[i]);
                }

                // creates an object with information that allows to create a prview form for processing command
                ProcessingCommandFormParameters processingCommandFormParameters =
                    new ProcessingCommandFormParameters(
                        getProcessingTargetDelegate,
                        createProcessingFormDelegate,
                        processingCommand);
                // create the hierarchical submenus for processing command
                CreateProcessingCommandMenu(
                    rootMenu,
                    commandNamespacesQueue,
                    processingCommandFormParameters);
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
            TTarget processingTarget,
            IProcessingCommand<TTarget>[] processingCommands)
        {
            return new ProcessingCommandWindow<TTarget>(processingTarget, processingCommands);
        }

        /// <summary>
        /// Creates the hierarchical submenus for processing command.
        /// </summary>
        /// <param name="rootMenu">The root menu, where new submenus must be added.</param>
        /// <param name="parentMenuNames">The parent menu names.</param>
        /// <param name="processingCommandFormParameters">The parameters
        /// for a preview form of processing command.</param>
        private static MenuItem CreateProcessingCommandMenu(
            MenuItem rootMenu,
            Queue<string> parentMenuNames,
            ProcessingCommandFormParameters processingCommandFormParameters)
        {
            // current menu
            MenuItem menu = null;

            // if menu does not have parent menus
            if (parentMenuNames.Count == 0)
            {
                // get the processing command
                IProcessingCommand<TTarget> processingCommand = processingCommandFormParameters.ProcessingCommand;

                // create menu for processing command
                menu = new MenuItem();
                menu.Header = string.Format("{0}...", processingCommand.Name);
                menu.Click += new RoutedEventHandler(processingMenuItem_Click);
                menu.Tag = processingCommandFormParameters;

                Image icon = new Image();
                icon.Width = 16;
                icon.Height = 16;
                icon.Stretch = System.Windows.Media.Stretch.None;
                icon.Source = ProcessingCommandViewer.GetImageResource(processingCommand);
                menu.Icon = icon;
            }
            // if menu has parent menus
            else
            {
                // get the the parent menu name
                string parentMenuName = parentMenuNames.Dequeue();
                // find the parent menu, in the root menu, by menu name
                menu = FindMenu(rootMenu, parentMenuName);
                // if the parent menu is not found
                if (menu == null)
                {
                    // create the parent menu
                    menu = new MenuItem();
                    menu.Header = parentMenuName;
                }

                // create the hierarchical submenus for processing command
                CreateProcessingCommandMenu(menu, parentMenuNames, processingCommandFormParameters);
            }

            if (menu.Parent == null)
                // add the menu to the root menu
                rootMenu.Items.Add(menu);
            return menu;
        }

        /// <summary>
        /// Menu item of processing command is clicked.
        /// </summary>
        private static void processingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // get menu
            MenuItem menu = (MenuItem)sender;
            // get information that allows to create a preview form for processing command
            ProcessingCommandFormParameters info = menu.Tag as ProcessingCommandFormParameters;
            // if preview form for processing command can be created
            if (info != null)
                // create and show the preview form for processing command
                info.ShowProcessingCommandWindow();
        }

        /// <summary>
        /// Finds menu by menu name.
        /// </summary>
        /// <param name="rootMenu">The root menu, where menu must be searched.</param>
        /// <param name="menuName">The name of menu.</param>
        /// <returns>
        /// The menu instance if menu is found; otherwise, <b>null</b>.
        /// </returns>
        private static MenuItem FindMenu(MenuItem rootMenu, string menuName)
        {
            foreach (MenuItem item in rootMenu.Items)
            {
                MenuItem menuItem = item as MenuItem;
                if (menuItem != null &&
                    menuItem.Header is string &&
                    (string)menuItem.Header == menuName)
                    return menuItem;
            }

            return null;
        }

        #endregion

        #endregion



        #region Delegates

        /// <summary>
        /// Delegat that allows to get the processing target.
        /// </summary>
        public delegate TTarget GetProcessingTargetDelegate();

        /// <summary>
        /// Delegat that allows to create the processing window.
        /// </summary>
        /// <param name="processingTarget">The processing target.</param>
        /// <param name="processingCommands">The processing commands.</param>
        public delegate Window CreateProcessingCommandsFormDelegate(
            TTarget processingTarget,
            IProcessingCommand<TTarget>[] processingCommands);

        #endregion

    }
}
