using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

#pragma warning disable CA1812

namespace Azure.Functions.Extensions.Http
{
    internal class MvcModelBindingProvider : IBindingProvider
    {
        private readonly IModelBinderFactory _modelBinderFactory;
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly MvcOptions _mvcOptions;

        public MvcModelBindingProvider(IModelBinderFactory modelBinderFactory, IModelMetadataProvider modelMetadataProvider, IOptions<MvcOptions> mvcOptions)
        {
            _modelBinderFactory = modelBinderFactory ?? throw new ArgumentNullException(nameof(modelBinderFactory));
            _modelMetadataProvider = modelMetadataProvider ?? throw new ArgumentNullException(nameof(modelMetadataProvider));
            _mvcOptions = mvcOptions?.Value ?? throw new ArgumentNullException(nameof(mvcOptions));
        }

        public Task<IBinding?> TryCreateAsync(BindingProviderContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var attribute = context.Parameter.GetCustomAttributes(inherit: false).OfType<IBindingSourceMetadata>().FirstOrDefault();

            if (!(attribute is null))
            {
                var modelMetadata = GetModelMetadata(context.Parameter);

                var modelBinderFactoryContext = new ModelBinderFactoryContext
                {
                    Metadata = modelMetadata,
                    BindingInfo = new BindingInfo
                    {
                        BinderModelName = modelMetadata.BinderModelName,
                        BinderType = modelMetadata.BinderType,
                        BindingSource = attribute.BindingSource,
                        PropertyFilterProvider = modelMetadata.PropertyFilterProvider
                    }
                };

                var modelBinder = _modelBinderFactory.CreateBinder(modelBinderFactoryContext);

                return Task.FromResult<IBinding?>(new MvcModelBinding(modelMetadata, modelBinder, attribute.BindingSource, _mvcOptions.ValueProviderFactories));
            }

            return Task.FromResult<IBinding?>(null);
        }

        private ModelMetadata GetModelMetadata(ParameterInfo parameter)
        {
            if (_modelMetadataProvider is ModelMetadataProvider provider)
            {
                return provider.GetMetadataForParameter(parameter);
            }

            return _modelMetadataProvider.GetMetadataForType(parameter.ParameterType);
        }
    }
}