using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lux.Table;

public static class LuxTableServiceCollectionExtensions
{
    public static IServiceCollection AddLuxTable(this IServiceCollection services, Action<LuxTableOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddLuxUi(options =>
        {
            var tools = options.Extension<LuxTableOptions>();
            configure?.Invoke(tools);
        });
        services.AddLuxQuery();
        services.TryAddSingleton<IDataListQueryExecutor>(SynchronousDataListQueryExecutor.Instance);
        services.TryAddSingleton<MissingServerWarning>();
        return services;
    }
}
