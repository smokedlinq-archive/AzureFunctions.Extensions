using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Azure.Functions.Extensions.Http.Tests
{
    public class ServiceProviderFixture
        : ICustomizeFixture<IServiceProvider>,
          ICustomizeFixture<IModelMetadataProvider>,
          ICustomizeFixture<ModelBinderFactory>,
          ICustomizeFixture<MvcOptions>
    {
        public IServiceProvider Customize(IFixture fixture)
        {
            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddLogging();
            services.AddOptions();
            services.AddMvcCore();
            services.AddTransient<ModelBinderFactory>();

            return services.BuildServiceProvider();
        }

        IModelMetadataProvider ICustomizeFixture<IModelMetadataProvider>.Customize(IFixture fixture)
            => fixture.Create<IServiceProvider>().GetRequiredService<IModelMetadataProvider>();

        ModelBinderFactory ICustomizeFixture<ModelBinderFactory>.Customize(IFixture fixture)
            => fixture.Create<IServiceProvider>().GetRequiredService<ModelBinderFactory>();

        MvcOptions ICustomizeFixture<MvcOptions>.Customize(IFixture fixture)
            => fixture.Create<IServiceProvider>().GetRequiredService<IOptions<MvcOptions>>().Value;
    }
}
