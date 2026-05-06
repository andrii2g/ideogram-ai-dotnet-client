using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace A2G.Ideogram.Client;

public static class IdeogramClientServiceCollectionExtensions
{
    public static IHttpClientBuilder AddIdeogramClient(
        this IServiceCollection services,
        IdeogramClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        return services.AddIdeogramClient(_ => options);
    }

    public static IHttpClientBuilder AddIdeogramClient(
        this IServiceCollection services,
        Func<IServiceProvider, IdeogramClientOptions> optionsFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(optionsFactory);

        services.AddSingleton(optionsFactory);
        services.AddTransient(static serviceProvider =>
            serviceProvider.GetRequiredService<Func<IServiceProvider, IdeogramClientOptions>>()(serviceProvider));

        services.AddHttpClient(IdeogramClientHttpClientNames.Download, static client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
        });

        var apiBuilder = services.AddHttpClient(IdeogramClientHttpClientNames.Api, static client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
        });

        services.TryAddTransient(static serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var options = serviceProvider.GetRequiredService<IdeogramClientOptions>();

            return new IdeogramClient(
                httpClientFactory.CreateClient(IdeogramClientHttpClientNames.Api),
                httpClientFactory.CreateClient(IdeogramClientHttpClientNames.Download),
                options);
        });

        services.TryAddTransient<IIdeogramClient>(static serviceProvider =>
            serviceProvider.GetRequiredService<IdeogramClient>());

        return apiBuilder;
    }
}
