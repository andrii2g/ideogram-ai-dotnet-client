using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace A2G.Ideogram.Client;

public static class IdeogramClientServiceCollectionExtensions
{
    private sealed class V3Registration
    {
        public required Func<IServiceProvider, IdeogramClientOptions> OptionsFactory { get; init; }
    }

    private sealed class V4Registration
    {
        public required Func<IServiceProvider, IdeogramClientOptions> OptionsFactory { get; init; }
    }

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

        services.AddSingleton(new V3Registration
        {
            OptionsFactory = optionsFactory
        });

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
            var registration = serviceProvider.GetRequiredService<V3Registration>();
            var options = registration.OptionsFactory(serviceProvider);

            return new IdeogramClient(
                httpClientFactory.CreateClient(IdeogramClientHttpClientNames.Api),
                httpClientFactory.CreateClient(IdeogramClientHttpClientNames.Download),
                options);
        });

        services.TryAddTransient<IIdeogramClient>(static serviceProvider =>
            serviceProvider.GetRequiredService<IdeogramClient>());

        return apiBuilder;
    }

    public static IHttpClientBuilder AddIdeogramV4Client(
        this IServiceCollection services,
        IdeogramClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        return services.AddIdeogramV4Client(_ => options);
    }

    public static IHttpClientBuilder AddIdeogramV4Client(
        this IServiceCollection services,
        Func<IServiceProvider, IdeogramClientOptions> optionsFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(optionsFactory);

        services.AddSingleton(new V4Registration
        {
            OptionsFactory = optionsFactory
        });

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
            var registration = serviceProvider.GetRequiredService<V4Registration>();
            var options = registration.OptionsFactory(serviceProvider);

            return new IdeogramV4Client(
                httpClientFactory.CreateClient(IdeogramClientHttpClientNames.Api),
                httpClientFactory.CreateClient(IdeogramClientHttpClientNames.Download),
                options);
        });

        services.TryAddTransient<IIdeogramV4Client>(static serviceProvider =>
            serviceProvider.GetRequiredService<IdeogramV4Client>());

        return apiBuilder;
    }
}
