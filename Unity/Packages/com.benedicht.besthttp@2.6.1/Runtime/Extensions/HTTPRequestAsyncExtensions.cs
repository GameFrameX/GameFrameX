#if CSHARP_7_OR_LATER
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BestHTTP.Logger;
using UnityEngine;

namespace BestHTTP
{
    public sealed class AsyncHTTPException : Exception
    {
        /// <summary>
        /// Status code of the server's response.
        /// </summary>
        public int StatusCode;

        /// <summary>
        /// Content sent by the server.
        /// </summary>
        public string Content;

        public AsyncHTTPException(string message)
            : base(message)
        {
        }

        public AsyncHTTPException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AsyncHTTPException(int statusCode, string message, string content)
            :base(message)
        {
            this.StatusCode = statusCode;
            this.Content = content;
        }

        public override string ToString()
        {
            return string.Format("StatusCode: {0}, Message: {1}, Content: {2}, StackTrace: {3}", this.StatusCode, this.Message, this.Content, this.StackTrace);
        }
    }

    public static class HTTPRequestAsyncExtensions
    {
        public static Task<HTTPResponse> GetHTTPResponseAsync(this HTTPRequest request, CancellationToken token = default)
        {
            return CreateTask<HTTPResponse>(request, token, (req, resp, tcs) =>
            {
                switch (req.State)
                {
                    // The request finished without any problem.
                    case HTTPRequestStates.Finished:
                        tcs.TrySetResult(resp);
                        break;

                    // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                    case HTTPRequestStates.Error:
                        VerboseLogging(request, "Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception"));

                        tcs.TrySetException(CreateException("No Exception", null, req.Exception));
                        break;

                    // The request aborted, initiated by the user.
                    case HTTPRequestStates.Aborted:
                        VerboseLogging(request, "Request Aborted!");

                        tcs.TrySetCanceled();
                        break;

                    // Connecting to the server is timed out.
                    case HTTPRequestStates.ConnectionTimedOut:
                        VerboseLogging(request, "Connection Timed Out!");

                        tcs.TrySetException(CreateException("Connection Timed Out!"));
                        break;

                    // The request didn't finished in the given time.
                    case HTTPRequestStates.TimedOut:
                        VerboseLogging(request, "Processing the request Timed Out!");

                        tcs.TrySetException(CreateException("Processing the request Timed Out!"));
                        break;
                }
            });
        }

