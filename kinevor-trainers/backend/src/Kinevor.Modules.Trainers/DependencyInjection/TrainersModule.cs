using Kinevor.Modules.Trainers.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Kinevor.Modules.Trainers.DependencyInjection;

/// <summary>Registers the trainers module application services.</summary>
public static class TrainersModule
{
    /// <summary>Adds the module's MediatR handlers to the service collection.</summary>
    public static IServiceCollection AddTrainersModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateTrainerHandler>());
        return services;
    }
}
