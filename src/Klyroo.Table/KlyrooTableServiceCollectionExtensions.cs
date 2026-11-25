using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klyroo.Table;

/// <summary>Registration of Klyroo.Table in the service container.</summary>
public static class KlyrooTableServiceCollectionExtensions
{
    /// <summary>
    /// Registers Klyroo.Table, and Klyroo.UI and Klyroo.Query with it. It also registers <see cref="SynchronousDataListQueryExecutor"/> as the <see cref="IDataListQueryExecutor"/>
    /// unless one is registered already. Call it in the <c>Program.cs</c> of the server and of the WebAssembly client, or in <c>MauiProgram.cs</c>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Changes the options of Klyroo.Table (themes, message tables, search delay, prerender persistence limits).</param>
    /// <returns>The same collection, for chaining.</returns>
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
