﻿using Android.App;
using Android.Runtime;

namespace AdminClient.Platforms.Android
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(nint handle, JniHandleOwnership ownership)
            : base(handle, ownership) { }

        protected override MauiApp CreateMauiApp()
        {
            return MauiProgram.CreateMauiApp();
        }
    }
}
