using ApplicationLayer.Common.Interfaces;
using ApplicationLayer.Models.SolutionPackage;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;

namespace ApplicationLayer.ViewModels.Messages
{
    public class OpenFileMessage : MessageBase
    {
        public DefaultFileHier SelectedFile { get; }

        public OpenFileMessage(DefaultFileHier selectedFile)
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

        public ISaveAndChangeTrackable Item { get; }
        public ChangedStatus Status { get; }

        public ChangedFileMessage()
        {
        }

        public ChangedFileMessage(ISaveAndChangeTrackable item, ChangedStatus status)
        {
            this.Item = item;
            this.Status = status;
        }
    }

    public class GetChangedListMessage : NotificationMessageAction<Collection<ISaveAndChangeTrackable>>
    {
        public GetChangedListMessage(string notification, Action<Collection<ISaveAndChangeTrackable>> callback) : base(notification, callback)
        {
        }
    }
}
