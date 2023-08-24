#if !REMOVE_PDF_PLUGIN

using System;
using System.Collections.Generic;

using Vintasoft.Imaging.Pdf;
using Vintasoft.Imaging.Pdf.Tree;
using Vintasoft.Imaging.Processing;


namespace WpfDemosCommonCode.Imaging
{
    /// <summary>
    /// A form that allows to view the commands for processing a PDF document.
    /// </summary>
    public class DocumentProcessingCommandControl : ProcessingCommandControl<PdfDocument>
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentProcessingCommandControl"/> class.
        /// </summary>
        public DocumentProcessingCommandControl()
        {
        }

        #endregion



        #region Properties

        ProcessingResultTreeType _viewType = ProcessingResultTreeType.Detailed;
        /// <summary>
        /// Gets or sets the type of the processing command tree.
        /// </summary>
        public ProcessingResultTreeType ViewType
        {
            get
            {
                return _viewType;
            }
            set
            {
                if (_viewType != value)
                {
                    _viewType = value;

                    RefreshResults();
                }
            }
        }

        #endregion



        #region Methods

        #region PROTECTED

        /// <summary>
        /// Processes the results.
        /// </summary>
        /// <param name="result">The result, which should be processed.</param>
        /// <returns>
        /// The changed result.
        /// </returns>
        protected override ProcessingResult ProcessResult(ProcessingResult result)
        {
            // if view type is "ByPage" type
            if (_viewType == ProcessingResultTreeType.ByPage)
            {
                // processing result list of commands which was applied to the document
                List<ProcessingResult> documentProcessingResults =
                   new List<ProcessingResult>();
                // processing result list of commands which was applied to the pages
                Dictionary<PdfPage, List<ProcessingResult>> pagesProcessingResult =
                    new Dictionary<PdfPage, List<ProcessingResult>>();

                FindAndSortPagesProcessingResult(result, ref documentProcessingResults, ref pagesProcessingResult);

                // list of processing results 
                List<ProcessingResult> namedProcessingResults = new List<ProcessingResult>();

                // if result is conversion result
                if (result is ConversionProfileResult)
                {
                    ConversionProfileResult conversionProfileResult = (ConversionProfileResult)result;

                    // lists of applied commands
                    Dictionary<IProcessingCommandInfo, List<ProcessingResult>> documentAppliedCommands =
                        new Dictionary<IProcessingCommandInfo, List<ProcessingResult>>();
                    Dictionary<PdfPage, Dictionary<IProcessingCommandInfo, List<ProcessingResult>>> pageAppliedCommands =
                        new Dictionary<PdfPage, Dictionary<IProcessingCommandInfo, List<ProcessingResult>>>();

                    // sort applied commands
                    FindAndSortPagesAppliedCommands(conversionProfileResult, ref documentAppliedCommands, ref pageAppliedCommands);

                    // processing result list of applied commands
                    List<ProcessingResult> appliedCommandsResult = new List<ProcessingResult>();
                    // root processing result for applied commands
                    NamedProcessingResult appliedCommands = new NamedProcessingResult(
                        "Applied Commands", null, null, appliedCommandsResult);
                    namedProcessingResults.Add(appliedCommands);


                    // Document

                    // if document has applied commands
                    if (documentAppliedCommands.Count > 0)
                    {
                        List<ProcessingResult> documentAppliedCommandsResult = new List<ProcessingResult>();
                        IProcessingCommandInfo[] documentAppliedCommandsList = new IProcessingCommandInfo[documentAppliedCommands.Keys.Count];
                        documentAppliedCommands.Keys.CopyTo(documentAppliedCommandsList, 0);
                        Array.Sort(documentAppliedCommandsList, CompareProcessingCommandByName);

                        // for each applied command
                        foreach (IProcessingCommandInfo processingCommand in documentAppliedCommandsList)
                        {
                            // group similar results
                            List<ProcessingResult> groupedDocumentAppliedCommands =
                                GroupSimilarProcessingResults(result, documentAppliedCommands[processingCommand]);
                            if (groupedDocumentAppliedCommands.Count == 1)
                                groupedDocumentAppliedCommands = documentAppliedCommands[processingCommand];

                            // create readable result
                            NamedProcessingResult namedResult = new NamedProcessingResult(((object)processingCommand).ToString(),
                                null, null, groupedDocumentAppliedCommands);
                            // add result to result list
                            documentAppliedCommandsResult.Add(namedResult);
                        }

                        // root result for document applied commands
                        NamedProcessingResult documentCommands = new NamedProcessingResult(
                            "Document", null, null, documentAppliedCommandsResult);
                        appliedCommandsResult.Add(documentCommands);
                    }


                    // Page


                    // create dictionary: index of PDF page => processing results
                    Dictionary<int, Dictionary<IProcessingCommandInfo, List<ProcessingResult>>> pageIndexToProcessingResults =
                        SortProcessingResultsByPage(pageAppliedCommands);

                    // for each processed page
                    foreach (int pageIndex in pageIndexToProcessingResults.Keys)
                    {
                        Dictionary<IProcessingCommandInfo, List<ProcessingResult>> currentPageAppliedCommands =
                                pageIndexToProcessingResults[pageIndex];

                        // if page has applied commands
                        if (currentPageAppliedCommands.Count > 0)
                        {
                            IProcessingCommandInfo[] pageAppliedCommandsList = new IProcessingCommandInfo[currentPageAppliedCommands.Keys.Count];
                            currentPageAppliedCommands.Keys.CopyTo(pageAppliedCommandsList, 0);
                            Array.Sort(pageAppliedCommandsList, CompareProcessingCommandByName);

                            List<ProcessingResult> pageAppliedCommandsResult = new List<ProcessingResult>();

                            // for each applied command
                            foreach (IProcessingCommandInfo processingCommand in pageAppliedCommandsList)
                            {
                                // group similar results
                                List<ProcessingResult> groupedCurrentPageAppliedCommands =
                                    GroupSimilarProcessingResults(result, currentPageAppliedCommands[processingCommand]);
                                if (groupedCurrentPageAppliedCommands.Count == 1)
                                    groupedCurrentPageAppliedCommands = currentPageAppliedCommands[processingCommand];

                                // create readable result
                                NamedProcessingResult namedResult = new NamedProcessingResult(((object)processingCommand).ToString(),
                                    null, null, groupedCurrentPageAppliedCommands);
                                // add result to result list
                                pageAppliedCommandsResult.Add(namedResult);
                            }

                            // root result for page applied commands
                            NamedProcessingResult pageResult = new NamedProcessingResult(
                                string.Format("Page {0}", pageIndex + 1), null, null, pageAppliedCommandsResult);
                            appliedCommandsResult.Add(pageResult);
                        }
                    }
                }

                // if PDF document is specified as a target in processing results
                if (documentProcessingResults.Count > 0)
                {
                    // get trigger activation results
                    documentProcessingResults = GetTriggerActivationResults(documentProcessingResults);
                    if (documentProcessingResults.Count > 0)
                    {
                        // group similar results
                        documentProcessingResults = GroupSimilarProcessingResults(result, documentProcessingResults);

                        // root result for document applied commands 
                        NamedProcessingResult documentResult = new NamedProcessingResult(
                            "Document", null, null, documentProcessingResults);
                        // add result to result list
                        namedProcessingResults.Add(documentResult);
                    }
                }

                // if page has processings results
                if (pagesProcessingResult.Count != 0)
                {
                    // create dictionary: index of PDF page => processing results
                    Dictionary<int, List<ProcessingResult>> pageIndexToProcessingResults =
                        SortProcessingResultsByPage(pagesProcessingResult);

                    PdfPage[] pages = new PdfPage[pagesProcessingResult.Count];
                    pagesProcessingResult.Keys.CopyTo(pages, 0);
                    PdfDocument document = pages[0].Document;

                    if (pageIndexToProcessingResults.Count > 0)
                    {
                        // add processing results of PDF pages
                        foreach (int pageIndex in pageIndexToProcessingResults.Keys)
                        {
                            // get trigger activation results
                            List<ProcessingResult> results = GetTriggerActivationResults(pageIndexToProcessingResults[pageIndex]);
                            if (results.Count > 0)
                            {
                                // group similar results
                                results = GroupSimilarProcessingResults(result, results);

                                // root result for document applied commands 
                                NamedProcessingResult pageResult = new NamedProcessingResult(
                                    string.Format("Page {0}", pageIndex + 1), null, null, results);
                                // add result to result list
                                namedProcessingResults.Add(pageResult);
                            }
                        }
                    }
                }

                // create processing result
                return new NamedProcessingResult(
                    result,
                    namedProcessingResults);
            }
            else
                return base.ProcessResult(result);
        }

