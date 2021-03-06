﻿using System;

namespace NetworkLogViewer
{
    internal class LoadingState
    {
        public LoadingState(string desc)
        {
            this.Description = desc;
            this.OnCancel = null;
        }

        public LoadingState(string desc, Action<MainWindow> onCancel)
        {
            this.Description = desc;
            this.OnCancel = onCancel;
        }

        public readonly string Description;
        public readonly Action<MainWindow> OnCancel;
    }
}
