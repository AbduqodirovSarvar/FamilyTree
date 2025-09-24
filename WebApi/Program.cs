using WebApi.Extentions;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AdApiDepencies(builder.Configuration);

var app = builder.Build();

app.AddWebAppExtention();

app.Run();