        #endregion


        #region PRIVATE

        #region SortAppliedCommands

        /// <summary>
        /// Finds and sorts commands, which were applied to the pages.
        /// </summary>
        /// <param name="inputProcessingResult">The input result of processing.</param>
        /// <param name="outputDocumentProcessingResults">The output results,
        /// which are related with PDF document.</param>
        /// <param name="outputPagesProcessingResults">The output results,
        /// which are related with PDF pages.</param>
        private void FindAndSortPagesAppliedCommands(
            ProcessingResult inputProcessingResult,
            ref Dictionary<IProcessingCommandInfo, List<ProcessingResult>> outputDocumentAppliedCommands,
            ref Dictionary<PdfPage, Dictionary<IProcessingCommandInfo, List<ProcessingResult>>> outputPagesAppliedCommands)
        {
            // new document applied commands
            Dictionary<IProcessingCommandInfo, List<ProcessingResult>> newDocumentAppliedCommands =
                new Dictionary<IProcessingCommandInfo, List<ProcessingResult>>();

            // get document
            PdfDocument document = inputProcessingResult.Target as PdfDocument;

            CompositeProcessingResult inputDetailedResult =
                (CompositeProcessingResult)((ConversionProfileResult)inputProcessingResult).DetailedResult;
            // get result to target table
            Dictionary<ProcessingTargetChangedResult, object> resultToTarget =
                GetTargetChangedResultToTargetTable(inputDetailedResult);

            // tree node to pages table
            Dictionary<PdfTreeNodeBase, List<PdfPage>> treeNodeToPages = new Dictionary<PdfTreeNodeBase, List<PdfPage>>();

            // if table is not empty
            if (resultToTarget.Count != 0)
            {
                // get tree node to pages table
                FillTreeNodeToPagesTable(document, resultToTarget.Values, treeNodeToPages);

                // for each target changed result
                foreach (ProcessingTargetChangedResult targetChangedResult in resultToTarget.Keys)
                {
                    // indicate that target belongs to the page 
                    bool isPageTarget = false;

                    // get target node
                    PdfTreeNodeBase targetNode = resultToTarget[targetChangedResult] as PdfTreeNodeBase;

                    if (targetNode != null)
                    {
                        List<PdfPage> pages = null;
                        if (treeNodeToPages.TryGetValue(targetNode, out pages))
                        {
                            isPageTarget = true;
                            // for each page in list
                            foreach (PdfPage page in pages)
                            {
                                // add result to the pages applied commands
                                Dictionary<IProcessingCommandInfo, List<ProcessingResult>> pageAppliedCommandsList = null;
                                if (!outputPagesAppliedCommands.TryGetValue(page, out pageAppliedCommandsList))
                                {
                                    pageAppliedCommandsList = new Dictionary<IProcessingCommandInfo, List<ProcessingResult>>();
                                    outputPagesAppliedCommands.Add(page, pageAppliedCommandsList);
                                }

                                List<ProcessingResult> resultList = null;
                                if (!pageAppliedCommandsList.TryGetValue(targetChangedResult.ProcessingCommand, out resultList))
                                {
                                    resultList = new List<ProcessingResult>();
                                    pageAppliedCommandsList.Add(targetChangedResult.ProcessingCommand, resultList);
                                }

                                resultList.Add(targetChangedResult);
                            }
                        }
                    }
                    // if result is not belong to the page
                    if (!isPageTarget)
                    {
                        List<ProcessingResult> resultList = null;
                        if (!newDocumentAppliedCommands.TryGetValue(targetChangedResult.ProcessingCommand, out resultList))
                        {
                            resultList = new List<ProcessingResult>();
                            newDocumentAppliedCommands.Add(targetChangedResult.ProcessingCommand, resultList);
                        }

                        resultList.Add(targetChangedResult);
                    }
                }
                outputDocumentAppliedCommands = newDocumentAppliedCommands;
            }
        }

