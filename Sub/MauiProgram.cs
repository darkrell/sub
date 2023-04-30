using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Sub.ApplicationLayer;
using Sub.Data;

namespace Sub;
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
            });
        builder.Services.AddMudServices();
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddScoped<AudioService>();

        builder.Services.AddScoped<ITokenContainer, TokenContainer>();
        builder.Services.AddScoped<ITokenValidator, DeepGramTokenValidator>();
        builder.Services.AddScoped<IParseService, DeepGramParseService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<WeatherForecastService>();

        return builder.Build();
    }
}
