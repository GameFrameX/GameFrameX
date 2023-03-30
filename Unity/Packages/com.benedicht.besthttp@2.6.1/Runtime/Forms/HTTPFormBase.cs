using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BestHTTP.Forms
{
    /// <summary>
    /// Base class of a concrete implementation. Don't use it to create a form, use instead one of the already wrote implementation(HTTPMultiPartForm, HTTPUrlEncodedForm), or create a new one by inheriting from this base class.
    /// </summary>
    public class HTTPFormBase
    {
        const int LongLength = 256;

        #region Properties

        /// <summary>
        /// A list that holds the form's fields.
        /// </summary>
        public List<HTTPFieldData> Fields { get; set; }

        /// <summary>
        /// Returns true if the Fields has no element.
        /// </summary>
        public bool IsEmpty { get { return Fields == null || Fields.Count == 0; } }

        /// <summary>
        /// True if new fields has been added to our list.
        /// </summary>
        public bool IsChanged { get; protected set; }

        /// <summary>
        /// True if there are at least one form-field with binary data.
        /// </summary>
        public bool HasBinary { get; protected set; }

        /// <summary>
        /// True if there are at least one form-field with a long textual data.
        /// </summary>
        public bool HasLongValue { get; protected set; }

        #endregion

        #region Field Management

        public void AddBinaryData(string fieldName, byte[] content)
        {
            AddBinaryData(fieldName, content, null, null);
        }

        public void AddBinaryData(string fieldName, byte[] content, string fileName)
        {
            AddBinaryData(fieldName, content, fileName, null);
        }

        public void AddBinaryData(string fieldName, byte[] content, string fileName, string mimeType)
        {
            if (Fields == null)
                Fields = new List<HTTPFieldData>();

            HTTPFieldData field = new HTTPFieldData();
            field.Name = fieldName;

            if (fileName == null)
                field.FileName = fieldName + ".dat";
            else
                field.FileName = fileName;

            if (mimeType == null)
                field.MimeType = "application/octet-stream";
            else
                field.MimeType = mimeType;

            field.Binary = content;

            Fields.Add(field);

            HasBinary = IsChanged = true;
        }

        public void AddField(string fieldName, string value)
        {
            AddField(fieldName, value, System.Text.Encoding.UTF8);
        }

        public void AddField(string fieldName, string value, System.Text.Encoding e)
        {
            if (Fields == null)
                Fields = new List<HTTPFieldData>();

            HTTPFieldData field = new HTTPFieldData();
            field.Name = fieldName;
            field.FileName = null;
            if (e != null)
                field.MimeType = "text/plain; charset=" + e.WebName;
            field.Text = value;
            field.Encoding = e;

            Fields.Add(field);

            IsChanged = true;

            HasLongValue |= value.Length > LongLength;
        }

        #endregion

        #region Virtual Functions

        /// <summary>
        /// It should 'clone' all the data from the given HTTPFormBase object.
        /// Called after the form-implementation created.
        /// </summary>
        public virtual void CopyFrom(HTTPFormBase fields)
        {
            this.Fields = new List<HTTPFieldData>(fields.Fields);
            this.IsChanged = true;

            this.HasBinary = fields.HasBinary;
            this.HasLongValue = fields.HasLongValue;
        }

        /// <summary>
        /// Prepares the request to sending a form. It should set only the headers.
        /// </summary>
        public virtual void PrepareRequest(HTTPRequest request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Prepares and returns with the form's raw data.
        /// </summary>
        public virtual byte[] GetData()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}