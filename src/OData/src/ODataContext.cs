using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;

namespace Azure.Functions.Extensions.OData
{
    internal sealed class ODataContext : IDisposable
    {
        private readonly Lazy<IEdmModel> _model;

        public ODataContext(Func<IEnumerable<ConfigureODataConventionModelBuilder>> conventions)
        {
            var services = new ServiceCollection();

            services.AddMvcCore();
            services.AddOData();
            services.AddTransient<ODataUriResolver>();
            services.AddTransient<ODataQueryValidator>();
            services.AddTransient<TopQueryValidator>();
            services.AddTransient<FilterQueryValidator>();
            services.AddTransient<SkipQueryValidator>();
            services.AddTransient<OrderByQueryValidator>();
            services.AddTransient<CountQueryValidator>();
            services.AddTransient<SelectExpandQueryValidator>();
            services.AddTransient<SkipTokenQueryValidator>();

            Services = services.BuildServiceProvider();

            var routeBuilder = new RouteBuilder(new ApplicationBuilder(Services));
            routeBuilder
                .Count()
                .Expand()
                .Filter()
                .MaxTop(null)
                .OrderBy()
                .Select()
                .SkipToken();
            routeBuilder.EnableDependencyInjection();

            _model = new Lazy<IEdmModel>(() =>
            {
                var builder = new ODataConventionModelBuilder(Services);

                foreach (var convention in conventions())
                {
                    convention.Configure(builder);
                }

                return builder.GetEdmModel();
            });
        }

        ~ODataContext()
            => Dispose(false);

        public ServiceProvider Services { get; private set; }

        public IEdmModel Model => _model.Value;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Services.Dispose();
            }
        }
    }
}