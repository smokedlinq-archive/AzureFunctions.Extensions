using A3;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Azure.Functions.Extensions.OData.Tests
{
    public class ODataBindingTests
    {
        [Fact]
        public Task BinderCanBindValuesFromHttpRequest()
            => A3<ODataBinding>
            .Arrange(setup =>
            {
                ParameterInfo GetParameterWithAttribute(string methodName, Type attributeType)
                    => GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)!.GetParameters().First(x => !(x.GetCustomAttribute(attributeType) is null));

                setup.Mock<HttpRequest>(request =>
                {
                    request.SetupGet(x => x.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>
                    {
                        ["$select"] = new StringValues("Name")
                    }));
                });

                var parameter = GetParameterWithAttribute(nameof(Function), typeof(ODataAttribute));
                var oDataContext = setup.Fixture.Create<IServiceProvider>().GetRequiredService<ODataContext>();
                var functionBindingContext = new FunctionBindingContext(Guid.NewGuid(), CancellationToken.None);
                var valueBindingContext = new ValueBindingContext(functionBindingContext, CancellationToken.None);

                setup.Sut(new ODataBinding(oDataContext, parameter.Member.GetCustomAttribute<EnableQueryAttribute>()!, parameter.ParameterType.GetGenericArguments()[0]));
                setup.Parameter(new BindingContext(valueBindingContext, new Dictionary<string, object>
                {
                    ["$request"] = setup.Mock<HttpRequest>().Object
                }));
            })
            .Act(async (ODataBinding sut, BindingContext bindingContext) =>
            {
                var binding = await sut.BindAsync(bindingContext);
                return await binding.GetValueAsync();
            })
            .Assert(result => result.Should().NotBeNull());

        [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
        private static void Function([OData] ODataQueryOptions<EntityFixture> odata)
        {
            // NOOP
        }
    }
}
