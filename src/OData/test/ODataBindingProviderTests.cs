using A3;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Azure.Functions.Extensions.OData.Tests
{
    public class ODataBindingProviderTests
    {
        [Fact]
        public Task ShouldReturnTheValueFromTheValueProvider()
            => A3<ODataBindingProvider>
            .Arrange(setup =>
            {
                ParameterInfo GetParameterWithAttribute(string methodName, Type attributeType)
                    => GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)!.GetParameters().First(x => !(x.GetCustomAttribute(attributeType) is null));

                var parameter = GetParameterWithAttribute(nameof(Function), typeof(ODataAttribute));
                var bindingDataContract = setup.Fixture.Create<Dictionary<string, Type>>();
                var oDataContext = setup.Fixture.Create<IServiceProvider>().GetRequiredService<ODataContext>();
                setup.Sut(new ODataBindingProvider(oDataContext));
                setup.Parameter(new BindingProviderContext(parameter, bindingDataContract, CancellationToken.None));
            })
            .Act(async (ODataBindingProvider sut, BindingProviderContext context) => await sut.TryCreateAsync(context))
            .Assert(result => result.Should().NotBeNull());

        [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
        private static void Function([OData] ODataQueryOptions<EntityFixture> odata)
        {
            // NOOP
        }
    }
}
