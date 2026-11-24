using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blux.UI.Tools;

public static class LuxTableServiceCollectionExtensions
{
    public static IServiceCollection AddLuxTable(this IServiceCollection services, Action<BluxToolsOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddBluxUi(options =>
        {
            var tools = options.Extension<BluxToolsOptions>();
            configure?.Invoke(tools);
        });
        services.AddLuxQuery();
        services.TryAddSingleton<IDataListQueryExecutor>(SynchronousDataListQueryExecutor.Instance);
        services.TryAddSingleton<MissingServerWarning>();
        return services;
    }
}
