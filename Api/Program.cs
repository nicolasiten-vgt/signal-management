using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using VGT.Galaxy.Backend.Services.SignalManagement.Api;
using VGT.Galaxy.Backend.Services.SignalManagement.Application;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services
    .AddBusiness()
    .AddPersistence(builder.Configuration);

var app = builder.Build();

app.Services.MigrateDatabase();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseExceptionHandler(o => { });
app.UseHttpsRedirection();
app.UseRouting();

app.MapControllers();

app.Run();

public partial class Program;