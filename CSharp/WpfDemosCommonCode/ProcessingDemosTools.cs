using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintasoft.Imaging.Processing;
using Vintasoft.Imaging.Processing.Analyzers;

namespace WpfDemosCommonCode
{
    /// <summary>
    /// Contains collection of helper-algorithms for
    /// processing commands in demo applications.
    /// </summary>
    public class ProcessingDemosTools
    {

        /// <summary>
        /// Executes the processing of target using the specified processing command.
        /// </summary>
        /// <param name="processingTarget">The target of processing.</param>
        /// <param name="command">The processing command.</param>
        public static void ExecuteProcessing<T>(
            T processingTarget,
            IProcessingCommand<T> command)
        {
            using (ProcessingState processingState = new ProcessingState())
            {
                try
                {
                    ProcessingResult result = command.Execute(processingTarget, processingState);
                    string message = "The processing command did not return the result.";
                    if (result != null)
                    {
                        if (result is AnalyzerResult)
                        {
                            AnalyzerResult analyzerResult = (AnalyzerResult)result;
                            message = string.Format("Result is {0}", analyzerResult.GetValue());
                        }
                        else if (result is ProcessingTargetChangedResult)
                        {
                            message = "The processing command is executed successfully.";
                        }
                        if (result is CompositeProcessingResult)
                        {
                            int changedTargets = 0;
                            foreach (ProcessingResult subResult in (CompositeProcessingResult)result)
                            {
                                if (subResult is ProcessingTargetChangedResult)
                                {
                                    changedTargets++;
                                }
                                else
                                {
                                    changedTargets = -1;
                                    break;
                                }
                            }
                            if (changedTargets > 0)
                            {
                                message = string.Format("The processing command is executed successfully (processed {0} target(s)).", changedTargets);
                            }
                            else
                            {
                                message = result.Description;
                            }
                        }
                        else
                        {
                            message = result.Description;
                        }
                    }
                    DemosTools.ShowInfoMessage(command.Name, message);
                }
                catch (Exception ex)
                {
                    DemosTools.ShowErrorMessage(ex);
                }
            }
        }

        /// <summary>
        /// Determines that the name of type or base type starts from the specified string.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        public static bool IsNameEqual(Type type, string name)
        {
            if (type != null)
            {
                if (type.Name.StartsWith(name, StringComparison.InvariantCulture) ||
                    IsNameEqual(type.BaseType, name))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns "readable name" of the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The "readable name" of the type.</returns>
        public static string GetReadableTypeName(Type type)
        {
            string name = string.Empty;

            if (type != null)
            {
                name = type.Name;

                int index = name.IndexOf('`');
                if (index > 0)
                    name = name.Substring(0, index);

                for (int i = 0; i < name.Length; i++)
                {
                    if (!char.IsLetterOrDigit(name, i))
                    {
                        name = GetReadableTypeName(type.BaseType);
                        break;
                    }
                }
            }

            return name;
        }

    }
}
