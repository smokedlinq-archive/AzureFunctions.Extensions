using AzureFunctions.Extensions.Sample.OData;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(AzureFunctions.Extensions.OData.Sample.Startup))]

namespace AzureFunctions.Extensions.OData.Sample
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            builder.Services.AddDbContext<EFContext>(options => options.UseInMemoryDatabase(nameof(EFContext)));
            builder.Services.AddOData(builder => builder.EntitySet<Product>("Products"));
        }
    }
}
