using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AzureFunctions.Extensions.Sample.OData
{
    public class GetProducts
    {
        private readonly EFContext _context;

        public GetProducts(EFContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _context.Database.EnsureCreated();
        }

        [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
        [FunctionName(nameof(GetProducts))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequest req,
            ODataQueryOptions<Product> odata)
        {
            _ = req ?? throw new ArgumentNullException(nameof(req));
            _ = odata ?? throw new ArgumentNullException(nameof(odata));
            var results = await odata.ApplyTo(_context.Products).Cast<Product>().ToListAsync().ConfigureAwait(false);
            return new OkObjectResult(results);
        }
    }
}
