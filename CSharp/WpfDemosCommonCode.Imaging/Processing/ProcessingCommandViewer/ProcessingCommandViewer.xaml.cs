using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Pdf.Processing.PdfA;
using Vintasoft.Imaging.Processing;
using Vintasoft.Imaging.Processing.Analyzers;
using Vintasoft.Imaging.Wpf;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A control that allows to view processing commands.
    /// </summary>
    public partial class ProcessingCommandViewer : TreeView
    {

        #region Constants

        /// <summary>
        /// Key of the default node image.
        /// </summary>
        private const string IMAGE_KEY_DEFAULT = "ProcessingCommandDefault";

        /// <summary>
        /// Key of the property node image.
        /// </summary>
        private const string IMAGE_KEY_PROPERTY = "ProcessingCommandProperty";

        /// <summary>
        /// Key of the command image.
        /// </summary>
        internal const string IMAGE_KEY_COMMAND = "Command";

        /// <summary>
        /// Key of the composite command image.
        /// </summary>
        private const string IMAGE_KEY_COMPOSITE_COMMAND = "CompositeCommand";

        /// <summary>
        /// Image key for the verifier command.
        /// </summary>
        internal const string IMAGE_KEY_VERIFIER_COMMAND = "VerificationProfile";

        /// <summary>
        /// Image key for the analyzer command.
        /// </summary>
        private const string IMAGE_KEY_ANALYZER_COMMAND = "Analyzer";

        /// <summary>
        /// Image key for the conditional command.
        /// </summary>
        private const string IMAGE_KEY_CONDITIONAL_COMMAND = "ConditionalCommand";

        /// <summary>
        /// Image key for the converter command.
        /// </summary>
        internal const string IMAGE_KEY_CONVERTER_COMMAND = "ConversionProfile";

        /// <summary>
        /// Image key for trigger command error.
        /// </summary>
        private const string IMAGE_KEY_TRIGGER_COMMAND_ERROR = "TriggerImportant";

        /// <summary>
        /// Image key for trigger command warning.
        /// </summary>
        private const string IMAGE_KEY_TRIGGER_COMMAND_WARNING = "TriggerUnimportant";

        /// <summary>
        /// Image key for trigger command information.
        /// </summary>
        private const string IMAGE_KEY_TRIGGER_COMMAND_INFORMATION = "TriggerInformation";

        /// <summary>
        /// Image key for predicate command.
        /// </summary>
        private const string IMAGE_KEY_PREDICATE_COMMAND = "Predicate";

        /// <summary>
        /// Image key for property getter command.
        /// </summary>
        private const string IMAGE_KEY_PROPERTY_GETTER = "PropertyGetter";

        /// <summary>
        /// Image key for the property setter command.
        /// </summary>
        private const string IMAGE_KEY_PROPERTY_SETTER = "PropertySetter";

        /// <summary>
        /// Image key for the composite predicate analyzer.
        /// </summary>
        private const string IMAGE_KEY_COMPOSITE_PREDICATE_ANALYZER = "CompositePredicateAnalyzer";

        /// <summary>
        /// Image key for the fixup command.
        /// </summary>
        private const string IMAGE_KEY_FIXUP_COMMAND = "FixupCommand";

        #endregion



        #region Fields

        /// <summary>
        /// Tree node to the processing command table.
        /// </summary>
        Dictionary<ProcessingTreeViewItem, IProcessingCommandTreeInfo> _processingCommandTreeInfo =
            new Dictionary<ProcessingTreeViewItem, IProcessingCommandTreeInfo>();

        /// <summary>
        /// Indicates that the processing command is adding now.
        /// </summary>
        int _addProcessingCommandTreeInfo = 0;

        /// <summary>
        /// Context menu of tree node.
        /// </summary>
        ContextMenu _treeNodeMenu = new ContextMenu();

        /// <summary>
        /// Dictionary: the image key => the resource name.
        /// </summary>
        static Dictionary<string, string> _imageKeyToResourceName =
            new Dictionary<string, string>();

        /// <summary>
        /// The image resources.
        /// </summary>
        Dictionary<string, BitmapSource> _imageResources =
            new Dictionary<string, BitmapSource>();

        /// <summary>
        /// The added PDF/A processing.
        /// </summary>
        Dictionary<IProcessingCommandInfo, ProcessingTreeViewItem> _addedPdfaProcessing = new Dictionary<IProcessingCommandInfo, ProcessingTreeViewItem>();


        #endregion



        #region Constructors

        /// <summary>
        /// Initializes the <see cref="ProcessingCommandViewer"/> class.
        /// </summary>
        static ProcessingCommandViewer()
        {
            string resourceFormatName =
                "WpfDemosCommonCode.Imaging.Processing.ProcessingCommandViewer.Resources.{0}.png";

            _imageKeyToResourceName.Add(IMAGE_KEY_DEFAULT, string.Format(resourceFormatName, "DefaultCommand"));
            _imageKeyToResourceName.Add(IMAGE_KEY_PROPERTY, string.Format(resourceFormatName, "PropertyCommand"));
            _imageKeyToResourceName.Add(IMAGE_KEY_COMMAND, string.Format(resourceFormatName, "Command"));
            _imageKeyToResourceName.Add(IMAGE_KEY_CONDITIONAL_COMMAND, string.Format(resourceFormatName, "ConditionalCommand"));
            _imageKeyToResourceName.Add(IMAGE_KEY_VERIFIER_COMMAND, string.Format(resourceFormatName, "VerificationProfile"));
            _imageKeyToResourceName.Add(IMAGE_KEY_ANALYZER_COMMAND, string.Format(resourceFormatName, "Analyzer"));
            _imageKeyToResourceName.Add(IMAGE_KEY_CONVERTER_COMMAND, string.Format(resourceFormatName, "ConversionProfile"));
            _imageKeyToResourceName.Add(IMAGE_KEY_TRIGGER_COMMAND_ERROR, string.Format(resourceFormatName, "TriggerImportant"));
            _imageKeyToResourceName.Add(IMAGE_KEY_TRIGGER_COMMAND_WARNING, string.Format(resourceFormatName, "TriggerUnimportant"));
            _imageKeyToResourceName.Add(IMAGE_KEY_TRIGGER_COMMAND_INFORMATION, string.Format(resourceFormatName, "TriggerInformation"));
            _imageKeyToResourceName.Add(IMAGE_KEY_COMPOSITE_COMMAND, string.Format(resourceFormatName, "CompositeCommand"));
            _imageKeyToResourceName.Add(IMAGE_KEY_PREDICATE_COMMAND, string.Format(resourceFormatName, "Predicate"));
            _imageKeyToResourceName.Add(IMAGE_KEY_PROPERTY_GETTER, string.Format(resourceFormatName, "PropertyGetter"));
            _imageKeyToResourceName.Add(IMAGE_KEY_PROPERTY_SETTER, string.Format(resourceFormatName, "PropertySetter"));
            _imageKeyToResourceName.Add(IMAGE_KEY_COMPOSITE_PREDICATE_ANALYZER, string.Format(resourceFormatName, "CompositePredicateAnalyzer"));
            _imageKeyToResourceName.Add(IMAGE_KEY_FIXUP_COMMAND, string.Format(resourceFormatName, "FixupCommand"));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingCommandViewer"/> class.
        /// </summary>
        public ProcessingCommandViewer()
        {
            InitializeComponent();

            Assembly assembly = typeof(ProcessingCommandViewer).Module.Assembly;
            LoadImageResources(_imageResources);

            ProcessingCommands = null;

            _treeNodeMenu.Opened += new System.Windows.RoutedEventHandler(treeNodeMenu_Opened);

            MenuItem expandAllMenuItem = new MenuItem();
            expandAllMenuItem.Header = "Expand All";
            expandAllMenuItem.Click += new System.Windows.RoutedEventHandler(expandAllMenuItem_Click);
            _treeNodeMenu.Items.Add(expandAllMenuItem);

            MenuItem collapseAllMenuItem = new MenuItem();
            collapseAllMenuItem.Header = "Collapse All";
            collapseAllMenuItem.Click += new System.Windows.RoutedEventHandler(collapseAllMenuItem_Click);
            _treeNodeMenu.Items.Add(collapseAllMenuItem);

            if (!VirtualizingStackPanel.GetIsVirtualizing(this))
                VirtualizingStackPanel.SetIsVirtualizing(this, true);
        }

        #endregion



        #region Properties

        IProcessingCommandInfo[] _processingCommands = null;
        /// <summary>
        /// Gets or sets the processing commands.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public IProcessingCommandInfo[] ProcessingCommands
        {
            get
            {
                return _processingCommands;
            }
            set
            {
                _processingCommands = value;

                IsEnabled = _processingCommands != null && _processingCommands.Length > 0;

                BeginInit();
                BuildProcessingCommandsTree();
                EndInit();
            }
        }

        IProcessingCommandInfo _selectedProcessingCommand = null;
        /// <summary>
        /// Gets or sets the selected processing command.
        /// </summary>
        [Browsable(false)]
        public IProcessingCommandInfo SelectedProcessingCommand
        {
            get
            {
                return _selectedProcessingCommand;
            }
            set
            {
                ProcessingTreeViewItem node = FindNode(Items, value);
                if (node != null)
                    node.IsSelected = true;
                _selectedProcessingCommand = value;
            }
        }

        bool _viewProcessingTreeStructure = false;
        /// <summary>
        /// Gets or sets a value indicating whether the processing commands must be shown as a tree.
        /// </summary>
        /// <value>
        /// <b>true</b> - the processing commands must be shown as a tree;<br />
        /// <b>false</b> - the processing commands must be shown as a list.<br />
        /// Default value is <b>false</b>.
        /// </value>
        [Browsable(true)]
        [DefaultValue(false)]
        [Description("View processing tree structure.")]
        public bool ViewProcessingTreeStructure
        {
            get
            {
                return _viewProcessingTreeStructure;
            }
            set
            {
                if (_viewProcessingTreeStructure != value)
                {
                    bool prevValue = _viewProcessingTreeStructure;
                    _viewProcessingTreeStructure = value;

                    if (_processingCommandTreeInfo.Count > 0)
                    {
                        ProcessingTreeViewItem[] nodes = new ProcessingTreeViewItem[_processingCommandTreeInfo.Count];
                        _processingCommandTreeInfo.Keys.CopyTo(nodes, 0);
                        foreach (ProcessingTreeViewItem node in nodes)
                        {
                            IProcessingCommandTreeInfo command = _processingCommandTreeInfo[node];
                            node.BeginInit();

                            if (prevValue)
                                RemoveNodes(node.Items, (IEnumerable)command);
                            else
                                RemoveNodes(node.Items, ((IProcessingCommandTreeInfo)command).ProcessingTreeNodes);

                            if (_viewProcessingTreeStructure)
                                AddNodes(node.Items, (IEnumerable)command);
                            else
                                AddNodes(node.Items, ((IProcessingCommandTreeInfo)command).ProcessingTreeNodes);

                            node.EndInit();
                        }
                    }
                }
            }
        }

        #endregion



        #region Methods

        #region PROTECTED

        /// <summary>
        /// Raises the <see cref="E:SelectedItemChanged" /> event.
        /// </summary>
        protected override void OnSelectedItemChanged(System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedProcessingCommand = GetProcessingCommandFromNode(e.NewValue as ProcessingTreeViewItem);

            base.OnSelectedItemChanged(e);
        }

        #endregion


        #region INTERNAL

        /// <summary>
        /// Updates the processing commands tree.
        /// </summary>
        internal void UpdateProcessingCommandsTree()
        {
            BeginInit();

            IProcessingCommandInfo selectedCommand = SelectedProcessingCommand;

            // save expanded nodes to dictionary
            Dictionary<string, bool> expandedNodes = new Dictionary<string, bool>();
            foreach (TreeViewItem node in Items)
                AddExpandedNodes(node, expandedNodes);

            // update processing commands tree
            BuildProcessingCommandsTree();

            // restore tree nodes expand state
            foreach (TreeViewItem node in Items)
                UpdateNodeExpandState(node, expandedNodes);

            SelectedProcessingCommand = selectedCommand;

            EndInit();
        }

        /// <summary>
        /// Returns the processing command associated with specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        /// The processing command if command is found; otherwise, <b>null</b>.
        /// </returns>
        internal IProcessingCommandInfo GetProcessingCommandFromNode(ProcessingTreeViewItem node)
        {
            if (node == null)
                return null;

            return node.Tag as IProcessingCommandInfo;
        }

        /// <summary>
        /// Returns the node associated with the specified processing command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>The node.</returns>
        internal ProcessingTreeViewItem GetNodeFromProcessingCommand(IProcessingCommandInfo command)
        {
            return FindNode(Items, command);
        }

        /// <summary>
        /// Loads the image resources.
        /// </summary>
        /// <param name="imageList">The image list.</param>
        internal static void LoadImageResources(Dictionary<string, BitmapSource> imageList)
        {
            foreach (string key in _imageKeyToResourceName.Keys)
                AddImageResourceToImageList(imageList, key);
        }

        /// <summary>
        /// Returns the image resource for the specified processing command.
        /// </summary>
        /// <param name="command">The command.</param>
        internal static BitmapSource GetImageResource(IProcessingCommandInfo command)
        {
            string key = GetCommandImageKey(command);
            return GetImageResource(key);
        }

        /// <summary>
        /// Gets the key of image.
        /// </summary>
        /// <param name="command">The command.</param>
        internal static string GetCommandImageKey(IProcessingCommandInfo command)
        {
            string imageKey = IMAGE_KEY_COMMAND;

            if (command != null)
            {
                Type commandType = command.GetType();

                if (ProcessingDemosTools.IsNameEqual(commandType, "ConversionProfile"))
                    imageKey = IMAGE_KEY_CONVERTER_COMMAND;
                else if (ProcessingDemosTools.IsNameEqual(commandType, "VerificationProfile"))
                    imageKey = IMAGE_KEY_VERIFIER_COMMAND;
                else if (ProcessingDemosTools.IsNameEqual(commandType, "PropertyGetter"))
                    imageKey = IMAGE_KEY_PROPERTY_GETTER;
                else if (ProcessingDemosTools.IsNameEqual(commandType, "PropertySetter"))
                    imageKey = IMAGE_KEY_PROPERTY_SETTER;
                else if (ProcessingDemosTools.IsNameEqual(commandType, "CompositePredicateAnalyzer"))
                    imageKey = IMAGE_KEY_COMPOSITE_PREDICATE_ANALYZER;
                else if (ProcessingDemosTools.IsNameEqual(commandType, "PredicateAnalyzer"))
                    imageKey = IMAGE_KEY_PREDICATE_COMMAND;
                else if (ProcessingDemosTools.IsNameEqual(commandType, "Analyzer"))
                    imageKey = IMAGE_KEY_ANALYZER_COMMAND;
                else if (!command.Name.StartsWith("Fixups") && command.Name.StartsWith("Fixup"))
                    imageKey = IMAGE_KEY_FIXUP_COMMAND;
                else if (ProcessingDemosTools.IsNameEqual(commandType, "ConditionalCommand"))
                    imageKey = IMAGE_KEY_CONDITIONAL_COMMAND;
                else if (command is ITriggerInfo)
                {
                    ITriggerInfo trigger = (ITriggerInfo)command;
                    switch (trigger.Severity)
                    {
                        case TriggerSeverity.Important:
                            imageKey = IMAGE_KEY_TRIGGER_COMMAND_ERROR;
                            break;

                        case TriggerSeverity.Unimportant:
                            imageKey = IMAGE_KEY_TRIGGER_COMMAND_WARNING;
                            break;

                        case TriggerSeverity.Information:
                            imageKey = IMAGE_KEY_TRIGGER_COMMAND_INFORMATION;
                            break;
                    }
                }
                else if (command is IEnumerable)
                    imageKey = IMAGE_KEY_COMPOSITE_COMMAND;
            }

            return imageKey;
        }

        #endregion


        #region PRIVATE

        /// <summary>
        /// Adds the expanded nodes to the specified dictionary.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="expandedNodes">The dictionary with expanded nodes.</param>
        private void AddExpandedNodes(TreeViewItem item, Dictionary<string, bool> expandedNodes)
        {
            if (item.IsExpanded)
                expandedNodes.Add(GetFullName(item as ProcessingTreeViewItem), true);

            foreach (TreeViewItem subItem in item.Items)
                AddExpandedNodes(subItem, expandedNodes);
        }

        /// <summary>
        /// Updates the node expand state.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="expandedNodes">The dictionary with expanded nodes.</param>
        private void UpdateNodeExpandState(TreeViewItem item, Dictionary<string, bool> expandedNodes)
        {
            if (expandedNodes.Count == 0)
                return;

            bool isExpanded;
            if (expandedNodes.TryGetValue(GetFullName(item as ProcessingTreeViewItem), out isExpanded) && isExpanded)
                item.IsExpanded = true;

            foreach (TreeViewItem subItem in item.Items)
                UpdateNodeExpandState(subItem, expandedNodes);
        }

        /// <summary>
        /// Returns the tree view item full name.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// The full name tre view item.
        /// </returns>
        private string GetFullName(ProcessingTreeViewItem item)
        {
            if (item == null)
                return string.Empty;

            string parentFullName = GetFullName(item.Parent as ProcessingTreeViewItem);
            string fullName = parentFullName + Path.DirectorySeparatorChar + item.Text;

            return fullName.TrimStart(Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Builds the processing commands tree.
        /// </summary>
        private void BuildProcessingCommandsTree()
        {
            Items.Clear();
            _processingCommandTreeInfo.Clear();
            if (_processingCommands != null)
                AddNodes(Items, _processingCommands);
        }

        /// <summary>
        /// Adds the processing commands to the processing commands tree.
        /// </summary>
        /// <param name="rootNodeCollection">The root node collection.</param>
        /// <param name="collection">Information about the processing command,
        /// which should be added to the processing commands tree.</param>
        private void AddNodes(ItemCollection rootNodeCollection, IEnumerable collection)
        {
            foreach (object item in collection)
            {
                if (item is IProcessingCommandInfo)
                    AddNode(rootNodeCollection, (IProcessingCommandInfo)item);
            }
        }

        /// <summary>
        /// Removes the processing commands from the processing commands tree.
        /// </summary>
        /// <param name="rootNodeCollection">The root node collection.</param>
        /// <param name="collection">Information about the processing command,
        /// which should be removed from the processing commands tree.</param>
        private void RemoveNodes(ItemCollection rootNodeCollection, IEnumerable collection)
        {
            foreach (object item in collection)
            {
                if (item is IProcessingCommandInfo)
                    RemoveNode(rootNodeCollection, (IProcessingCommandInfo)item);
            }
        }

        /// <summary>
        /// Adds the object set to the node collection.
        /// </summary>
        /// <param name="rootNodeCollection">The root node collection.</param>
        /// <param name="objectSet">The set of objects.</param>
        private void AddObjectSet(
            ItemCollection rootNodeCollection,
            IEnumerable objectSet)
        {
            foreach (object obj in objectSet)
            {
                ProcessingTreeViewItem node = new ProcessingTreeViewItem(obj.ToString());
                rootNodeCollection.Add(node);
                InitTreeViewItem(node);
            }
        }

        /// <summary>
        /// Adds the processing command to the processing commands tree.
        /// </summary>
        /// <param name="rootNodeCollection">The root node collection.</param>
        /// <param name="command">The command.</param>
        private ProcessingTreeViewItem AddNode(
            ItemCollection rootNodeCollection,
            IProcessingCommandInfo command)
        {
            string commandName = command.ToString();

            ProcessingTreeViewItem node = new ProcessingTreeViewItem(commandName);
            node.BeginInit();
            rootNodeCollection.Add(node);
            InitTreeViewItem(node);
            node.Tag = command;
            string imageKey = GetCommandImageKey(command);
            SetImageKey(node, imageKey);

            string commandTypeNodeName = string.Format("Type: {0}",
                ProcessingDemosTools.GetReadableTypeName(command.GetType()));
            AddPropertyTreeNode(node.Items, commandTypeNodeName);

            string targetTypeNodeName = string.Format("Target: {0}",
                ProcessingDemosTools.GetReadableTypeName(command.TargetType));
            AddPropertyTreeNode(node.Items, targetTypeNodeName);

            if (command is IValueProcessing)
            {
                IValueProcessing targetConverter = (IValueProcessing)command;

                string destTargetTypeNodeName = string.Format("ProcessingTarget: {0}",
                    ProcessingDemosTools.GetReadableTypeName(targetConverter.ProcessingTargetType));
                AddPropertyTreeNode(node.Items, destTargetTypeNodeName);

                ProcessingTreeViewItem converterNode = AddPropertyTreeNode(node.Items, "Analyzer");
                converterNode.BeginInit();
                AddNode(converterNode.Items, targetConverter.Analyzer);
                converterNode.EndInit();
            }


#if !REMOVE_PDF_PLUGIN
            if (command is PdfAVerifier || command is PdfAConverter)
            {
                if (_addedPdfaProcessing.ContainsKey(command))
                    return node;
                _addedPdfaProcessing.Add(command, node);
            }
#endif

            if (command is ITriggerInfo)
            {
                ITriggerInfo trigger = (ITriggerInfo)command;

                if (trigger.Predicate != null)
                {
                    ProcessingTreeViewItem predicateNode = AddPropertyTreeNode(node.Items, "Predicate");
                    predicateNode.BeginInit();
                    AddNode(predicateNode.Items, trigger.Predicate);
                    predicateNode.EndInit();
                }

                string activateValueNodeName = string.Format("ActivateValue: {0}", trigger.ActivationValue);
                AddPropertyTreeNode(node.Items, activateValueNodeName);

                string severityNodeName = string.Format("Severity: {0}", trigger.Severity);
                AddPropertyTreeNode(node.Items, severityNodeName);
            }
            else if (command is ICompositePredicateAnalyzerInfo)
            {
                ICompositePredicateAnalyzerInfo targetPredicateExpression = (ICompositePredicateAnalyzerInfo)command;

                string expressionOperatorNodeName = string.Format("ConditionalOperator: {0}",
                    targetPredicateExpression.ConditionalOperator);
                AddPropertyTreeNode(node.Items, expressionOperatorNodeName);

                if (targetPredicateExpression.Arguments != null)
                {
                    ProcessingTreeViewItem argumentsNode = AddPropertyTreeNode(node.Items, "Arguments");
                    argumentsNode.BeginInit();
                    AddNodes(argumentsNode.Items, targetPredicateExpression.Arguments);
                    argumentsNode.EndInit();
                }
            }
            else if (command is IAnalyzerResultsComparerInfo)
            {
                IAnalyzerResultsComparerInfo targetComparsionPredicate = (IAnalyzerResultsComparerInfo)command;

                ProcessingTreeViewItem leftArgumentNode = AddPropertyTreeNode(node.Items, "LeftAnalyzer");
                leftArgumentNode.BeginInit();
                AddNode(leftArgumentNode.Items, targetComparsionPredicate.LeftAnalyzer);
                leftArgumentNode.EndInit();

                string operatorNodeName = string.Format("ComparisonOperator: {0}",
                    targetComparsionPredicate.ComparisonOperator);
                AddPropertyTreeNode(node.Items, operatorNodeName);

                if (targetComparsionPredicate.RightAnalyzer != null)
                {
                    ProcessingTreeViewItem rightArgumentNode = AddPropertyTreeNode(node.Items, "RightAnalyzer");
                    rightArgumentNode.BeginInit();
                    AddNode(rightArgumentNode.Items, targetComparsionPredicate.RightAnalyzer);
                    rightArgumentNode.EndInit();
                }
                else
                {
                    string valueNodeName = string.Format("RightConstantValue: {0}",
                        targetComparsionPredicate.RightConstantValue);
                    AddPropertyTreeNode(node.Items, valueNodeName);
                }
            }
            else if (command is IConditionalCommandInfo)
            {
                IConditionalCommandInfo conditionalCommand = (IConditionalCommandInfo)command;

                ProcessingTreeViewItem conditionNode = AddPropertyTreeNode(node.Items, "Condition");
                conditionNode.BeginInit();
                AddNode(conditionNode.Items, conditionalCommand.Condition);
                conditionNode.EndInit();

                if (conditionalCommand.IfBranch != null)
                {
                    ProcessingTreeViewItem ifNode = AddPropertyTreeNode(node.Items, "IfBranch");
                    ifNode.BeginInit();
                    AddNode(ifNode.Items, conditionalCommand.IfBranch);
                    ifNode.EndInit();
                }
                if (conditionalCommand.ElseBranch != null)
                {
                    ProcessingTreeViewItem elseNode = AddPropertyTreeNode(node.Items, "ElseBranch");
                    elseNode.BeginInit();
                    AddNode(elseNode.Items, conditionalCommand.ElseBranch);
                    elseNode.EndInit();
                }
            }
            else if (command is ISetContainsAnalyzerResultPredicateInfo)
            {
                ISetContainsAnalyzerResultPredicateInfo setContainsAnalyzerResultPredicate =
                    (ISetContainsAnalyzerResultPredicateInfo)command;

                ProcessingTreeViewItem analyzerNode = AddPropertyTreeNode(node.Items, "Analyzer");
                analyzerNode.BeginInit();
                AddNode(analyzerNode.Items, setContainsAnalyzerResultPredicate.Analyzer);
                analyzerNode.EndInit();

                ProcessingTreeViewItem referenceSetNode = AddPropertyTreeNode(node.Items, "ReferenceSet");
                referenceSetNode.BeginInit();
                AddObjectSet(referenceSetNode.Items, setContainsAnalyzerResultPredicate.ReferenceSet);
                referenceSetNode.EndInit();
            }
            else if (command is IProcessingCommandTreeInfo)
            {
                if (_addProcessingCommandTreeInfo == 0)
                    _processingCommandTreeInfo[node] = (IProcessingCommandTreeInfo)command;
                _addProcessingCommandTreeInfo++;
                try
                {
                    if (_viewProcessingTreeStructure)
                        AddNodes(node.Items, (IEnumerable)command);
                    else
                        AddNodes(node.Items, ((IProcessingCommandTreeInfo)command).ProcessingTreeNodes);
                }
                finally
                {
                    _addProcessingCommandTreeInfo--;
                }
            }
            else if (command is IAnalyzerWrapperInfo)
            {
                ProcessingTreeViewItem analyzerNode = AddPropertyTreeNode(node.Items, "Analyzer");
                analyzerNode.BeginInit();
                AddNode(analyzerNode.Items, ((IAnalyzerWrapperInfo)command).Analyzer);
                analyzerNode.EndInit();
            }
            else if (command is IEnumerable)
            {
                AddNodes(node.Items, (IEnumerable)command);
            }

            node.EndInit();
            return node;
        }

        /// <summary>
        /// Removes the processing command from the processing commands tree.
        /// </summary>
        /// <param name="rootNodeCollection">The root node collection.</param>
        /// <param name="command">The processing command,
        /// which should be removed from the processing commands tree.</param>
        private void RemoveNode(
            ItemCollection rootNodeCollection,
            IProcessingCommandInfo command)
        {
            ProcessingTreeViewItem node = FindNode(rootNodeCollection, command);
            rootNodeCollection.Remove(node);
        }

        /// <summary>
        /// Adds the tree node and sets icon of property.
        /// </summary>
        /// <param name="rootNodeCollection">The root node collection.</param>
        /// <param name="text">The text.</param>
        private ProcessingTreeViewItem AddPropertyTreeNode(ItemCollection rootNodeCollection, string text)
        {
            // if string does not contain any inappropriate symbol
            if (GetIsValidPropertyName(text))
            {
                ProcessingTreeViewItem node = new ProcessingTreeViewItem(text);
                rootNodeCollection.Add(node);
                InitTreeViewItem(node);
                SetImageKey(node, IMAGE_KEY_PROPERTY);
                return node;
            }
            return null;
        }

        /// <summary>
        /// Returns a value that indicates that property name is correct.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>
        /// <b>true</b> - if property name is correct;
        /// <b>false</b> - if property name is not correct.
        /// </returns>
        private bool GetIsValidPropertyName(string name)
        {
            // for each symbol of property name
            foreach (Char c in name)
            {
                // if symbol is not number or latin letter
                if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == ':' || c == ' '))
                    // property name is not correct
                    return false;
            }
            // property name is correct
            return true;
        }

        /// <summary>
        /// Finds a node.
        /// </summary>
        /// <param name="items">The nodes, where node must be searched.</param>
        /// <param name="processingCommand">The processing command,
        /// which is associated with the searching node.</param>
        /// <returns>
        /// A tree node if node is found; otherwise, <b>null</b>.
        /// </returns>
        private ProcessingTreeViewItem FindNode(ItemCollection items, IProcessingCommandInfo processingCommand)
        {
            if (processingCommand == null || items.Count == 0)
                return null;

            foreach (object item in items)
            {
                ProcessingTreeViewItem node = item as ProcessingTreeViewItem;
                if (node != null)
                {
                    if (GetProcessingCommandFromNode(node) == processingCommand)
                        return node;

                    ProcessingTreeViewItem result = FindNode(node.Items, processingCommand);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the image key of node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="imageKey">The image key.</param>
        private void SetImageKey(ProcessingTreeViewItem node, string imageKey)
        {
            BitmapSource source = null;
            if (_imageResources.TryGetValue(imageKey, out source))
                node.Image = source;
        }

        /// <summary>
        /// Adds the image resource to the image list of the tree view.
        /// </summary>
        /// <param name="imageList">The image list of tree view.</param>
        /// <param name="imageKey">The image key.</param>
        private static void AddImageResourceToImageList(Dictionary<string, BitmapSource> imageList, string imageKey)
        {
            BitmapSource image = GetImageResource(imageKey);
            if (image == null)
                throw new KeyNotFoundException(imageKey);

            imageList.Add(imageKey, image);
        }

        /// <summary>
        /// Returns the image resource, which is associated with the specified key.
        /// </summary>
        /// <param name="imageKey">The image key.</param>
        /// <returns>
        /// The image resource if resource is found; otherwise, <b>null</b>.</returns>
        private static BitmapSource GetImageResource(string imageKey)
        {
            string resourceFullName = null;
            if (_imageKeyToResourceName.TryGetValue(imageKey, out resourceFullName))
            {
                BitmapSource source = DemosResourcesManager.GetResourceAsBitmap(resourceFullName);
                source.Freeze();
                return source;
            }

            return null;
        }

        /// <summary>
        /// Initializes the TreeView item.
        /// </summary>
        /// <param name="item">The item.</param>
        private void InitTreeViewItem(TreeViewItem item)
        {
            item.ContextMenu = _treeNodeMenu;
            item.PreviewMouseRightButtonUp += new MouseButtonEventHandler(item_PreviewMouseRightButtonUp);
        }

        /// <summary>
        /// Selects the element when element's handler was called.
        /// </summary>
        private void item_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            item.IsSelected = true;
        }

        /// <summary>
        /// Context menu of tree node is opening.
        /// </summary>
        private void treeNodeMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            ItemsControl itemsControl = (ItemsControl)sender;

            TreeViewItem node = SelectedItem as TreeViewItem;
            ((UIElement)itemsControl.Items[0]).IsEnabled = CanExpand(node);
            ((UIElement)itemsControl.Items[1]).IsEnabled = CanCollapse(node);
        }

        /// <summary>
        /// Determines whether the tree node can be expanded.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        /// <b>True</b> if tree node can be expanded; otherwise, <b>false</b>.
        /// </returns>
        private bool CanExpand(TreeViewItem node)
        {
            if (node == null)
                return false;

            if (node.Items.Count > 0 && !node.IsExpanded)
                return true;

            foreach (object item in node.Items)
            {
                TreeViewItem children = item as TreeViewItem;
                if (children != null && CanExpand(children))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the tree node can be collapsed.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        /// <b>True</b> if tree node can be collapsed; otherwise, <b>false</b>.
        /// </returns>
        private bool CanCollapse(TreeViewItem node)
        {
            if (node == null)
                return false;

            if (node.Items.Count > 0 && node.IsExpanded)
                return true;

            foreach (object item in node.Items)
            {
                TreeViewItem children = item as TreeViewItem;
                if (children != null && CanCollapse(children))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// The "Expand All" button is clicked.
        /// </summary>
        private void expandAllMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SetIsExpanded(SelectedItem as TreeViewItem, true);
        }

        /// <summary>
        /// The "Collapse All" button is clicked.
        /// </summary>
        private void collapseAllMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SetIsExpanded(SelectedItem as TreeViewItem, false);
        }

        /// <summary>
        /// Sets the value of <see cref="System.Windows.Controls.TreeViewItem.IsExpanded"/>
        /// property for the current element and his children.
        /// </summary>
        /// <param name="currentTreeViewItem">The TreeView item.</param>
        /// <param name="value">The value of
        /// <see cref="System.Windows.Controls.TreeViewItem.IsEnabled"/>.</param>
        private void SetIsExpanded(TreeViewItem currentTreeViewItem, bool value)
        {
            if (currentTreeViewItem == null)
                return;

            currentTreeViewItem.IsExpanded = value;
            foreach (object item in currentTreeViewItem.Items)
                SetIsExpanded(item as TreeViewItem, value);
        }

        #endregion

        #endregion



        #region Delegates

        private delegate IProcessingCommandInfo GetSelectedProcessingCommandDelegate();

        #endregion

    }
}
