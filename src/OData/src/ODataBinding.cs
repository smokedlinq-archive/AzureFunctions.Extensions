using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Azure.Functions.Extensions.OData
{
    internal class ODataBinding : IBinding
    {
        private readonly ODataContext _context;
        private readonly EnableQueryAttribute _attribute;
        private readonly Type _type;
        private readonly Func<HttpRequest, ODataQueryContext, ODataQueryOptions> _optionsBuilder;

        public ODataBinding(ODataContext context, EnableQueryAttribute attribute, Type type)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _optionsBuilder = BuildODataQueryOptionsFactory(_type);
        }

        public bool FromAttribute => false;

        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
        {
            var request = value as HttpRequest ?? throw new InvalidOperationException($"{nameof(value)} must be an {nameof(HttpRequest)}");
            var queryContext = new ODataQueryContext(_context.Model, _type, new Microsoft.AspNet.OData.Routing.ODataPath());
            var query = _optionsBuilder(request, queryContext);

            _attribute.ValidateQuery(request, query);

            var provider = new ODataBindingValueProvider(query);
            return Task.FromResult<IValueProvider>(provider);
        }

        public Task<IValueProvider> BindAsync(BindingContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (!(context.BindingData["$request"] is HttpRequest request))
            {
                throw new NotSupportedException("The OData binding can only be used with the HttpTrigger binding; add a parameter with the HttpTrigger binding attribute.");
            }

            var http = new DefaultHttpContext
            {
                RequestServices = _context.Services
            };

            var odataRequest = http.Request;
            odataRequest.Method = request.Method;
            odataRequest.Host = request.Host;
            odataRequest.Path = request.Path;
            odataRequest.QueryString = request.QueryString;

            return BindAsync(odataRequest, context.ValueContext);
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor
            {
                Name = "odata"
            };
        }

        private static Func<HttpRequest, ODataQueryContext, ODataQueryOptions> BuildODataQueryOptionsFactory(Type type)
        {
            var queryOptionsType = typeof(ODataQueryOptions<>).MakeGenericType(type);
            var queryOptionsCtor = queryOptionsType.GetConstructor(new[] { typeof(ODataQueryContext), typeof(HttpRequest) });
            var queryOptionsParams = new[]
            {
                Expression.Parameter(typeof(ODataQueryContext), "context"),
                Expression.Parameter(typeof(HttpRequest), "request")
            };
            var lambda = Expression.Lambda<Func<ODataQueryContext, HttpRequest, ODataQueryOptions>>(Expression.New(queryOptionsCtor, queryOptionsParams), queryOptionsParams).Compile();

            return (request, context) => lambda.Invoke(context, request);
        }
    }
}