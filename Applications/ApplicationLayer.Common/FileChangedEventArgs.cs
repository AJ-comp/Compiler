using System;

namespace ApplicationLayer.Common
{
    public enum FileChangedKind { Add, Remove, Rename, Unload }

    public class FileChangedEventArgs : EventArgs
    {
        public FileChangedKind Kind { get; }
        public string OriginalFullPath { get; }
        public string ChangedFullPath { get; }

        public FileChangedEventArgs(FileChangedKind kind, string originalFullPath, string changedFullPath)
        {
            Kind = kind;
            OriginalFullPath = originalFullPath;
            ChangedFullPath = changedFullPath;
        }
    }
}
