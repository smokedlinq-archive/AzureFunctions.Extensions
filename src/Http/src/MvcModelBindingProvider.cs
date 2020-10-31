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
                var modelMetadata = _modelMetadataProvider switch
                {
                    ModelMetadataProvider provider => provider.GetMetadataForParameter(context.Parameter),
                    _ => _modelMetadataProvider.GetMetadataForType(context.Parameter.ParameterType)
                };

                var modelBinderFactoryContext = new ModelBinderFactoryContext
                {
                    Metadata = modelMetadata,
                    BindingInfo = BindingInfo.GetBindingInfo(context.Parameter.GetCustomAttributes())
                };

                var modelBinder = _modelBinderFactory.CreateBinder(modelBinderFactoryContext);

                return Task.FromResult<IBinding?>(new MvcModelBinding(context.Parameter, modelMetadata, modelBinder, _mvcOptions.ValueProviderFactories));
            }

            return Task.FromResult<IBinding?>(null);
        }
    }
}