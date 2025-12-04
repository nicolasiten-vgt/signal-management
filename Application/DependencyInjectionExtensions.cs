using Microsoft.Extensions.DependencyInjection;
using VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddBusiness(this IServiceCollection services)
    {
        return services
            .AddTransient<ISignalService, SignalService>();
    }
}