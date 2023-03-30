using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using BestHTTP;

namespace BestHTTP.Examples.HTTP
{
    public class StreamingSample : BestHTTP.Examples.Helpers.SampleBase
    {
#pragma warning disable 0649

        [Tooltip("The url of the resource to download")]
        [SerializeField]
        protected string _downloadPath = "/test100mb.dat";

        [Header("Streaming Setup")]

        [SerializeField]
        protected RectTransform _streamingSetupRoot;

        [SerializeField]
        protected Slider _fragmentSizeSlider;

        [SerializeField]
        protected Text _fragmentSizeText;

        [SerializeField]
        protected Toggle _disableCacheToggle;

        [Header("Reporting")]

        [SerializeField]
        protected RectTransform _reportingRoot;

        [SerializeField]
        protected Slider _downloadProgressSlider;

        [SerializeField]
        protected Text _downloadProgressText;

        [SerializeField]
        protected Slider _processedDataSlider;

        [SerializeField]
        protected Text _processedDataText;

        [SerializeField]
        protected Text _statusText;

        [SerializeField]
        protected Button _startDownload;

        [SerializeField]
        protected Button _cancelDownload;

#pragma warning restore
        
        /// <summary>
        /// Cached request to be able to abort it
        /// </summary>
        protected HTTPRequest request;

        /// <summary>
        /// Download(processing) progress. Its range is between [0..1]
        /// </summary>
        protected float progress;

        /// <summary>
        /// The fragment size that we will set to the request
        /// </summary>
        protected int fragmentSize = HTTPResponse.MinReadBufferSize;

        protected virtual long DownloadLength { get; set; }

        protected virtual long ProcessedBytes { get; set; }

        protected override void Start()
        {
            base.Start();

            this._streamingSetupRoot.gameObject.SetActive(true);
            this._reportingRoot.gameObject.SetActive(false);

            this._startDownload.interactable = true;
            this._cancelDownload.interactable = false;

            this._fragmentSizeSlider.value = (1024 * 1024 - HTTPResponse.MinReadBufferSize) / 1024;
            this._fragmentSizeText.text = GUIHelper.GetBytesStr(1024 * 1024, 1);
        }

        protected void OnDestroy()
        {
            // Stop the download if we are leaving this example
            if (request != null && request.State < HTTPRequestStates.Finished)
            {
                request.OnDownloadProgress = null;
                request.Callback = null;
                request.Abort();
            }
        }

        public void OnFragmentSizeSliderChanged(float value)
        {
            this.fragmentSize = HTTPResponse.MinReadBufferSize + (int)value * 1024;
            this._fragmentSizeText.text = GUIHelper.GetBytesStr(this.fragmentSize, 1);
        }

        public void Cancel()
        {
            if (this.request != null)
                this.request.Abort();
        }

        protected virtual void SetupRequest()
        {
            request = new HTTPRequest(new Uri(base.sampleSelector.BaseURL + this._downloadPath), OnRequestFinished);

#if !BESTHTTP_DISABLE_CACHING
            // If we are writing our own file set it to true(disable), so don't duplicate it on the file-system
            request.DisableCache = this._disableCacheToggle.isOn;
#endif

            request.StreamFragmentSize = fragmentSize;

            request.Tag = DateTime.Now;

            request.OnHeadersReceived += OnHeadersReceived;
            request.OnDownloadProgress += OnDownloadProgress;
            request.OnStreamingData += OnDataDownloaded;
        }

        public virtual void StartStreaming()
        {
            SetupRequest();

            // Start Processing the request
            request.Send();

            this._statusText.text = "Download started!";

            // UI
            this._streamingSetupRoot.gameObject.SetActive(false);
            this._reportingRoot.gameObject.SetActive(true);

            this._startDownload.interactable = false;
            this._cancelDownload.interactable = true;

            ResetProcessedValues();
        }

        private void OnHeadersReceived(HTTPRequest req, HTTPResponse resp, Dictionary<string, List<string>> newHeaders)
        {
            var range = resp.GetRange();
            if (range != null)
                this.DownloadLength = range.ContentLength;
            else
            {
                var contentLength = resp.GetFirstHeaderValue("content-length");
                if (contentLength != null)
                {
                    long length = 0;
                    if (long.TryParse(contentLength, out length))
                        this.DownloadLength = length;
                }
            }
        }

        protected virtual void OnRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        DateTime downloadStarted = (DateTime)req.Tag;
                        TimeSpan diff = DateTime.Now - downloadStarted;
                        
                        this._statusText.text = string.Format("Streaming finished in {0:N0}ms", diff.TotalMilliseconds);
                    }
                    else
                    {
                        this._statusText.text = string.Format("Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                                                        resp.StatusCode,
                                                        resp.Message,
                                                        resp.DataAsText);
                        Debug.LogWarning(this._statusText.text);

                        request = null;
                    }
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    this._statusText.text = "Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception");
                    Debug.LogError(this._statusText.text);

                    request = null;
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    this._statusText.text = "Request Aborted!";
                    Debug.LogWarning(this._statusText.text);

                    request = null;
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    this._statusText.text = "Connection Timed Out!";
                    Debug.LogError(this._statusText.text);

                    request = null;
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    this._statusText.text = "Processing the request Timed Out!";
                    Debug.LogError(this._statusText.text);

                    request = null;
                    break;
            }

            // UI

            this._streamingSetupRoot.gameObject.SetActive(true);
            this._reportingRoot.gameObject.SetActive(false);

            this._startDownload.interactable = true;
            this._cancelDownload.interactable = false;
            request = null;
        }

        protected virtual void OnDownloadProgress(HTTPRequest originalRequest, long downloaded, long downloadLength)
        {
            double downloadPercent = (downloaded / (double)downloadLength) * 100;
            this._downloadProgressSlider.value = (float)downloadPercent;
            this._downloadProgressText.text = string.Format("{0:F1}%", downloadPercent);
        }

        protected virtual bool OnDataDownloaded(HTTPRequest request, HTTPResponse response, byte[] dataFragment, int dataFragmentLength)
        {
            this.ProcessedBytes += dataFragmentLength;
            SetDataProcessedUI(this.ProcessedBytes, this.DownloadLength);

            // Use downloaded data

            // Return true if dataFrament is processed so the plugin can recycle the byte[]
            return true;
        }

        protected void SetDataProcessedUI(long processed, long length)
        {
            float processedPercent = (processed / (float)length) * 100f;

            this._processedDataSlider.value = processedPercent;
            this._processedDataText.text = GUIHelper.GetBytesStr(processed, 0);
        }

        protected virtual void ResetProcessedValues()
        {
            this.ProcessedBytes = 0;
            this.DownloadLength = 0;

            SetDataProcessedUI(this.ProcessedBytes, this.DownloadLength);
        }
    }
}
