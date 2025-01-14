using Telegram.Bot;
using TgBot.Services;

namespace TgBot;

public class Startup
{
    public IConfiguration Configuration { get; }
    private BotConfiguration BotConfig { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        BotConfig = Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHostedService<ConfigureWebhook>();
        services.AddHttpClient("tgwebhook")
                .AddTypedClient<ITelegramBotClient>(httpClient
                    => new TelegramBotClient(BotConfig.BotToken!, httpClient));

        services.AddScoped<HandleUpdateService>();
        services.AddScoped<HandleNotifyService>();
        services.AddControllers()
                .AddNewtonsoftJson();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
