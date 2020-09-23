using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using IModelBinder = Microsoft.AspNetCore.Mvc.ModelBinding.IModelBinder;
using ModelMetadata = Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata;
using ValueProviderFactory = Microsoft.AspNetCore.Mvc.ModelBinding.IValueProviderFactory;

namespace Azure.Functions.Extensions.Http
{
    internal class MvcModelBinding : IBinding
    {
        private readonly ParameterInfo _parameter;
        private readonly ModelMetadata _metadata;
        private readonly IModelBinder _binder;
        private readonly IList<ValueProviderFactory> _valueProviderFactories;

        public MvcModelBinding(ParameterInfo parameter, ModelMetadata metadata, IModelBinder binder, IList<ValueProviderFactory> valueProviderFactories)
        {
            _parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _binder = binder ?? throw new ArgumentNullException(nameof(binder));
            _valueProviderFactories = valueProviderFactories ?? throw new ArgumentNullException(nameof(valueProviderFactories));
        }

        public bool FromAttribute => true;

        public async Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
        {
            var request = value as HttpRequest ?? throw new ArgumentException($"{nameof(value)} must be an {nameof(HttpRequest)}", nameof(value));
            var actionContext = new ActionContext
            {
                HttpContext = request.HttpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData()
            };
            var valueProvider = await Microsoft.AspNetCore.Mvc.ModelBinding.CompositeValueProvider.CreateAsync(actionContext, _valueProviderFactories).ConfigureAwait(false);
            var bindingInfo = Microsoft.AspNetCore.Mvc.ModelBinding.BindingInfo.GetBindingInfo(_parameter.GetCustomAttributes());
            var modelBindingContext = Microsoft.AspNetCore.Mvc.ModelBinding.DefaultModelBindingContext.CreateBindingContext(actionContext, valueProvider, _metadata, bindingInfo, _metadata.BinderModelName ?? _metadata.Name);

            await _binder.BindModelAsync(modelBindingContext).ConfigureAwait(false);

            return new MvcModelValueProvider(_metadata.ModelType, () => modelBindingContext.Result.Model);
        }

        public Task<IValueProvider> BindAsync(BindingContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (!(context.BindingData["$request"] is HttpRequest request))
            {
                throw new NotSupportedException("The binding can only be used with the HttpTrigger binding; add a parameter with the HttpTrigger binding attribute.");
            }

            return BindAsync(request, context.ValueContext);
        }

        public ParameterDescriptor ToParameterDescriptor()
            => new ParameterDescriptor
            {
                Name = _metadata.ParameterName,
                Type = _metadata.ModelType.FullName
            };
    }
}