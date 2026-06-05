using Microsoft.Extensions.Logging;

namespace AplicacionMovil
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Manrope-Variable.ttf", "Manrope");
                    fonts.AddFont("Inter-Variable.ttf", "Inter");
                    fonts.AddFont("JetBrainsMono-Variable.ttf", "JetBrainsMono");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
