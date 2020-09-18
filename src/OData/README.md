# Azure.Functions.Extensions.OData

This package adds binding extensions to handle OData queries on HTTP requests. 

## Features

The underlying OData support is provided by the [ASP.NET Core OData package](https://github.com/OData/AspNetCoreOData) and only supports querying IQueryable<T> data sources.

> __Note:__ [EF Core has several limitations](https://github.com/dotnet/efcore/issues?q=is%3Aissue+is%3Aopen+odata) with OData queries.

## Example usage

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
