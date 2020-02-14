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

        public OpenFileMessage(DefaultFileHier selectedFile) => this.SelectedFile = selectedFile;
    }

    /// <summary>
    /// This message informs to the target that a changed file to add is.
    /// </summary>
    public class AddChangedFileMessage : MessageBase
    {
        public ISaveAndChangeTrackable Item { get; }

        public AddChangedFileMessage()
        { }

        public AddChangedFileMessage(ISaveAndChangeTrackable item) => this.Item = item;
    }

    public class RemoveChangedFileMessage : MessageBase
    {
        public ISaveAndChangeTrackable Item { get; }

        public RemoveChangedFileMessage()
        { }

        public RemoveChangedFileMessage(ISaveAndChangeTrackable item) => Item = item;
    }

    public class GetChangedListMessage : NotificationMessageAction<Collection<ISaveAndChangeTrackable>>
    {
        public GetChangedListMessage(string notification, Action<Collection<ISaveAndChangeTrackable>> callback) : base(notification, callback)
        { }
    }
}
