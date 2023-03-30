#if NETFX_CORE
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Foundation;

namespace BestHTTP.PlatformSupport.FileSystem
{
    public sealed class NETFXCOREIOService : IIOService
    {
        public Stream CreateFileStream(string path, FileStreamModes mode)
        {
            switch (mode)
            {
                case FileStreamModes.Create:
                    return new FileStream(path, FileMode.Create);
                case FileStreamModes.OpenRead:
                    return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                case FileStreamModes.OpenReadWrite:
                    return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                case FileStreamModes.Append:
                    return new FileStream(path, FileMode.Append);
            }

            throw new NotImplementedException("DefaultIOService.CreateFileStream - mode not implemented: " + mode.ToString());
        }

        public void DirectoryCreate(string path)
        {
            if (path == null)
                throw new ArgumentNullException();
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("Path is empty");
            StorageFolder folder = (StorageFolder)null;
            path = path.Replace('/', '\\');
            string path1 = path;
            Stack<string> stack = new Stack<string>();
            do
            {
                try
                {
                    folder = NETFXCOREIOService.GetDirectoryForPath(path1);
                    break;
                }
                catch
                {
                    int length = path1.LastIndexOf('\\');
                    if (length < 0)
                    {
                        path1 = (string)null;
                    }
                    else
                    {
                        stack.Push(path1.Substring(length + 1));
                        path1 = path1.Substring(0, length);
                    }
                }
            }
            while (path1 != null);
            if (path1 == null)
            {
                System.Diagnostics.Debug.WriteLine("NETFXCOREIOService.DirectoryCreate: Could not find any part of the path: " + path);
                throw new IOException("Could not find any part of the path: " + path);
            }
            try
            {
                while (stack.Count > 0)
                {
                    string desiredName = stack.Pop();
                    if (string.IsNullOrWhiteSpace(desiredName) && stack.Count > 0)
                        throw new ArgumentNullException("Empty directory name in the path");
                    IAsyncOperation<StorageFolder> folderAsync = folder.CreateFolderAsync(desiredName);
                    WindowsRuntimeSystemExtensions.AsTask<StorageFolder>(folderAsync).Wait();
                    folder = folderAsync.GetResults();
                }
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine("NETFXCOREIOService.DirectoryCreate: " + ex.Message + "\n" + ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("NETFXCOREIOService.DirectoryCreate: " + ex.Message + "\n" + ex.StackTrace);
                throw new IOException(ex.Message, ex);
            }
        }

        public bool DirectoryExists(string path)
        {
            try
            {
                return NETFXCOREIOService.GetDirectoryForPath(path) != null;
            }
            catch
            {
                return false;
            }
        }

        public string[] GetFiles(string path)
        {
            if (path == null)
                throw new ArgumentNullException();
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path is empty");
            try
            {
                IAsyncOperation<IReadOnlyList<StorageFile>> filesAsync = NETFXCOREIOService.GetDirectoryForPath(path).GetFilesAsync();
                WindowsRuntimeSystemExtensions.AsTask<IReadOnlyList<StorageFile>>(filesAsync).Wait();
                IReadOnlyList<StorageFile> results = filesAsync.GetResults();
                List<string> list = new List<string>(Enumerable.Count<StorageFile>((IEnumerable<StorageFile>)results));
                foreach (StorageFile storageFile in (IEnumerable<StorageFile>)results)
                    list.Add(storageFile.Path);
                return list.ToArray();
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine("NETFXCOREIOService.GetFiles: " + ex.Message + "\n" + ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("NETFXCOREIOService.GetFiles: " + ex.Message + "\n" + ex.StackTrace);
                throw new IOException(ex.Message, ex);
            }
        }

        public void FileDelete(string path)
        {
            if (path == null)
                throw new ArgumentNullException();
            if (path.Trim() == "")
                throw new ArgumentException();
            try
            {
                WindowsRuntimeSystemExtensions.AsTask(NETFXCOREIOService.GetFileForPathOrURI(path).DeleteAsync())
                    .Wait();
            }
            catch (Exception ex)
            {
                throw NETFXCOREIOService.GetRethrowException(ex);
            }
        }

        public bool FileExists(string path)
        {
            try
            {
                return NETFXCOREIOService.GetFileForPathOrURI(path) != null;
            }
            catch
            {
                return false;
            }
        }

        private const string LOCAL_FOLDER = "ms-appdata:///local/";
        private const string ROAMING_FOLDER = "ms-appdata:///roaming/";
        private const string TEMP_FOLDER = "ms-appdata:///temp/";
        private const string STORE_FOLDER = "isostore:/";

        private static Exception GetRethrowException(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("NETFXCOREIOService.GetRethrowException: " + ex.Message + "\n" + ex.StackTrace);
            if (ex.GetType() == typeof(IOException))
                return ex;
            else
                return (Exception)new IOException(ex.Message, ex);
        }

        private static StorageFolder GetDirectoryForPath(string path)
        {
            IAsyncOperation<StorageFolder> folderFromPathAsync = StorageFolder.GetFolderFromPathAsync(path);
            WindowsRuntimeSystemExtensions.AsTask<StorageFolder>(folderFromPathAsync).Wait();
            return folderFromPathAsync.GetResults();
        }

        private static StorageFolder GetFolderForURI(string uri)
        {
            uri = uri.ToLower();
            StorageFolder storageFolder1;
            if (uri.StartsWith("ms-appdata:///local/"))
            {
                storageFolder1 = ApplicationData.Current.LocalFolder;
                uri = uri.Replace("ms-appdata:///local/", "");
            }
            else if (uri.StartsWith("ms-appdata:///roaming/"))
            {
                storageFolder1 = ApplicationData.Current.RoamingFolder;
                uri = uri.Replace("ms-appdata:///roaming/", "");
            }
            else
            {
                if (!uri.StartsWith("ms-appdata:///temp/"))
                    throw new Exception("Unsupported URI: " + uri);
                storageFolder1 = ApplicationData.Current.TemporaryFolder;
                uri = uri.Replace("ms-appdata:///temp/", "");
            }
            string[] strArray = uri.Split(new char[1] { '/' });
            for (int index = 0; index < strArray.Length - 1; ++index)
            {
                Task<IReadOnlyList<StorageFolder>> task = WindowsRuntimeSystemExtensions.AsTask<IReadOnlyList<StorageFolder>>(storageFolder1.CreateFolderQuery().GetFoldersAsync());
                task.Wait();
                if (task.Status != TaskStatus.RanToCompletion)
                    throw new Exception("Failed to find folder: " + strArray[index]);
                IReadOnlyList<StorageFolder> result = task.Result;
                bool flag = false;
                foreach (StorageFolder storageFolder2 in (IEnumerable<StorageFolder>)result)
                {
                    if (storageFolder2.Name == strArray[index])
                    {
                        storageFolder1 = storageFolder2;
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    throw new Exception("Folder not found: " + strArray[index]);
            }
            return storageFolder1;
        }

        internal static StorageFolder GetFolderForPathOrURI(string path)
        {
            if (System.Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute))
                return NETFXCOREIOService.GetFolderForURI(path);
            IAsyncOperation<StorageFolder> folderFromPathAsync = StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
            WindowsRuntimeSystemExtensions.AsTask<StorageFolder>(folderFromPathAsync).Wait();
            return folderFromPathAsync.GetResults();
        }

        internal static StorageFile GetFileForPathOrURI(string path)
        {
            IAsyncOperation<StorageFile> source = !System.Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute) ? StorageFile.GetFileFromPathAsync(path) : StorageFile.GetFileFromApplicationUriAsync(new System.Uri(path));
            WindowsRuntimeSystemExtensions.AsTask<StorageFile>(source).Wait();
            return source.GetResults();
        }
    }
}
#endif
