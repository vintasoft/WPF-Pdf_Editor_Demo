using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;


namespace WpfDemosCommonCode
{
    /// <summary>
    /// Represents controller of progress visualization of an action.
    /// </summary>
    public class StatusBarActionController
    {

        #region Fields

        /// <summary>
        /// The view of action animation.
        /// </summary>
        string[] _subActionAnimation = new string[] { ".", "..", "..." };

        /// <summary>
        /// The index of action animation view.
        /// </summary>
        int _subActionAnimationIndex = 0;

        /// <summary>
        /// The status label.
        /// </summary>
        Label _statusLabel;

        /// <summary>
        /// The progress bar.
        /// </summary>
        RangeBase _progressBar;

        /// <summary>
        /// The status bar with status label and progress bar.
        /// </summary>
        FrameworkElement _statusBar;

        /// <summary>
        /// The action start time.
        /// </summary>
        DateTime _actionStartTime;

        /// <summary>
        /// The name of current action.
        /// </summary>
        string _actionName;

        Window _main = null;

        #endregion



        #region Constructors

        public StatusBarActionController(
           FrameworkElement statusStrip,
           Label statusLabel,
           RangeBase progressBar,
            Window main)
            : this(statusStrip, statusLabel, progressBar)
        {
            _main = main;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusStripActionController"/> class.
        /// </summary>
        /// <param name="statusStrip">The status strip.</param>
        /// <param name="statusLabel">The status label.</param>
        /// <param name="progressBar">The progress bar.</param>
        public StatusBarActionController(
            FrameworkElement statusStrip,
            Label statusLabel,
            RangeBase progressBar)
        {
            _statusLabel = statusLabel;
            _progressBar = progressBar;
            _statusBar = statusStrip;
            Reset();
        }

        #endregion



        #region Methods

        /// <summary>
        /// Visualizes a next sub action.
        /// </summary>
        public void NextSubAction()
        {
            NextSubAction(null);
        }

        /// <summary>
        ///  Visualizes a next sub action with specified name.
        /// </summary>
        /// <param name="name">The sub action name.</param>
        public void NextSubAction(string name)
        {
            if (_progressBar.Visibility == Visibility.Visible)
            {
                if (_progressBar.Value < _progressBar.Maximum)
                {
                    _progressBar.Value++;
                }
            }
            else
            {
                if (name == null)
                {
                    _statusLabel.Content = string.Format("{0}{1}", _actionName, _subActionAnimation[_subActionAnimationIndex]);
                    _subActionAnimationIndex++;
                    _subActionAnimationIndex %= _subActionAnimation.Length;
                }
                else
                {
                    _statusLabel.Content = string.Format("{0}... ({1})", _actionName, name);
                }
            }
            DemosTools.DoEvents();
        }

        /// <summary>
        /// Starts an action with specified name.
        /// </summary>
        /// <param name="anctionName">Name of the action.</param>
        public void StartAction(string anctionName)
        {
            StartAction(anctionName, 0);
        }

        /// <summary>
        /// Starts the action with specified name and count of sub actions.
        /// </summary>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="progressSteps">The progress steps.</param>
        public void StartAction(string actionName, int progressSteps)
        {
            _actionName = actionName;
            bool useProgress = progressSteps > 0;
            if (useProgress)
                _statusLabel.Content = string.Format("{0}: ", _actionName);
            else
                _statusLabel.Content = string.Format("{0}...", _actionName);
            _statusLabel.Visibility = Visibility.Visible;
            if (useProgress)
            {
                _progressBar.Visibility = Visibility.Visible;
                _progressBar.Maximum = progressSteps;
                _progressBar.Value = 0;
            }
            _actionStartTime = DateTime.Now;
            DemosTools.DoEvents();
        }

        /// <summary>
        /// Ends the action.
        /// </summary>
        public void EndAction()
        {
            double actionMs = (DateTime.Now - _actionStartTime).TotalMilliseconds;
            _progressBar.Visibility = Visibility.Hidden;
            _statusLabel.Content = string.Format("{0}: {1} ms.", _actionName, actionMs);
        }

        /// <summary>
        /// Resets this action controller.
        /// </summary>
        public void Reset()
        {
            _statusLabel.Visibility = Visibility.Hidden;
            _progressBar.Visibility = Visibility.Hidden;
        }

        #endregion

    }
}