        public static Task<string> GetAsStringAsync(this HTTPRequest request, CancellationToken token = default)
        {
            return CreateTask<string>(request, token, (req, resp, tcs) =>
            {
                switch (req.State)
                {
                    // The request finished without any problem.
                    case HTTPRequestStates.Finished:
                        if (resp.IsSuccess)
                            tcs.TrySetResult(resp.DataAsText);
                        else
                            tcs.TrySetException(CreateException("Request finished Successfully, but the server sent an error.", resp));
                        break;

                    // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                    case HTTPRequestStates.Error:
                        VerboseLogging(request, "Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception"));

                        tcs.TrySetException(CreateException("No Exception", null, req.Exception));
                        break;

                    // The request aborted, initiated by the user.
                    case HTTPRequestStates.Aborted:
                        VerboseLogging(request, "Request Aborted!");

                        tcs.TrySetCanceled();
                        break;

                    // Connecting to the server is timed out.
                    case HTTPRequestStates.ConnectionTimedOut:
                        VerboseLogging(request, "Connection Timed Out!");

                        tcs.TrySetException(CreateException("Connection Timed Out!"));
                        break;

                    // The request didn't finished in the given time.
                    case HTTPRequestStates.TimedOut:
                        VerboseLogging(request, "Processing the request Timed Out!");

                        tcs.TrySetException(CreateException("Processing the request Timed Out!"));
                        break;
                }
            });
        }
        
        public static Task<Texture2D> GetAsTexture2DAsync(this HTTPRequest request, CancellationToken token = default)
        {
            return CreateTask<Texture2D>(request, token, (req, resp, tcs) =>
            {
                switch (req.State)
                {
                    // The request finished without any problem.
                    case HTTPRequestStates.Finished:
                        if (resp.IsSuccess)
                            tcs.TrySetResult(resp.DataAsTexture2D);
                        else
                            tcs.TrySetException(CreateException("Request finished Successfully, but the server sent an error.", resp));
                        break;

                    // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                    case HTTPRequestStates.Error:
                        VerboseLogging(request, "Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception"));

                        tcs.TrySetException(CreateException("No Exception", null, req.Exception));
                        break;

                    // The request aborted, initiated by the user.
                    case HTTPRequestStates.Aborted:
                        VerboseLogging(request, "Request Aborted!");

                        tcs.TrySetCanceled();
                        break;

                    // Connecting to the server is timed out.
                    case HTTPRequestStates.ConnectionTimedOut:
                        VerboseLogging(request, "Connection Timed Out!");

                        tcs.TrySetException(CreateException("Connection Timed Out!"));
                        break;

                    // The request didn't finished in the given time.
                    case HTTPRequestStates.TimedOut:
                        VerboseLogging(request, "Processing the request Timed Out!");

                        tcs.TrySetException(CreateException("Processing the request Timed Out!"));
                        break;
                }
            });
        }

        public static Task<byte[]> GetRawDataAsync(this HTTPRequest request, CancellationToken token =  default)
        {
            return CreateTask<byte[]>(request, token, (req, resp, tcs) =>
            {
                switch (req.State)
                {
                    // The request finished without any problem.
                    case HTTPRequestStates.Finished:
                        if (resp.IsSuccess)
                            tcs.TrySetResult(resp.Data);
                        else
                            tcs.TrySetException(CreateException("Request finished Successfully, but the server sent an error.", resp));
                        break;

                    // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                    case HTTPRequestStates.Error:
                        VerboseLogging(request, "Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception"));

                        tcs.TrySetException(CreateException("No Exception", null, req.Exception));
                        break;

                    // The request aborted, initiated by the user.
                    case HTTPRequestStates.Aborted:
                        VerboseLogging(request, "Request Aborted!");

                        tcs.TrySetCanceled();
                        break;

                    // Connecting to the server is timed out.
                    case HTTPRequestStates.ConnectionTimedOut:
                        VerboseLogging(request, "Connection Timed Out!");

                        tcs.TrySetException(CreateException("Connection Timed Out!"));
                        break;

                    // The request didn't finished in the given time.
                    case HTTPRequestStates.TimedOut:
                        VerboseLogging(request, "Processing the request Timed Out!");

                        tcs.TrySetException(CreateException("Processing the request Timed Out!"));
                        break;
                }
            });
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static Task<T> CreateTask<T>(HTTPRequest request, CancellationToken token, Action<HTTPRequest, HTTPResponse, TaskCompletionSource<T>> callback)
        {
            HTTPManager.Setup();

            var tcs = new TaskCompletionSource<T>();

            request.Callback = (req, resp) =>
            {
                if (token.IsCancellationRequested)
                    tcs.SetCanceled();
                else
                    callback(req, resp, tcs);
            };

            if (token.CanBeCanceled)
                token.Register((state) => (state as HTTPRequest)?.Abort(), request);

            if (request.State == HTTPRequestStates.Initial)
                request.Send();

            return tcs.Task;
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void VerboseLogging(HTTPRequest request, string str)
        {
            HTTPManager.Logger.Verbose("HTTPRequestAsyncExtensions", str, request.Context);
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static Exception CreateException(string errorMessage, HTTPResponse resp = null, Exception ex = null)
        {
            if (resp != null)
                return new AsyncHTTPException(resp.StatusCode, resp.Message, resp.DataAsText);
            else if (ex != null)
                return new AsyncHTTPException(ex.Message, ex);
            else
                return new AsyncHTTPException(errorMessage);
        }
    }
}
#endif
