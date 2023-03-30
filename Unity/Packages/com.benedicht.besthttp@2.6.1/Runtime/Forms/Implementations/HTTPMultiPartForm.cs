using BestHTTP.Extensions;

namespace BestHTTP.Forms
{
    /// <summary>
    /// A HTTP Form implementation to send textual and binary values.
    /// </summary>
    public sealed class HTTPMultiPartForm : HTTPFormBase
    {
        #region Private Fields

        /// <summary>
        /// A random boundary generated in the constructor.
        /// </summary>
        private string Boundary;

        /// <summary>
        ///
        /// </summary>
        private byte[] CachedData;

        #endregion

        public HTTPMultiPartForm()
        {
            this.Boundary = "BestHTTP_HTTPMultiPartForm_" + this.GetHashCode().ToString("X");
        }

        #region IHTTPForm Implementation

        public override void PrepareRequest(HTTPRequest request)
        {
            // Set up Content-Type header for the request
            request.SetHeader("Content-Type", "multipart/form-data; boundary=" + Boundary);
        }

        public override byte[] GetData()
        {
            if (CachedData != null)
                return CachedData;

            using (var ms = new BufferPoolMemoryStream())
            {
                for (int i = 0; i < Fields.Count; ++i)
                {
                    HTTPFieldData field = Fields[i];

                    // Set the boundary
                    ms.WriteLine("--" + Boundary);

                    // Set up Content-Disposition header to our form with the name
                    ms.WriteLine("Content-Disposition: form-data; name=\"" + field.Name + "\"" + (!string.IsNullOrEmpty(field.FileName) ? "; filename=\"" + field.FileName + "\"" : string.Empty));

                    // Set up Content-Type head for the form.
                    if (!string.IsNullOrEmpty(field.MimeType))
                        ms.WriteLine("Content-Type: " + field.MimeType);

                    ms.WriteLine();

                    // Write the actual data to the MemoryStream
                    ms.Write(field.Payload, 0, field.Payload.Length);

                    ms.Write(HTTPRequest.EOL, 0, HTTPRequest.EOL.Length);
                }

                // Write out the trailing boundary
                ms.WriteLine("--" + Boundary + "--");

                IsChanged = false;

                // Set the RawData of our request
                return CachedData = ms.ToArray();
            }
        }

        #endregion
    };
}
