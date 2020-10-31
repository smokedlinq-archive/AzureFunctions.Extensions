using System.ComponentModel.DataAnnotations;

namespace Azure.Functions.Extensions.OData.Tests
{
    public class EntityFixture
    {
        [Key]
        public string Name { get; set; } = string.Empty;
    }
}
