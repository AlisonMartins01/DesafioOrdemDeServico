
using Microsoft.Extensions.DependencyInjection;

namespace OsService.Services;

public static class ServicesServiceCollection
{
    public static IServiceCollection ServicesInjection(this IServiceCollection services)
    {
        return services;
    }
}