        /// <summary>
        /// Compares two processing commands by name.
        /// </summary>
        /// <param name="command1">The first processing command to compare.</param>
        /// <param name="command2">The second processing command to compare.</param>
        /// <returns>
        /// Less than zero - <see cref="command1"/> name value less than <see cref="command2"/> name value.
        /// Zero - command names are equals. 
        /// Greater than zero - <see cref="command1"/> name value greater than <see cref="command2"/> name value.
        /// </returns>
        private int CompareProcessingCommandByName(IProcessingCommandInfo command1, IProcessingCommandInfo command2)
        {
            return string.Compare(command1.Name, command2.Name);
        }

        /// <summary>
        /// Returns a dictionary: processing result => target.
        /// </summary>
        /// <param name="results">Composite processing result.</param>
        /// <returns>A dictionary: processing result => target.</returns>
        private Dictionary<ProcessingTargetChangedResult, object> GetTargetChangedResultToTargetTable(
            CompositeProcessingResult result)
        {
            // dictionary: processing result => target type
            Dictionary<ProcessingTargetChangedResult, object> resultToTargetType = new Dictionary<ProcessingTargetChangedResult, object>();

            if (result is CompositeProcessingResult)
            {
                // get results of composite result
                Dictionary<ProcessingResult, ProcessingResult> resultToParentResult = ((CompositeProcessingResult)result).GetResultToParentTable();

                // for each result
                foreach (KeyValuePair<ProcessingResult, ProcessingResult> resultPair in resultToParentResult)
                {
                    // if result is trigger activation result
                    if (resultPair.Key is ProcessingTargetChangedResult)
                    {
                        PdfTreeNodeBase targetNode = GetTargetNode(resultPair.Key, resultToParentResult);

                        if (targetNode != null)
                            // add result to the dictionary
                            resultToTargetType.Add((ProcessingTargetChangedResult)resultPair.Key, targetNode);
                        else
                            resultToTargetType.Add((ProcessingTargetChangedResult)resultPair.Key, result.Target);
                    }
                }
            }
            return resultToTargetType;
        }

