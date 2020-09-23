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
        [InlineAutoMoq(typeof(FromQueryAttribute))]
        [InlineAutoMoq(typeof(FromHeaderAttribute))]
        [InlineAutoMoq(typeof(FromFormAttribute))]
        [InlineAutoMoq(typeof(FromServicesAttribute))]
        public async Task BinderCanBindValuesFromHttpRequest(
            Type parameterAttributeType,
            IServiceProvider serviceProvider,
            IModelMetadataProvider modelMetadataProvider,
            MvcOptions mvcOptions,
            ModelBinderFactory modelBinderFactory,
            Mock<HttpContext> httpContext,
            Mock<HttpRequest> request)
        {
            // Arrange
            httpContext.SetupGet(x => x.RequestServices).Returns(serviceProvider);
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
            httpContext.SetupGet(x => x.Request).Returns(() => request.Object);
            request.SetupGet(x => x.HttpContext).Returns(() => httpContext.Object);

            var parameter = GetParameterWithAttribute(nameof(Function), parameterAttributeType);
            var metadata = ((ModelMetadataProvider)modelMetadataProvider).GetMetadataForParameter(parameter);
            var modelBinderFactoryContext = new ModelBinderFactoryContext
            {
                BindingInfo = BindingInfo.GetBindingInfo(parameter.GetCustomAttributes()),
                Metadata = metadata
            };
            var binder = modelBinderFactory.CreateBinder(modelBinderFactoryContext);
            var functionBindingContext = new FunctionBindingContext(Guid.NewGuid(), CancellationToken.None);
            var valueBindingContext = new ValueBindingContext(functionBindingContext, CancellationToken.None);
            var bindingContext = new BindingContext(valueBindingContext, new Dictionary<string, object>
            {
                ["$request"] = request.Object
            });
            var binding = new MvcModelBinding(parameter, metadata, binder, mvcOptions.ValueProviderFactories);

            // Assert
            var sut = await binding.BindAsync(bindingContext).ConfigureAwait(false);
            var result = await sut.GetValueAsync().ConfigureAwait(false);

            // Act
            result.Should().NotBeNull();
        }

        private ParameterInfo GetParameterWithAttribute(string methodName, Type attributeType)
            => GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static).GetParameters().First(x => !(x.GetCustomAttribute(attributeType) is null));

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
