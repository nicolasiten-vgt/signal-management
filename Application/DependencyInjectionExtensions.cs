using Microsoft.Extensions.DependencyInjection;
using VGT.Galaxy.Backend.Services.SignalManagement.Application.Services;
using VGT.Galaxy.Backend.Services.SignalManagement.Domain.SignalProcessing;

namespace VGT.Galaxy.Backend.Services.SignalManagement.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddBusiness(this IServiceCollection services)
    {
        return services
            .AddTransient<ISignalService, SignalService>()
            .AddTransient<ICustomFunctionService, CustomFunctionService>()
            .AddTransient<ISignalProcessorOperationTypeService, SignalProcessorOperationTypeService>()
            .AddTransient<ISignalProcessorService, SignalProcessorService>()
            .AddSingleton<ISignalProcessorOperationRegistry, SignalProcessorOperationRegistry>();
    }
}