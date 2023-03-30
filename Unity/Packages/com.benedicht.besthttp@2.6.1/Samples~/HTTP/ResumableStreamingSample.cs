using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using BestHTTP;

namespace BestHTTP.Examples.HTTP
{
    public sealed class ResumableStreamingSample : StreamingSample
    {
        const string ProcessedBytesKey = "ProcessedBytes";
        const string DownloadLengthKey = "DownloadLength";

        /// <summary>
        /// Expected content length
        /// </summary>
        protected override long DownloadLength { get { return PlayerPrefs.GetInt(this._downloadPath + DownloadLengthKey); } set { PlayerPrefs.SetInt(this._downloadPath + DownloadLengthKey, (int)value); } }

        /// <summary>
        /// Total processed bytes
        /// </summary>
        protected override long ProcessedBytes { get { return PlayerPrefs.GetInt(this._downloadPath + ProcessedBytesKey, 0); } set { PlayerPrefs.SetInt(this._downloadPath + ProcessedBytesKey, (int)value); } }

        private long downloadStartedAt = 0;

        protected override void Start()
        {
            base.Start();

            // If we have a non-finished download, set the progress to the value where we left it
            float progress = GetSavedProgress();
            if (progress > 0.0f)
            {
                this._downloadProgressSlider.value = progress;
                base._statusText.text = progress.ToString("F2");
            }
        }

        protected override void SetupRequest()
        {
            base.SetupRequest();

            // Are there any progress, that we can continue?
            this.downloadStartedAt = this.ProcessedBytes;

            if (this.downloadStartedAt > 0)
            {
                // Set the range header
                request.SetRangeHeader(this.downloadStartedAt);
            }
            else
                // This is a new request
                DeleteKeys();
        }

        protected override void OnRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            base.OnRequestFinished(req, resp);

            if (req.State == HTTPRequestStates.Finished && resp.IsSuccess)
                DeleteKeys();
        }

        protected override void OnDownloadProgress(HTTPRequest originalRequest, long downloaded, long downloadLength)
        {
            double downloadPercent = ((this.downloadStartedAt + downloaded) / (double)this.DownloadLength) * 100;

            this._downloadProgressSlider.value = (float)downloadPercent;
            this._downloadProgressText.text = string.Format("{0:F1}%", downloadPercent);
        }

        protected override void ResetProcessedValues()
        {
            SetDataProcessedUI(this.ProcessedBytes, this.DownloadLength);
        }

        private float GetSavedProgress()
        {
            long down = this.ProcessedBytes;
            long length = this.DownloadLength;

            if (down > 0 && length > 0)
                return (down / (float)length) * 100f;

            return -1;
        }

        private void DeleteKeys()
        {
            PlayerPrefs.DeleteKey(this._downloadPath + ProcessedBytesKey);
            PlayerPrefs.DeleteKey(this._downloadPath + DownloadLengthKey);
            PlayerPrefs.Save();
        }
    }
}