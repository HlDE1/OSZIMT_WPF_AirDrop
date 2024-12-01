using Microsoft.Extensions.Logging;
using WinDropApp.Resources.Interfaces;

namespace WinDropApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.Logging.SetMinimumLevel(LogLevel.None);
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Logging.ClearProviders(); 
            builder.Logging.AddFilter("Default", LogLevel.None); 

                                                                 // builder.Logging.SetMinimumLevel(LogLevel.None);

            // builder.Services.AddSingleton<IBluetoothService, BluetoothService>();


            //#if DEBUG
            //            builder.Logging.AddDebug();
            //#endif

            return builder.Build();
        }
    }
}
