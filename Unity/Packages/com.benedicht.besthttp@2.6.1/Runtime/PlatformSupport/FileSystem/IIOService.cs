using System;
using System.IO;

namespace BestHTTP.PlatformSupport.FileSystem
{
    /// <summary>
    /// These are the different modes that the plugin want's to use a filestream.
    /// </summary>
    public enum FileStreamModes
    {
        /// <summary>
        /// Create a new file.
        /// </summary>
        Create,

        /// <summary>
        /// Open an existing file for reading.
        /// </summary>
        OpenRead,

        /// <summary>
        /// Open or create a file for read and write.
        /// </summary>
        OpenReadWrite,

        /// <summary>
        /// Open an existing file for writing to the end.
        /// </summary>
        Append
    }

    public interface IIOService
    {
        /// <summary>
        /// Create a directory for the given path.
        /// </summary>
        void DirectoryCreate(string path);

        /// <summary>
        /// Return true if the directory exists for the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Return with the file names for the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string[] GetFiles(string path);

        /// <summary>
        /// Delete the file for the given path.
        /// </summary>
        void FileDelete(string path);

        /// <summary>
        /// Return true if the file exists on the given path.
        /// </summary>
        bool FileExists(string path);

        /// <summary>
        /// Create a stream that can read and/or write a file on the given path.
        /// </summary>
        Stream CreateFileStream(string path, FileStreamModes mode);
    }
}