        #endregion


        #region SortProcessingResult

        /// <summary>
        /// Finds and sorts pages processing result.
        /// </summary>
        /// <param name="inputProcessingResult">The input result of processing.</param>
        /// <param name="outputDocumentProcessingResults">The output results,
        /// which are related with PDF document.</param>
        /// <param name="outputPagesProcessingResults">The output results,
        /// which are related with PDF pages.</param>
        private void FindAndSortPagesProcessingResult(
            ProcessingResult inputProcessingResult,
            ref List<ProcessingResult> outputDocumentProcessingResults,
            ref Dictionary<PdfPage, List<ProcessingResult>> outputPagesProcessingResults)
        {
            // new document processing results
            List<ProcessingResult> newDocumentProcessingResults = new List<ProcessingResult>();

            // get document
            PdfDocument document = inputProcessingResult.Target as PdfDocument;

            // get result to target table
            Dictionary<TriggerActivationResult, object> resultToTarget =
                new Dictionary<TriggerActivationResult, object>();

            ProcessingProfileResult processingProfileResult =
                inputProcessingResult as ProcessingProfileResult;

            if (processingProfileResult != null)
            {
                ProcessingResult detailedResult = processingProfileResult.DetailedResult;

                if (detailedResult != null)
                {
                    if (detailedResult is CompositeProcessingResult)
                    {
                        // get result to target table
                        resultToTarget = GetTriggerActivationResultToTargetTable((CompositeProcessingResult)detailedResult);
                    }
                    else if (detailedResult is TriggerActivationResult)
                    {
                        TriggerActivationResult triggerActionResult = (TriggerActivationResult)detailedResult;

                        PdfTreeNodeBase targetNode = GetTargetNode(triggerActionResult, null);

                        if (targetNode != null)
                            // add result to dictionary
                            resultToTarget.Add(triggerActionResult, targetNode);
                        else
                            resultToTarget.Add(triggerActionResult, triggerActionResult.Target);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            // tree node to pages table
            Dictionary<PdfTreeNodeBase, List<PdfPage>> treeNodeToPages = new Dictionary<PdfTreeNodeBase, List<PdfPage>>();

            // if table is not empty
            if (resultToTarget.Count != 0)
            {
                // get tree node to pages table
                FillTreeNodeToPagesTable(document, resultToTarget.Values, treeNodeToPages);

                // for each trigger activation result
                foreach (TriggerActivationResult triggerResult in resultToTarget.Keys)
                {
                    // indicate that target belongs to the page 
                    bool isPageTarget = false;

                    // get target node
                    PdfTreeNodeBase targetNode = resultToTarget[triggerResult] as PdfTreeNodeBase;

                    if (targetNode != null)
                    {
                        List<PdfPage> pages = null;
                        if (treeNodeToPages.TryGetValue(targetNode, out pages))
                        {
                            isPageTarget = true;
                            // for each page in list
                            foreach (PdfPage page in pages)
                            {
                                // add result to the pages processing result
                                List<ProcessingResult> pageProcessingResultList = null;
                                if (!outputPagesProcessingResults.TryGetValue(page, out pageProcessingResultList))
                                {
                                    pageProcessingResultList = new List<ProcessingResult>();
                                    outputPagesProcessingResults.Add(page, pageProcessingResultList);
                                }
                                pageProcessingResultList.Add(triggerResult);
                            }
                        }
                    }
                    // if result is not belong to the page
                    if (!isPageTarget)
                    {
                        newDocumentProcessingResults.Add(triggerResult);
                    }
                }
                // for each page processing results
                foreach (List<ProcessingResult> pageProcessingResults in outputPagesProcessingResults.Values)
                {
                    // sort results by description
                    pageProcessingResults.Sort(CompareProcessingResultByDescription);
                }
                // sort results by description
                newDocumentProcessingResults.Sort(CompareProcessingResultByDescription);
                outputDocumentProcessingResults = newDocumentProcessingResults;
            }
        }

        /// <summary>
        /// Compares two processing results by description.
        /// </summary>
        /// <param name="result1">The first processing result to compare.</param>
        /// <param name="result2">The second processing result to compare.</param>
        /// <returns>
        /// Less than zero - <see cref="result1"/> description value less than <see cref="result2"/> description value.
        /// Zero - result descriptions are equals. 
        /// Greater than zero - <see cref="result1"/> description value greater than <see cref="result2"/> description value.
        /// </returns>
        private int CompareProcessingResultByDescription(ProcessingResult result1, ProcessingResult result2)
        {
            return string.Compare(result1.Description, result2.Description);
        }

        /// <summary>
        /// Returns a dictionary: processing result => target.
        /// </summary>
        /// <param name="results">The composite processing result.</param>
        /// <returns>A dictionary: processing result => target.</returns>
        private Dictionary<TriggerActivationResult, object> GetTriggerActivationResultToTargetTable(CompositeProcessingResult result)
        {
            // dictionary: processing result => target type
            Dictionary<TriggerActivationResult, object> resultToTargetType = new Dictionary<TriggerActivationResult, object>();

            if (result is CompositeProcessingResult)
            {
                // get results of composite result
                Dictionary<ProcessingResult, ProcessingResult> resultToParentResult = ((CompositeProcessingResult)result).GetResultToParentTable();

                // for each result
                foreach (KeyValuePair<ProcessingResult, ProcessingResult> resultPair in resultToParentResult)
                {
                    // if result is trigger activation result
                    if (resultPair.Key is TriggerActivationResult)
                    {
                        PdfTreeNodeBase targetNode = GetTargetNode(resultPair.Key, resultToParentResult);

                        if (targetNode != null)
                            // add result to dictionary
                            resultToTargetType.Add((TriggerActivationResult)resultPair.Key, targetNode);
                        else
                            resultToTargetType.Add((TriggerActivationResult)resultPair.Key, result.Target);
                    }
                }
            }
            return resultToTargetType;
        }

        #endregion


        /// <summary>
        /// Returns target node.
        /// </summary>
        /// <param name="result">Processing result.</param>
        /// <param name="resultToParentResult">Dictionary: child result => parent composite result.</param>
        /// <returns>Target node.</returns>
        private static PdfTreeNodeBase GetTargetNode(ProcessingResult result, Dictionary<ProcessingResult, ProcessingResult> resultToParentResult)
        {
            PdfTreeNodeBase targetNode = null;

            // if target is PdfTreeNodeBase
            if (result.Target is PdfTreeNodeBase)
            {
                targetNode = (PdfTreeNodeBase)result.Target;
            }
            else if (result.Target is Vintasoft.Imaging.Pdf.Processing.PdfContentStreamProcessingParams)
            {
                targetNode = ((Vintasoft.Imaging.Pdf.Processing.PdfContentStreamProcessingParams)result.Target).TreeNode;
            }
            else if (result.Target is Vintasoft.Imaging.Pdf.Processing.PdfContentStreamResourcesProcessingParams)
            {
                targetNode = ((Vintasoft.Imaging.Pdf.Processing.PdfContentStreamResourcesProcessingParams)result.Target).TreeNode;
            }
            else if (result.Target is Vintasoft.Imaging.Pdf.Processing.PdfFontProcessingParams)
            {
                targetNode = ((Vintasoft.Imaging.Pdf.Processing.PdfFontProcessingParams)result.Target).Font;
            }
            else if (result.Target is Vintasoft.Imaging.Pdf.Processing.PdfImageResourceAsImageDecoderProcessingParams)
            {
                targetNode = ((Vintasoft.Imaging.Pdf.Processing.PdfImageResourceAsImageDecoderProcessingParams)result.Target).ImageResource;
            }
            else if (result.Target is Vintasoft.Imaging.Pdf.Processing.PdfContentStreamCommandProcessingParams ||
                result.Target is Vintasoft.Imaging.Pdf.Processing.IccProfileProcessingParams)
            {
                if (resultToParentResult != null && resultToParentResult.ContainsKey(result))
                    targetNode = GetTargetNode(resultToParentResult[result], resultToParentResult);
            }
            return targetNode;
        }

        /// <summary>
        /// Fills dictionary: tree node => the pages table.
        /// </summary>
        /// <param name="document">PDF document.</param>
        /// <param name="objectList">Object list.</param>
        /// <param name="treeNodeToPages">Tree node to pages table.</param>
        private void FillTreeNodeToPagesTable(
            PdfDocument document,
            ICollection<object> objectList,
            Dictionary<PdfTreeNodeBase, List<PdfPage>> treeNodeToPages)
        {
            // get type collection
            List<Type> typeCollection = new List<Type>(objectList.Count);
            foreach (object item in objectList)
            {
                Type tagretType = item.GetType();
                if (!typeCollection.Contains(tagretType))
                    typeCollection.Add(tagretType);
            }

            // for each page in document
            foreach (PdfPage page in document.Pages)
            {
                // get page linearized subtree
                PdfTreeNodeBase[] linearizedSubtree = page.GetLinearizedSubtree(
                    new Vintasoft.Imaging.Processing.Analyzers.TargetTypeAnalyzer<PdfTreeNodeBase>(typeCollection.ToArray()));

                // for each node in linearized subtree
                foreach (PdfTreeNodeBase node in linearizedSubtree)
                {
                    List<PdfPage> pageList = null;
                    // if node is not used
                    if (!treeNodeToPages.TryGetValue(node, out pageList))
                    {
                        pageList = new List<PdfPage>();
                        // add node to the dictionary
                        treeNodeToPages.Add(node, pageList);
                    }
                    // add page to page list
                    pageList.Add(page);
                }
            }
        }

        /// <summary>
        /// Sorts the dictionary by PDF page.
        /// </summary>
        /// <param name="targetToProcessingResult">The dictionary.</param>
        private Dictionary<int, T> SortProcessingResultsByPage<T>(Dictionary<PdfPage, T> dictionary)
        {
            List<int> pageIndexesList = new List<int>();
            List<T> pageResultsList = new List<T>();

            // search PDF pages of PDF document

            foreach (PdfPage page in dictionary.Keys)
            {
                int index = page.Document.Pages.IndexOf(page);
                if (index >= 0)
                {
                    pageIndexesList.Add(index);
                    pageResultsList.Add(dictionary[page]);
                }
            }

            // sort indexes of pages
            int[] pageIndexes = pageIndexesList.ToArray();
            T[] pageResults = pageResultsList.ToArray();
            Array.Sort(pageIndexes, pageResults);

            // create dictionary
            Dictionary<int, T> result = new Dictionary<int, T>();

            for (int i = 0; i < pageIndexes.Length; i++)
                result.Add(pageIndexes[i], pageResults[i]);

            return result;
        }

        /// <summary>
        /// Sorts the processing results.
        /// </summary>
        /// <param name="inputProcessingResult">The input result of processing.</param>
        /// <param name="previousTarget">The previous target of input processing result.</param>
        /// <param name="outputDocumentProcessingResults">The output results,
        /// which are related with PDF document.</param>
        /// <param name="outputPagesProcessingResults">The output results,
        /// which are related with PDF pages.</param>
        private void SortProcessingResult(
            ProcessingResult inputProcessingResult,
            PdfPage previousTarget,
            List<ProcessingResult> documentProcessingResults,
            Dictionary<PdfPage, List<ProcessingResult>> pagesProcessingResult)
        {
            PdfPage targetPage = inputProcessingResult.Target as PdfPage;
            if (targetPage == null)
                targetPage = previousTarget;

            // if processing result is trigger
            if (inputProcessingResult is TriggerActivationResult)
            {
                if (targetPage != null)
                    AddToDictionary(pagesProcessingResult, targetPage, inputProcessingResult);
                else
                    documentProcessingResults.Add(inputProcessingResult);
            }
            // if processing result is collection
            else if (inputProcessingResult is IEnumerable<ProcessingResult>)
            {
                IEnumerable<ProcessingResult> processingResultArray =
                    (IEnumerable<ProcessingResult>)inputProcessingResult;
                foreach (ProcessingResult result in processingResultArray)
                {
                    SortProcessingResult(result, targetPage, documentProcessingResults, pagesProcessingResult);
                }
            }
            else
            {
                if (targetPage != null)
                    AddToDictionary(pagesProcessingResult, (PdfPage)targetPage, inputProcessingResult);
                else
                    documentProcessingResults.Add(inputProcessingResult);
            }
        }

        /// <summary>
        /// Groups the similar processing results.
        /// </summary>
        /// <param name="sourceResult">The source result.</param>
        /// <param name="results">The results.</param>
        private List<ProcessingResult> GroupSimilarProcessingResults(
            ProcessingResult sourceResult,
            List<ProcessingResult> results)
        {
            // create a dictionary (description => processing result)
            Dictionary<string, List<ProcessingResult>> descriptionToProcessingResult =
                new Dictionary<string, List<ProcessingResult>>();

            // for each processing result
            foreach (ProcessingResult result in results)
            {
                AddToDictionary(descriptionToProcessingResult, result.Description, result);
            }

            // if processing results are not grouped
            if (descriptionToProcessingResult.Count == results.Count)
                // return the source processing results
                return results;


            // create new list with grouped processing results

            List<ProcessingResult> resultProcessingResultList = new List<ProcessingResult>();

            foreach (string key in descriptionToProcessingResult.Keys)
            {
                List<ProcessingResult> list = descriptionToProcessingResult[key];

                if (list.Count == 1)
                {
                    resultProcessingResultList.Add(list[0]);
                }
                else
                {
                    // create named processing result
                    NamedProcessingResult namedProcessingResult = new NamedProcessingResult(
                            string.Format("{0} ({1} matches)", key, list.Count), null, null, list);
                    resultProcessingResultList.Add(namedProcessingResult);
                }
            }

            return resultProcessingResultList;
        }

        /// <summary>
        /// Adds the processing results to the specified dictionary with processing results.
        /// </summary>
        /// <typeparam name="T">The type of key in dictionary.</typeparam>
        /// <param name="dict">The dictionary, where the processing result must be added.</param>
        /// <param name="key">The key in dictionary.</param>
        /// <param name="processingResult">The processing result, which should be added.</param>
        private void AddToDictionary<T>(
            Dictionary<T, List<ProcessingResult>> dict,
            T key,
            ProcessingResult processingResult)
        {
            List<ProcessingResult> results = null;
            if (!dict.TryGetValue(key, out results))
            {
                results = new List<ProcessingResult>();
                dict.Add(key, results);
            }

            results.Add(processingResult);
        }

        /// <summary>
        /// Returns the result of trigger activation.
        /// </summary>
        /// <param name="sourceProcessingResults">The source processing results.</param>
        private List<ProcessingResult> GetTriggerActivationResults(List<ProcessingResult> sourceProcessingResults)
        {
            List<ProcessingResult> resultList = new List<ProcessingResult>();
            foreach (ProcessingResult processingResult in sourceProcessingResults)
            {
                if (processingResult is TriggerActivationResult)
                    resultList.Add(processingResult);
            }

            return resultList;
        }

        #endregion

        #endregion

    }
}

#endif