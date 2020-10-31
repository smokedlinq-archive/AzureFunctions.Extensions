using A3;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Azure.Functions.Extensions.Http.Tests
{
    public class MvcModelBindingTests
    {
        [Theory]
        [InlineData(typeof(FromQueryAttribute))]
        [InlineData(typeof(FromHeaderAttribute))]
        [InlineData(typeof(FromFormAttribute))]
        [InlineData(typeof(FromServicesAttribute))]
        public Task BinderCanBindValuesFromHttpRequest(Type parameterAttributeType)
            => A3<MvcModelBinding>
            .Arrange(setup =>
            {
                ParameterInfo GetParameterWithAttribute(string methodName, Type attributeType)
                    => GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)!.GetParameters().First(x => !(x.GetCustomAttribute(attributeType) is null));

                setup.Mock<HttpContext>(httpContext =>
                {
                    httpContext.SetupGet(x => x.RequestServices).Returns(setup.Fixture.Create<IServiceProvider>());
                    httpContext.SetupGet(x => x.Request).Returns(() => setup.Mock<HttpRequest>().Object);
                });

                setup.Mock<HttpRequest>(request =>
                {
                    request.SetupGet(x => x.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>
                    {
                        ["fromQuery"] = new StringValues("value from query")
                    }));

                    request.SetupGet(x => x.Headers).Returns(new HeaderDictionary
                    {
                        ["fromHeader"] = new StringValues("value from header")
                    });

                    request.SetupGet(x => x.HasFormContentType).Returns(true);

                    request.Setup(x => x.ReadFormAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new FormCollection(new Dictionary<string, StringValues>
                        {
                            ["fromForm"] = "value from form"
                        }));

                    request.SetupGet(x => x.HttpContext).Returns(() => setup.Mock<HttpContext>().Object);
                });

                var parameter = GetParameterWithAttribute(nameof(Function), parameterAttributeType);
                var metadata = ((ModelMetadataProvider)setup.Fixture.Create<IModelMetadataProvider>()).GetMetadataForParameter(parameter);
                var modelBinderFactoryContext = new ModelBinderFactoryContext
                {
                    BindingInfo = BindingInfo.GetBindingInfo(parameter.GetCustomAttributes()),
                    Metadata = metadata
                };
                var binder = setup.Fixture.Create<ModelBinderFactory>().CreateBinder(modelBinderFactoryContext);
                var functionBindingContext = new FunctionBindingContext(Guid.NewGuid(), CancellationToken.None);
                var valueBindingContext = new ValueBindingContext(functionBindingContext, CancellationToken.None);
                var mvcOptions = setup.Fixture.Create<MvcOptions>();

                setup.Sut(new MvcModelBinding(parameter, metadata, binder, mvcOptions.ValueProviderFactories));
                setup.Parameter(new BindingContext(valueBindingContext, new Dictionary<string, object>
                {
                    ["$request"] = setup.Mock<HttpRequest>().Object
                }));
            })
            .Act(async (MvcModelBinding sut, BindingContext bindingContext) =>
            {
                var binding = await sut.BindAsync(bindingContext);
                return await binding.GetValueAsync();
            })
            .Assert(result => result.Should().NotBeNull());

        private static void Function(
            [FromQuery] string fromQuery,
            [FromHeader] string fromHeader,
            [FromForm] string fromForm,
            [FromServices] IConfiguration fromServices)
        {
            // NOOP
        }
    }
}
