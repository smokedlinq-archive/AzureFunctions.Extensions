using AutoFixture;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Azure.Functions.Extensions.OData.Tests
{
    public class ServiceProviderFixture
        : ICustomizeFixture<IServiceProvider>
    {
        public IServiceProvider Customize(IFixture fixture)
        {
            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddLogging();
            services.AddOptions();
            services.AddMvcCore();
            services.AddOData(builder => builder.EntitySet<EntityFixture>(nameof(EntityFixture)));
            services.AddSingleton(serviceProvider => new ODataContext(() => serviceProvider.GetServices<ConfigureODataConventionModelBuilder>()));

            return services.BuildServiceProvider();
        }
    }
}
