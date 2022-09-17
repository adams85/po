using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using WebApp.Infrastructure.Localization;

namespace WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        ConfigureServices(builder);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        Configure(app);

        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<AppLocalizationOptions>(builder.Configuration.GetSection("Localization"));

        builder.Services
            .AddSingleton<ITranslationsProvider, DefaultTranslationsProvider>()
            .AddSingleton<IStringLocalizerFactory, POStringLocalizerFactory>()
            .AddSingleton<IHtmlLocalizerFactory, ExtendedHtmlLocalizerFactory>()
            .AddTransient<IViewLocalizer, ExtendedViewLocalizer>();

        builder.Services.AddRazorPages()
            .AddMvcLocalization();
    }

    private static void Configure(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        var localizationOptions = app.Services.GetRequiredService<IOptions<AppLocalizationOptions>>();
        var supportedCultures = localizationOptions.Value.SupportedCultureInfos.ToArray();
        var defaultCulture = localizationOptions.Value.DefaultCultureInfo;
        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(defaultCulture, defaultCulture),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures,
        });

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();
    }
}
