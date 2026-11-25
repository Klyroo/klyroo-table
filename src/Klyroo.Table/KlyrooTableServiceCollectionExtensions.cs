using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klyroo.Table;

public static class KlyrooTableServiceCollectionExtensions
{
    public static IServiceCollection AddKlyrooTable(this IServiceCollection services, Action<KlyrooTableOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddKlyrooUi(options =>
        {
            var tools = options.Extension<KlyrooTableOptions>();
            configure?.Invoke(tools);
        });
        services.AddKlyrooQuery();
        services.TryAddSingleton<IDataListQueryExecutor>(SynchronousDataListQueryExecutor.Instance);
        services.TryAddSingleton<MissingServerWarning>();
        return services;
    }
}
