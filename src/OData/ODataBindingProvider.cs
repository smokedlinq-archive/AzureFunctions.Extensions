using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Reflection;
using System.Threading.Tasks;

#pragma warning disable CA1812

namespace Azure.Functions.Extensions.OData
{
    internal class ODataBindingProvider : IBindingProvider
    {
        private readonly ODataContext _context;

        public ODataBindingProvider(ODataContext context)
            => _context = context ?? throw new ArgumentNullException(nameof(context));

        public Task<IBinding?> TryCreateAsync(BindingProviderContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var attribute = context.Parameter.Member.GetCustomAttribute<EnableQueryAttribute>(inherit: false);

            if (attribute != null && context.Parameter.ParameterType.IsGenericType && context.Parameter.ParameterType.GetGenericTypeDefinition() == typeof(ODataQueryOptions<>))
            {
                return Task.FromResult<IBinding?>(new ODataBinding(_context, attribute, context.Parameter.ParameterType.GenericTypeArguments[0]));
            }

            return Task.FromResult<IBinding?>(null);
        }
    }
}
