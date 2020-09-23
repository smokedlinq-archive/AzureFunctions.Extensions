using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
    public class MvcModelBindingProviderTests
    {
        [Theory]
        [InlineAutoMoq(typeof(FromQueryAttribute))]
        [InlineAutoMoq(typeof(FromHeaderAttribute))]
        [InlineAutoMoq(typeof(FromFormAttribute))]
        [InlineAutoMoq(typeof(FromBodyAttribute))]
        [InlineAutoMoq(typeof(FromServicesAttribute))]
        public async Task FunctionWithMvcModelBindingAttributeParameterShouldBeSupported(
            Type parameterAttributeType,
            Dictionary<string, Type> bindingDataContract,
            Mock<IModelBinderFactory> modelBinderFactory,
            Mock<IModelMetadataProvider> modelMetadataProvider,
            MvcOptions mvcOptions)
        {
            // Arrange
            var parameter = GetParameterWithAttribute(nameof(Function), parameterAttributeType);
            var context = new BindingProviderContext(parameter, bindingDataContract, CancellationToken.None);

            modelMetadataProvider.Setup(x => x.GetMetadataForType(It.IsAny<Type>()))
                .Returns(() =>
                {
                    var identity = ModelMetadataIdentity.ForParameter(parameter);
                    var details = new DefaultMetadataDetails(identity, ModelAttributes.GetAttributesForParameter(parameter));
                    return new DefaultModelMetadata(modelMetadataProvider.Object, Mock.Of<ICompositeMetadataDetailsProvider>(), details);
                });

            modelBinderFactory.Setup(x => x.CreateBinder(It.IsAny<ModelBinderFactoryContext>()))
                .Returns(Mock.Of<IModelBinder>());

            // Act
            var sut = new MvcModelBindingProvider(modelBinderFactory.Object, modelMetadataProvider.Object, Options.Create(mvcOptions));
            var result = await sut.TryCreateAsync(context).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }

        private ParameterInfo GetParameterWithAttribute(string methodName, Type attributeType)
            => GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static).GetParameters().First(x => !(x.GetCustomAttribute(attributeType) is null));

        private static void Function(
            [FromQuery] string fromQuery,
            [FromHeader] string fromHeader,
            [FromForm] string fromForm,
            [FromBody] string fromBody,
            [FromServices] IConfiguration fromServices)
        {
            // NOOP
        }
    }
}
