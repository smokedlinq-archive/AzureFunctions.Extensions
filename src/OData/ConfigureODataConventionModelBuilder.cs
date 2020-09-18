using Microsoft.AspNet.OData.Builder;
using System;

namespace Azure.Functions.Extensions.OData
{
    internal class ConfigureODataConventionModelBuilder
    {
        private readonly Action<ODataConventionModelBuilder> _callback;

        public ConfigureODataConventionModelBuilder(Action<ODataConventionModelBuilder> configure)
            => _callback = configure ?? throw new ArgumentNullException(nameof(configure));

        public void Configure(ODataConventionModelBuilder builder)
            => _callback(builder);
    }
}
