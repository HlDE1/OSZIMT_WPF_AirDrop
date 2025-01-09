using Android.App;
using Android.Bluetooth;
using Android.Runtime;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Extensions;
using System.Diagnostics;
using WinDropApp.Platforms.Android.Services;
using WinDropApp.Resources.Interfaces;
namespace WinDropApp
{
    [Application]
    public class MainApplication : MauiApplication
    {


        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
            Debug.WriteLine("Android application constructor...");
        }


        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override void OnCreate()
        {
            base.OnCreate();

            Debug.WriteLine("Android application starting...");
        }



     
    }

}
