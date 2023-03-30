using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using BestHTTP;

namespace BestHTTP.Examples.HTTP
{
    public sealed class TextureDownloadSample : BestHTTP.Examples.Helpers.SampleBase
    {
#pragma warning disable 0649
        [Header("Texture Download Example")]

        [Tooltip("The URL of the server that will serve the image resources")]
        [SerializeField]
        private string _path = "/images/Demo/";

        [Tooltip("The downloadable images")]
        [SerializeField]
        private string[] _imageNames = new string[9] { "One.png", "Two.png", "Three.png", "Four.png", "Five.png", "Six.png", "Seven.png", "Eight.png", "Nine.png" };

        [SerializeField]
        private RawImage[] _images = new RawImage[0];

        [SerializeField]
        private Text _maxConnectionPerServerLabel;

        [SerializeField]
        private Text _cacheLabel;

#pragma warning restore

        private byte savedMaxConnectionPerServer;

        #if !BESTHTTP_DISABLE_CACHING
        private bool allDownloadedFromLocalCache;
        #endif

        private List<HTTPRequest> activeRequests = new List<HTTPRequest>();

        protected override void Start()
        {
            base.Start();

            this.savedMaxConnectionPerServer = HTTPManager.MaxConnectionPerServer;

            // Set a well observable value
            // This is how many concurrent requests can be made to a server
            HTTPManager.MaxConnectionPerServer = 1;

            this._maxConnectionPerServerLabel.text = HTTPManager.MaxConnectionPerServer.ToString();
        }
        
        void OnDestroy()
        {
            // Set back to its defualt value.
            HTTPManager.MaxConnectionPerServer = this.savedMaxConnectionPerServer;
            foreach (var request in this.activeRequests)
                request.Abort();
            this.activeRequests.Clear();
        }

        public void OnMaxConnectionPerServerChanged(float value)
        {
            HTTPManager.MaxConnectionPerServer = (byte)Mathf.RoundToInt(value);
            this._maxConnectionPerServerLabel.text = HTTPManager.MaxConnectionPerServer.ToString();
        }

        public void DownloadImages()
        {
            // Set these metadatas to its initial values
#if !BESTHTTP_DISABLE_CACHING
            allDownloadedFromLocalCache = true;
#endif
            
            for (int i = 0; i < _imageNames.Length; ++i)
            {
                // Set a blank placeholder texture, overriding previously downloaded texture
                this._images[i].texture = null;

                // Construct the request
                var request = new HTTPRequest(new Uri(this.sampleSelector.BaseURL + this._path + this._imageNames[i]), ImageDownloaded);

                // Set the Tag property, we can use it as a general storage bound to the request
                request.Tag = this._images[i];

                // Send out the request
                request.Send();

                this.activeRequests.Add(request);
            }

            this._cacheLabel.text = string.Empty;
        }

        /// <summary>
        /// Callback function of the image download http requests
        /// </summary>
        void ImageDownloaded(HTTPRequest req, HTTPResponse resp)
        {
            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        // The target RawImage reference is stored in the Tag property
                        RawImage rawImage = req.Tag as RawImage;
                        rawImage.texture = resp.DataAsTexture2D;

#if !BESTHTTP_DISABLE_CACHING
                        // Update the cache-info variable
                        allDownloadedFromLocalCache = allDownloadedFromLocalCache && resp.IsFromCache;
#endif
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                                                        resp.StatusCode,
                                                        resp.Message,
                                                        resp.DataAsText));
                    }
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    Debug.LogError("Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception"));
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    Debug.LogWarning("Request Aborted!");
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    Debug.LogError("Connection Timed Out!");
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    Debug.LogError("Processing the request Timed Out!");
                    break;
            }

            this.activeRequests.Remove(req);
            if (this.activeRequests.Count == 0)
            {
#if !BESTHTTP_DISABLE_CACHING
                if (this.allDownloadedFromLocalCache)
                    this._cacheLabel.text = "All images loaded from local cache!";
                else
#endif
                    this._cacheLabel.text = string.Empty;
            }
        }
    }
}
