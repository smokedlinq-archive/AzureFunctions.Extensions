using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Azure.Functions.Extensions.Http.Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class AutoMoqAttribute : AutoDataAttribute
    {
        public AutoMoqAttribute()
            : base(() => CreateFixture())
        {
        }

        private static IFixture CreateFixture()
        {
            var fixture = new Fixture();

            fixture.Customize(new AutoMoqCustomization());

            fixture.Register<IServiceCollection>(() =>
            {
                var services = new ServiceCollection();

                services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
                services.AddLogging();
                services.AddOptions();
                services.AddMvcCore();
                services.AddTransient<ModelBinderFactory>();

                return services;
            });

            fixture.Register<IServiceProvider>(() => fixture.Create<IServiceCollection>().BuildServiceProvider());
            fixture.Register(() => fixture.Create<IServiceProvider>().GetRequiredService<ILoggerFactory>());
            fixture.Register(() => fixture.Create<IServiceProvider>().GetRequiredService<IOptions<MvcOptions>>().Value);
            fixture.Register(() => fixture.Create<IServiceProvider>().GetRequiredService<IModelMetadataProvider>());
            fixture.Register(() => fixture.Create<IServiceProvider>().GetRequiredService<ModelBinderFactory>());

            return fixture;
        }
    }
}
