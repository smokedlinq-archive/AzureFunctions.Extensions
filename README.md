# AzureFunctions.Extensions

![Build](https://github.com/smokedlinq/AzureFunctions.Extensions/workflows/Build/badge.svg)
[![SonarCloud Status](https://sonarcloud.io/api/project_badges/measure?project=smokedlinq_AzureFunctions.Extensions&metric=alert_status)](https://sonarcloud.io/dashboard?id=smokedlinq_AzureFunctions.Extensions)

Enhancements that make adoption of Azure Function Apps easier for .NET developers.

## AzureFunctions.Extensions.Http

[![NuGet](https://img.shields.io/nuget/dt/AzureFunctions.Extensions.Http.svg)](https://www.nuget.org/packages/AzureFunctions.Extensions.Http)
[![NuGet](https://img.shields.io/nuget/vpre/AzureFunctions.Extensions.Http.svg)](https://www.nuget.org/packages/AzureFunctions.Extensions.Http)

## AzureFunctions.Extensions.OData

[![NuGet](https://img.shields.io/nuget/dt/AzureFunctions.Extensions.OData.svg)](https://www.nuget.org/packages/AzureFunctions.Extensions.OData)
[![NuGet](https://img.shields.io/nuget/vpre/AzureFunctions.Extensions.OData.svg)](https://www.nuget.org/packages/AzureFunctions.Extensions.OData)

This package adds binding extensions to handle OData queries on HTTP requests.

### Features

The underlying OData support is provided by the [ASP.NET Core OData package](https://github.com/OData/AspNetCoreOData) and only supports querying IQueryable<T> data sources.

> __Note:__ [EF Core has several limitations](https://github.com/dotnet/efcore/issues?q=is%3Aissue+is%3Aopen+odata) with OData queries.

### Example usage
#### Setup
```csharp
public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddOData(builder =>
            {
                builder.EntitySet<Department>("Departments");
                builder.EntitySet<Product>("Products");
            }); 
    }
}
```  
HttpTrigger Function
```csharp
public class FnGetProductsHttp
{
    private readonly DbContext _context;

    public FnGetProductsHttp(DbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
    [FunctionName(nameof(FnGetProductsHttp))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequest req,
        ODataQueryOptions<Product> odata)
    {
        var results = await odata.ApplyTo(_context.Products).Cast<dynamic>().ToListAsync().ConfigureAwait(false);
        return new OkObjectResult(results);
    }
}
```
