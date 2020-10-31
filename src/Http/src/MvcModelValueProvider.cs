using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Threading.Tasks;

namespace Azure.Functions.Extensions.Http
{
    internal class MvcModelValueProvider : IValueProvider
    {
        private readonly Lazy<object?> _value;

        public MvcModelValueProvider(Type type, Func<object?> valueProvider)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            _value = new Lazy<object?>(valueProvider ?? throw new ArgumentNullException(nameof(valueProvider)));
        }

        public Type Type { get; }

        public Task<object?> GetValueAsync()
        {
            return Task.FromResult(_value.Value);
        }

        public string? ToInvokeString()
            => _value.Value?.ToString();
    }
}