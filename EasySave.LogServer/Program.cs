using EasySave.LogServer.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); 
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Swagger with proper namespace
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EasySave Log Server API", Version = "v1" });
});

// Configure log storage service
string logDirectory = builder.Configuration["LogSettings:Directory"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
builder.Services.AddSingleton(new LogStorageService(logDirectory));

// Add user connection service
builder.Services.AddSingleton<UserConnectionService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EasySave Log Server API v1"));
}

//app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Ensure log directory exists
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}

Console.WriteLine($"EasySave Log Server v3.0");
Console.WriteLine($"Log files will be stored in: {logDirectory}");
Console.WriteLine($"User tracking is enabled");

app.Run();