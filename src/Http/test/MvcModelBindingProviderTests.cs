using A3;
using AutoFixture;
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
        [InlineData(typeof(FromQueryAttribute))]
        [InlineData(typeof(FromHeaderAttribute))]
        [InlineData(typeof(FromFormAttribute))]
        [InlineData(typeof(FromBodyAttribute))]
        [InlineData(typeof(FromServicesAttribute))]
        public Task FunctionWithMvcModelBindingAttributeParameterShouldBeSupported(Type parameterAttributeType)
            => A3<MvcModelBindingProvider>
            .Arrange(setup =>
            {
                ParameterInfo GetParameterWithAttribute(string methodName, Type attributeType)
                    => GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)!.GetParameters().First(x => !(x.GetCustomAttribute(attributeType) is null));

                var parameter = GetParameterWithAttribute(nameof(Function), parameterAttributeType);
                var bindingDataContract = setup.Fixture.Create<Dictionary<string, Type>>();

                setup.Mock<IModelMetadataProvider>(modelMetadataProvider => modelMetadataProvider
                    .Setup(x => x.GetMetadataForType(It.IsAny<Type>()))
                    .Returns(() =>
                    {
                        var identity = ModelMetadataIdentity.ForParameter(parameter);
                        var details = new DefaultMetadataDetails(identity, ModelAttributes.GetAttributesForParameter(parameter));
                        return new DefaultModelMetadata(modelMetadataProvider.Object, Mock.Of<ICompositeMetadataDetailsProvider>(), details);
                    }));

                setup.Mock<IModelBinderFactory>(modelBinderFactory => modelBinderFactory
                    .Setup(x => x.CreateBinder(It.IsAny<ModelBinderFactoryContext>()))
                    .Returns(Mock.Of<IModelBinder>()));

                var mvcOptions = setup.Fixture.Create<MvcOptions>();
                var modelBinderFactory = setup.Mock<IModelBinderFactory>().Object;
                var modelMetadataProvider = setup.Mock<IModelMetadataProvider>().Object;

                setup.Sut(new MvcModelBindingProvider(modelBinderFactory, modelMetadataProvider, Options.Create(mvcOptions)));
                setup.Parameter(new BindingProviderContext(parameter, bindingDataContract, CancellationToken.None));
            })
            .Act(async (MvcModelBindingProvider sut, BindingProviderContext context) => await sut.TryCreateAsync(context))
            .Assert(result => result.Should().NotBeNull());

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
