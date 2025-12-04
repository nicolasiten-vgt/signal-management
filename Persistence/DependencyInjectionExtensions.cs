using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VGT.Galaxy.Backend.Services.SignalManagement.Persistence.Repositories;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Persistence;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SignalDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Default");
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<ISignalRepository, SignalRepository>();
        services.AddScoped<ICustomFunctionRepository, CustomFunctionRepository>();
        services.AddScoped<ISimpleOperationTypeRepository, SimpleOperationTypeRepository>();

        return services;
    }
    
    public static void MigrateDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SignalDbContext>();
        dbContext.Database.Migrate();
    }
}