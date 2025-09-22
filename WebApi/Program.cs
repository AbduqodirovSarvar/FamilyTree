using WebApi.Extentions;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AdApiDepencies(builder.Configuration);

        var app = builder.Build();

        await app.AddWebAppExtention();

        app.Run();
    }
}
