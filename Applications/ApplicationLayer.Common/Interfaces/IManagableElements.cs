using System;

namespace ApplicationLayer.Common.Interfaces
{
    public interface IManagableElements : IRestorable, ISaveable
    {
        /// <summary>
        /// This event is called when child element is changed (rename, delete, move)
        /// </summary>
        event EventHandler<FileChangedEventArgs> ChildrenChanged;
    }
}
