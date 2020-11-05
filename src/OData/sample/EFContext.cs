using Microsoft.EntityFrameworkCore;
using System;

namespace AzureFunctions.Extensions.Sample.OData
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public class EFContext : DbContext
    {
        public EFContext(DbContextOptions<EFContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder ?? throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasData(
                        new Product
                        {
                            Sku = 1,
                            Name = "Apple"
                        },
                        new Product
                        {
                            Sku = 2,
                            Name = "Orange"
                        },
                        new Product
                        {
                            Sku = 3,
                            Name = "Lettuce"
                        },
                        new Product
                        {
                            Sku = 4,
                            Name = "Potato"
                        });
            });
        }
    }
}
