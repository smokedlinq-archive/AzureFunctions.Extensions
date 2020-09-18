# Azure.Functions.Extensions.Http

This package adds binding extensions to HTTP requests.

## Features

This packages adds support for ASP.NET MVC model binding attributes such as `FromHeader`, `FromQuery`, `FromForm`, and `FromServices`.

## Example usage

```csharp
public static class FnGetHttp
{
    [FunctionName(nameof(FnGetHttp))]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "people")] HttpRequest req,
        [FromForm] Person person,
        [FromServices] Repository<Person> repository)
    {
        await repository.AddAsync(person);
        return new CreatedResult($"people/{person.Id}");
    }
}

public class Person
{
    [FromQuery(Name = "id")]
    public string Id { get; set; }

    public string Name { get; set; }

    [FromHeader(Name = "x-forwarded-for")]
    public string CreatedBy { get; set; }
}
```
