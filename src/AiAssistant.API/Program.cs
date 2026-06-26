using AiAssistant.API.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAiAssistant(builder.Configuration);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();

public partial class Program { }   // expose Program for potential test hosting
