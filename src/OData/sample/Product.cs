using System.ComponentModel.DataAnnotations;

namespace AzureFunctions.Extensions.Sample.OData
{
    public class Product
    {
        [Key]
        public int Sku { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
