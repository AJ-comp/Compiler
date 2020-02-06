﻿using System;

namespace ApplicationLayer.Models
{
    public enum MessageResult { Yes, No, Cancel }

    public interface IMessageBoxService
    {
        void ShowError(Exception Error, string Title);
        void ShowError(string Message, string Title);
        void ShowInfo(string Message, string Title);
        void ShowMessage(string Message, string Title);
        bool ShowQuestion(string Message, string Title);
        void ShowWarning(string Message, string Title);

        MessageResult ShowSelectMessage(string Message, string Title);
    }
}