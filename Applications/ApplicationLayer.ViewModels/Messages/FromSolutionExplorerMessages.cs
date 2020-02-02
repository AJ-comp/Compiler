using ApplicationLayer.Models.SolutionPackage;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;

namespace ApplicationLayer.ViewModels.Messages
{
    public class OpenFileMessage : MessageBase
    {
        public DefaultFileStruct SelectedFile { get; }

        public OpenFileMessage(DefaultFileStruct selectedFile)
        {
            this.SelectedFile = selectedFile;
        }
    }

    public class ChangedFileListMessage : MessageBase
    {
        public enum ChangedStatus { Changed, Restored }

        public class ChangedFile
        {
            public HirStruct Item { get; }
            public ChangedStatus ChangedStatus { get; }

            public ChangedFile(HirStruct item, ChangedStatus changedStatus)
            {
                Item = item;
                ChangedStatus = changedStatus;
            }
        }

        public List<ChangedFile> ChangedFiles { get; } = new List<ChangedFile>();

        public ChangedFileListMessage()
        {
            this.ChangedFiles = new List<ChangedFile>();
        }

        public ChangedFileListMessage(List<ChangedFile> changedFiles)
        {
            this.ChangedFiles = changedFiles;
        }


        public void AddFile(ChangedFile changedFile)
        {
            this.ChangedFiles.Add(changedFile);
        }
    }
}
