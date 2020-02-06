using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ApplicationLayer.Models.SolutionPackage;
using GalaSoft.MvvmLight.Messaging;

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

        public HierarchicalData Item { get; }
        public ChangedStatus Status { get; }

        public ChangedFileMessage()
        {
        }

        public ChangedFileMessage(HierarchicalData item, ChangedStatus status)
        {
            this.Item = item;
            this.Status = status;
        }
    }

    public class GetChangedListMessage : NotificationMessageAction<Collection<HierarchicalData>>
    {
        public GetChangedListMessage(string notification, Action<Collection<HierarchicalData>> callback) : base(notification, callback)
        {
        }
    }
}
