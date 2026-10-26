using Kinevor.Modules.Trainers.Application;
using Kinevor.Platform.Application.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Kinevor.Modules.Trainers.EntityFrameworkCore;

/// <summary>Registers the trainers module persistence services.</summary>
public static class TrainersEntityFrameworkCoreExtensions
{
    /// <summary>Adds the module repository and unit of work; the host must register <see cref="TrainersDbContext"/> with a provider.</summary>
    public static IServiceCollection AddTrainersEntityFrameworkCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ITrainerRepository, TrainerRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<TrainersDbContext>());
        return services;
    }
}
