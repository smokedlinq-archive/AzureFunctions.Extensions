using Azure.Functions.Extensions.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.CompilerServices;

#pragma warning disable CA1812

[assembly: WebJobsStartup(typeof(WebJobsStartup))]
[assembly: InternalsVisibleTo("Azure.Functions.Extensions.Http.Tests")]

namespace Azure.Functions.Extensions.Http
{
    internal class WebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.Services.AddOptions<MvcOptions>()
                .Configure<IHttpRequestStreamReaderFactory>((options, requestStreamReaderFactory) =>
                {
                    options.ModelBinderProviders.Add(new BinderTypeModelBinderProvider());
                    options.ModelBinderProviders.Add(new ServicesModelBinderProvider());
                    options.ModelBinderProviders.Add(new BodyModelBinderProvider(options.InputFormatters, requestStreamReaderFactory));
                    options.ModelBinderProviders.Add(new HeaderModelBinderProvider());
                    options.ModelBinderProviders.Add(new FloatingPointTypeModelBinderProvider());
                    options.ModelBinderProviders.Add(new EnumTypeModelBinderProvider(options));
                    options.ModelBinderProviders.Add(new SimpleTypeModelBinderProvider());
                    options.ModelBinderProviders.Add(new CancellationTokenModelBinderProvider());
                    options.ModelBinderProviders.Add(new ByteArrayModelBinderProvider());
                    options.ModelBinderProviders.Add(new FormFileModelBinderProvider());
                    options.ModelBinderProviders.Add(new FormCollectionModelBinderProvider());
                    options.ModelBinderProviders.Add(new KeyValuePairModelBinderProvider());
                    options.ModelBinderProviders.Add(new DictionaryModelBinderProvider());
                    options.ModelBinderProviders.Add(new ArrayModelBinderProvider());
                    options.ModelBinderProviders.Add(new CollectionModelBinderProvider());
                    options.ModelBinderProviders.Add(new ComplexTypeModelBinderProvider());

                    options.ValueProviderFactories.Add(new FormValueProviderFactory());
                    options.ValueProviderFactories.Add(new RouteValueProviderFactory());
                    options.ValueProviderFactories.Add(new QueryStringValueProviderFactory());
                    options.ValueProviderFactories.Add(new JQueryFormValueProviderFactory());

                    options.ModelValidatorProviders.Add(new DefaultModelValidatorProvider());
                });

            builder.Services.AddSingleton<IBindingProvider, MvcModelBindingProvider>();
        }
    }
}
