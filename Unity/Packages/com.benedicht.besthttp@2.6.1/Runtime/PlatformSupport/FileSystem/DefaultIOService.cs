#if !NETFX_CORE //&& (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using System.IO;

namespace BestHTTP.PlatformSupport.FileSystem
{
    public sealed class DefaultIOService : IIOService
    {
        public Stream CreateFileStream(string path, FileStreamModes mode)
        {
            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("DefaultIOService", string.Format("CreateFileStream path: '{0}' mode: {1}", path, mode));

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
            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("DefaultIOService", string.Format("DirectoryCreate path: '{0}'", path));
            Directory.CreateDirectory(path);
        }

        public bool DirectoryExists(string path)
        {
            bool exists = Directory.Exists(path);

            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("DefaultIOService", string.Format("DirectoryExists path: '{0}' exists: {1}", path, exists));

            return exists;
        }

        public string[] GetFiles(string path)
        {
            var files = Directory.GetFiles(path);

            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("DefaultIOService", string.Format("GetFiles path: '{0}' files count: {1}", path, files.Length));

            return files;
        }

        public void FileDelete(string path)
        {
            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("DefaultIOService", string.Format("FileDelete path: '{0}'", path));
            File.Delete(path);
        }

        public bool FileExists(string path)
        {
            bool exists = File.Exists(path);

            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("DefaultIOService", string.Format("FileExists path: '{0}' exists: {1}", path, exists));

            return exists;
        }
    }
}

#endif
