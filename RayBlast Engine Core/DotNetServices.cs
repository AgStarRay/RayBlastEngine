using Microsoft.Extensions.DependencyInjection;

namespace RayBlast;

public class DotNetServices {
    private static readonly IHttpClientFactory HTTP_CLIENT_FACTORY;

    static DotNetServices() {
        var services = new ServiceCollection();
        services.AddHttpClient();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        HTTP_CLIENT_FACTORY = serviceProvider.GetRequiredService<IHttpClientFactory>();
    }
}
