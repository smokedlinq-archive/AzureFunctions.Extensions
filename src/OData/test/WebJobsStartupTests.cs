using A3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Xunit;

namespace Azure.Functions.Extensions.OData.Tests
{
    public class WebJobsStartupTests
    {
        [Fact]
        public void AfterStartupThenTheMvcModelBindingProviderShouldBeRegisteredAsAnIBindingProvider()
            => A3<ServiceCollection>
            .Arrange(setup =>
            {
                var services = new ServiceCollection();

                services.AddOptions();
                services.AddSingleton(setup.Mock<IHttpRequestStreamReaderFactory>().Object);

                setup.Mock<IWebJobsBuilder>(builder => builder.SetupGet(x => x.Services).Returns(services));

                setup.Sut(services);
                setup.Parameter(setup.Mock<IWebJobsBuilder>().Object);
            })
            .Act((ServiceCollection sut, IWebJobsBuilder builder) =>
            {
                new WebJobsStartup().Configure(builder);
                return sut;
            })
            .Assert(result =>
            {
                result.Any(x => x.ServiceType == typeof(IBindingProvider) && x.ImplementationType == typeof(ODataBindingProvider))
                    .Should().BeTrue();

                using var serviceProvider = result.BuildServiceProvider();
                serviceProvider.GetService<ODataContext>().Should().NotBeNull();
            });
    }
}
