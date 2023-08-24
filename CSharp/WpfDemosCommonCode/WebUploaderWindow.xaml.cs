using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;

namespace WpfDemosCommonCode
{
    /// <summary>
    /// A window that allows to upload data to a web server.
    /// </summary>
    public partial class WebUploaderWindow : Window
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebUploaderWindow"/> class.
        /// </summary>
        public WebUploaderWindow()
        {
            InitializeComponent();
        }

        #endregion



        #region Methods

        /// <summary>
        /// Uploads data asynchronous.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="stream">The data stream.</param>
        public void UploadAsync(string url, string contentType, Stream stream)
        {
            try
            {
                closeButton.IsEnabled = false;
                // create web client
                WebClient webClient = new WebClient();
                webClient.UploadDataCompleted += new UploadDataCompletedEventHandler(webClient_UploadDataCompleted);

                // set handlers
                AppendLog(string.Format("Content-Type={0}", contentType));
                webClient.Headers.Add("Content-Type", contentType);

                // read data
                byte[] data = new byte[(int)stream.Length];
                stream.Position = 0;
                stream.Read(data, 0, data.Length);

                // starting upload 
                AppendLog(string.Format("Upload {0} bytes to {1}...", data.Length, url));
                webClient.UploadDataAsync(new Uri(url), data);
            }
            catch (Exception ex)
            {
                AppendLog(string.Format("Error: {0}", ex.ToString()));
            }
        }

        /// <summary>
        /// Handles the UploadDataCompleted event of the webClient.
        /// </summary>
        private void webClient_UploadDataCompleted(object sender, UploadDataCompletedEventArgs e)
        {
            closeButton.IsEnabled = true;
            ((WebClient)sender).UploadDataCompleted -= webClient_UploadDataCompleted;
            if (e.Cancelled)
                AppendLog("Canceled.");
            if (e.Error != null)
                AppendLog(string.Format("Error: {0}", e.Error.Message));
            else
                AppendLog(string.Format("Response received: {0}", Encoding.ASCII.GetString(e.Result)));
        }

        /// <summary>
        /// Appends message to the log.
        /// </summary>
        /// <param name="text">The text.</param>
        private void AppendLog(string text)
        {
            logTextBox.AppendText(text + Environment.NewLine);
            logTextBox.ScrollToEnd();
        }

        /// <summary>
        /// Closes form.
        /// </summary>
        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

    }
}
