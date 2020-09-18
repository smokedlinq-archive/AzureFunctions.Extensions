using Microsoft.AspNet.OData.Builder;
using Azure.Functions.Extensions.OData;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ODataServiceCollectionExtensions
    {
        public static IServiceCollection AddOData(this IServiceCollection services, Action<ODataConventionModelBuilder> configure)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            services.AddSingleton(_ => new ConfigureODataConventionModelBuilder(configure));
            return services;
        }
    }
}
