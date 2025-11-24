using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace WorkshopOilApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Register DB Service
            //builder.Services.AddSingleton(DatabaseService.InstanceAsync);

            //builder.Services.AddSingleton<LoginViewModel>();
            //builder.Services.AddSingleton<RegisterViewModel>();
            //builder.Services.AddSingleton<ForgotPasswordPage>();

            //builder.Services.AddSingleton<LoginPage>();
            //builder.Services.AddSingleton<RegisterPage>();
            //builder.Services.AddSingleton<ForgotPasswordPage>();

            return builder.Build();
        }
    }
}