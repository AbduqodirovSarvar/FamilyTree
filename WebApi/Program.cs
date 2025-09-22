using WebApi.Extentions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AdApiDepencies(builder.Configuration);

var app = builder.Build();

await app.AddWebAppExtention();
app.Run();
