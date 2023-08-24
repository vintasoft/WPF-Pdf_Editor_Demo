using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

using Vintasoft.Imaging;
using Vintasoft.Imaging.Processing;
using Vintasoft.Imaging.Processing.Analyzers;
using Vintasoft.Imaging.Wpf;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A control that allows to view the execution result of the processing commands.
    /// </summary>
    public partial class ProcessingResultViewer : TreeView
    {

        #region Classes

        /// <summary>
        /// Contains information about items, which are added to the tree view.
        /// </summary>
        class AddedItemsInfo
        {

            /// <summary>
            /// Initializes a new instance of the <see cref="AddedItemsInfo"/> class.
            /// </summary>
            /// <param name="countAdded">The count of added items.</param>
            /// <param name="collection">The added items.</param>
            public AddedItemsInfo(int addedItemsCount, IEnumerable addedItems)
            {
                _count = addedItemsCount;
                _collection = addedItems;
            }



            int _count;
            /// <summary>
            /// Gets the count of added items.
            /// </summary>
            public int Count
            {
                get
                {
                    return _count;
                }
            }

            IEnumerable _collection;
            /// <summary>
            /// Gets the collection of added items.
            /// </summary>
            public IEnumerable Collection
            {
                get
                {
                    return _collection;
                }
            }
        }

        #endregion



        #region Constants

        /// <summary>
        /// The maximum count of elements, which can be added to the tree view
        /// without the "Show next elements" button.
        /// </summary>
        private const int MAX_SHOW_PROCESSING_RESULTS = 100;

        /// <summary>
        /// Image key for the default node.
        /// </summary>
        private const string IMAGE_KEY_DEFAULT = "ProcessingResultDefault";

        /// <summary>
        /// Image key for the property node.
        /// </summary>
        private const string IMAGE_KEY_PROPERTY = "ProcessingResultProperty";

        /// <summary>
        /// Image key for the verification failed.
        /// </summary>
        private const string IMAGE_KEY_VERIFICATION_FAILED = "ProfileResultFail";

        /// <summary>
        /// Image key for the verification passed.
        /// </summary>
        private const string IMAGE_KEY_VERIFICATION_PASSED = "ProfileResultSuccess";

        /// <summary>
        /// Image key for the failed conversion.
        /// </summary>
        private const string IMAGE_KEY_CONVERSION_FAILED = IMAGE_KEY_VERIFICATION_FAILED;

        /// <summary>
        /// Image key for the passed conversion.
        /// </summary>
        private const string IMAGE_KEY_CONVERSION_PASSED = IMAGE_KEY_VERIFICATION_PASSED;

        /// <summary>
        /// Image key for the composite result.
        /// </summary>
        private const string IMAGE_KEY_COMPOSITE_RESULT = "CompositeResult";

        /// <summary>
        /// Image key for the trigger error.
        /// </summary>
        private const string IMAGE_KEY_TRIGGER_ERROR = "TriggerActivationImportant";

        /// <summary>
        /// Image key for the trigger warning.
        /// </summary>
        private const string IMAGE_KEY_TRIGGER_WARNING = "TriggerActivationUnimportant";

        /// <summary>
        /// Image key for the trigger information.
        /// </summary>
        private const string IMAGE_KEY_TRIGGER_INFORMATION = "TriggerActivationInformation";

        /// <summary>
        /// Image key for the applied commands.
        /// </summary>
        private const string IMAGE_KEY_APPLIED_COMMANDS = "AppliedCommand";

        /// <summary>
        /// Name of the private node.
        /// </summary>
        private const string PRIVATE_TAG_NAME = "_PRIVATE";

        #endregion



        #region Fields

        /// <summary>
        /// The image resources.
        /// </summary>
        Dictionary<string, BitmapSource> _imageResources = new Dictionary<string, BitmapSource>();

        /// <summary>
        /// The count of viewer initializations.
        /// </summary>
        int _initCount = 0;

        #endregion



        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingResultViewer"/> class.
        /// </summary>
        public ProcessingResultViewer()
        {
            Assembly assembly = typeof(ProcessingResultViewer).Module.Assembly;

            AddImageResource(assembly, IMAGE_KEY_DEFAULT, "DefaultResult");
            AddImageResource(assembly, IMAGE_KEY_PROPERTY, "PropertyResult");

            AddImageResource(assembly, IMAGE_KEY_VERIFICATION_PASSED, "ProfileResultSuccess");
            AddImageResource(assembly, IMAGE_KEY_VERIFICATION_FAILED, "ProfileResultFail");

            AddImageResource(assembly, IMAGE_KEY_COMPOSITE_RESULT, "CompositeResult");

            AddImageResource(assembly, IMAGE_KEY_TRIGGER_ERROR, "TriggerActivationImportant");
            AddImageResource(assembly, IMAGE_KEY_TRIGGER_WARNING, "TriggerActivationUnimportant");
            AddImageResource(assembly, IMAGE_KEY_TRIGGER_INFORMATION, "TriggerActivationInformation");

            AddImageResource(assembly, IMAGE_KEY_APPLIED_COMMANDS, "AppliedCommand");

            ProcessingCommandViewer.LoadImageResources(_imageResources);
        }

        #endregion



        #region Properties

        ProcessingResult _processingResult = null;
        /// <summary>
        /// Gets or sets the processing result.
        /// </summary>
        /// <value>
        /// Default value is <b>null</b>.
        /// </value>
        public ProcessingResult ProcessingResult
        {
            get
            {
                return _processingResult;
            }
            set
            {
                _processingResult = value;

                IsEnabled = _processingResult != null;

                BeginInit();
                Items.Clear();
                AddTreeNodeItem(Items, _processingResult);
                if (Items.Count == 1)
                {
                    TreeViewItem item = (TreeViewItem)Items[0];
                    item.IsExpanded = true;
                }
                EndInit();
            }
        }

        #endregion



        #region Methods

        /// <summary>
        /// Begins the viewer initialization.
        /// </summary>
        public override void BeginInit()
        {
            if (_initCount == 0)
                base.BeginInit();

            _initCount++;
        }

        /// <summary>
        /// Ends the viewer initialization.
        /// </summary>
        public override void EndInit()
        {
            _initCount--;

            if (_initCount == 0)
                base.EndInit();
        }

        /// <summary>
        /// Builds new level of tree before node expands.
        /// </summary>
        private void PrivateNode_Expanded(object sender, RoutedEventArgs e)
        {
            ProcessingTreeViewItem node = e.Source as ProcessingTreeViewItem;
            try
            {
                // if node is processing result
                if (node.Tag is ProcessingResult)
                {
                    IEnumerable<ProcessingResult> innerResults =
                        ((ProcessingResult)node.Tag) as IEnumerable<ProcessingResult>;
                    // if processing result is collection of results
                    if (innerResults != null)
                    {
                        BeginInit();
                        // remove private node
                        node.Items.RemoveAt(node.Items.Count - 1);
                        // add tree node items from collection
                        AddTreeNodeItems(node, innerResults);
                        EndInit();
                    }

                    return;
                }

                Dictionary<IProcessingCommandInfo, List<ProcessingTargetChangedResult>> appliedCommands =
                    node.Tag as Dictionary<IProcessingCommandInfo, List<ProcessingTargetChangedResult>>;
                // if node is applied commands
                if (appliedCommands != null)
                {
                    BeginInit();
                    // remove private node
                    node.Items.RemoveAt(node.Items.Count - 1);
                    // for each processing command
                    foreach (IProcessingCommandInfo processingCommand in appliedCommands.Keys)
                    {
                        // add processing command node
                        ProcessingTreeViewItem processingCommadNode =
                            AddProcessingCommandNode(node, processingCommand, false);
                        processingCommadNode.Tag = appliedCommands[processingCommand];
                        // add private node
                        AddPrivateTag(processingCommadNode);
                    }
                    EndInit();

                    return;
                }

                List<ProcessingTargetChangedResult> triggerActivationResult =
                    node.Tag as List<ProcessingTargetChangedResult>;
                // if node is trigger activation result
                if (triggerActivationResult != null)
                {
                    BeginInit();
                    // remove private node
                    node.Items.RemoveAt(node.Items.Count - 1);
                    // add all trigger activation results
                    AddTreeNodeItems(node, triggerActivationResult);
                    EndInit();

                    return;
                }

                Dictionary<IProcessingCommandInfo, List<ProcessingErrorResult>> processingErrors =
                        node.Tag as Dictionary<IProcessingCommandInfo, List<ProcessingErrorResult>>;
                // if node is processing errors
                if (processingErrors != null)
                {
                    BeginInit();
                    // for each processing command
                    foreach (IProcessingCommandInfo processingCommand in processingErrors.Keys)
                    {
                        // add all error results
                        AddTreeNodeItemsFromList(node, processingErrors[processingCommand], IMAGE_KEY_TRIGGER_ERROR);
                    }
                    EndInit();

                    return;
                }

                List<ProcessingErrorResult> processingErrorResults =
                        node.Tag as List<ProcessingErrorResult>;
                // if node is processing error result
                if (processingErrorResults != null)
                {
                    // add all error results
                    AddTreeNodeItemsFromList(node, processingErrorResults, IMAGE_KEY_TRIGGER_ERROR);
                    return;
                }

            }
            finally
            {
                node.Expanded -= PrivateNode_Expanded;
            }
        }

        /// <summary> 
        /// Adds tree node items form list.
        /// </summary>
        /// <param name="node">A tree node.</param>
        /// <param name="items">A list of results.</param>
        /// <param name="nodeImageKey">Image key of new nodes.</param>
        private void AddTreeNodeItemsFromList<T>(ProcessingTreeViewItem node, List<T> items, string nodeImageKey) where T : ProcessingResult
        {
            // get dictionary: processing command name => list of processing results
            Dictionary<string, List<T>> sortedItem = new Dictionary<string, List<T>>();

            // for each processing result
            foreach (T item in items)
            {
                // get node name
                string nodeName = GetNodeName(item);

                List<T> list = null;
                // if dictionary does not have processing command name
                if (!sortedItem.TryGetValue(nodeName, out list))
                {
                    // create new list
                    list = new List<T>();
                    // add processing command name with new list to dictionary
                    sortedItem.Add(nodeName, list);
                }
                // add processing result
                list.Add(item);
            }

            BeginInit();
            // remove private node
            node.Items.RemoveAt(node.Items.Count - 1);
            // if number of processing results is not equal to number of processing commands
            if (items.Count != sortedItem.Count)
            {
                // for each processing command
                foreach (string itemName in sortedItem.Keys)
                {
                    // get processing results
                    List<T> list = sortedItem[itemName];
                    // get node name
                    string nodeName = string.Format("{0} ({1} matches)", itemName, list.Count);
                    // add processing result node
                    ProcessingTreeViewItem newNode = new ProcessingTreeViewItem(nodeName);
                    node.Items.Add(newNode);
                    if (nodeImageKey != "")
                        SetImageKey(newNode, nodeImageKey);
                    // add all processing results subnodes
                    AddTreeNodeItems(newNode, list);
                }
            }
            else
            {
                // add processing results
                AddTreeNodeItems(node, items);
            }
            EndInit();
        }

        /// <summary>
        /// Adds the processing results.
        /// </summary>
        private void showNextElements_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ProcessingTreeViewItem node = (ProcessingTreeViewItem)sender;
            AddedItemsInfo nextItemsInfo = (AddedItemsInfo)node.Tag;

            BeginInit();
            ProcessingTreeViewItem parentNode = (ProcessingTreeViewItem)node.Parent;
            int index = parentNode.Items.IndexOf(node);
            parentNode.Items.RemoveAt(index);

            AddTreeNodeItems(parentNode, nextItemsInfo);

            EndInit();

            if (parentNode.Items.Count > index)
            {
                TreeViewItem item = (TreeViewItem)parentNode.Items[index];
                item.IsSelected = true;
            }
        }

        /// <summary>
        /// Adds the tree node item.
        /// </summary>
        /// <param name="rootNodeCollection">The root node collection.</param>
        /// <param name="processingResult">The processing result.</param>
        private ProcessingTreeViewItem AddTreeNodeItem(
            ItemCollection rootNodeCollection,
            ProcessingResult processingResult)
        {
            if (processingResult == null)
                return null;

            string nodeImageKey = GetImageKey(processingResult);
            string processingResultName = GetNodeName(processingResult);
            ProcessingTreeViewItem node = new ProcessingTreeViewItem(processingResultName);
            rootNodeCollection.Add(node);

            if (processingResult.Target != null)
            {
                string targetNodeName = "Target: ";
                targetNodeName += ProcessingDemosTools.GetReadableTypeName(processingResult.Target.GetType());
                ProcessingTreeViewItem targetNode = new ProcessingTreeViewItem(targetNodeName);
                node.Items.Add(targetNode);
                SetImageKey(targetNode, IMAGE_KEY_PROPERTY);
                targetNode.Items.Add(processingResult.Target.ToString());
            }

            if (processingResult.ProcessingCommand != null)
                AddProcessingCommandNode(node, processingResult.ProcessingCommand, true);

            if (processingResult is AnalyzerResult)
            {
                object value = ((AnalyzerResult)processingResult).GetValue();
                if (value != null)
                {
                    ProcessingTreeViewItem valueNode = new ProcessingTreeViewItem(
                        string.Format("Value: {0}", value));
                    node.Items.Add(valueNode);
                    SetImageKey(valueNode, IMAGE_KEY_PROPERTY);
                }
            }

            if (processingResult is ConversionProfileResult)
            {
                ConversionProfileResult conversionProfileResult = (ConversionProfileResult)processingResult;

                if (conversionProfileResult.AppliedCommands.Count > 0)
                {
                    ProcessingTreeViewItem appliedCommands = new ProcessingTreeViewItem("Applied Commands");
                    node.Items.Add(appliedCommands);
                    AddPrivateTag(appliedCommands);
                    SetImageKey(appliedCommands, IMAGE_KEY_APPLIED_COMMANDS);
                    appliedCommands.Tag = conversionProfileResult.AppliedCommands;
                }
            }

            if (processingResult is ProcessingErrorResult)
            {
                Exception ex = ((ProcessingErrorResult)processingResult).ProcessingException;
                ProcessingTreeViewItem processingCommandNode = new ProcessingTreeViewItem(
                    string.Format("ProcessingException: {0}", ex.Message));
                node.Items.Add(processingCommandNode);
                SetImageKey(processingCommandNode, IMAGE_KEY_PROPERTY);
            }

            if (processingResult is ProcessingProfileResult)
            {
                ProcessingProfileResult processingProfileResult = (ProcessingProfileResult)processingResult;

                // if processing result has errors
                if (processingProfileResult.ProcessingErrors != null && processingProfileResult.ProcessingErrors.Count > 0)
                {
                    // add processing errors node
                    ProcessingTreeViewItem processingCommandNode = new ProcessingTreeViewItem("Processing Errors");
                    node.Items.Add(processingCommandNode);
                    AddPrivateTag(processingCommandNode);
                    SetImageKey(processingCommandNode, IMAGE_KEY_TRIGGER_ERROR);
                    processingCommandNode.Tag = processingProfileResult.ProcessingErrors;
                }

                foreach (ITriggerInfo triggerInfo in processingProfileResult.ActivatedTriggers.Keys)
                {
                    List<TriggerActivationResult> activatedTrigger =
                        processingProfileResult.ActivatedTriggers[triggerInfo];

                    string processingCommandInfoName = triggerInfo.Name;
                    if (activatedTrigger.Count > 1)
                        processingCommandInfoName += string.Format(" ({0} matches)", activatedTrigger.Count);
                    ProcessingTreeViewItem processingCommandInfoNode =
                        new ProcessingTreeViewItem(processingCommandInfoName);
                    node.Items.Add(processingCommandInfoNode);
                    SetImageKey(processingCommandInfoNode, GetTriggerImageKey(triggerInfo.Severity));

                    AddTreeNodeItems(processingCommandInfoNode, activatedTrigger);
                }

                if (processingProfileResult.DetailedResult != null)
                {
                    ProcessingTreeViewItem detailedResultNode = new ProcessingTreeViewItem("DetailedResult");
                    node.Items.Add(detailedResultNode);
                    SetImageKey(detailedResultNode, IMAGE_KEY_COMPOSITE_RESULT);
                    AddTreeNodeItem(detailedResultNode.Items, processingProfileResult.DetailedResult);
                }
            }
            else if (processingResult is TriggerActivationResult)
            {
                TriggerActivationResult triggerResult = (TriggerActivationResult)processingResult;

                ProcessingTreeViewItem severityNode =
                    new ProcessingTreeViewItem(string.Format("Severity: {0}", triggerResult.Severity));
                node.Items.Add(severityNode);
                SetImageKey(severityNode, IMAGE_KEY_PROPERTY);

                if (triggerResult.PredicateResult != null)
                {
                    ProcessingTreeViewItem predicateResultNode =
                        new ProcessingTreeViewItem("PredicateResult");
                    node.Items.Add(predicateResultNode);
                    SetImageKey(predicateResultNode, IMAGE_KEY_PROPERTY);
                    AddTreeNodeItem(predicateResultNode.Items, triggerResult.PredicateResult);
                }
            }
            else if (processingResult is IEnumerable<ProcessingResult>)
            {
                AddPrivateTag(node);
            }


            SetImageKey(node, nodeImageKey);
            node.Tag = processingResult;

            return node;
        }

        /// <summary>
        /// Adds the private tag.
        /// </summary>
        /// <param name="node">The node.</param>
        private void AddPrivateTag(ProcessingTreeViewItem node)
        {
            ProcessingTreeViewItem privateNode = new ProcessingTreeViewItem(PRIVATE_TAG_NAME);
            node.Expanded += new RoutedEventHandler(PrivateNode_Expanded);
            node.Items.Add(privateNode);
        }

        /// <summary>
        /// Adds the processing command node.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="processingCommand">The processing command.</param>
        /// <param name="showProcessingCommandType">Indicate whether processing command type can be showed.</param>
        private ProcessingTreeViewItem AddProcessingCommandNode(
            ProcessingTreeViewItem node,
            IProcessingCommandInfo processingCommand,
            bool showProcessingCommandType)
        {
            string nodeHeader = string.Empty;
            if (showProcessingCommandType)
            {
                string processingCommandType = ProcessingDemosTools.GetReadableTypeName(((object)processingCommand).GetType());

                if (GetIsValidTypename(processingCommandType))
                {
                    nodeHeader = string.Format("ProcessingCommand ({0}): ", processingCommandType);
                }
                else
                {
                    nodeHeader = "ProcessingCommand: ";
                }
            }
            nodeHeader = string.Format("{0}{1}", nodeHeader, processingCommand);

            ProcessingTreeViewItem processingCommadNode = new ProcessingTreeViewItem(nodeHeader);
            node.Items.Add(processingCommadNode);
            string processingCommandImageKey = ProcessingCommandViewer.GetCommandImageKey(processingCommand);
            if (processingCommandImageKey == ProcessingCommandViewer.IMAGE_KEY_CONVERTER_COMMAND ||
                processingCommandImageKey == ProcessingCommandViewer.IMAGE_KEY_VERIFIER_COMMAND)
                processingCommandImageKey = ProcessingCommandViewer.IMAGE_KEY_COMMAND;
            SetImageKey(processingCommadNode, processingCommandImageKey);
            return processingCommadNode;
        }

        /// <summary>
        /// Returns the name of the node.
        /// </summary>
        /// <param name="processingResult">The processing result.</param>
        /// <returns>The node name.</returns>
        private string GetNodeName(ProcessingResult processingResult)
        {
            string name = processingResult.Description;

            if (processingResult is NamedProcessingResult)
            {
                NamedProcessingResult namedProcessingResult =
                    (NamedProcessingResult)processingResult;

                if (namedProcessingResult.ParentProcessingResult != null)
                    name = GetNodeName(namedProcessingResult.ParentProcessingResult);
            }

            return name;
        }

        /// <summary>
        /// Returns the image key of processing result.
        /// </summary>
        /// <param name="processingResult">The processing result.</param>
        /// <returns>The image key.</returns>
        private string GetImageKey(ProcessingResult processingResult)
        {
            string nodeImageKey = IMAGE_KEY_DEFAULT;

            // if named result
            if (processingResult is NamedProcessingResult)
            {
                NamedProcessingResult namedProcessingResult =
                    (NamedProcessingResult)processingResult;

                if (namedProcessingResult.ParentProcessingResult == null)
                {
                    if (namedProcessingResult.Results.Count > 0)
                    {
                        ProcessingResult[] results =
                            new ProcessingResult[namedProcessingResult.Results.Count];
                        namedProcessingResult.Results.CopyTo(results, 0);

                        string subNodeImageKey = GetImageKey(results[0]);
                        string currentNodeImageKey;

                        for (int i = 1; i < results.Length; i++)
                        {
                            currentNodeImageKey = GetImageKey(results[i]);
                            if (subNodeImageKey != currentNodeImageKey)
                            {
                                subNodeImageKey = null;
                                break;
                            }
                        }

                        if (subNodeImageKey != null)
                            // get image key of first result
                            nodeImageKey = subNodeImageKey;
                    }
                }
                else
                {
                    nodeImageKey = GetImageKey(namedProcessingResult.ParentProcessingResult);
                }

                if (nodeImageKey == IMAGE_KEY_DEFAULT)
                    nodeImageKey = IMAGE_KEY_COMPOSITE_RESULT;
            }
            // if verification result
            else if (processingResult is VerificationProfileResult)
            {
                VerificationProfileResult verificationProfileResult =
                    (VerificationProfileResult)processingResult;

                if (verificationProfileResult.IsPassed)
                    nodeImageKey = IMAGE_KEY_VERIFICATION_PASSED;
                else
                    nodeImageKey = IMAGE_KEY_VERIFICATION_FAILED;
            }
            // if conversion result
            else if (processingResult is ConversionProfileResult)
            {
                ConversionProfileResult conversionProfileResult =
                    (ConversionProfileResult)processingResult;

                if (conversionProfileResult.IsSuccessful)
                    nodeImageKey = IMAGE_KEY_CONVERSION_PASSED;
                else
                    nodeImageKey = IMAGE_KEY_CONVERSION_FAILED;
            }
            // if trigger activate result
            else if (processingResult is TriggerActivationResult)
            {
                TriggerActivationResult triggerResult = (TriggerActivationResult)processingResult;
                nodeImageKey = GetTriggerImageKey(triggerResult.Severity);
            }
            // if collection of processing result
            else if (processingResult is IEnumerable<ProcessingResult>)
            {
                if (nodeImageKey == IMAGE_KEY_DEFAULT)
                    nodeImageKey = IMAGE_KEY_COMPOSITE_RESULT;
            }
            // if processing error result
            else if (processingResult is ProcessingErrorResult)
            {
                nodeImageKey = IMAGE_KEY_TRIGGER_ERROR;
            }
            return nodeImageKey;
        }

        /// <summary>
        /// Adds the tree node items.
        /// </summary>
        /// <param name="root">The root node.</param>
        /// <param name="collection">The collection of processing result.</param>
        private void AddTreeNodeItems(ProcessingTreeViewItem root, IEnumerable collection)
        {
            AddTreeNodeItems(root, new AddedItemsInfo(0, collection));
        }

        /// <summary> 
        /// Adds the tree node items.
        /// </summary>
        /// <param name="root">The root node.</param>
        /// <param name="nextItemsInfo">The information about next items.</param>
        private void AddTreeNodeItems(ProcessingTreeViewItem root, AddedItemsInfo nextItemsInfo)
        {
            int index = 0;
            int startIndex = nextItemsInfo.Count;
            int lastItemIndex = nextItemsInfo.Count + MAX_SHOW_PROCESSING_RESULTS - 1;

            foreach (object result in nextItemsInfo.Collection)
            {
                if (!(result is ProcessingResult))
                    continue;

                if (index >= startIndex)
                {
                    AddTreeNodeItem(root.Items, (ProcessingResult)result);
                    if (index == lastItemIndex)
                    {
                        string nodeName = string.Format("Show next {0} elements.", MAX_SHOW_PROCESSING_RESULTS);
                        ProcessingTreeViewItem showNextElements = new ProcessingTreeViewItem(nodeName);
                        SetImageKey(showNextElements, IMAGE_KEY_DEFAULT);
                        showNextElements.MouseLeftButtonUp +=
                            new System.Windows.Input.MouseButtonEventHandler(showNextElements_MouseLeftButtonUp);
                        root.Items.Add(showNextElements);
                        showNextElements.Tag = new AddedItemsInfo(index + 1, nextItemsInfo.Collection);
                        break;
                    }
                }
                index++;
            }
        }

        /// <summary>
        /// Returns the trigger image key.
        /// </summary>
        /// <param name="triggerSeverity">The trigger severity.</param>
        /// <returns>The trigger image key.</returns>
        private string GetTriggerImageKey(TriggerSeverity triggerSeverity)
        {
            switch (triggerSeverity)
            {
                case TriggerSeverity.Important:
                    return IMAGE_KEY_TRIGGER_ERROR;

                case TriggerSeverity.Unimportant:
                    return IMAGE_KEY_TRIGGER_WARNING;

                case TriggerSeverity.Information:
                    return IMAGE_KEY_TRIGGER_INFORMATION;
            }

            throw new NotImplementedException();
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
        /// Adds an image resource to an image list.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="key">The key.</param>
        /// <param name="name">The name.</param>
        private void AddImageResource(Assembly assembly, string key, string name)
        {
            string resourceFormatName =
                "WpfDemosCommonCode.Imaging.Processing.ProcessingResultViewer.Resources.{0}.png";

            string resourceFullName = string.Format(resourceFormatName, name);

            try
            {
                BitmapSource source = DemosResourcesManager.GetResourceAsBitmap(resourceFullName);
                source.Freeze();
                _imageResources.Add(key, source);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Returns a value that indicates that type name is correct.
        /// </summary>
        /// <param name="name">The type name.</param>
        /// <returns>
        /// <b>true</b> - if type name is correct;
        /// <b>false</b> - if type name is not correct.
        /// </returns>
        private bool GetIsValidTypename(string name)
        {
            // for each symbol of type name
            foreach (Char c in name)
            {
                // if symbol is not number or latin letter
                if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')))
                    // type name is not correct
                    return false;
            }
            // type name is correct
            return true;
        }

        #endregion

    }
}
