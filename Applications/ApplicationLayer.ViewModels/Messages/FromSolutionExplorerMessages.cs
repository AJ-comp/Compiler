using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ApplicationLayer.Models.SolutionPackage;
using GalaSoft.MvvmLight.Messaging;

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

    /// <summary>
    /// This message informs to the target that a changed file is.
    /// </summary>
    public class ChangedFileMessage : MessageBase
    {
        public enum ChangedStatus { Changed, Restored }

        public HirStruct Item { get; }
        public ChangedStatus Status { get; }

        public ChangedFileMessage()
        {
        }

        public ChangedFileMessage(HirStruct item, ChangedStatus status)
        {
            this.Item = item;
            this.Status = status;
        }
    }

    public class GetChangedListMessage : NotificationMessageAction<Collection<HirStruct>>
    {
        public GetChangedListMessage(string notification, Action<Collection<HirStruct>> callback) : base(notification, callback)
        {
        }
    }
}
