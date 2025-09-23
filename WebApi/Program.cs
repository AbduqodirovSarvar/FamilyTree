using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using WebApi.Extentions;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AdApiDepencies(builder.Configuration);

var conn = builder.Configuration.GetConnectionString("PostgreSqlConnection");
Console.WriteLine("?? Runtime PostgreSqlConnection: " + conn);




var app = builder.Build();

Console.WriteLine("DB bilan bog‘lanishni tekshiryapmiz...");
using var scope = app.Services.CreateScope();
Console.WriteLine("Scope yaratildi...");
var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
Console.WriteLine("CanConnect: " + context.Database.CanConnect());

app.AddWebAppExtention();

app.Run();
