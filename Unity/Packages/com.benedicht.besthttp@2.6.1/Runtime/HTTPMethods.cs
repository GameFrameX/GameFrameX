namespace BestHTTP
{
    /// <summary>
    /// Some supported methods described in the rfc: http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9
    /// </summary>
    public enum HTTPMethods : byte
    {
        /// <summary>
        /// The GET method means retrieve whatever information (in the form of an entity) is identified by the Request-URI.
        /// If the Request-URI refers to a data-producing process, it is the produced data which shall be returned as the
        /// entity in the response and not the source text of the process, unless that text happens to be the output of the process.
        /// </summary>
        Get,

        /// <summary>
        /// The HEAD method is identical to GET except that the server MUST NOT return a message-body in the response.
        /// The metainformation contained in the HTTP headers in response to a HEAD request SHOULD be identical to the information sent in response to a GET request.
        /// This method can be used for obtaining metainformation about the entity implied by the request without transferring the entity-body itself.
        /// This method is often used for testing hypertext links for validity, accessibility, and recent modification.
        /// </summary>
        Head,

        /// <summary>
        /// The POST method is used to request that the origin server accept the entity enclosed in the request as a new subordinate of the resource identified by the Request-URI in the Request-Line.
        /// POST is designed to allow a uniform method to cover the following functions:
        /// <list type="bullet">
        ///     <item><description>Annotation of existing resources;</description></item>
        ///     <item><description>Posting a message to a bulletin board, newsgroup, mailing list, or similar group of articles;</description></item>
        ///     <item><description>Providing a block of data, such as the result of submitting a form, to a data-handling process;</description></item>
        ///     <item><description>Extending a database through an append operation.</description></item>
        /// </list>
        /// The actual function performed by the POST method is determined by the server and is usually dependent on the Request-URI.
        /// The posted entity is subordinate to that URI in the same way that a file is subordinate to a directory containing it,
        /// a news article is subordinate to a newsgroup to which it is posted, or a record is subordinate to a database.
        /// The action performed by the POST method might not result in a resource that can be identified by a URI. In this case,
        /// either 200 (OK) or 204 (No Content) is the appropriate response status, depending on whether or not the response includes an entity that describes the result.
        /// </summary>
        Post,

        /// <summary>
        /// The PUT method requests that the enclosed entity be stored under the supplied Request-URI.
        /// If the Request-URI refers to an already existing resource, the enclosed entity SHOULD be considered as a modified version of the one residing on the origin server.
        /// If the Request-URI does not point to an existing resource, and that URI is capable of being defined as a new resource by the requesting user agent,
        /// the origin server can create the resource with that URI. If a new resource is created, the origin server MUST inform the user agent via the 201 (Created) response.
        /// If an existing resource is modified, either the 200 (OK) or 204 (No Content) response codes SHOULD be sent to indicate successful completion of the request.
        /// If the resource could not be created or modified with the Request-URI, an appropriate error response SHOULD be given that reflects the nature of the problem.
        /// The recipient of the entity MUST NOT ignore any Content-* (e.g. Content-Range) headers that it does not understand or implement and MUST return a 501 (Not Implemented) response in such cases.
        /// </summary>
        Put,

        /// <summary>
        /// The DELETE method requests that the origin server delete the resource identified by the Request-URI. This method MAY be overridden by human intervention (or other means) on the origin server.
        /// The client cannot be guaranteed that the operation has been carried out, even if the status code returned from the origin server indicates that the action has been completed successfully.
        /// However, the server SHOULD NOT indicate success unless, at the time the response is given, it intends to delete the resource or move it to an inaccessible location.
        /// A successful response SHOULD be 200 (OK) if the response includes an entity describing the status, 202 (Accepted) if the action has not yet been enacted, or 204 (No Content)
        /// if the action has been enacted but the response does not include an entity.
        /// </summary>
        Delete,

        /// <summary>
        /// http://tools.ietf.org/html/rfc5789
        /// The PATCH method requests that a set of changes described in the request entity be applied to the resource identified by the Request-URI.
        /// The set of changes is represented in a format called a "patchdocument" identified by a media type. If the Request-URI does not point to an existing resource,
        /// the server MAY create a new resource, depending on the patch document type (whether it can logically modify a null resource) and permissions, etc.
        /// </summary>
        Patch,

        /// <summary>
        /// The HTTP methods PATCH can be used to update partial resources. For instance, when you only need to update one field of the resource, PUTting a complete resource representation might be cumbersome and utilizes more bandwidth.
        /// <seealso href="http://restcookbook.com/HTTP%20Methods/patch/"/>
        /// </summary>
        Merge,

        Options,

        /// <summary>
        /// https://tools.ietf.org/html/rfc8441
        /// </summary>
        Connect
    }
}
