using ApplicationLayer.Common.Interfaces;
using ApplicationLayer.Models.SolutionPackage;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;

namespace ApplicationLayer.ViewModels.Messages
{
    /// <summary>
    /// This message informs to the target that a changed file to add is.
    /// </summary>
    public class AddChangedFileMessage : MessageBase
    {
        public IManagableElements Item { get; }

        public AddChangedFileMessage()
        { }

        public AddChangedFileMessage(IManagableElements item) => this.Item = item;
    }

    public class RemoveChangedFileMessage : MessageBase
    {
        public IManagableElements Item { get; }

        public RemoveChangedFileMessage()
        { }

        public RemoveChangedFileMessage(IManagableElements item) => Item = item;
    }

    public class GetChangedListMessage : NotificationMessageAction<Collection<IManagableElements>>
    {
        public GetChangedListMessage(string notification, Action<Collection<IManagableElements>> callback) : base(notification, callback)
        { }
    }
}
